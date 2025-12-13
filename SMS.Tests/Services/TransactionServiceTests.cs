using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using SMS.Enums;
using SMS.Generic;
using SMS.Interfaces;
using SMS.Models;
using SMS.Services;
using SMS.Tests.Helpers;
using Xunit;

namespace SMS.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<IRepository> _repositoryMock;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _repositoryMock = MockHelpers.CreateMockRepository();
            _transactionService = new TransactionService(_repositoryMock.Object);
        }

        #region GetAllTransactions Tests

        [Fact]
        public void GetAllTransactions_WithValidDateRange_ReturnsAllTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                MockHelpers.CreateTestTransaction(transactionId: 1),
                MockHelpers.CreateTestTransaction(transactionId: 2),
            };
            _repositoryMock.SetupGet(transactions);
            var dateRange = new DateTimeRange { From = DateTime.Now.AddDays(-30), To = DateTime.Now };

            // Act
            var result = _transactionService.GetAllTransactions(dateRange);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion

        #region GetTransactionById Tests

        [Fact]
        public void GetTransactionById_ExistingTransaction_ReturnsTransaction()
        {
            // Arrange
            var transaction = MockHelpers.CreateTestTransaction(transactionId: 1);
            _repositoryMock.SetupGet(new List<Transaction> { transaction });

            // Act
            var result = _transactionService.GetTransactionById(1);

            // Assert
            result.Should().NotBeNull();
            result.TransactionId.Should().Be(1);
        }

        [Fact]
        public void GetTransactionById_NonExistingTransaction_ReturnsNull()
        {
            // Arrange
            _repositoryMock.SetupGet(new List<Transaction>());

            // Act
            var result = _transactionService.GetTransactionById(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateTransaction Tests

        [Fact]
        public void CreateTransaction_ValidTransaction_CallsRepository()
        {
            // Arrange
            var transaction = MockHelpers.CreateTestTransaction();

            // Act
            _transactionService.CreateTransaction(transaction);

            // Assert
            _repositoryMock.Verify(r => r.Create<Transaction>(transaction, null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
            _repositoryMock.Verify(r => r.CommitTransaction(), Times.Once);
        }

        #endregion

        #region UpdateTransaction Tests

        [Fact]
        public void UpdateTransaction_ExistingTransaction_UpdatesFields()
        {
            // Arrange
            var existingTransaction = MockHelpers.CreateTestTransaction(transactionId: 1);
            _repositoryMock.SetupGetById(existingTransaction, 1);

            var updatedTransaction = MockHelpers.CreateTestTransaction(transactionId: 1, totalAmount: 20000m);

            // Act
            _transactionService.UpdateTransaction(updatedTransaction);

            // Assert
            _repositoryMock.Verify(r => r.Update<Transaction>(It.IsAny<Transaction>(), null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region DeleteTransaction Tests

        [Fact]
        public void DeleteTransaction_ExistingTransaction_SoftDeletes()
        {
            // Arrange
            var transaction = MockHelpers.CreateTestTransaction(transactionId: 1);
            _repositoryMock.SetupGetById(transaction, 1);

            // Act
            _transactionService.DeleteTransaction(1);

            // Assert
            _repositoryMock.Verify(r => r.Update<Transaction>(It.Is<Transaction>(t => t.DeletedAt != null), null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region GetTransactionsByCustomerId Tests

        [Fact]
        public void GetTransactionsByCustomerId_ExistingCustomer_ReturnsTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                MockHelpers.CreateTestTransaction(transactionId: 1, customerId: 1),
                MockHelpers.CreateTestTransaction(transactionId: 2, customerId: 1),
            };
            _repositoryMock.SetupGet(transactions);

            // Act
            var result = _transactionService.GetTransactionsByCustomerId(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        #endregion

        #region GetRevenue Tests

        [Fact]
        public void GetRevenue_WithInstallmentPayments_ReturnsInterestAmountSum()
        {
            // Arrange - GetRevenue only sums InterestAmount from InstallmentPayment transactions
            var transactions = new List<Transaction>
            {
                MockHelpers.CreateTestTransaction(transactionId: 1, interestAmount: 500m, transactionType: TransactionType.InstallmentPayment),
                MockHelpers.CreateTestTransaction(transactionId: 2, interestAmount: 300m, transactionType: TransactionType.InstallmentPayment),
                MockHelpers.CreateTestTransaction(transactionId: 3, interestAmount: 700m, transactionType: TransactionType.LoanIssuance), // Should be excluded
            };
            _repositoryMock.SetupGet(transactions);

            // Act
            var result = _transactionService.GetRevenue();

            // Assert
            result.Should().Be(800m); // Only InstallmentPayment transactions: 500 + 300
        }

        #endregion

        #region GetTransactionCount Tests

        [Fact]
        public void GetTransactionCount_ReturnsCorrectCount()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                MockHelpers.CreateTestTransaction(transactionId: 1),
                MockHelpers.CreateTestTransaction(transactionId: 2),
            };
            _repositoryMock.SetupGet(transactions);

            // Act
            var result = _transactionService.GetTransactionCount();

            // Assert
            result.Should().Be(2);
        }

        #endregion
    }
}
