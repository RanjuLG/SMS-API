using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMS.Business;
using SMS.Generic;
using SMS.Interfaces;
using SMS.Models.DTO;

namespace SMS.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;
        private readonly IItemService _itemService;
        private readonly ITransactionService _transactionService;
        private readonly ITransactionItemService _transactionItemService;
        private readonly IInstallmentService _installmentService;
        private readonly BusinessLogic _businessLogic;
        public ReportController(IInvoiceService invoiceService,
           ICustomerService customerService, IMapper mapper,
           IItemService itemService,
           ITransactionService transactionService,
           ITransactionItemService transactionItemService,
           IInstallmentService installmentService,
           BusinessLogic businessLogic)
        {
            _invoiceService = invoiceService;
            _mapper = mapper;
            _itemService = itemService;
            _transactionService = transactionService;
            _transactionItemService = transactionItemService;
            _installmentService = installmentService;
            _businessLogic = businessLogic;
            _customerService = customerService;
        }
        [HttpGet]
        [Route("customer/{customerNIC}")]
        public ActionResult<ReportDTO> GetReports(string customerNIC)
        {
            try
            {
                var customer = _customerService.GetCustomerByNIC(customerNIC);

                if(customer != null)
                {
                    var report = _businessLogic.ProcessSingleReport(customer.CustomerId);

                    if (report != null)
                    {
                        return Ok(report);
                    }
                    else
                    {
                        return NotFound();
                    }

                }
                else
                {
                    return BadRequest();
                }
               

               

            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
