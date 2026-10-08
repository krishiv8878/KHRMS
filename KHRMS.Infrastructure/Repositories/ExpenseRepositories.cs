using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class ExpenseCategoryRepository : GenericRepository<ExpenseCategory>, IExpenseCategoryRepository
    {
        public ExpenseCategoryRepository(KHRMSContextClass context) : base(context) { }
    }

    public class ExpenseReportRepository : GenericRepository<ExpenseReport>, IExpenseReportRepository
    {
        public ExpenseReportRepository(KHRMSContextClass context) : base(context) { }
    }

    public class ExpenseItemRepository : GenericRepository<ExpenseItem>, IExpenseItemRepository
    {
        public ExpenseItemRepository(KHRMSContextClass context) : base(context) { }
    }

    public class ExpenseMileageRepository : GenericRepository<ExpenseMileage>, IExpenseMileageRepository
    {
        public ExpenseMileageRepository(KHRMSContextClass context) : base(context) { }
    }

    public class ExpenseAdvanceRepository : GenericRepository<ExpenseAdvance>, IExpenseAdvanceRepository
    {
        public ExpenseAdvanceRepository(KHRMSContextClass context) : base(context) { }
    }
}
