using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMS.Interfaces;
using SMS.Models.DTO.SMS.Models.DTO;
using SMS.Models.DTO;
using SMS.Models;
using SMS.Migrations;

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

                        transaction = CreateTransaction(customer.CustomerId, request.SubTotal, request.Interest, request.TotalAmount, TransactionType.LoanIssuance, request.LoanPeriodId);
                        loan = CreateLoan(transaction.TransactionId, request.Date, request.LoanPeriodId);
                        ProcessItems(request.Items, transaction.TransactionId, customer.CustomerId);
                        invoice = CreateInvoice(transaction.TransactionId, InvoiceType.InitialPawnInvoice);
                        break;

                    case InvoiceType.InstallmentPaymentInvoice:
                        transaction = CreateTransaction(customer.CustomerId, request.SubTotal, request.Interest, request.TotalAmount, TransactionType.InstallmentPayment);
                        installment = CreateInstallment(initialInvoiceNumber, transaction.TransactionId, installmentNumber);
                        invoice = CreateInvoice(transaction.TransactionId, InvoiceType.InstallmentPaymentInvoice);
                        break;

                    case InvoiceType.SettlementInvoice:
                        transaction = CreateTransaction(customer.CustomerId, request.SubTotal, request.Interest, request.TotalAmount, TransactionType.LoanClosure);
                        invoice = CreateInvoice(transaction.TransactionId, InvoiceType.SettlementInvoice);
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

        private Transaction CreateTransaction(int customerId, decimal subTotal, decimal interestRate, decimal totalAmount, TransactionType transactionType, int? loanPeriodId = null)
        {
            var transaction = new Transaction
            {
                CustomerId = customerId,
                SubTotal = subTotal,
                InterestRate = interestRate,
                TotalAmount = totalAmount,
                TransactionType = transactionType,
                LoanPeriodId = loanPeriodId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _transactionService.CreateTransaction(transaction);
            return transaction;
        }

        private void ProcessItems(CustomItemDTO[] items, int transactionId, int customerId)
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
                        Status = 0,
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
                initialLoan = _loanService.GetAllLoans().Where(x => x.TransactionId == intialInvoice.TransactionId).ToList().FirstOrDefault();
               
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

        private Loan CreateLoan (int TransactionID,DateTime StartDate,int? loanPeriodId )
        {
            var loanPeriod = _karatageService.GetLoanPeriodById(loanPeriodId.Value);

            if(loanPeriod != null)
            {
                var loan = new Loan
                {
                    TransactionId = TransactionID,
                    StartDate = StartDate,
                    EndDate = StartDate.AddMonths(loanPeriod.Period)

                };

                _loanService.CreateLoan(loan);

                return loan;
            }
            else
            {
                return null;
            }
           


        }

       
    }
}
