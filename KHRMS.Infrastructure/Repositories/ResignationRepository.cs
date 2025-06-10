using KHRMS.Core;

namespace KHRMS.Infrastructure
{
    public class ResignationRepository : GenericRepository<Resignation>, IResignationRepository
    {
        public ResignationRepository(KHRMSContextClass dbContext) : base(dbContext)
        {

        }
    }
}
