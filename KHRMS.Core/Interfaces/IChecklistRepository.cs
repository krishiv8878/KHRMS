using KHRMS.Core.Models;

namespace KHRMS.Core.Interfaces
{
    public interface IChecklistTemplateRepository : IGenericRepository<ChecklistTemplate> { }
    public interface IChecklistTemplateTaskRepository : IGenericRepository<ChecklistTemplateTask> { }
    public interface IEmployeeChecklistRepository : IGenericRepository<EmployeeChecklist> { }
    public interface IChecklistTaskRepository : IGenericRepository<ChecklistTask> { }
    public interface ISystemSettingRepository : IGenericRepository<SystemSetting> { }
}
