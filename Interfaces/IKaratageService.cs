using System.Collections.Generic;
using SMS.Models;

namespace SMS.Interfaces
{
    public interface IKaratageService
    {
        // Karat CRUD operations
        void CreateKarat(Karat karat);
        IList<Karat> GetAllKarats();
        Karat GetKaratById(int karatId);
        void UpdateKarat(Karat karat);
        void DeleteKarat(int karatId);

        // LoanPeriod CRUD operations
        void CreateLoanPeriod(LoanPeriod loanPeriod);
        IList<LoanPeriod> GetAllLoanPeriods();
        LoanPeriod GetLoanPeriodById(int loanPeriodId);
        void UpdateLoanPeriod(LoanPeriod loanPeriod);
        void DeleteLoanPeriod(int loanPeriodId);

        // Pricing CRUD operations
        void CreatePricing(Pricing pricing);
        IList<Pricing> GetAllPricings();
        Pricing GetPricingById(int pricingId);
        void UpdatePricing(Pricing pricing);
        void DeletePricing(int pricingId);

        // Custom operation to get Pricings by KaratId and LoanPeriodId
        IList<Pricing> GetPricingsByKaratAndLoanPeriod(int karatId, int loanPeriodId);
    }
}
