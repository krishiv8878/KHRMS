using KHRMS.Core.Models;

namespace KHRMS.Core.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByEmployeeId(long employeeId);
        Task<int> GetUnreadCount(long employeeId);
        Task<bool> MarkAsRead(long notificationId, long employeeId);
        Task<bool> MarkAllAsRead(long employeeId);
        Task<bool> DeleteNotification(long notificationId, long employeeId);
        Task<bool> ClearAllNotifications(long employeeId);
    }
}
