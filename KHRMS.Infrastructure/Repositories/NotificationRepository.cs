using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Infrastructure.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Notification>> GetByEmployeeId(long employeeId)
        {
            return await _dbContext.Notifications
                .Where(n => !n.IsDeleted && (n.EmployeeId == employeeId || n.EmployeeId == 0))
                .OrderByDescending(n => n.Id)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCount(long employeeId)
        {
            return await _dbContext.Notifications
                .CountAsync(n => !n.IsDeleted && !n.IsRead && (n.EmployeeId == employeeId || n.EmployeeId == 0));
        }

        public async Task<bool> MarkAsRead(long notificationId, long employeeId)
        {
            var notif = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notif != null)
            {
                notif.IsRead = true;
                notif.ReadDate = DateTime.UtcNow;
                notif.UpdatedDate = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        public async Task<bool> MarkAllAsRead(long employeeId)
        {
            var notifs = await _dbContext.Notifications
                .Where(n => !n.IsDeleted && !n.IsRead && (n.EmployeeId == employeeId || n.EmployeeId == 0))
                .ToListAsync();

            foreach (var n in notifs)
            {
                n.IsRead = true;
                n.ReadDate = DateTime.UtcNow;
                n.UpdatedDate = DateTime.UtcNow;
            }
            return true;
        }

        public async Task<bool> DeleteNotification(long notificationId, long employeeId)
        {
            var notif = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsDeleted);
            if (notif != null)
            {
                notif.IsDeleted = true;
                notif.UpdatedDate = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        public async Task<bool> ClearAllNotifications(long employeeId)
        {
            var notifs = await _dbContext.Notifications
                .Where(n => !n.IsDeleted && (n.EmployeeId == employeeId || n.EmployeeId == 0))
                .ToListAsync();

            foreach (var n in notifs)
            {
                n.IsDeleted = true;
                n.UpdatedDate = DateTime.UtcNow;
            }
            return true;
        }
    }
}
