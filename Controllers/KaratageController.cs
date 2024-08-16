using Microsoft.AspNetCore.Mvc;
using SMS.Interfaces;
using SMS.Models;
using System.Collections.Generic;

namespace SMS.Controllers
{
    [Route("api/karatage")]
    [ApiController]
    public class KaratageController : ControllerBase
    {
        private readonly IKaratageService _karatageService;

        public KaratageController(IKaratageService karatageService)
        {
            _karatageService = karatageService;
        }

        // Karatage Operations

        [HttpGet("karats")]
        public ActionResult<IEnumerable<Karat>> GetAllKarats()
        {
            var karats = _karatageService.GetAllKarats();
            return Ok(karats);
        }

        [HttpGet("karats/{karatId}")]
        public ActionResult<Karat> GetKaratById(int karatId)
        {
            var karat = _karatageService.GetKaratById(karatId);
            if (karat == null)
            {
                return NotFound();
            }
            return Ok(karat);
        }

        [HttpPost("karats")]
        public ActionResult CreateKarat([FromBody] Karat karat)
        {
            _karatageService.CreateKarat(karat);
            return Ok();
        }

        [HttpPut("karats/{karatId}")]
        public ActionResult UpdateKarat(int karatId, [FromBody] Karat karat)
        {
            var existingKarat = _karatageService.GetKaratById(karatId);
            if (existingKarat == null)
            {
                return NotFound();
            }

            _karatageService.UpdateKarat(karat);
            return Ok();
        }

        [HttpDelete("karats/{karatId}")]
        public ActionResult DeleteKarat(int karatId)
        {
            var existingKarat = _karatageService.GetKaratById(karatId);
            if (existingKarat == null)
            {
                return NotFound();
            }

            _karatageService.DeleteKarat(karatId);
            return Ok();
        }

        // LoanPeriod Operations

        [HttpGet("loanperiods")]
        public ActionResult<IEnumerable<LoanPeriod>> GetAllLoanPeriods()
        {
            var loanPeriods = _karatageService.GetAllLoanPeriods();
            return Ok(loanPeriods);
        }

        [HttpGet("loanperiods/{loanPeriodId}")]
        public ActionResult<LoanPeriod> GetLoanPeriodById(int loanPeriodId)
        {
            var loanPeriod = _karatageService.GetLoanPeriodById(loanPeriodId);
            if (loanPeriod == null)
            {
                return NotFound();
            }
            return Ok(loanPeriod);
        }

        [HttpPost("loanperiods")]
        public ActionResult CreateLoanPeriod([FromBody] LoanPeriod loanPeriod)
        {
            _karatageService.CreateLoanPeriod(loanPeriod);
            return Ok();
        }

        [HttpPut("loanperiods/{loanPeriodId}")]
        public ActionResult UpdateLoanPeriod(int loanPeriodId, [FromBody] LoanPeriod loanPeriod)
        {
            var existingLoanPeriod = _karatageService.GetLoanPeriodById(loanPeriodId);
            if (existingLoanPeriod == null)
            {
                return NotFound();
            }

            _karatageService.UpdateLoanPeriod(loanPeriod);
            return Ok();
        }

        [HttpDelete("loanperiods/{loanPeriodId}")]
        public ActionResult DeleteLoanPeriod(int loanPeriodId)
        {
            var existingLoanPeriod = _karatageService.GetLoanPeriodById(loanPeriodId);
            if (existingLoanPeriod == null)
            {
                return NotFound();
            }

            _karatageService.DeleteLoanPeriod(loanPeriodId);
            return Ok();
        }

        // Pricing Operations

        [HttpGet("pricings")]
        public ActionResult<IEnumerable<Pricing>> GetAllPricings()
        {
            var pricings = _karatageService.GetAllPricings();
            return Ok(pricings);
        }

        [HttpGet("pricings/{pricingId}")]
        public ActionResult<Pricing> GetPricingById(int pricingId)
        {
            var pricing = _karatageService.GetPricingById(pricingId);
            if (pricing == null)
            {
                return NotFound();
            }
            return Ok(pricing);
        }

        [HttpPost("pricings")]
        public ActionResult CreatePricing([FromBody] Pricing pricing)
        {
            _karatageService.CreatePricing(pricing);
            return Ok();
        }

        [HttpPut("pricings/{pricingId}")]
        public ActionResult UpdatePricing(int pricingId, [FromBody] Pricing pricing)
        {
            var existingPricing = _karatageService.GetPricingById(pricingId);
            if (existingPricing == null)
            {
                return NotFound();
            }

            _karatageService.UpdatePricing(pricing);
            return Ok();
        }

        [HttpDelete("pricings/{pricingId}")]
        public ActionResult DeletePricing(int pricingId)
        {
            var existingPricing = _karatageService.GetPricingById(pricingId);
            if (existingPricing == null)
            {
                return NotFound();
            }

            _karatageService.DeletePricing(pricingId);
            return Ok();
        }

        // Custom Operation: Get Pricings by Karat and LoanPeriod
        [HttpGet("pricings/karat/{karatId}/loanperiod/{loanPeriodId}")]
        public ActionResult<IEnumerable<Pricing>> GetPricingsByKaratAndLoanPeriod(int karatId, int loanPeriodId)
        {
            var pricings = _karatageService.GetPricingsByKaratAndLoanPeriod(karatId, loanPeriodId);
            return Ok(pricings);
        }
    }
}
