using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class SalaryComponentRepository : GenericRepository<SalaryComponent>, ISalaryComponentRepository
    {
        public SalaryComponentRepository(KHRMSContextClass dbContext) : base(dbContext) { }
    }

    public class SalaryStructureRepository : GenericRepository<SalaryStructure>, ISalaryStructureRepository
    {
        public SalaryStructureRepository(KHRMSContextClass dbContext) : base(dbContext) { }
    }

    public class EmployeeSalaryAssignmentRepository : GenericRepository<EmployeeSalaryAssignment>, IEmployeeSalaryAssignmentRepository
    {
        public EmployeeSalaryAssignmentRepository(KHRMSContextClass dbContext) : base(dbContext) { }
    }

    public class PayRunRepository : GenericRepository<PayRun>, IPayRunRepository
    {
        public PayRunRepository(KHRMSContextClass dbContext) : base(dbContext) { }
    }

    public class PayRunEmployeeDetailRepository : GenericRepository<PayRunEmployeeDetail>, IPayRunEmployeeDetailRepository
    {
        public PayRunEmployeeDetailRepository(KHRMSContextClass dbContext) : base(dbContext) { }
    }

    public class PayslipRepository : GenericRepository<Payslip>, IPayslipRepository
    {
        public PayslipRepository(KHRMSContextClass dbContext) : base(dbContext) { }
    }

    public class ReimbursementClaimRepository : GenericRepository<ReimbursementClaim>, IReimbursementClaimRepository
    {
        public ReimbursementClaimRepository(KHRMSContextClass dbContext) : base(dbContext) { }
    }

    public class FullAndFinalSettlementRepository : GenericRepository<FullAndFinalSettlement>, IFullAndFinalSettlementRepository
    {
        public FullAndFinalSettlementRepository(KHRMSContextClass dbContext) : base(dbContext) { }
    }
}
