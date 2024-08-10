using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMS.Generic;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using System;
using System.Collections.Generic;

namespace SMS.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public TransactionController(ITransactionService transactionService, IInvoiceService invoiceService, ICustomerService customerService, IMapper mapper)
        {
            _transactionService = transactionService;
            _invoiceService = invoiceService;
            _customerService = customerService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("")]
        public ActionResult<IEnumerable<TransactionDTO>> GetTransactions([FromQuery] DateTimeRange dataParams)
        {
            try
            {
                var transactions = _transactionService.GetAllTransactions(dataParams);
                var transactionDTOs = _mapper.Map<IEnumerable<TransactionDTO>>(transactions);
                return Ok(transactionDTOs);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("byIds")]
        public ActionResult<List<TransactionDTO>> GetTransactionById([FromBody] List<int> transactionIds)
        {
            try
            {
                var transactions = _transactionService.GetTransactionsByIds(transactionIds);
                if (transactions == null || transactions.Count == 0)
                {
                    return NotFound();
                }

                var transactionDTOs = _mapper.Map<List<TransactionDTO>>(transactions);
                return Ok(transactionDTOs);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("")]
        public ActionResult CreateTransaction([FromBody] CreateTransactionDTO request)
        {
            try
            {
                var customer = _customerService.GetCustomerByNIC(request.CustomerNIC);
                if (customer == null)
                {
                    return NotFound("Customer not found.");
                }

                var transaction = new Transaction
                {
                    CustomerId = customer.CustomerId,
                    CreatedAt = DateTime.Now,
                    SubTotal = request.SubTotal,
                    InterestRate = request.InterestRate,
                    TotalAmount = request.TotalAmount,
                    TransactionItems = request.Items.Select(i => new TransactionItem { Item = _mapper.Map<Item>(i) }).ToList()
                };

                _transactionService.CreateTransaction(transaction);

                var invoice = new Invoice
                {
                    TransactionId = transaction.TransactionId,
                    DateGenerated = DateTime.Now,
                    Status = 1,
                    CreatedBy = transaction.CreatedBy,
                    UpdatedBy = transaction.UpdatedBy,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _invoiceService.CreateInvoice(invoice);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Route("{transactionId}")]
        public ActionResult UpdateTransaction(int transactionId, [FromBody] UpdateTransactionDTO request)
        {
            try
            {
                var existingTransaction = _transactionService.GetTransactionById(transactionId);
                if (existingTransaction == null)
                {
                    return NotFound();
                }

                _mapper.Map(request, existingTransaction);

                _transactionService.UpdateTransaction(existingTransaction);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete-multiple")]
        public IActionResult DeleteMultipleTransactions([FromBody] List<int> transactionIds)
        {
            if (transactionIds == null || transactionIds.Count == 0)
            {
                return BadRequest("No transaction IDs provided.");
            }

            _transactionService.DeleteTransactions(transactionIds);

            return Ok();
        }

        [HttpGet("customer/{customerNIC}")]
        public ActionResult<IEnumerable<TransactionDTO>> GetTransactionsByCustomerNIC(string customerNIC)
        {
            try
            {
                var customer = _customerService.GetCustomerByNIC(customerNIC);
                if (customer == null)
                {
                    return NotFound("Customer not found.");
                }

                var transactions = _transactionService.GetTransactionsByCustomerId(customer.CustomerId);
                var transactionDTOs = _mapper.Map<IEnumerable<TransactionDTO>>(transactions);

                return Ok(transactionDTOs);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
