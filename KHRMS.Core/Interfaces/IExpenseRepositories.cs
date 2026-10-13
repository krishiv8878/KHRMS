using KHRMS.Core.Models;

namespace KHRMS.Core.Interfaces
{
    public interface IExpenseCategoryRepository : IGenericRepository<ExpenseCategory>
    {
    }

    public interface IExpenseReportRepository : IGenericRepository<ExpenseReport>
    {
    }

    public interface IExpenseItemRepository : IGenericRepository<ExpenseItem>
    {
    }

    public interface IExpenseMileageRepository : IGenericRepository<ExpenseMileage>
    {
    }

    public interface IExpenseAdvanceRepository : IGenericRepository<ExpenseAdvance>
    {
    }
}
