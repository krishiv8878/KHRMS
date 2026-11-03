using KHRMS.Core;

namespace KHRMS.Services
{
    public class LeaveTypeService(IUnitOfWork unitOfWork, ISendEmailService emailRepository) : ILeaveTypeService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public ISendEmailService _sendEmailService = emailRepository;



        public async Task<bool> AddLeaveType(LeaveType leaveType)

        {

            if (leaveType != null)

            {

                leaveType.CreatedDate = DateTime.Now;

                await _unitOfWork.LeaveType.Add(leaveType);

                var result = _unitOfWork.Save();

                if (result > 0)

                    return true;

                else

                    return false;

            }

            return false;

        }

       

        public async Task<bool> DeleteLeaveType(long LeaveTypeId)
        {
            if (LeaveTypeId > 0)
            {
                var leaveTypeDetail = await _unitOfWork.LeaveType.GetById(LeaveTypeId);
                if (leaveTypeDetail != null)
                {
                    leaveTypeDetail.IsDeleted = true;
                    leaveTypeDetail.IsActive = false;

                    _unitOfWork.LeaveType.Update(leaveTypeDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                       
                }
            }
            return false;
        }

        public async Task<IEnumerable<LeaveType>> GetAllLeaveType()
        {
            var leaveTypeDetail = await _unitOfWork.LeaveType.GetAll();
            return leaveTypeDetail;
        }

        public async Task<LeaveType> GetLeaveTypeById(int LeaveTypeId)
        {
            if (LeaveTypeId > 0)
            {
                var leaveTypeDetail = await _unitOfWork.LeaveType.GetById(LeaveTypeId);
                if (leaveTypeDetail != null)
                {
                    return leaveTypeDetail;
                }
            }
            return null;
        }

        public async Task<bool> UpdateLeaveType(LeaveType leaveType)
        {
            if (leaveType != null)
            {
                var leaveTypeDetail = await _unitOfWork.LeaveType.GetById(leaveType.Id);
                if (leaveTypeDetail != null)
                {
                   
                    bool allowedDaysChanged = leaveTypeDetail.AllowedDays != leaveType.AllowedDays;
                    leaveTypeDetail.Description = leaveType.Description;
                    leaveTypeDetail.Type = leaveType.Type;
                    leaveTypeDetail.AllowedDays = leaveType.AllowedDays;
                    leaveTypeDetail.UpdatedDate = DateTime.Now;
                    leaveTypeDetail.IsActive = leaveType.IsActive;

                    // Phase 5: Complete Policy Configuration Fields
                    leaveTypeDetail.IsPaid = leaveType.IsPaid;
                    leaveTypeDetail.SandwichRuleEnabled = leaveType.SandwichRuleEnabled;
                    leaveTypeDetail.MaxSandwichLeavesAllowed = leaveType.MaxSandwichLeavesAllowed;
                    leaveTypeDetail.IsNegativeBalanceAllowed = leaveType.IsNegativeBalanceAllowed;
                    leaveTypeDetail.NegativeBalanceLimit = leaveType.NegativeBalanceLimit;
                    leaveTypeDetail.AccrualEnabled = leaveType.AccrualEnabled;
                    leaveTypeDetail.AccrualFrequency = leaveType.AccrualFrequency ?? "Monthly";
                    leaveTypeDetail.AccrualAmount = leaveType.AccrualAmount;
                    leaveTypeDetail.CarryForwardEnabled = leaveType.CarryForwardEnabled;
                    leaveTypeDetail.MaxCarryForwardDays = leaveType.MaxCarryForwardDays;
                    leaveTypeDetail.ExcessBalanceTreatment = leaveType.ExcessBalanceTreatment ?? "Lapse";
                    leaveTypeDetail.EncashmentAllowed = leaveType.EncashmentAllowed;
                    leaveTypeDetail.MinBalanceForEncashment = leaveType.MinBalanceForEncashment;
                    leaveTypeDetail.MaxEncashmentDaysPerRequest = leaveType.MaxEncashmentDaysPerRequest;
                    leaveTypeDetail.EncashmentSalaryFormula = leaveType.EncashmentSalaryFormula ?? "Basic/30";
                    leaveTypeDetail.IsCompOff = leaveType.IsCompOff;
                    leaveTypeDetail.CompOffValidityDays = leaveType.CompOffValidityDays;
                    leaveTypeDetail.NoticeDaysRequired = leaveType.NoticeDaysRequired;
                    leaveTypeDetail.MaxConsecutiveDays = leaveType.MaxConsecutiveDays;
                    leaveTypeDetail.ApplicableGender = leaveType.ApplicableGender ?? "All";
                    leaveTypeDetail.ApplicableEmploymentType = leaveType.ApplicableEmploymentType ?? "All";

                    _unitOfWork.LeaveType.Update(leaveTypeDetail);

                    // If AllowedDays was modified on a fixed/baseline quota, synchronize opening balances for current year
                    if (allowedDaysChanged && !leaveTypeDetail.AccrualEnabled && leaveTypeDetail.AllowedDays >= 0)
                    {
                        int curYear = DateTime.UtcNow.Year;
                        var openingLedgers = (await _unitOfWork.LeaveLedgers.GetAll())
                            .Where(l => l.LeaveTypeId == leaveTypeDetail.Id && l.TransactionType == "OpeningBalance" && l.EffectiveDate.Year == curYear && l.IsDeleted != true)
                            .ToList();

                        foreach (var ol in openingLedgers)
                        {
                            decimal diff = leaveTypeDetail.AllowedDays - ol.Days;
                            ol.Days = leaveTypeDetail.AllowedDays;
                            ol.BalanceAfter += diff;
                            _unitOfWork.LeaveLedgers.Update(ol);
                        }

                        var baselineEntitlements = (await _unitOfWork.LeaveEntitlements.GetAll())
                            .Where(e => e.LeaveTypeId == leaveTypeDetail.Id && e.SourceType == "OpeningBalance" && e.CreditDate.Year == curYear && e.IsDeleted != true)
                            .ToList();

                        foreach (var be in baselineEntitlements)
                        {
                            be.TotalDays = leaveTypeDetail.AllowedDays;
                            be.RemainingDays = Math.Max(0, leaveTypeDetail.AllowedDays - be.ConsumedDays);
                            _unitOfWork.LeaveEntitlements.Update(be);
                        }
                    }

                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }
    }
}
