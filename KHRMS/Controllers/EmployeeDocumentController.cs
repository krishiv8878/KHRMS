using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS
{
    /// <summary>
    /// API Controller for managing Employee Documents Information.
    /// Provides endpoints to Create, Read, Update, and Delete employee documents records.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeDocumentController : ControllerBase
    {
        private readonly IEmployeeDocumentService _employeeDocumentService;
        private readonly IUserContextService? _userContextService;
        private readonly ILogger<EmployeeDocumentController>? _logger;
        private readonly List<string> _allowedExtensions = new List<string> { ".doc", ".docx", ".xaml" };

        public EmployeeDocumentController(
            IEmployeeDocumentService employeeDocumentService,
            ILogger<EmployeeDocumentController> logger,
            IUserContextService? userContextService = null)
        {
            _employeeDocumentService = employeeDocumentService;
            _logger = logger;
            _userContextService = userContextService;
        }

        [HttpGet("GetAllDocumentsInfo")]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDocumentInfo>>>> GetAll()
        {
            Log.Information("EmployeeDocumentController - GetAllDocumentsInfo called.");

            var documents = await _employeeDocumentService.GetAllAsync();

            if (_userContextService != null)
            {
                var currentUserId = _userContextService.GetCurrentEmployeeId();
                var isHrOrAdmin = _userContextService.IsHR() || _userContextService.IsAdmin();

                // HR / Admins see all documents (Pending, Approved, Rejected).
                // Regular employees see all Approved documents, or documents they uploaded themselves (regardless of status).
                if (!isHrOrAdmin && currentUserId > 0)
                {
                    documents = documents.Where(d =>
                        (!string.IsNullOrWhiteSpace(d.Status) && d.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)) ||
                        d.EmployeeId == currentUserId ||
                        d.UploadedBy == currentUserId);
                }
            }

            Log.Information("EmployeeDocumentController - {Count} documents found.", documents.Count());

            return Ok(new ApiResponse<IEnumerable<EmployeeDocumentInfo>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeDocumentFound,
                Data = documents
            });
        }

        /// <summary>
        /// Retrieves employee document information by ID.
        /// </summary>
        /// <param name="id">Employee Document Info ID</param>


        [HttpGet("GetDocument/{id}")]
        public async Task<IActionResult> GetDocument(long id)
        {
            Log.Information("EmployeeDocumentController - GetDocument called with ID: {Id}", id);

            var document = await _employeeDocumentService.GetByIdAsync(id);
            if (document == null)
            {
                Log.Warning("EmployeeDocumentController - Document not found with ID: {Id}", id);
                return Ok(new ApiResponse<EmployeeDocumentInfo>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeeDocumentNotFound,
                    Data = null
                });
            }

            Log.Information("EmployeeDocumentController - Document retrieved for ID: {Id}", id);
            return Ok(new ApiResponse<EmployeeDocumentInfo>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeDocumentFound,
                Data = document
            });
        }


        /// <summary>
        /// Creates a new employee Document information record.
        /// </summary>
        /// <param Id="employeeId" file="IFormFile" )>Employee Document Info object</param>


        [HttpPost("UploadDocument")]
        public async Task<IActionResult> UploadDocument([FromForm] long employeeId, [FromForm] string category, string documentName, IFormFile file)
        {
            Log.Information("EmployeeDocumentController - UploadDocument called for EmployeeID: {EmployeeId}", employeeId);

            if (file == null || file.Length == 0)
            {
                Log.Warning("EmployeeDocumentController - Invalid file upload attempt.");
                return Ok("File is not provided or empty.");
            }

            if (string.IsNullOrWhiteSpace(documentName))
            {
                Log.Warning("EmployeeDocumentController - Document name is missing.");
                return BadRequest("Document name is required.");
            }

            var extension = Path.GetExtension(file.FileName)?.ToLower();
            if (extension != ".pdf" && extension != ".docx")
            {
                Log.Warning("EmployeeDocumentController - Invalid file extension: {Extension}", extension);
                return BadRequest("Only .pdf and .docx files are allowed.");
            }

            try
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                Directory.CreateDirectory(uploadsDir);

                var filePath = Path.Combine(uploadsDir, Path.GetFileName(file.FileName));

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var currentUserId = _userContextService?.GetCurrentEmployeeId() ?? 0;
                var document = new EmployeeDocumentInfo
                {
                    EmployeeId = employeeId > 0 ? employeeId : currentUserId,
                    FilePath = filePath,
                    DocumentName = documentName,
                    Category = category,
                    UploadedBy = currentUserId > 0 ? currentUserId : employeeId,
                    UploadedDate = DateTime.UtcNow,
                    Status = "Pending"
                };

                await _employeeDocumentService.AddAsync(document);

                Log.Information("EmployeeDocumentController - Document uploaded successfully for EmployeeID: {EmployeeId}", employeeId);

                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, new ApiResponse<EmployeeDocumentInfo>
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Message = ApiMessageConstant.DocumentRequestAdded,
                    Data = document
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "EmployeeDocumentController - Exception occurred while uploading document.");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = "Internal server error",
                    Data = null
                });
            }
        }

        /// <summary>
        /// View an employee payment information record by ID.
        /// </summary>
        /// <param name="id">Employee Document Info ID</param>     

        [HttpGet("View/{id}")]
        public async Task<IActionResult> ViewFile(long id)
        {
            Log.Information("EmployeeDocumentController - ViewFile called with ID: {Id}", id);

            var document = await _employeeDocumentService.GetByIdAsync(id);
            if (document == null)
            {
                Log.Warning("EmployeeDocumentController - Document not found for ID: {Id}", id);
                return Ok("Document not found.");
            }

            var filePath = document.FilePath;
            if (!System.IO.File.Exists(filePath))
            {
                Log.Warning("EmployeeDocumentController - File not found on disk at {Path}", filePath);
                return NotFound("File not found.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var extension = Path.GetExtension(filePath)?.ToLower();
            var contentType = extension == ".pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            Log.Information("EmployeeDocumentController - File successfully retrieved from path: {Path}", filePath);
            return File(fileBytes, contentType, Path.GetFileName(filePath));
        }


        /// <summary>
        /// Deletes an employee payment information record by ID.
        /// </summary>
        /// <param name="id">Employee Document Info ID</param>     

        [HttpDelete("DeleteDocument/{id}")]
        public async Task<IActionResult> DeleteDocument(long id)
        {
            Log.Information("EmployeeDocumentController - DeleteDocument called with ID: {Id}", id);

            var isDeleted = await _employeeDocumentService.DeleteAsync(id);
            if (isDeleted)
            {
                Log.Information("EmployeeDocumentController - Document with ID {Id} deleted successfully.", id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.DocumentRequestDeleted,
                    Data = true
                });
            }

            Log.Error("EmployeeDocumentController - Failed to delete document with ID: {Id}", id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeDocumentNotFound,
                Data = false
            });
        }

        /// <summary>
        /// Approves or rejects an employee document submission.
        /// </summary>
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations")]
        [HttpPost("ApproveOrRejectDocument")]
        public async Task<IActionResult> ApproveOrRejectDocument([FromBody] DocumentApprovalDTO dto)
        {
            Log.Information("EmployeeDocumentController - ApproveOrRejectDocument called for ID: {Id}, Status: {Status}", dto?.Id, dto?.Status);

            if (dto == null || dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Status))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid document approval payload.",
                    Data = false
                });
            }

            var currentUserId = _userContextService?.GetCurrentEmployeeId() ?? 0;
            var success = await _employeeDocumentService.ApproveOrRejectAsync(dto.Id, dto.Status, dto.RejectionReason, currentUserId);

            if (!success)
            {
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Document not found or could not be updated.",
                    Data = false
                });
            }

            var updatedDoc = await _employeeDocumentService.GetByIdAsync(dto.Id);
            var isApproved = dto.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase);

            return Ok(new ApiResponse<EmployeeDocumentInfo>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = isApproved ? ApiMessageConstant.DocumentApproved : ApiMessageConstant.DocumentRejected,
                Data = updatedDoc
            });
        }
    }
}



