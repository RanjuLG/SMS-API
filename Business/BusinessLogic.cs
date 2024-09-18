using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMS.Interfaces;
using SMS.Models.DTO.SMS.Models.DTO;
using SMS.Models.DTO;
using SMS.Models;
using SMS.Migrations;
using SMS.Generic;
using Azure.Core;
using SMS.Enums;

namespace SMS.Business
{
    public class BusinessLogic
    {
        private readonly ICustomerService _customerService;
        private readonly ITransactionService _transactionService;
        private readonly IItemService _itemService;
        private readonly ITransactionItemService _transactionItemService;
        private readonly IInvoiceService _invoiceService;
        private readonly ILoanService _loanService;
        private readonly IKaratageService _karatageService;
        private readonly IInstallmentService _installmentService;
        private readonly IMapper _mapper;

        public object StartDate { get; private set; }

        public BusinessLogic(
            ICustomerService customerService,
            ITransactionService transactionService,
            IItemService itemService,
            ITransactionItemService transactionItemService,
            IInvoiceService invoiceService,
            ILoanService loanService,
            IKaratageService karatageService,
            IInstallmentService installmentService,
            IMapper mapper)
        {
            _customerService = customerService;
            _transactionService = transactionService;
            _itemService = itemService;
            _transactionItemService = transactionItemService;
            _invoiceService = invoiceService;
            _loanService = loanService;
            _karatageService = karatageService;
            _installmentService = installmentService;
            _mapper = mapper;
        }

        public IActionResult ProcessInvoice(CreateInvoiceDTO request,string initialInvoiceNumber, int installmentNumber)
        {
            try
            {
                // Step 1: Handle Customer
                var customer = GetOrCreateCustomer(request.Customer);

                // Step 2: Process Transaction and Invoice Based on Invoice Type
                Transaction transaction;
                Invoice invoice;
                Loan loan;
                Installment installment;
           

                switch (request.InvoiceTypeId)
                {
                    case InvoiceType.InitialPawnInvoice:

                        transaction = CreateTransaction(customer.CustomerId,request, TransactionType.LoanIssuance);
                        loan = CreateLoan(transaction.TransactionId, request);
                        ProcessInitialItems(request.Items, transaction.TransactionId, customer.CustomerId);
                        invoice = CreateInvoice(transaction.TransactionId, InvoiceType.InitialPawnInvoice);
                        break;

                    case InvoiceType.InstallmentPaymentInvoice:
                        transaction = CreateTransaction(customer.CustomerId, request, TransactionType.InstallmentPayment);
                        installment = CreateInstallment(initialInvoiceNumber, transaction.TransactionId, installmentNumber);
                        loan = UpdateInitialLoan(initialInvoiceNumber, transaction.TotalAmount);
                        invoice = CreateInvoice(transaction.TransactionId, InvoiceType.InstallmentPaymentInvoice);
                        break;

                    case InvoiceType.SettlementInvoice:
                        transaction = CreateTransaction(customer.CustomerId,request, TransactionType.LoanClosure);
                        var isLoanSettled = SettleLoan(initialInvoiceNumber);

                        if (isLoanSettled)
                        {
                            ProcesSettlementItems(initialInvoiceNumber);
                            invoice = CreateInvoice(transaction.TransactionId, InvoiceType.SettlementInvoice);
                        }

                        break;

                    default:
                        return new BadRequestObjectResult("Invalid Invoice Type");
                }

                // Step 3: Return the Created Invoice ID
                var createdInvoice = _invoiceService.GetLastInvoice();
                return new OkObjectResult(createdInvoice.InvoiceId);
            }
            catch (Exception ex)
            {
                // Log the exception here if needed
                return new StatusCodeResult(500);
            }
        }

        private Customer GetOrCreateCustomer(CreateCustomerDTO customerDto)
        {
            var existingCustomer = _customerService.GetCustomerByNIC(customerDto.CustomerNIC);

            if (existingCustomer != null)
            {
                return existingCustomer;
            }
            else
            {
                var customer = _mapper.Map<Customer>(customerDto);
                _customerService.CreateCustomer(customer);
                return _customerService.GetCustomerByNIC(customerDto.CustomerNIC); // Re-fetch to get the newly created customer's details
            }
        }

        private Transaction CreateTransaction(int customerId, CreateInvoiceDTO request, TransactionType transactionType)
        {
            var transaction = new Transaction
            {
                CustomerId = customerId,
                SubTotal = request.SubTotal,
                InterestRate = request.InterestRate,
                InterestAmount = request.InterestAmount,
                TotalAmount = request.TotalAmount,
                TransactionType = transactionType,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _transactionService.CreateTransaction(transaction);
            return transaction;
        }

        private void ProcessInitialItems(CustomItemDTO[] items, int transactionId, int customerId)
        {
            foreach (var item in items)
            {
                if (item.itemId == 0) // Assuming itemId of 0 means new item
                {
                    var newItem = new Item
                    {
                        ItemDescription = item.ItemDescription,
                        ItemCaratage = item.ItemCaratage,
                        ItemGoldWeight = item.ItemGoldWeight,
                        ItemValue = item.ItemValue,
                        Status = (int)ItemStatus.InStock,
                        CustomerId = customerId,
                    };

                    _itemService.CreateItem(newItem);

                    var transactionItem = new TransactionItem
                    {
                        TransactionId = transactionId,
                        ItemId = newItem.ItemId
                    };

                    _transactionItemService.CreateTransactionItem(transactionItem);
                }
                else
                {
                    var transactionItem = new TransactionItem
                    {
                        TransactionId = transactionId,
                        ItemId = item.itemId
                    };

                    _transactionItemService.CreateTransactionItem(transactionItem);
                }
            }
        }
        private void ProcesSettlementItems(string initialInvoiceNumber)
        {
            var initialInvoice = _invoiceService.GetInvoiceByInvoiceNo(initialInvoiceNumber).FirstOrDefault();

            if (initialInvoice != null)
            {
                var settlementItems = initialInvoice.Transaction.TransactionItems
                    .SelectMany(i => new[] { i.Item });

                foreach (var item in settlementItems)
                {
                    item.Status = (int)ItemStatus.Redeemed;
                   
                    _itemService.UpdateItem(item);
                }
            }
        }

        private Invoice CreateInvoice(int transactionId, InvoiceType invoiceType)
        {
            var invoice = new Invoice
            {
                InvoiceNo = _invoiceService.GenerateInvoiceNumber(),
                InvoiceTypeId = invoiceType,
                TransactionId = transactionId,
                DateGenerated = DateTime.Now,
                Status = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _invoiceService.CreateInvoice(invoice);
            return invoice;
        }
        private Installment CreateInstallment(string InitialInvoiceNumber, int transactionId, int installmentNumber)
        {
            //var initialLoan = null;
            int loanId = 0;
            var intialInvoice = _invoiceService.GetInvoiceByInvoiceNo(InitialInvoiceNumber).FirstOrDefault();
            var transaction = _transactionService.GetTransactionById(transactionId);
            var initialLoan = new Loan();

            if(intialInvoice != null)
            {
                IDateTimeRange dateTimeRange = new DateTimeRange
                {
                    From = DateTime.MinValue, // Set appropriate start date
                    To = DateTime.MinValue // Set appropriate end date
                };
                initialLoan = _loanService.GetAllLoans(dateTimeRange).Where(x => x.TransactionId == intialInvoice.TransactionId).ToList().FirstOrDefault();
               
            }

            if (intialInvoice != null && initialLoan != null)
            {

                var installment = new Installment
                {
                    TransactionId = transactionId,
                    LoanId = initialLoan.LoanId,
                    InstallmentNumber = installmentNumber,
                    AmountPaid = transaction.TotalAmount,
                    DueDate = initialLoan.StartDate.AddMonths(installmentNumber),
                    PaymentDate = DateTime.Now,
                };

                _installmentService.CreateInstallment(installment);

                return installment;
            };

            return null;
        }

        private Loan CreateLoan (int TransactionID,CreateInvoiceDTO request)
        {
            var loanPeriod = _karatageService.GetLoanPeriodById(request.LoanPeriodId.Value);

            if(loanPeriod != null)
            {
                var loan = new Loan
                {   LoanPeriodId = request.LoanPeriodId,
                    TransactionId = TransactionID,
                    StartDate = request.Date,
                    OutstandingAmount = request.TotalAmount,
                    EndDate = request.Date.AddMonths(loanPeriod.Period)

                };

                _loanService.CreateLoan(loan);

                return loan;
            }
            else
            {
                return null;
            }
           


        }

        public LoanInfo ProcessInstallments(string initialInvoiceNumber)
        {
            // Retrieve and sort installments, ensure the list is non-null
            var installments = _installmentService.GetInstallmentsByInitialInvoiceNumber(initialInvoiceNumber)
                                                  ?.OrderBy(x => x.InstallmentNumber)
                                                  ?.ToList();
           

            // Get initial invoice details, ensure it's non-null
            var initialInvoice = _invoiceService.GetInvoiceByInvoiceNo(initialInvoiceNumber)?.FirstOrDefault();
            if (initialInvoice == null)
            {
                return null;
            }

            // Get initial loan details, ensure it's non-null
            var initialLoan = _loanService.GetLoanByInitialInvoiceNumber(initialInvoiceNumber);
            if (initialLoan == null)
            {
                return null;
            }

            // Get transaction details, ensure it's non-null
            var initialTransaction = _transactionService.GetTransactionById(initialInvoice.TransactionId);
            if (initialTransaction == null)
            {
                return null;
            }

            // Ensure the loan period and period value are non-null before accessing them
            if (initialLoan.LoanPeriod == null || initialLoan.LoanPeriod.Period <= 0)
            {
                return null;
            }

            // Create a data object to return
            var loanInfo = new LoanInfo
            {
                PrincipleAmount = initialTransaction.SubTotal,
                InterestRate = initialTransaction.InterestRate,
                InterestAmount = initialTransaction.SubTotal * initialTransaction.InterestRate / 100,
                TotalAmount = initialTransaction.TotalAmount,
                LoanPeriod = initialLoan.LoanPeriod.Period,
                NumberOfInstallmentsPaid = installments != null ? installments.Count : 0,
                IsLoanSettled = installments.Count >= initialLoan.LoanPeriod.Period // Logic for loan settlement
            };

            // Return the loan information object
            return loanInfo;
        }


        public ReportDTO ProcessSingleReport(int customerId)
        {
            try
            {
                // Fetch customer details
                var customer = _customerService.GetCustomerById(customerId);
                if (customer == null)
                {
                    return null;
                }

                // Fetch all loans for the customer
                var loans = _loanService.GetLoansByCustomerId(customerId);

                // Calculate totals
                var totalLoanedAmount = loans.Sum(l => l.Transaction.TotalAmount);
                var totalAmountPaid = loans.SelectMany(l => l.Installments)
                                           .Where(i => i.PaymentDate.HasValue)
                                           .Sum(i => i.AmountPaid);
                var totalOutstandingAmount = totalLoanedAmount - totalAmountPaid;

                // Prepare loans and installment data
                var loanDtos = loans.Select(l => new LoanDTO
                {
                    LoanId = l.LoanId,
                    InvoiceNo = l.Transaction.Invoice.InvoiceNo,
                    TransactionId = l.TransactionId,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    AmountPaid = l.AmountPaid,
                    IsSettled = l. IsSettled,
                    OutstandingAmount = l.OutstandingAmount,
                    
                    Transaction = new TransactionDTO
                    {
                        TransactionId = l.Transaction.TransactionId,
                        CreatedAt = l.Transaction.CreatedAt,
                        SubTotal = l.Transaction.SubTotal,
                        InterestRate = l.Transaction.InterestRate,
                        InterestAmount = l.Transaction.InterestAmount,
                        TotalAmount = l.Transaction.TotalAmount,
                        Customer = new GetCustomerDTO
                        {
                            CustomerId = customer.CustomerId,
                            CustomerNIC = customer.CustomerNIC,
                            CustomerName = customer.CustomerName
                        },
                        Items = l.Transaction.TransactionItems.Select(i => new GetItemDTO
                        {
                            ItemId = i.Item.ItemId,
                            ItemDescription = i.Item.ItemDescription,
                            ItemCaratage = i.Item.ItemCaratage,
                            ItemGoldWeight = i.Item.ItemGoldWeight,
                            ItemValue = i.Item.ItemValue,
                            Status = i.Item.Status, // Assuming `Item` has a Status property
                            CreatedAt = i.Item.CreatedAt,  // Optional: If you want the created date of the transaction item
                            CustomerNIC = l.Transaction.Customer.CustomerNIC // Assuming CustomerNIC is in the Customer object
                        }).ToList()

            },
                    Installments = l.Installments.Select(i => new InstallmentDTO
                    {
                        InstallmentId = i.InstallmentId,
                        InvoiceNo = i.Transaction.Invoice.InvoiceNo,
                        LoanId = l.LoanId,
                        PrincipleAmountPaid = i.Transaction.SubTotal,
                        InterestAmountPaid = i.Transaction.InterestAmount,
                        TotalAmountPaid = i.Transaction.TotalAmount,
                        DatePaid = i.PaymentDate ?? default(DateTime)
                    }).ToList()
                }).ToList();

                // Create the report DTO
                var report = new ReportDTO
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    CustomerNIC = customer.CustomerNIC,
                    Loans = loanDtos.OrderByDescending(l=> l.StartDate).ToList() // Include the list of loans
                };

                return report;
            }
            catch (Exception ex)
            {
                return null;
            }
           
        }
        /*
        public IList<ReportDTO> ProcessTimelyReports(IDateTimeRange dateTimeRange)
        {
            try
            {
               DateTime from = dateTimeRange.From;
               DateTime to = dateTimeRange.To;

                if (from == DateTime.MinValue || to == DateTime.MinValue
)
                {
                    return null;
                }

                // Fetch all loans for the customer
                

                // Calculate totals
                var totalLoanedAmount = loans.Sum(l => l.Transaction.TotalAmount);
                var totalAmountPaid = loans.SelectMany(l => l.Installments)
                                           .Where(i => i.PaymentDate.HasValue)
                                           .Sum(i => i.AmountPaid);
                var totalOutstandingAmount = totalLoanedAmount - totalAmountPaid;

                // Prepare loans and installment data
                var loanDtos = loans.Select(l => new LoanDTO
                {
                    LoanId = l.LoanId,
                    TransactionId = l.TransactionId,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    AmountPaid = l.AmountPaid,
                    IsSettled = l.IsSettled,
                    OutstandingAmount = l.OutstandingAmount,

                    Transaction = new TransactionDTO
                    {
                        TransactionId = l.Transaction.TransactionId,
                        CreatedAt = l.Transaction.CreatedAt,
                        SubTotal = l.Transaction.SubTotal,
                        InterestRate = l.Transaction.InterestRate,
                        TotalAmount = l.Transaction.TotalAmount,
                        Customer = new GetCustomerDTO
                        {
                            CustomerId = customer.CustomerId,
                            CustomerNIC = customer.CustomerNIC,
                            CustomerName = customer.CustomerName
                        },
                        Items = l.Transaction.TransactionItems.Select(i => new GetItemDTO
                        {
                            ItemId = i.Item.ItemId,
                            ItemDescription = i.Item.ItemDescription,
                            ItemCaratage = i.Item.ItemCaratage,
                            ItemGoldWeight = i.Item.ItemGoldWeight,
                            ItemValue = i.Item.ItemValue,
                            Status = i.Item.Status, // Assuming `Item` has a Status property
                            CreatedAt = i.Item.CreatedAt,  // Optional: If you want the created date of the transaction item
                            CustomerNIC = l.Transaction.Customer.CustomerNIC // Assuming CustomerNIC is in the Customer object
                        }).ToList()

                    },
                    Installments = l.Installments.Select(i => new InstallmentDTO
                    {
                        InstallmentId = i.InstallmentId,
                        LoanId = l.LoanId,
                        AmountPaid = i.AmountPaid,
                        DatePaid = i.PaymentDate ?? default(DateTime)
                    }).ToList()
                }).ToList();

                // Create the report DTO
                var report = new ReportDTO
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    CustomerNIC = customer.CustomerNIC,
                    Loans = loanDtos // Include the list of loans
                };

                return report;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        */


        public bool SettleLoan(string initialInvoiceNumber)
        {
            // Get the Transaction ID associated with the invoice number
            var transactionId = _invoiceService.GetInvoiceByInvoiceNo(initialInvoiceNumber)
                                                .FirstOrDefault()?.Transaction.TransactionId;

            if (transactionId != null)
            {
                IDateTimeRange dateTimeRange = new DateTimeRange
                {
                    From = DateTime.MinValue, // Set appropriate start date
                    To = DateTime.MinValue // Set appropriate end date
                };
                // Find the loan associated with the transaction ID
                var loan = _loanService.GetAllLoans(dateTimeRange)
                                       .Where(i => i.TransactionId == transactionId)
                                       .FirstOrDefault();

                if (loan != null)
                {
                    // Set the loan as settled
                    loan.IsSettled = true;

                    // Call update method in _loanService to save changes
                    _loanService.UpdateLoan(loan);

                    return true;
                }
            }
            return false;
        }


        public Loan UpdateInitialLoan(string initialInvoiceNumber,decimal installmentPaid)
        {
            // Get the Transaction ID associated with the invoice number
            var transactionId = _invoiceService.GetInvoiceByInvoiceNo(initialInvoiceNumber)
                                                .FirstOrDefault()?.Transaction.TransactionId;

            if (transactionId != null)
            {
                IDateTimeRange dateTimeRange = new DateTimeRange
                {
                    From = DateTime.MinValue, // Set appropriate start date
                    To = DateTime.MinValue // Set appropriate end date
                };
                // Find the loan associated with the transaction ID
                var loan = _loanService.GetAllLoans(dateTimeRange)
                                       .Where(i => i.TransactionId == transactionId)
                                       .FirstOrDefault();
                var installments = _installmentService.GetInstallmentsByInitialInvoiceNumber(initialInvoiceNumber);

                if (loan != null)
                {
                   loan.AmountPaid = installments.Sum(i => i.AmountPaid);   
                    loan.OutstandingAmount = loan.Transaction.TotalAmount - loan.AmountPaid;

                    // Call update method in _loanService to save changes
                    _loanService.UpdateLoan(loan);

                    return loan;
                }
            }
            return null;
        }

        public IEnumerable<GetInvoiceDTO> GetInvoicesByCustomer(Customer customer)
        {
            var invoices = _invoiceService.GetInvoicesByCustomerId(customer.CustomerId);

            var initialLoan = _loanService.GetLoansByCustomerId(customer.CustomerId);

            var invoiceDTOs = invoices.Select(invoice => new GetInvoiceDTO
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceTypeId = invoice.InvoiceTypeId,
                InvoiceNo = invoice.InvoiceNo,
                TransactionId = invoice.TransactionId,
                CustomerNIC = customer.CustomerNIC,
                PrincipleAmount = invoice.Transaction.SubTotal,
                InterestRate = invoice.Transaction.InterestRate,
                InterestAmount = invoice.Transaction.InterestAmount,
                TotalAmount = invoice.Transaction.TotalAmount,
                DateGenerated = invoice.DateGenerated,
                Status = invoice.Status,
                LoanPeriod = invoice.InvoiceTypeId==InvoiceType.InitialPawnInvoice ?  initialLoan.Where(t => t.TransactionId == invoice.TransactionId).FirstOrDefault()?.LoanPeriod.Period: null,

            });

            return invoiceDTOs;
        } 


    }
}
