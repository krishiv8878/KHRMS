using KHRMS.Core;
using KHRMS.Services.Interfaces;

namespace KHRMS.Services
{

    public class EmployeePaymentInfoService(IUnitOfWork unitOfWork,IUserContextService userContextService) : IEmployeePaymentInfoService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public IUserContextService _userContext = userContextService;
        

        public async Task<IEnumerable<EmployeePaymentInfo>> GetAllAsync()
            => await _unitOfWork.EmployeePaymentInfo.GetAll();

        public async Task<EmployeePaymentInfo?> GetByIdAsync(long id)
            => await _unitOfWork.EmployeePaymentInfo.GetById(id);

        public async Task<EmployeePaymentInfo?> GetByEmployeeIdAsync(long employeeId)
        {
            var list = await _unitOfWork.EmployeePaymentInfo.GetAll();
            return list.FirstOrDefault(p => p.EmployeeId == employeeId);
        }

        public async Task AddAsync(EmployeePaymentInfo entity)
        {
            if (entity == null) return;
            if (entity.EmployeeId <= 0)
            {
                entity.EmployeeId = _userContext.GetCurrentEmployeeId();
            }
            entity.Id = 0;
            entity.CreatedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.EmployeePaymentInfo.Add(entity);
            _unitOfWork.Save();
        }

        public async Task UpdateAsync(EmployeePaymentInfo entity)
        {
            if (entity == null) return;

            EmployeePaymentInfo? existing = null;
            if (entity.Id > 0)
            {
                existing = await _unitOfWork.EmployeePaymentInfo.GetById(entity.Id);
            }

            if (existing == null && entity.EmployeeId > 0)
            {
                var list = await _unitOfWork.EmployeePaymentInfo.GetAll();
                existing = list.FirstOrDefault(p => p.EmployeeId == entity.EmployeeId);
            }

            if (existing != null)
            {
                existing.BankName = entity.BankName;
                existing.IFSCCode = entity.IFSCCode;
                existing.AccountNumber = entity.AccountNumber;
                existing.NameOnAccount = entity.NameOnAccount;
                existing.IsActive = entity.IsActive;
                existing.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.EmployeePaymentInfo.Update(existing);
            }
            else
            {
                if (entity.EmployeeId <= 0)
                {
                    entity.EmployeeId = _userContext.GetCurrentEmployeeId();
                }
                entity.Id = 0;
                entity.CreatedDate = DateTime.UtcNow;
                entity.UpdatedDate = DateTime.UtcNow;
                await _unitOfWork.EmployeePaymentInfo.Add(entity);
            }
            _unitOfWork.Save();
        }

        public Task DeleteAsync(long id)
        {
            unitOfWork.EmployeePaymentInfo.DeleteAsync(id);
           _unitOfWork.Save();
            return Task.CompletedTask;

        }

    }

}