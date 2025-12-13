using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using SMS.Generic;
using SMS.Interfaces;
using SMS.Models;
using SMS.Services;
using SMS.Tests.Helpers;
using Xunit;

namespace SMS.Tests.Services
{
    public class LoanServiceTests
    {
        private readonly Mock<IRepository> _repositoryMock;
        private readonly LoanService _loanService;

        public LoanServiceTests()
        {
            _repositoryMock = MockHelpers.CreateMockRepository();
            _loanService = new LoanService(_repositoryMock.Object);
        }

        #region GetLoanById Tests

        [Fact]
        public void GetLoanById_ExistingLoan_ReturnsLoan()
        {
            // Arrange
            var loan = MockHelpers.CreateTestLoan(loanId: 1);
            _repositoryMock.SetupGet(new List<Loan> { loan });

            // Act
            var result = _loanService.GetLoanById(1);

            // Assert
            result.Should().NotBeNull();
            result.LoanId.Should().Be(1);
        }

        [Fact]
        public void GetLoanById_NonExistingLoan_ReturnsNull()
        {
            // Arrange
            _repositoryMock.SetupGet(new List<Loan>());

            // Act
            var result = _loanService.GetLoanById(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAllLoans Tests

        [Fact]
        public void GetAllLoans_WithDateRange_FiltersLoans()
        {
            // Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var loans = new List<Loan>
            {
                MockHelpers.CreateTestLoan(loanId: 1, startDate: startDate),
                MockHelpers.CreateTestLoan(loanId: 2, startDate: startDate.AddDays(15)),
            };
            _repositoryMock.SetupGet(loans);

            var dateRange = new DateTimeRange
            {
                From = startDate,
                To = DateTime.Now
            };

            // Act
            var result = _loanService.GetAllLoans(dateRange);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void GetAllLoans_WithMinDateRange_ReturnsAllLoans()
        {
            // Arrange
            var loans = MockHelpers.CreateTestLoans(3);
            _repositoryMock.SetupGet(loans);

            var dateRange = new DateTimeRange
            {
                From = DateTime.MinValue,
                To = DateTime.MinValue
            };

            // Act
            var result = _loanService.GetAllLoans(dateRange);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
        }

        #endregion

        #region GetLoansByCustomerId Tests

        [Fact]
        public void GetLoansByCustomerId_ExistingCustomer_ReturnsLoans()
        {
            // Arrange
            var transaction = MockHelpers.CreateTestTransaction(customerId: 1);
            var loan = MockHelpers.CreateTestLoan(loanId: 1);
            loan.Transaction = transaction;
            _repositoryMock.SetupGet(new List<Loan> { loan });

            // Act
            var result = _loanService.GetLoansByCustomerId(1);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion

        #region CreateLoan Tests

        [Fact]
        public void CreateLoan_ValidLoan_CallsRepository()
        {
            // Arrange
            var loan = MockHelpers.CreateTestLoan();

            // Act
            _loanService.CreateLoan(loan);

            // Assert
            _repositoryMock.Verify(r => r.Create<Loan>(loan, null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
            _repositoryMock.Verify(r => r.CommitTransaction(), Times.Once);
        }

        #endregion

        #region UpdateLoan Tests

        [Fact]
        public void UpdateLoan_ExistingLoan_UpdatesFields()
        {
            // Arrange
            var existingLoan = MockHelpers.CreateTestLoan(loanId: 1);
            _repositoryMock.SetupGetById(existingLoan, 1);

            var updatedLoan = MockHelpers.CreateTestLoan(loanId: 1, amountPaid: 5000m);

            // Act
            _loanService.UpdateLoan(updatedLoan);

            // Assert
            _repositoryMock.Verify(r => r.Update<Loan>(It.IsAny<Loan>(), null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region DeleteLoan Tests

        [Fact]
        public void DeleteLoan_ExistingLoan_SoftDeletes()
        {
            // Arrange
            var loan = MockHelpers.CreateTestLoan(loanId: 1);
            _repositoryMock.SetupGetById(loan, 1);

            // Act
            _loanService.DeleteLoan(1);

            // Assert
            _repositoryMock.Verify(r => r.Update<Loan>(It.Is<Loan>(l => l.DeletedAt != null), null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region Count Methods Tests

        [Fact]
        public void GetActiveLoanCount_ReturnsCountOfActiveLoans()
        {
            // Arrange
            var loans = new List<Loan>
            {
                MockHelpers.CreateTestLoan(loanId: 1, isSettled: false),
                MockHelpers.CreateTestLoan(loanId: 2, isSettled: false),
                MockHelpers.CreateTestLoan(loanId: 3, isSettled: true),
            };
            _repositoryMock.SetupGet(loans);

            // Act
            var result = _loanService.GetActiveLoanCount();

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public void GetSettledLoanCount_ReturnsCountOfSettledLoans()
        {
            // Arrange
            var loans = new List<Loan>
            {
                MockHelpers.CreateTestLoan(loanId: 1, isSettled: false),
                MockHelpers.CreateTestLoan(loanId: 2, isSettled: true),
                MockHelpers.CreateTestLoan(loanId: 3, isSettled: true),
            };
            _repositoryMock.SetupGet(loans);

            // Act
            var result = _loanService.GetSettledLoanCount();

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public void GetTotalOutstandingAmount_ReturnsSumOfOutstanding()
        {
            // Arrange
            var loans = new List<Loan>
            {
                MockHelpers.CreateTestLoan(loanId: 1, outstandingAmount: 10000m, isSettled: false),
                MockHelpers.CreateTestLoan(loanId: 2, outstandingAmount: 15000m, isSettled: false),
                MockHelpers.CreateTestLoan(loanId: 3, outstandingAmount: 5000m, isSettled: true), // Should be excluded
            };
            _repositoryMock.SetupGet(loans);

            // Act
            var result = _loanService.GetTotalOutstandingAmount();

            // Assert
            result.Should().Be(25000m);
        }

        #endregion
    }
}
