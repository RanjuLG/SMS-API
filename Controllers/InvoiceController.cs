using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMS.Generic;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using SMS.Models.DTO.SMS.Models.DTO;

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

        public InvoiceController(IInvoiceService invoiceService,ICustomerService customerService, IMapper mapper,IItemService itemService, ITransactionService transactionService,ITransactionItemService transactionItemService)
        {
            _invoiceService = invoiceService;
            _mapper = mapper;
            _itemService = itemService;
            _transactionService = transactionService;
            _transactionItemService = transactionItemService;
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
        [Route("")]
        public ActionResult<CreateInvoiceDTO> CreateInvoice([FromBody] CreateInvoiceDTO request)
        {
            try
            {

                // Check if the customer exists
                var existingCustomer = _customerService.GetCustomerByNIC(request.Customer.CustomerNIC);

                Customer customer;
                if (existingCustomer != null)
                {
                    // Use the existing customer
                    customer = existingCustomer;
                }
                else
                {
                    // Create a new customer
                    var customerDto = new CreateCustomerDTO
                    {
                        CustomerName = request.Customer.CustomerName,
                        CustomerNIC = request.Customer.CustomerNIC,
                        CustomerAddress = request.Customer.CustomerAddress,
                        CustomerContactNo = request.Customer.CustomerContactNo,
                    };

                    customer = _mapper.Map<Customer>(customerDto);
                    _customerService.CreateCustomer(customer);
                }

                var Customer = _customerService.GetCustomerByNIC(request.Customer.CustomerNIC);

                if(request.InvoiceTypeId == InvoiceType.InitialPawnInvoice)
                {
                    // Create a new transaction
                    var transaction = new Transaction
                    {
                        CustomerId = customer.CustomerId,
                        SubTotal = request.SubTotal,
                        InterestRate = request.Interest,
                        TotalAmount = request.TotalAmount,
                        TransactionType = TransactionType.LoanIssuance,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                       LoanPeriodId = request.LoanPeriodId,
                    };

                    _transactionService.CreateTransaction(transaction);

                    // Create a new item
                    foreach (var item in request.Items)
                    {
                        if (item.itemId == null || item.itemId == 0)
                        {

                            var itemDto = new Item
                            {
                                ItemDescription = item.ItemDescription,
                                ItemCaratage = item.ItemCaratage,
                                ItemGoldWeight = item.ItemGoldWeight,
                                ItemValue = item.ItemValue,
                                Status = 0,
                                CustomerId = Customer.CustomerId,
                            };

                            var newItem = _mapper.Map<Item>(itemDto);
                            _itemService.CreateItem(newItem);

                            var transactionItem = new TransactionItem
                            {
                                TransactionId = transaction.TransactionId,
                                ItemId = newItem.ItemId
                            };

                            _transactionItemService.CreateTransactionItem(transactionItem);
                        }
                        else
                        {
                            var transactionItem = new TransactionItem
                            {
                                TransactionId = transaction.TransactionId,
                                ItemId = item.itemId
                            };

                            _transactionItemService.CreateTransactionItem(transactionItem);


                        }


                    }

                    // Create a new invoice
                    var invoice = new Invoice
                    {
                        InvoiceNo = _invoiceService.GenerateInvoiceNumber(),
                        InvoiceTypeId = InvoiceType.InitialPawnInvoice,
                        TransactionId = transaction.TransactionId,
                        DateGenerated = DateTime.Now,
                        Status = 1,
                        //CreatedBy = request.CreatedBy,
                        //UpdatedBy = request.UpdatedBy,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now

                    };

                    _invoiceService.CreateInvoice(invoice);

                    var createdInvoice = _invoiceService.GetLastInvoice();

                    return Ok(createdInvoice.InvoiceId);
                }
                else if (request.InvoiceTypeId == InvoiceType.InstallmentPaymentInvoice) 
                {
                    // Create a new transaction
                    var transaction = new Transaction
                    {
                        CustomerId = customer.CustomerId,
                        SubTotal = request.SubTotal,
                        InterestRate = request.Interest,
                        TotalAmount = request.TotalAmount,
                        TransactionType = TransactionType.InstallmentPayment,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _transactionService.CreateTransaction(transaction);


                    // Create a new invoice
                    var invoice = new Invoice
                    {
                        InvoiceNo = _invoiceService.GenerateInvoiceNumber(),
                        InvoiceTypeId = InvoiceType.InstallmentPaymentInvoice,
                        TransactionId = transaction.TransactionId,
                        DateGenerated = DateTime.Now,
                        Status = 1,
                        //CreatedBy = request.CreatedBy,
                        //UpdatedBy = request.UpdatedBy,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now

                    };

                    _invoiceService.CreateInvoice(invoice);

                    var createdInvoice = _invoiceService.GetLastInvoice();

                    return Ok(createdInvoice.InvoiceId);
                }

                else 
                {
                    // Create a new transaction
                    var transaction = new Transaction
                    {
                        CustomerId = customer.CustomerId,
                        SubTotal = request.SubTotal,
                        InterestRate = request.Interest,
                        TotalAmount = request.TotalAmount,
                        TransactionType = TransactionType.LoanClosure,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _transactionService.CreateTransaction(transaction);


                    // Create a new invoice
                    var invoice = new Invoice
                    {
                        InvoiceNo = _invoiceService.GenerateInvoiceNumber(),
                        InvoiceTypeId = InvoiceType.SettlementInvoice,
                        TransactionId = transaction.TransactionId,
                        DateGenerated = DateTime.Now,
                        Status = 1,
                        //CreatedBy = request.CreatedBy,
                        //UpdatedBy = request.UpdatedBy,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now

                    };

                    _invoiceService.CreateInvoice(invoice);

                    var createdInvoice = _invoiceService.GetLastInvoice();

                    return Ok(createdInvoice.InvoiceId);

                }

              
            }
            catch (Exception ex)
            {
                // Log the exception
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

                var invoices = _invoiceService.GetInvoicesByCustomerId(customer.CustomerId);
                var invoiceDTOs = invoices.Select(invoice => new GetInvoiceDTO
                {
                    InvoiceId = invoice.InvoiceId,
                    InvoiceTypeId = invoice.InvoiceTypeId,
                    InvoiceNo = invoice.InvoiceNo,
                    TransactionId = invoice.TransactionId,
                    CustomerNIC = customerNIC,
                    TotalAmount = invoice.Transaction != null ? invoice.Transaction.TotalAmount : null,
                    DateGenerated = invoice.DateGenerated,
                    Status = invoice.Status
                }).ToList();

                return Ok(invoiceDTOs);
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
    }
}
