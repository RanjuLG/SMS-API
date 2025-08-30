using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Business;
using SMS.Generic;
using SMS.Interfaces;
using SMS.Models.DTO;

namespace SMS.Controllers
{
    [Route("api/reports")]
    [ApiController]
    [Authorize]
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
        public ActionResult<CustomerReportDTO> GetCustomerReport(string customerNIC)
        {
            try
            {
                var customer = _customerService.GetCustomerByNIC(customerNIC);
                if (customer == null)
                {
                    return NotFound("Customer not found");
                }

                // Use existing business logic for backward compatibility
                var legacyReport = _businessLogic.ProcessSingleReport(customer.CustomerId);
                
                // Convert to new format
                var report = new CustomerReportDTO
                {
                    Customer = new GetCustomerDTO
                    {
                        CustomerId = customer.CustomerId,
                        CustomerNIC = customer.CustomerNIC,
                        CustomerName = customer.CustomerName,
                        CustomerAddress = customer.CustomerAddress,
                        CustomerContactNo = customer.CustomerContactNo,
                        NICPhotoPath = customer.NICPhotoPath,
                        CreatedAt = customer.CreatedAt
                    },
                    // TODO: Implement actual data mapping from legacyReport
                    Transactions = new List<GetTransactionDTO>(),
                    Invoices = new List<GetInvoiceDTO>(),
                    Items = new List<GetItemDTO>()
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet]
        [Route("overview")]
        public ActionResult<OverviewReportDTO> GetOverviewReport()
        {
            try
            {
                var legacyOverview = _businessLogic.ProcessOverview();
                
                // Convert to new format
                var overview = new OverviewReportDTO
                {
                    TotalCustomers = legacyOverview?.CustomerCount ?? 0,
                    TotalItems = legacyOverview?.InventoryCount ?? 0,
                    TotalTransactions = 0, // TODO: Get from actual data
                    TotalInvoices = legacyOverview?.TotalInvoices ?? 0,
                    TotalTransactionAmount = legacyOverview?.RevenueGenerated ?? 0,
                    TotalOutstandingAmount = 0, // TODO: Calculate
                    ActiveLoans = legacyOverview?.TotalActiveLoans ?? 0,
                    SettledLoans = 0, // TODO: Calculate
                    MonthlyTransactions = new List<MonthlyTransactionSummary>(),
                    TransactionsByType = new List<TransactionTypeSummary>(),
                    TopCustomers = new List<TopCustomerSummary>()
                };

                return Ok(overview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // Legacy endpoints for backward compatibility
        [HttpGet]
        [Route("legacy/customer/{customerNIC}")]
        public ActionResult<ReportDTO> GetReportsLegacy(string customerNIC)
        {
            try
            {
                var customer = _customerService.GetCustomerByNIC(customerNIC);

                if (customer != null)
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
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("legacy/overview")]
        public ActionResult<Overview> GetOverviewLegacy()
        {
            try
            {
                var report = _businessLogic.ProcessOverview();

                if (report != null)
                {
                    return Ok(report);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
