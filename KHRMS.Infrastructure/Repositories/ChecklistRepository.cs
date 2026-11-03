using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class ChecklistTemplateRepository : GenericRepository<ChecklistTemplate>, IChecklistTemplateRepository
    {
        public ChecklistTemplateRepository(KHRMSContextClass context) : base(context) { }
    }

    public class ChecklistTemplateTaskRepository : GenericRepository<ChecklistTemplateTask>, IChecklistTemplateTaskRepository
    {
        public ChecklistTemplateTaskRepository(KHRMSContextClass context) : base(context) { }
    }

    public class EmployeeChecklistRepository : GenericRepository<EmployeeChecklist>, IEmployeeChecklistRepository
    {
        public EmployeeChecklistRepository(KHRMSContextClass context) : base(context) { }
    }

    public class ChecklistTaskRepository : GenericRepository<ChecklistTask>, IChecklistTaskRepository
    {
        public ChecklistTaskRepository(KHRMSContextClass context) : base(context) { }
    }

    public class SystemSettingRepository : GenericRepository<SystemSetting>, ISystemSettingRepository
    {
        public SystemSettingRepository(KHRMSContextClass context) : base(context) { }
    }
}
