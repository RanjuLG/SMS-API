using AutoMapper;
using Castle.Core.Resource;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMS.Business;
using SMS.Generic;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using SMS.Models.DTO.SMS.Models.DTO;
using SMS.Services;

namespace SMS.Controllers
{
    [Route("api/invoices")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;
        private readonly IItemService _itemService;
        private readonly ITransactionService _transactionService;
        private readonly ITransactionItemService _transactionItemService;
        private readonly IInstallmentService _installmentService;
        private readonly BusinessLogic _businessLogic;

        public InvoiceController(IInvoiceService invoiceService,
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
        [Route("")]
        public ActionResult<IEnumerable<GetInvoiceDTO>> GetInvoices([FromQuery] DateTimeRange dataParams)
        {
            try
            {
                var invoices = _invoiceService.GetInvoices(dataParams);

                var invoiceDTOs = _mapper.Map<IEnumerable<GetInvoiceDTO>>(invoices);
                return Ok(invoiceDTOs);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("{invoiceId}")]
        public ActionResult<GetInvoiceDTO> GetInvoiceById(int invoiceId)
        {
            try
            {
                var invoice = _invoiceService.GetInvoiceById(invoiceId);
                if (invoice == null)
                {
                    return NotFound();
                }

                //var invoiceDTO = _mapper.Map<InvoiceDTO>(invoice);
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost]
        [Route("{initialInvoiceNumber}/{installmentNumber}")]
        public ActionResult<CreateInvoiceDTO> CreateInvoice([FromBody] CreateInvoiceDTO request, string initialInvoiceNumber, int installmentNumber)
        {
            try
            {
                int? statusCode = 0;
                // Process the invoice using the business logic method
                var result = _businessLogic.ProcessInvoice(request, initialInvoiceNumber, installmentNumber);
                if (result is OkObjectResult okResult)
                {
                   statusCode = okResult.StatusCode; // Access StatusCode
                }
                else if (result is BadRequestObjectResult badRequestResult)
                {
                    statusCode = badRequestResult.StatusCode;
                }
                if (statusCode != 200)
                {
                    // If something went wrong and the invoice was not created
                    return BadRequest("Failed to create invoice.");
                }

                // Fetch the created invoice details to return (uncomment if needed)
                // var createdInvoice = _invoiceService.GetInvoiceById(createdInvoiceId);

                // If the invoice creation was successful, return the created invoice details (or Ok if not fetching details)
                // return Ok(createdInvoice); // Uncomment if fetching and returning details
                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception (assuming a logger is available)
                //_logger.LogError(ex, "Error occurred while creating the invoice"); // Uncomment if you have a logger

                // Return an internal server error status code with a generic message
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Route("{invoiceId}")]
        public ActionResult UpdateInvoice(int invoiceId, [FromBody] UpdateInvoiceDTO request)
        {
            return Ok();
        }

        [HttpDelete]
        [Route("{invoiceId}")]
        public ActionResult DeleteInvoice(int invoiceId)
        {
            try
            {
                var existingInvoice = _invoiceService.GetInvoiceById(invoiceId);
                if (existingInvoice == null)
                {
                    return NotFound();
                }

                _invoiceService.DeleteInvoice(invoiceId);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete-multiple")]
        public IActionResult DeleteMultipleInvoices([FromBody] List<int> invoiceIds)
        {
            if (invoiceIds == null || invoiceIds.Count == 0)
            {
                return BadRequest("No invoice IDs provided.");
            }

            _invoiceService.DeleteInvoices(invoiceIds);

            return Ok();
        }


        [HttpGet("customer/{customerNIC}")]
        public ActionResult<IEnumerable<GetInvoiceDTO>> GetInvoicesByCustomerNIC(string customerNIC)
        {
            try
            {
                var customer = _customerService.GetCustomerByNIC(customerNIC);
                if (customer == null)
                {
                    return NotFound("Customer not found.");
                }

                // var invoices = _invoiceService.GetInvoicesByCustomerId(customer.CustomerId);

                var invoices = _businessLogic.GetInvoicesByCustomer(customer);


                return Ok(invoices);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("invoiceNo/{invoiceNo}")]
        public ActionResult<IEnumerable<GetInvoiceDTO>> GetInvoiceByInvoiceNo(string invoiceNo)
        {
            try
            {
                var invoice = _invoiceService.GetInvoiceByInvoiceNo(invoiceNo);
                if (invoice == null)
                {
                    return NotFound("Invoice not found.");
                }

               
                     var invoiceDTO = _mapper.Map<IEnumerable<GetInvoiceDTO>>(invoice);

                return Ok(invoiceDTO);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("InitialInvoice/{invoiceNumber}")]
        public ActionResult<IEnumerable<LoanInfo>> GetInfoByInvoiceNumber(string invoiceNumber)
        {
            try
            {
                var info = _businessLogic.ProcessInstallments(invoiceNumber);

                return Ok(info);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }



    }
}
