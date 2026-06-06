using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using SMS.Interfaces;
using SMS.Models;
using SMS.Enums;
using Microsoft.EntityFrameworkCore.Storage;

namespace SMS.Tests.Helpers
{
    /// <summary>
    /// Helper class for creating mock objects and test data
    /// </summary>
    public static class MockHelpers
    {
        #region Mock Repository Setup

        /// <summary>
        /// Creates a basic mock IRepository
        /// </summary>
        public static Mock<IRepository> CreateMockRepository()
        {
            var mockRepo = new Mock<IRepository>();

            // Setup basic transaction methods
            mockRepo.Setup(r => r.CreateTransaction()).Returns(Mock.Of<IDbContextTransaction>());
            mockRepo.Setup(r => r.CommitTransaction());
            mockRepo.Setup(r => r.RollbackTransaction());
            mockRepo.Setup(r => r.Save());

            return mockRepo;
        }

        /// <summary>
        /// Sets up Get method to return queryable data
        /// </summary>
        public static void SetupGet<T>(this Mock<IRepository> mockRepo, List<T> data) where T : class
        {
            mockRepo.Setup(r => r.Get<T>(It.IsAny<Expression<Func<T, bool>>>()))
                .Returns((Expression<Func<T, bool>> predicate) =>
                {
                    if (predicate == null)
                        return data.AsQueryable();
                    return data.AsQueryable().Where(predicate);
                });
        }

        /// <summary>
        /// Sets up GetById to return specific entity
        /// </summary>
        public static void SetupGetById<T>(this Mock<IRepository> mockRepo, T entity, object id) where T : class
        {
            mockRepo.Setup(r => r.GetById<T>(id)).Returns(entity);
        }

        #endregion

        #region Test Data Builders

        /// <summary>
        /// Creates a test Customer
        /// </summary>
        public static Customer CreateTestCustomer(
            int customerId = 1,
            string customerNIC = "123456789V",
            string customerName = "Test Customer",
            string address = "123 Test Street",
            string contactNo = "0771234567")
        {
            return new Customer
            {
                CustomerId = customerId,
                CustomerNIC = customerNIC,
                CustomerName = customerName,
                CustomerAddress = address,
                CustomerContactNo = contactNo,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Creates a test Transaction
        /// </summary>
        public static Transaction CreateTestTransaction(
            int transactionId = 1,
            int customerId = 1,
            decimal subTotal = 10000m,
            decimal? interestRate = 5m,
            decimal? interestAmount = 500m,
            decimal totalAmount = 10500m,
            TransactionType transactionType = TransactionType.LoanIssuance)
        {
            return new Transaction
            {
                TransactionId = transactionId,
                CustomerId = customerId,
                SubTotal = subTotal,
                InterestRate = interestRate,
                InterestAmount = interestAmount,
                TotalAmount = totalAmount,
                TransactionType = transactionType,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Creates a test Loan
        /// </summary>
        public static Loan CreateTestLoan(
            int loanId = 1,
            int transactionId = 1,
            int? loanPeriodId = 1,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal outstandingAmount = 10500m,
            decimal amountPaid = 0m,
            bool isSettled = false)
        {
            var start = startDate ?? DateTime.Now;
            var end = endDate ?? start.AddMonths(3);

            return new Loan
            {
                LoanId = loanId,
                TransactionId = transactionId,
                LoanPeriodId = loanPeriodId,
                StartDate = start,
                EndDate = end,
                OutstandingAmount = outstandingAmount,
                AmountPaid = amountPaid,
                IsSettled = isSettled
            };
        }

        /// <summary>
        /// Creates a test Invoice
        /// </summary>
        public static Invoice CreateTestInvoice(
            int invoiceId = 1,
            string invoiceNo = "INV-001",
            int transactionId = 1,
            InvoiceType invoiceTypeId = InvoiceType.InitialPawnInvoice,
            int status = 1)
        {
            return new Invoice
            {
                InvoiceId = invoiceId,
                InvoiceNo = invoiceNo,
                TransactionId = transactionId,
                InvoiceTypeId = invoiceTypeId,
                Status = status,
                DateGenerated = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Creates a test Item
        /// </summary>
        public static Item CreateTestItem(
            int itemId = 1,
            int customerId = 1,
            string description = "Gold Chain",
            string remarks = "18K",
            decimal caratage = 18m,
            decimal weight = 10m,
            decimal goldWeight = 7.5m,
            decimal value = 50000m,
            int status = (int)ItemStatus.InStock)
        {
            return new Item
            {
                ItemId = itemId,
                CustomerId = customerId,
                ItemDescription = description,
                ItemRemarks = remarks,
                ItemCaratage = caratage,
                ItemWeight = weight,
                ItemGoldWeight = goldWeight,
                ItemValue = value,
                Status = status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Creates a test Installment (Note: CreatedAt is private in the model)
        /// </summary>
        public static Installment CreateTestInstallment(
            int installmentId = 1,
            int loanId = 1,
            int transactionId = 1,
            int installmentNumber = 1,
            decimal amountPaid = 1000m,
            DateTime? paymentDate = null,
            DateTime? dueDate = null)
        {
            return new Installment
            {
                InstallmentId = installmentId,
                LoanId = loanId,
                TransactionId = transactionId,
                InstallmentNumber = installmentNumber,
                AmountPaid = amountPaid,
                PaymentDate = paymentDate ?? DateTime.Now,
                DueDate = dueDate ?? DateTime.Now.AddMonths(1),
                UpdatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Creates a test LoanPeriod
        /// </summary>
        public static LoanPeriod CreateTestLoanPeriod(
            int loanPeriodId = 1,
            int period = 3)
        {
            return new LoanPeriod
            {
                LoanPeriodId = loanPeriodId,
                Period = period
            };
        }

        #endregion

        #region List Builders

        /// <summary>
        /// Creates a list of test customers
        /// </summary>
        public static List<Customer> CreateTestCustomers(int count = 5)
        {
            return Enumerable.Range(1, count)
                .Select(i => CreateTestCustomer(
                    customerId: i,
                    customerNIC: $"12345678{i}V",
                    customerName: $"Customer {i}"))
                .ToList();
        }

        /// <summary>
        /// Creates a list of test loans
        /// </summary>
        public static List<Loan> CreateTestLoans(int count = 3, bool includeSettled = false)
        {
            var loans = Enumerable.Range(1, count)
                .Select(i => CreateTestLoan(
                    loanId: i,
                    transactionId: i,
                    isSettled: includeSettled && i % 2 == 0))
                .ToList();

            return loans;
        }

        /// <summary>
        /// Creates a list of test items
        /// </summary>
        public static List<Item> CreateTestItems(int count = 5, int customerId = 1)
        {
            return Enumerable.Range(1, count)
                .Select(i => CreateTestItem(
                    itemId: i,
                    customerId: customerId,
                    description: $"Item {i}"))
                .ToList();
        }

        #endregion
    }
}
