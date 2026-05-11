using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using SMS.Repositories;

namespace SMS.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IRepository _dbContext;
        private readonly ITransactionService _transactionService;
        private readonly ILoanService _loanService;
        private readonly ITransactionItemService _transactionItemService;
        private readonly IInstallmentService _installmentService;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IRepository dbContext, ITransactionService transactionService, ILoanService loanService,ITransactionItemService transactionItemService,IInstallmentService installmentService, ILogger<InvoiceService> logger)
        {
            _dbContext = dbContext;
            _transactionService = transactionService;
            _loanService = loanService;
            _transactionItemService = transactionItemService;
            _installmentService = installmentService;
            _logger = logger;
        }

        public IList<Invoice> GetAllInvoices()
        {
            var invoices = _dbContext.Get<Invoice>(i => i.DeletedAt == null)
                .Include(i => i.Transaction)
                .Include(i => i.InvoiceTypeId)
                .ToList();

            return invoices;
        }

        public IList<GetInvoiceDTO> GetInvoices(IDateTimeRange dateTimeRange)
        {
            var startTime = dateTimeRange.From;
            var endTime = dateTimeRange.To;

            var invoices = new List<GetInvoiceDTO>();
            var invoices_ = _dbContext.Get<Invoice>(i => i.DeletedAt == null && i.CreatedAt <= endTime && i.CreatedAt >= startTime)
                .Include(i => i.Transaction) // Ensure Transaction is include
                .ToList();

            foreach (var invoice_ in invoices_)
            {
                var transaction = _transactionService.GetTransactionById(invoice_.TransactionId);

                if (transaction == null)
                {
                    continue;
                }

                var loans = _loanService.GetAllLoans(dateTimeRange);

                var invoice = new GetInvoiceDTO
                {
                    InvoiceId = invoice_.InvoiceId,
                    InvoiceTypeId = (int)invoice_.InvoiceTypeId,
                    InvoiceNo = invoice_.InvoiceNo,
                    TransactionId = transaction.TransactionId,
                    CustomerNIC = transaction.Customer?.CustomerNIC,
                    TotalAmount = transaction.TotalAmount,
                    DateGenerated = invoice_.DateGenerated,
                    Status = invoice_.Status,
                   LoanPeriod = (int)invoice_.InvoiceTypeId == (int)InvoiceType.InitialPawnInvoice ? loans.Where(t => t.TransactionId == invoice_.TransactionId).FirstOrDefault()?.LoanPeriod?.Period : null,
                };

                invoices.Add(invoice);
            }

            return invoices;
        }

        public async Task<PaginatedResponse<GetInvoiceDTO>> GetInvoicesPaginatedAsync(InvoiceSearchRequest request)
        {
            var query = _dbContext.Get<Invoice>(i => i.DeletedAt == null)
                .Include(i => i.Transaction)
                    .ThenInclude(t => t.Customer)
                .AsQueryable();

            // Apply date range filter
            if (request.From != default && request.To != default)
            {
                query = query.Where(i => i.CreatedAt >= request.From && i.CreatedAt <= request.To);
            }

            // Apply customer NIC filter
            if (!string.IsNullOrEmpty(request.CustomerNIC))
            {
                query = query.Where(i => i.Transaction != null && 
                    i.Transaction.Customer != null && 
                    i.Transaction.Customer.CustomerNIC.Contains(request.CustomerNIC));
            }

            // Apply status filter
            if (request.Status.HasValue)
            {
                query = query.Where(i => i.Status == request.Status.Value);
            }

            // Apply invoice type filter
            if (request.InvoiceTypeId.HasValue)
            {
                query = query.Where(i => (int)i.InvoiceTypeId == request.InvoiceTypeId.Value);
            }

            // Apply search filter (InvoiceNo, CustomerNIC)
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(i => 
                    i.InvoiceNo.Contains(request.Search) ||
                    (i.Transaction != null && i.Transaction.Customer != null && 
                        i.Transaction.Customer.CustomerNIC.Contains(request.Search)));
            }

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortOrder);

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var invoices = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Map to DTOs
            var invoiceDTOs = new List<GetInvoiceDTO>();
            var dateTimeRange = new Generic.DateTimeRange { From = request.From, To = request.To };
            var loans = _loanService.GetAllLoans(dateTimeRange);

            foreach (var invoice in invoices)
            {
                var transaction = invoice.Transaction ?? _transactionService.GetTransactionById(invoice.TransactionId);

                if (transaction == null)
                {
                    continue;
                }

                var invoiceDTO = new GetInvoiceDTO
                {
                    InvoiceId = invoice.InvoiceId,
                    InvoiceTypeId = (int)invoice.InvoiceTypeId,
                    InvoiceNo = invoice.InvoiceNo,
                    TransactionId = transaction.TransactionId,
                    CustomerNIC = transaction.Customer?.CustomerNIC,
                    CustomerName = transaction.Customer?.CustomerName,
                    TotalAmount = transaction.TotalAmount,
                    DateGenerated = invoice.DateGenerated,
                    Status = invoice.Status,
                    LoanPeriod = (int)invoice.InvoiceTypeId == (int)InvoiceType.InitialPawnInvoice 
                        ? loans.Where(t => t.TransactionId == invoice.TransactionId).FirstOrDefault()?.LoanPeriod?.Period 
                        : null,
                };

                invoiceDTOs.Add(invoiceDTO);
            }

            // Create pagination metadata
            var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);
            var paginationMetadata = new PaginationMetadata
            {
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNextPage = request.Page < totalPages,
                HasPreviousPage = request.Page > 1
            };

            // Create filter metadata
            var filterMetadata = new FilterMetadata
            {
                Search = request.Search,
                SortBy = request.SortBy,
                SortOrder = request.SortOrder,
                AppliedFilters = new 
                { 
                    request.From, 
                    request.To, 
                    request.CustomerNIC, 
                    request.Status, 
                    request.InvoiceTypeId 
                }
            };

            return new PaginatedResponse<GetInvoiceDTO>
            {
                Data = invoiceDTOs,
                Pagination = paginationMetadata,
                Filters = filterMetadata
            };
        }

        private IQueryable<Invoice> ApplySorting(IQueryable<Invoice> query, string? sortBy, string? sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "invoiceno" => isDescending 
                    ? query.OrderByDescending(i => i.InvoiceNo) 
                    : query.OrderBy(i => i.InvoiceNo),
                "dategenerated" => isDescending 
                    ? query.OrderByDescending(i => i.DateGenerated) 
                    : query.OrderBy(i => i.DateGenerated),
                "status" => isDescending 
                    ? query.OrderByDescending(i => i.Status) 
                    : query.OrderBy(i => i.Status),
                "totalamount" => isDescending 
                    ? query.OrderByDescending(i => i.Transaction.TotalAmount) 
                    : query.OrderBy(i => i.Transaction.TotalAmount),
                _ => query.OrderByDescending(i => i.CreatedAt) // Default sort by creation date
            };
        }

        public GetInvoiceDTO GetInvoiceById(int invoiceId)
        {
            var invoice_ = _dbContext.GetById<Invoice>(invoiceId);
            if (invoice_ == null)
            {
                return null;
            }

            var transaction = _transactionService.GetTransactionById(invoice_.TransactionId);
            var invoice = new GetInvoiceDTO
            {
                InvoiceId = invoice_.InvoiceId,
                InvoiceTypeId = (int)invoice_.InvoiceTypeId,
                InvoiceNo = invoice_.InvoiceNo,
                TransactionId = transaction?.TransactionId ?? 0,
                CustomerNIC = transaction?.Customer?.CustomerNIC,
                TotalAmount = transaction?.TotalAmount ?? 0,
                DateGenerated = invoice_.DateGenerated,
                Status = invoice_.Status,
               //LoanPeriod = transaction.LoanPeriod?.Period // Map LoanPeriod
            };

            return invoice;
        }


        public void CreateInvoice(Invoice invoice)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    _dbContext.Create(invoice);
                    _dbContext.Save();
                    dbTransaction.Commit();
                }
                catch (Exception)
                {
                    dbTransaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateInvoice(Invoice invoice)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    invoice.UpdatedAt = DateTime.Now;
                    _dbContext.Update(invoice);
                    _dbContext.Save();
                    dbTransaction.Commit();
                }
                catch (Exception)
                {
                    dbTransaction.Rollback();
                    throw;
                }
            }
        }

        public void DeleteInvoice(int invoiceId)
        {
            if (invoiceId <= 0)
            {
                _logger.LogWarning("Invalid Invoice ID: {InvoiceId}", invoiceId);
                throw new ArgumentException("Invalid Invoice ID");
            }

            var invoice = _dbContext.GetById<Invoice>(invoiceId); // Using repository for fetching invoice
            if (invoice == null)
            {
                _logger.LogWarning("Invoice with ID {InvoiceId} not found", invoiceId);
                return;
            }

            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    if (invoice.InvoiceTypeId == InvoiceType.InitialPawnInvoice)
                    {
                        if (DeleteInitialInvoice(invoice))
                        {
                            _logger.LogInformation("Invoice {InvoiceId} deleted successfully", invoiceId);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to delete invoice {InvoiceId}", invoiceId);
                        }
                    }

                    dbTransaction.Commit(); // Commit transaction
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback(); // Rollback transaction in case of error
                    _logger.LogError(ex, "Error while deleting invoice {InvoiceId}", invoiceId);
                    throw;
                }
            }
        }


        private bool DeleteInitialInvoice(Invoice initialInvoice)
        {
            bool isDeleted = false;

            var transaction = _dbContext.GetById<Transaction>(initialInvoice.TransactionId);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction with ID {TransactionId} not found", initialInvoice.TransactionId);
                return false;
            }

            try
            {
                // Delete Transaction Items - use direct service calls without transactions
                var transactionItems = _transactionItemService
                    .GetTransactionItemsByTransactionId(transaction.TransactionId)
                    .Select(t => t.TransactionItemId)
                    .ToList();

                if (transactionItems.Any())
                {
                    DeleteTransactionItemsWithoutTransaction(transactionItems);
                }

                // Delete Loan - use direct repository call
                var loan = _loanService.GetLoanByInitialInvoiceNumber(initialInvoice.InvoiceNo);
                if (loan != null)
                {
                    DeleteLoanWithoutTransaction(loan.LoanId);
                }

                // Delete Installments and related data
                var installments = _installmentService
                    .GetInstallmentsByInitialInvoiceNumber(initialInvoice.InvoiceNo)
                    .ToList();

                if (installments.Any())
                {
                    DeleteInstallmentsWithoutTransaction(installments.Select(i => i.InstallmentId));

                    var installmentTransactions = installments.Select(i => i.TransactionId).ToList();
                    var installmentInvoices = _dbContext
                        .Get<Invoice>(i => i.DeletedAt == null && installmentTransactions.Contains(i.TransactionId))
                        .ToList();

                    DeleteInvoicesWithoutTransaction(installmentInvoices.Select(i => i.InvoiceId));
                    DeleteTransactionsWithoutTransaction(installmentTransactions);
                }

                // Mark invoice and transaction as deleted
                initialInvoice.DeletedAt = DateTime.Now;
                transaction.DeletedAt = DateTime.Now;
                _dbContext.Update(initialInvoice);
                _dbContext.Update(transaction);
                _dbContext.Save();

                isDeleted = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting invoice {InvoiceId}", initialInvoice.InvoiceId);
                throw;
            }

            return isDeleted;
        }

        private void DeleteTransactionItemsWithoutTransaction(IEnumerable<int> transactionItemIds)
        {
            var transactionItems = _dbContext.Get<TransactionItem>(t => transactionItemIds.Contains(t.TransactionItemId) && t.DeletedAt == null).ToList();
            foreach (var transactionItem in transactionItems)
            {
                transactionItem.DeletedAt = DateTime.Now;
                _dbContext.Update(transactionItem);
            }
            _dbContext.Save();
        }

        private void DeleteLoanWithoutTransaction(int loanId)
        {
            var loan = _dbContext.GetById<Loan>(loanId);
            if (loan != null)
            {
                loan.DeletedAt = DateTime.Now;
                _dbContext.Update(loan);
                _dbContext.Save();
            }
        }

        private void DeleteInstallmentsWithoutTransaction(IEnumerable<int> installmentIds)
        {
            var installments = _dbContext.Get<Installment>(i => installmentIds.Contains(i.InstallmentId) && i.DeletedAt == null).ToList();
            foreach (var installment in installments)
            {
                installment.DeletedAt = DateTime.Now;
                _dbContext.Update(installment);
            }
            _dbContext.Save();
        }

        private void DeleteInvoicesWithoutTransaction(IEnumerable<int> invoiceIds)
        {
            var invoices = _dbContext.Get<Invoice>(i => invoiceIds.Contains(i.InvoiceId) && i.DeletedAt == null).ToList();
            foreach (var invoice in invoices)
            {
                invoice.DeletedAt = DateTime.Now;
                _dbContext.Update(invoice);
            }
            _dbContext.Save();
        }

        private void DeleteTransactionsWithoutTransaction(IEnumerable<int> transactionIds)
        {
            var transactions = _dbContext.Get<Transaction>(t => transactionIds.Contains(t.TransactionId) && t.DeletedAt == null).ToList();
            foreach (var transaction in transactions)
            {
                transaction.DeletedAt = DateTime.Now;
                _dbContext.Update(transaction);
            }
            _dbContext.Save();
        }

        public void DeleteInvoices(IEnumerable<int> invoiceIds)
        {
            var invoices = _dbContext.Get<Invoice>(i => invoiceIds.Contains(i.InvoiceId) && i.DeletedAt == null).ToList();
            foreach (var invoice in invoices)
            {
                invoice.DeletedAt = DateTime.Now;
                _dbContext.Update(invoice);
            }
            _dbContext.Save();
        }

        public Invoice GetLastInvoice()
        {
            return _dbContext.Get<Invoice>(i => i.DeletedAt == null)
                .OrderByDescending(i => i.InvoiceId)
                .FirstOrDefault();
        }

        public string GenerateInvoiceNumber()
        {
            var lastInvoice = GetLastInvoice();
            int nextInvoiceNumber = lastInvoice == null ? 1 : lastInvoice.InvoiceId + 1;
            string todaysDate = DateTime.Today.ToString("yyyyMMdd");

            return $"GC{todaysDate}{nextInvoiceNumber}";
        }

        public IEnumerable<Invoice> GetInvoicesByCustomerId(int customerId)
        {


            return _dbContext.Get<Invoice>(i => i.Transaction.CustomerId == customerId && i.DeletedAt == null).ToList();
        }

        public IEnumerable<Invoice> GetInvoiceByInvoiceNo(string invoiceNo)
        {
            return _dbContext.Get<Invoice>(i => i.InvoiceNo.ToLower() == invoiceNo.ToLower() && i.DeletedAt == null).ToList();
        }


        public int? GetInvoiceCount()
        {


            return _dbContext.Get<Invoice>(i =>  i.DeletedAt == null).Count();
        }
    }
}
