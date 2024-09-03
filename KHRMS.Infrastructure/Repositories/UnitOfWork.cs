﻿using KHRMS.Core;
using KHRMS.Core.Interfaces;

namespace KHRMS.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly KHRMSContextClass _dbContext;
        public ICandidateRepository Candidates { get; }

        public ISkillRepository Skills { get; }
        public IDesignationRepository Designations { get; }

        public IHolidayRepository Holidays { get; }

        public IEmployeeRepository Employees { get; }
        public UnitOfWork(KHRMSContextClass dbContext,
                            ICandidateRepository candidateRepository,ISkillRepository skillRepository,IDesignationRepository designationRepository, IHolidayRepository holidayRepository)
        {
            _dbContext = dbContext;
            Candidates = candidateRepository;
            Skills = skillRepository;
            Designations = designationRepository;
            Employees = employeesRepository;
            Holidays = holidayRepository;
        }

        public int Save()
        {
            return _dbContext.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
        }

    }
}
