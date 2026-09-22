using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KHRMS.Services
{
    public class AssetRequestService : IAssetRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AssetRequestService> _logger;

        public AssetRequestService(IUnitOfWork unitOfWork, ILogger<AssetRequestService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AssetRequestResponseModel?> CreateAssetRequest(CreateAssetRequestModel model)
        {
            try
            {
                if (model == null)
                    return null;

                var asset = (await _unitOfWork.AssetsMasters.GetAll())
                    .FirstOrDefault(a => a.Id == model.AssetId && !a.IsDeleted);

                if (asset == null)
                    return null;

                var employee = (await _unitOfWork.Employees.GetAll())
                    .FirstOrDefault(e => e.Id == model.EmployeeId && !e.IsDeleted);

                if (employee == null)
                    return null;

                var initialStatus = model.RequestType?.Trim().ToLower() switch
                {
                    "return" => "Return Initiated",
                    "replacement" => "Replacement Requested",
                    _ => "Repair Requested"
                };

                var now = DateTime.UtcNow;

                var assetRequest = new AssetRequest
                {
                    AssetId = model.AssetId,
                    EmployeeId = model.EmployeeId,
                    RequestType = model.RequestType,
                    Priority = model.Priority ?? "Medium",
                    Reason = model.Reason,
                    Description = model.Description,
                    ImageUrls = model.ImageUrls,
                    Status = initialStatus,
                    CreatedDate = now,
                    UpdatedDate = now,
                    IsActive = true,
                    IsDeleted = false
                };

                await _unitOfWork.AssetRequests.Add(assetRequest);
                _unitOfWork.Save();

                // Add initial audit log
                var initialLog = new AssetRequestLog
                {
                    AssetRequestId = assetRequest.Id,
                    FromStatus = null,
                    ToStatus = initialStatus,
                    ActionByEmployeeId = model.EmployeeId,
                    ActionByName = $"{employee.FirstName} {employee.LastName}".Trim(),
                    Remarks = $"Request submitted by employee. Reason: {model.Reason}",
                    CreatedDate = now
                };

                await _unitOfWork.AssetRequestLogs.Add(initialLog);

                // Update asset master status to reflect ongoing ticket
                if (model.RequestType?.Equals("Repair", StringComparison.OrdinalIgnoreCase) == true)
                {
                    asset.Status = "In Repair";
                    asset.UpdatedDate = now;
                    _unitOfWork.AssetsMasters.Update(asset);
                }

                var empFullName = $"{employee.FirstName} {employee.LastName}".Trim();

                // 1. Notification for Employee (Ticket Creator)
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = model.EmployeeId,
                    Title = $"Asset Ticket #{assetRequest.Id} Created",
                    Message = $"Your {assetRequest.RequestType} request for {asset.AssetsMasterName} ({asset.SerialNumber ?? asset.AssetType}) has been submitted with status '{initialStatus}'.",
                    Category = "Asset",
                    Type = "request",
                    Icon = "inventory_2",
                    IconBg = "#e0f2fe",
                    IconColor = "#0284c7",
                    Route = "/index/assets",
                    IsRead = false,
                    CreatedDate = now,
                    UpdatedDate = now,
                    IsActive = true,
                    IsDeleted = false
                });

                // 2. Notification for Approvers (Admin / HR / Manager)
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = 0,
                    Title = $"New Asset Request: Ticket #{assetRequest.Id}",
                    Message = $"{empFullName} submitted a {assetRequest.RequestType} request for {asset.AssetsMasterName} ({asset.SerialNumber ?? asset.AssetType}). Reason: {model.Reason}",
                    Category = "Asset",
                    Type = "request",
                    Icon = "inventory_2",
                    IconBg = "#e0f2fe",
                    IconColor = "#0284c7",
                    Route = "/index/request",
                    QueryParams = "tab=asset",
                    IsRead = false,
                    CreatedDate = now,
                    UpdatedDate = now,
                    IsActive = true,
                    IsDeleted = false
                });

                _unitOfWork.Save();

                return MapToResponseModel(assetRequest, asset, employee, new List<AssetRequestLog> { initialLog });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset request.");
                return null;
            }
        }

        public async Task<bool> UpdateAssetRequestStatus(UpdateAssetRequestStatusModel model)
        {
            try
            {
                if (model == null || model.RequestId <= 0 || string.IsNullOrWhiteSpace(model.NewStatus))
                    return false;

                var request = (await _unitOfWork.AssetRequests.GetAll())
                    .FirstOrDefault(r => r.Id == model.RequestId && !r.IsDeleted);

                if (request == null)
                    return false;

                var asset = (await _unitOfWork.AssetsMasters.GetAll())
                    .FirstOrDefault(a => a.Id == request.AssetId && !a.IsDeleted);

                var reqEmployee = (await _unitOfWork.Employees.GetAll())
                    .FirstOrDefault(e => e.Id == request.EmployeeId && !e.IsDeleted);
                var empFullName = reqEmployee != null ? $"{reqEmployee.FirstName} {reqEmployee.LastName}".Trim() : asset?.AssignedTo;

                var fromStatus = request.Status;
                var now = DateTime.UtcNow;

                request.Status = model.NewStatus.Trim();
                request.UpdatedDate = now;

                if (!string.IsNullOrWhiteSpace(model.AdminRemarks))
                    request.AdminRemarks = model.AdminRemarks.Trim();

                if (!string.IsNullOrWhiteSpace(model.InspectionRemarks))
                    request.InspectionRemarks = model.InspectionRemarks.Trim();

                // Check if this request is rejected or was previously rejected
                var normalizedStatus = model.NewStatus.Trim().ToLower();
                bool isRejectedFlow = fromStatus.Contains("Reject", StringComparison.OrdinalIgnoreCase)
                    || (request.Status != null && request.Status.Contains("Reject", StringComparison.OrdinalIgnoreCase))
                    || normalizedStatus.Contains("reject")
                    || (model.AdminRemarks != null && model.AdminRemarks.Contains("Reject", StringComparison.OrdinalIgnoreCase))
                    || (model.Remarks != null && model.Remarks.Contains("Reject", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(model.CourierPartner))
                    request.CourierPartner = model.CourierPartner.Trim();

                if (!string.IsNullOrWhiteSpace(model.TrackingNumber))
                    request.TrackingNumber = model.TrackingNumber.Trim();

                if (normalizedStatus.Contains("transit") || normalizedStatus.Contains("pickup") || normalizedStatus.Contains("dispatch") || normalizedStatus.Contains("desk") || normalizedStatus.Contains("handover"))
                {
                    if (!request.DispatchedDate.HasValue)
                        request.DispatchedDate = now;
                }
                else if (normalizedStatus.Contains("deliver"))
                {
                    request.DeliveredDate = now;
                }
                else if (normalizedStatus.Contains("received"))
                {
                    request.ReceivedDate = now;

                    // If employee received -> ticket is finished and asset is active
                    if (!normalizedStatus.Contains("admin"))
                    {
                        request.Status = "Completed";

                        if (asset != null)
                        {
                            asset.Status = "Active";
                            asset.IsActive = true;
                            asset.EmployeeId = request.EmployeeId;
                            if (!string.IsNullOrWhiteSpace(empFullName))
                            {
                                asset.AssignedTo = empFullName;
                            }
                            asset.UpdatedDate = now;
                            _unitOfWork.AssetsMasters.Update(asset);
                        }
                    }
                }
                else if (normalizedStatus.Equals("closed", StringComparison.OrdinalIgnoreCase) || normalizedStatus.Contains("closed"))
                {
                    request.Status = "Closed";

                    // If genuine Return flow completed (and NOT a rejected ticket) -> asset becomes Available in company inventory
                    if (asset != null && request.RequestType.Equals("Return", StringComparison.OrdinalIgnoreCase) && !isRejectedFlow)
                    {
                        asset.Status = "Available";
                        asset.AssignedTo = "Unassigned";
                        asset.EmployeeId = null;
                        asset.IsActive = true;
                        asset.UpdatedDate = now;
                        _unitOfWork.AssetsMasters.Update(asset);
                    }
                    else if (asset != null)
                    {
                        // If closed after rejection or completed service, asset stays active with the assigned employee
                        asset.Status = "Active";
                        asset.IsActive = true;
                        asset.EmployeeId = request.EmployeeId;
                        if (!string.IsNullOrWhiteSpace(empFullName))
                        {
                            asset.AssignedTo = empFullName;
                        }
                        asset.UpdatedDate = now;
                        _unitOfWork.AssetsMasters.Update(asset);
                    }
                }
                else if (normalizedStatus.Contains("completed"))
                {
                    request.Status = "Completed";

                    // Only genuine completed Return flow unassigns asset back to company Available inventory
                    if (asset != null && request.RequestType.Equals("Return", StringComparison.OrdinalIgnoreCase) && !isRejectedFlow)
                    {
                        asset.Status = "Available";
                        asset.AssignedTo = "Unassigned";
                        asset.EmployeeId = null;
                        asset.IsActive = true;
                        asset.UpdatedDate = now;
                        _unitOfWork.AssetsMasters.Update(asset);
                    }
                    else if (asset != null)
                    {
                        asset.Status = "Active";
                        asset.IsActive = true;
                        asset.EmployeeId = request.EmployeeId;
                        if (!string.IsNullOrWhiteSpace(empFullName))
                        {
                            asset.AssignedTo = empFullName;
                        }
                        asset.UpdatedDate = now;
                        _unitOfWork.AssetsMasters.Update(asset);
                    }
                }
                else if (normalizedStatus.Contains("reject"))
                {
                    request.Status = "Rejected";

                    // When any request is rejected (Repair, Replacement, or Return),
                    // the asset remains in the possession of the requesting employee and is Active.
                    if (asset != null)
                    {
                        asset.Status = "Active";
                        asset.IsActive = true;
                        asset.EmployeeId = request.EmployeeId;
                        if (!string.IsNullOrWhiteSpace(empFullName))
                        {
                            asset.AssignedTo = empFullName;
                        }
                        asset.UpdatedDate = now;
                        _unitOfWork.AssetsMasters.Update(asset);
                    }
                }
                else if (normalizedStatus.Contains("repair"))
                {
                    if (asset != null)
                    {
                        asset.Status = "In Repair";
                        asset.IsActive = false;
                        asset.UpdatedDate = now;
                        _unitOfWork.AssetsMasters.Update(asset);
                    }
                }

                _unitOfWork.AssetRequests.Update(request);

                // Add tracking audit log
                var log = new AssetRequestLog
                {
                    AssetRequestId = request.Id,
                    FromStatus = fromStatus,
                    ToStatus = request.Status,
                    ActionByEmployeeId = model.ActionByEmployeeId,
                    ActionByName = model.ActionByName ?? "System Admin",
                    Remarks = model.Remarks ?? $"Status updated to '{request.Status}'",
                    CreatedDate = now
                };

                await _unitOfWork.AssetRequestLogs.Add(log);

                // Determine icon, colors, and type based on status
                string icon = "inventory_2";
                string iconBg = "#e0f2fe";
                string iconColor = "#0284c7";
                string notifType = "update";

                var sLower = (request.Status ?? "").ToLower();
                if (sLower.Contains("reject"))
                {
                    icon = "cancel";
                    iconBg = "#fef2f2";
                    iconColor = "#dc2626";
                    notifType = "alert";
                }
                else if (sLower.Contains("complet") || sLower.Contains("deliver") || sLower.Contains("approv"))
                {
                    icon = "check_circle";
                    iconBg = "#f0fdf4";
                    iconColor = "#16a34a";
                    notifType = "approval";
                }
                else if (sLower.Contains("transit") || sLower.Contains("dispatch") || sLower.Contains("pickup"))
                {
                    icon = "local_shipping";
                    iconBg = "#eff6ff";
                    iconColor = "#2563eb";
                    notifType = "status";
                }
                else if (sLower.Contains("repair") || sLower.Contains("inspect"))
                {
                    icon = "build";
                    iconBg = "#fef3c7";
                    iconColor = "#d97706";
                    notifType = "status";
                }
                else if (sLower.Contains("close"))
                {
                    icon = "task_alt";
                    iconBg = "#f3f4f6";
                    iconColor = "#4b5563";
                    notifType = "info";
                }

                // Compose detailed message for Employee
                string empNotifMsg = $"Your {request.RequestType} ticket for {asset?.AssetsMasterName ?? "asset"} status changed to '{request.Status}'.";
                if (!string.IsNullOrWhiteSpace(request.TrackingNumber) && !string.IsNullOrWhiteSpace(request.CourierPartner))
                {
                    empNotifMsg += $" Tracking: {request.TrackingNumber} ({request.CourierPartner}).";
                }
                if (!string.IsNullOrWhiteSpace(model.AdminRemarks))
                {
                    empNotifMsg += $" Remarks: {model.AdminRemarks}.";
                }
                else if (!string.IsNullOrWhiteSpace(model.Remarks))
                {
                    empNotifMsg += $" Remarks: {model.Remarks}.";
                }

                // 1. Notification for Ticket Owner (Employee)
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = request.EmployeeId,
                    Title = $"Asset Ticket #{request.Id}: {request.Status}",
                    Message = empNotifMsg,
                    Category = "Asset",
                    Type = notifType,
                    Icon = icon,
                    IconBg = iconBg,
                    IconColor = iconColor,
                    Route = "/index/assets",
                    IsRead = false,
                    CreatedDate = now,
                    UpdatedDate = now,
                    IsActive = true,
                    IsDeleted = false
                });

                // 2. Notification for Approvers (Admin / HR / Manager)
                string approverMsg = $"Ticket #{request.Id} for {asset?.AssetsMasterName ?? "asset"} ({empFullName}) status updated to '{request.Status}' by {model.ActionByName ?? "Admin"}.";
                if (!string.IsNullOrWhiteSpace(model.Remarks))
                {
                    approverMsg += $" Note: {model.Remarks}";
                }

                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = 0,
                    Title = $"Asset Ticket #{request.Id} Updated",
                    Message = approverMsg,
                    Category = "Asset",
                    Type = notifType,
                    Icon = icon,
                    IconBg = iconBg,
                    IconColor = iconColor,
                    Route = "/index/request",
                    QueryParams = "tab=asset",
                    IsRead = false,
                    CreatedDate = now,
                    UpdatedDate = now,
                    IsActive = true,
                    IsDeleted = false
                });

                var saveResult = _unitOfWork.Save();

                return saveResult > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset request status for ID {Id}", model?.RequestId);
                return false;
            }
        }

        public async Task<List<AssetRequestResponseModel>> GetAssetRequests(long? employeeId = null)
        {
            try
            {
                var allRequests = (await _unitOfWork.AssetRequests.GetAll())
                    .Where(r => !r.IsDeleted)
                    .ToList();

                if (employeeId.HasValue && employeeId.Value > 0)
                {
                    allRequests = allRequests.Where(r => r.EmployeeId == employeeId.Value).ToList();
                }

                var allAssets = (await _unitOfWork.AssetsMasters.GetAll()).ToDictionary(a => a.Id, a => a);
                var allEmployees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id, e => e);
                var allLogs = (await _unitOfWork.AssetRequestLogs.GetAll())
                    .GroupBy(l => l.AssetRequestId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(l => l.CreatedDate).ToList());

                var result = allRequests.OrderByDescending(r => r.CreatedDate).Select(r =>
                {
                    allAssets.TryGetValue(r.AssetId, out var asset);
                    allEmployees.TryGetValue(r.EmployeeId, out var emp);
                    allLogs.TryGetValue(r.Id, out var logs);

                    return MapToResponseModel(r, asset, emp, logs ?? new List<AssetRequestLog>());
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset requests.");
                return new List<AssetRequestResponseModel>();
            }
        }

        public async Task<AssetRequestResponseModel?> GetAssetRequestById(long id)
        {
            try
            {
                var request = (await _unitOfWork.AssetRequests.GetAll())
                    .FirstOrDefault(r => r.Id == id && !r.IsDeleted);

                if (request == null)
                    return null;

                var asset = (await _unitOfWork.AssetsMasters.GetAll()).FirstOrDefault(a => a.Id == request.AssetId);
                var employee = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(e => e.Id == request.EmployeeId);
                var logs = (await _unitOfWork.AssetRequestLogs.GetAll())
                    .Where(l => l.AssetRequestId == id)
                    .OrderBy(l => l.CreatedDate)
                    .ToList();

                return MapToResponseModel(request, asset, employee, logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset request ID {Id}", id);
                return null;
            }
        }

        public async Task<List<string>> UploadAssetImages(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return new List<string>();

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "AssetUploads");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var uploadedUrls = new List<string>();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                        continue;

                    var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadDir, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    uploadedUrls.Add($"/AssetUploads/{uniqueFileName}");
                }

                return uploadedUrls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading asset images.");
                return new List<string>();
            }
        }

        private static AssetRequestResponseModel MapToResponseModel(
            AssetRequest request,
            AssetsMaster? asset,
            Employee? employee,
            List<AssetRequestLog> logs)
        {
            return new AssetRequestResponseModel
            {
                Id = request.Id,
                AssetId = request.AssetId,
                AssetName = asset?.AssetsMasterName,
                SerialNumber = asset?.SerialNumber,
                AssetType = asset?.AssetType,
                AssetStatus = asset?.Status,
                EmployeeId = request.EmployeeId,
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : null,
                EmployeeEmail = employee?.EmailAddress,
                RequestType = request.RequestType,
                Priority = request.Priority,
                Reason = request.Reason,
                Description = request.Description,
                ImageUrls = request.ImageUrls,
                Status = request.Status,
                CourierPartner = request.CourierPartner,
                TrackingNumber = request.TrackingNumber,
                DispatchedDate = request.DispatchedDate,
                DeliveredDate = request.DeliveredDate,
                ReceivedDate = request.ReceivedDate,
                AdminRemarks = request.AdminRemarks,
                InspectionRemarks = request.InspectionRemarks,
                CreatedDate = request.CreatedDate,
                UpdatedDate = request.UpdatedDate,
                Logs = logs.Select(l => new AssetRequestLogDto
                {
                    Id = l.Id,
                    FromStatus = l.FromStatus,
                    ToStatus = l.ToStatus,
                    ActionByEmployeeId = l.ActionByEmployeeId,
                    ActionByName = l.ActionByName,
                    Remarks = l.Remarks,
                    CreatedDate = l.CreatedDate
                }).ToList()
            };
        }
    }
}
