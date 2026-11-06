using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class CompanyPolicyRepository : GenericRepository<CompanyPolicy>, ICompanyPolicyRepository
    {
        public CompanyPolicyRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class PolicyAcknowledgementRepository : GenericRepository<PolicyAcknowledgement>, IPolicyAcknowledgementRepository
    {
        public PolicyAcknowledgementRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class HelpdeskTicketRepository : GenericRepository<HelpdeskTicket>, IHelpdeskTicketRepository
    {
        public HelpdeskTicketRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class HelpdeskTicketActivityRepository : GenericRepository<HelpdeskTicketActivity>, IHelpdeskTicketActivityRepository
    {
        public HelpdeskTicketActivityRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
