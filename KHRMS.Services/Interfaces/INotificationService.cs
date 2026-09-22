using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Core.Models;

namespace KHRMS.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetNotifications(long employeeId);
        Task<int> GetUnreadCount(long employeeId);
        Task<bool> MarkAsRead(long notificationId, long employeeId);
        Task<bool> MarkAllAsRead(long employeeId);
        Task<bool> DeleteNotification(long notificationId, long employeeId);
        Task<bool> ClearAllNotifications(long employeeId);
        Task<Notification> CreateNotification(Notification notification);
        Task SyncInitialNotifications(long employeeId);
    }
}
