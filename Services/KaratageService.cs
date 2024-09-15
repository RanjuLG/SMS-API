using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SMS.Interfaces;
using SMS.Models;

namespace SMS.Services
{
    public class KaratageService:IKaratageService
    {
        private readonly IRepository _dbContext;

        public KaratageService(IRepository dbContext)
        {
            _dbContext = dbContext;
        }

        // Create a new Karat
        public void CreateKarat(Karat karat)
        {
            _dbContext.Create(karat);
            _dbContext.Save();
        }

        // Get all Karats
        public IList<Karat> GetAllKarats()
        {
            return _dbContext.Get<Karat>().ToList();
        }

        // Get Karat by ID
        public Karat GetKaratById(int karatId)
        {
            return _dbContext.GetById<Karat>(karatId);
        }

        // Update an existing Karat
        public void UpdateKarat(Karat karat)
        {
          //  karat.UpdatedAt = DateTime.Now;
            _dbContext.Update(karat);
            _dbContext.Save();
        }

        // Delete a Karat by ID
        public void DeleteKarat(int karatId)
        {
            var karat = _dbContext.GetById<Karat>(karatId);
            if (karat != null)
            {
                _dbContext.Delete(karat);
                _dbContext.Save();
            }
        }

        // Create a new LoanPeriod
        public void CreateLoanPeriod(LoanPeriod loanPeriod)
        {

            _dbContext.Create(loanPeriod);
            _dbContext.Save();
        }

        // Get all LoanPeriods
        public IList<LoanPeriod> GetAllLoanPeriods()
        {
            return _dbContext.Get<LoanPeriod>().ToList();
        }

        // Get LoanPeriod by ID
        public LoanPeriod GetLoanPeriodById(int loanPeriodId)
        {
            return _dbContext.GetById<LoanPeriod>(loanPeriodId);
        }

        // Update an existing LoanPeriod
        public void UpdateLoanPeriod(LoanPeriod loanPeriod)
        {
           // loanPeriod.UpdatedAt = DateTime.Now;
            _dbContext.Update(loanPeriod);
            _dbContext.Save();
        }

        // Delete a LoanPeriod by ID
        public void DeleteLoanPeriod(int loanPeriodId)
        {
            var loanPeriod = _dbContext.GetById<LoanPeriod>(loanPeriodId);
            if (loanPeriod != null)
            {
                _dbContext.Delete(loanPeriod);
                _dbContext.Save();
            }
        }

        // Create a new Pricing
        public void CreatePricing(Pricing pricing)
        {
            _dbContext.Create(pricing);
            _dbContext.Save();
        }

        // Get all Pricings
        public IList<Pricing> GetAllPricings()
        {
            return _dbContext.Get<Pricing>()
                .Include(p => p.Karat)
                .Include(p => p.LoanPeriod)
                .OrderByDescending(a => a.Karat) // Change to OrderByDescending
                .ToList();
        }

        // Get Pricing by ID
        public Pricing GetPricingById(int pricingId)
        {
            return _dbContext.GetById<Pricing>(pricingId);
        }

        // Update an existing Pricing
        public void UpdatePricing(Pricing pricing)
        {
           // pricing.UpdatedAt = DateTime.Now;
            _dbContext.Update(pricing);
            _dbContext.Save();
        }

        // Delete a Pricing by ID
        public void DeletePricing(int pricingId)
        {
            var pricing = _dbContext.GetById<Pricing>(pricingId);
            if (pricing != null)
            {
                _dbContext.Delete(pricing);
                _dbContext.Save();
            }
        }

        // Get Pricings by KaratId and LoanPeriodId
        public IList<Pricing> GetPricingsByKaratAndLoanPeriod(int karatId, int loanPeriodId)
        {
            return _dbContext.Get<Pricing>(p => p.KaratId == karatId && p.LoanPeriodId == loanPeriodId).ToList();
        }
    }
}
