using KHRMS.Core.Models;

namespace KHRMS.Core.Interfaces
{
    public interface ICompanyPolicyRepository : IGenericRepository<CompanyPolicy>
    {
    }

    public interface IPolicyAcknowledgementRepository : IGenericRepository<PolicyAcknowledgement>
    {
    }

    public interface IHelpdeskTicketRepository : IGenericRepository<HelpdeskTicket>
    {
    }
}
