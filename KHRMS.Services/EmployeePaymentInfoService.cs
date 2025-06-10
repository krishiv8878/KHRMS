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

        public async Task AddAsync(EmployeePaymentInfo entity)
        {
            var empid = _userContext.GetCurrentEmployeeId();
            entity.EmployeeId = empid;
            await _unitOfWork.EmployeePaymentInfo.Add(entity);
            _unitOfWork.Save();
        }

        public Task UpdateAsync(EmployeePaymentInfo entity)
        {
            _unitOfWork.EmployeePaymentInfo.Update(entity);
            _unitOfWork.Save();
            return Task.CompletedTask;
        }

        public Task DeleteAsync(long id)
        {
            unitOfWork.EmployeePaymentInfo.DeleteAsync(id);
           _unitOfWork.Save();
            return Task.CompletedTask;

        }

    }

}