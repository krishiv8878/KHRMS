using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;

        public NotificationService(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        private async Task<bool> IsApproverUser(long employeeId)
        {
            if (_userContextService.IsAdmin() || _userContextService.IsHR() || _userContextService.IsManager())
            {
                return true;
            }

            if (employeeId > 0)
            {
                try
                {
                    var adminRoles = new[] { "Admin", "System Admin", "HR", "HR Operations", "Manager", "Management" };
                    var empRoles = await _unitOfWork.EmployeeRoleMappings.GetAll();
                    var roles = await _unitOfWork.RoleMaster.GetAll();
                    var userRoles = from erm in empRoles
                                    join r in roles on erm.RoleId equals r.Id
                                    where erm.EmployeeId == employeeId && erm.IsActive == true && erm.IsDeleted != true
                                    select r.RoleName;
                    if (userRoles.Any(r => adminRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
                    {
                        return true;
                    }
                }
                catch
                {
                    // Fall back to false if lookup fails
                }
            }

            return false;
        }

        public async Task<IEnumerable<Notification>> GetNotifications(long employeeId)
        {
            bool isApprover = await IsApproverUser(employeeId);
            var all = await _unitOfWork.Notifications.GetByEmployeeId(employeeId);
            if (!isApprover)
            {
                return all.Where(n => n.EmployeeId == employeeId).OrderByDescending(n => n.Id);
            }
            return all.OrderByDescending(n => n.Id);
        }

        public async Task<int> GetUnreadCount(long employeeId)
        {
            bool isApprover = await IsApproverUser(employeeId);
            if (!isApprover)
            {
                var userNotifs = await _unitOfWork.Notifications.GetByEmployeeId(employeeId);
                return userNotifs.Count(n => !n.IsDeleted && !n.IsRead && n.EmployeeId == employeeId);
            }
            return await _unitOfWork.Notifications.GetUnreadCount(employeeId);
        }

        public async Task<bool> MarkAsRead(long notificationId, long employeeId)
        {
            var res = await _unitOfWork.Notifications.MarkAsRead(notificationId, employeeId);
            if (res)
            {
                _unitOfWork.Save();
            }
            return res;
        }

        public async Task<bool> MarkAllAsRead(long employeeId)
        {
            bool isApprover = await IsApproverUser(employeeId);
            var res = await _unitOfWork.Notifications.MarkAllAsRead(employeeId, isApprover);
            if (res)
            {
                _unitOfWork.Save();
            }
            return res;
        }

        public async Task<bool> DeleteNotification(long notificationId, long employeeId)
        {
            var res = await _unitOfWork.Notifications.DeleteNotification(notificationId, employeeId);
            if (res)
            {
                _unitOfWork.Save();
            }
            return res;
        }

        public async Task<bool> ClearAllNotifications(long employeeId)
        {
            bool isApprover = await IsApproverUser(employeeId);
            var res = await _unitOfWork.Notifications.ClearAllNotifications(employeeId, isApprover);
            if (res)
            {
                _unitOfWork.Save();
            }
            return res;
        }

        public async Task<Notification> CreateNotification(Notification notification)
        {
            notification.CreatedDate = DateTime.UtcNow;
            notification.UpdatedDate = DateTime.UtcNow;
            notification.IsActive = true;
            notification.IsDeleted = false;
            notification.IsRead = false;

            await _unitOfWork.Notifications.Add(notification);
            _unitOfWork.Save();
            return notification;
        }

        public async Task SyncInitialNotifications(long employeeId)
        {
            // Historical backfill is disabled; notifications are strictly event-driven.
            await Task.CompletedTask;
        }
    }
}
