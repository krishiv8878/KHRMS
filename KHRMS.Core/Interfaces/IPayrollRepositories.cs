using KHRMS.Core.Models;

namespace KHRMS.Core.Interfaces
{
    public interface ISalaryComponentRepository : IGenericRepository<SalaryComponent>
    {
    }

    public interface ISalaryStructureRepository : IGenericRepository<SalaryStructure>
    {
    }

    public interface IEmployeeSalaryAssignmentRepository : IGenericRepository<EmployeeSalaryAssignment>
    {
    }

    public interface IPayRunRepository : IGenericRepository<PayRun>
    {
    }

    public interface IPayRunEmployeeDetailRepository : IGenericRepository<PayRunEmployeeDetail>
    {
    }

    public interface IPayslipRepository : IGenericRepository<Payslip>
    {
    }

    public interface IReimbursementClaimRepository : IGenericRepository<ReimbursementClaim>
    {
    }

    public interface IFullAndFinalSettlementRepository : IGenericRepository<FullAndFinalSettlement>
    {
    }
}
