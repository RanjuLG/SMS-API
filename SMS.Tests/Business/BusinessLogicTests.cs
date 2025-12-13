using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SMS.Business;
using SMS.Enums;
using SMS.Generic;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using SMS.Services;
using SMS.Tests.Helpers;
using Xunit;

namespace SMS.Tests.Business
{
    public class BusinessLogicTests
    {
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly Mock<IItemService> _itemServiceMock;
        private readonly Mock<ITransactionItemService> _transactionItemServiceMock;
        private readonly Mock<IInvoiceService> _invoiceServiceMock;
        private readonly Mock<ILoanService> _loanServiceMock;
        private readonly Mock<IKaratageService> _karatageServiceMock;
        private readonly Mock<IInstallmentService> _installmentServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<InvoiceService>> _loggerMock;
        private readonly Mock<IRepository> _repositoryMock;
        private readonly BusinessLogic _businessLogic;

        public BusinessLogicTests()
        {
            _customerServiceMock = new Mock<ICustomerService>();
            _transactionServiceMock = new Mock<ITransactionService>();
            _itemServiceMock = new Mock<IItemService>();
            _transactionItemServiceMock = new Mock<ITransactionItemService>();
            _invoiceServiceMock = new Mock<IInvoiceService>();
            _loanServiceMock = new Mock<ILoanService>();
            _karatageServiceMock = new Mock<IKaratageService>();
            _installmentServiceMock = new Mock<IInstallmentService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<InvoiceService>>();
            _repositoryMock = MockHelpers.CreateMockRepository();

            _businessLogic = new BusinessLogic(
                _customerServiceMock.Object,
                _transactionServiceMock.Object,
                _itemServiceMock.Object,
                _transactionItemServiceMock.Object,
                _invoiceServiceMock.Object,
                _loanServiceMock.Object,
                _karatageServiceMock.Object,
                _installmentServiceMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _repositoryMock.Object
            );
        }

        #region ProcessInvoice Tests

        [Fact]
        public void ProcessInvoice_NullRequest_ReturnsBadRequest()
        {
            // Act
            var result = _businessLogic.ProcessInvoice(null, "INV-001", 1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().Be("Invalid request data.");
        }

        [Fact]
        public void ProcessInvoice_ZeroInvoiceTypeId_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateInvoiceDTO { InvoiceTypeId = 0 };

            // Act
            var result = _businessLogic.ProcessInvoice(request, "INV-001", 1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void ProcessInvoice_InitialPawnInvoice_CreatesTransactionLoanAndInvoice()
        {
            // Arrange
            var customer = MockHelpers.CreateTestCustomer();
            var loanPeriod = MockHelpers.CreateTestLoanPeriod();
            var invoice = MockHelpers.CreateTestInvoice();

            var request = new CreateInvoiceDTO
            {
                InvoiceTypeId = (int)InvoiceType.InitialPawnInvoice,
                Customer = new CreateCustomerDTO { CustomerNIC = "123456789V", CustomerName = "Test" },
                SubTotal = 10000m,
                InterestRate = 5m,
                InterestAmount = 500m,
                TotalAmount = 10500m,
                LoanPeriodId = 1,
                Date = DateTime.Now,
                Items = new[] { new CustomItemDTO { itemId = 0, ItemDescription = "Gold Chain", ItemValue = 10000m } }
            };

            _customerServiceMock.Setup(s => s.GetCustomerByNIC(It.IsAny<string>())).Returns(customer);
            _karatageServiceMock.Setup(s => s.GetLoanPeriodById(It.IsAny<int>())).Returns(loanPeriod);
            _invoiceServiceMock.Setup(s => s.GenerateInvoiceNumber()).Returns("INV-002");
            _invoiceServiceMock.Setup(s => s.GetLastInvoice()).Returns(invoice);

            // Act
            var result = _businessLogic.ProcessInvoice(request, "", 0);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _repositoryMock.Verify(r => r.Create<Transaction>(It.IsAny<Transaction>(), null), Times.Once);
            _repositoryMock.Verify(r => r.Create<Loan>(It.IsAny<Loan>(), null), Times.Once);
            _repositoryMock.Verify(r => r.Create<Invoice>(It.IsAny<Invoice>(), null), Times.Once);
        }

        [Fact]
        public void ProcessInvoice_InvalidInvoiceType_ReturnsBadRequest()
        {
            // Arrange
            var customer = MockHelpers.CreateTestCustomer();
            var request = new CreateInvoiceDTO
            {
                InvoiceTypeId = 999, // Invalid type
                Customer = new CreateCustomerDTO { CustomerNIC = "123456789V" }
            };

            _customerServiceMock.Setup(s => s.GetCustomerByNIC(It.IsAny<string>())).Returns(customer);

            // Act
            var result = _businessLogic.ProcessInvoice(request, "INV-001", 1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region ProcessInstallments Tests

        [Fact]
        public void ProcessInstallments_InvalidInvoice_ReturnsNull()
        {
            // Arrange
            _invoiceServiceMock.Setup(s => s.GetInvoiceByInvoiceNo(It.IsAny<string>()))
                .Returns(new List<Invoice>());

            // Act
            var result = _businessLogic.ProcessInstallments("INVALID-INV");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ProcessInstallments_ValidInvoice_ReturnsLoanInfo()
        {
            // Arrange
            var transaction = MockHelpers.CreateTestTransaction();
            var invoice = MockHelpers.CreateTestInvoice();
            invoice.Transaction = transaction;
            invoice.TransactionId = transaction.TransactionId;

            var loanPeriod = MockHelpers.CreateTestLoanPeriod(period: 3);
            var loan = MockHelpers.CreateTestLoan();
            loan.LoanPeriod = loanPeriod;
            loan.StartDate = DateTime.Now.AddMonths(-1);
            loan.EndDate = DateTime.Now.AddMonths(2);

            _installmentServiceMock.Setup(s => s.GetInstallmentsByInitialInvoiceNumber(It.IsAny<string>()))
                .Returns(new List<Installment>());
            _invoiceServiceMock.Setup(s => s.GetInvoiceByInvoiceNo(It.IsAny<string>()))
                .Returns(new List<Invoice> { invoice });
            _loanServiceMock.Setup(s => s.GetLoanByInitialInvoiceNumber(It.IsAny<string>()))
                .Returns(loan);
            _transactionServiceMock.Setup(s => s.GetTransactionById(It.IsAny<int>()))
                .Returns(transaction);

            // Act
            var result = _businessLogic.ProcessInstallments("INV-001");

            // Assert
            result.Should().NotBeNull();
            result.PrincipleAmount.Should().Be(transaction.SubTotal);
            result.InterestRate.Should().Be(transaction.InterestRate ?? 0);
        }

        #endregion

        #region ProcessSingleReport Tests

        [Fact]
        public void ProcessSingleReport_InvalidCustomer_ReturnsNull()
        {
            // Arrange
            _customerServiceMock.Setup(s => s.GetCustomerById(It.IsAny<int>())).Returns((Customer)null!);

            // Act
            var result = _businessLogic.ProcessSingleReport(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ProcessSingleReport_ValidCustomer_ReturnsReport()
        {
            // Arrange
            var customer = MockHelpers.CreateTestCustomer();
            var transaction = MockHelpers.CreateTestTransaction();
            transaction.Customer = customer;
            transaction.TransactionItems = new List<TransactionItem>();
            
            var loan = MockHelpers.CreateTestLoan();
            loan.Transaction = transaction;
            loan.Installments = new List<Installment>();

            _customerServiceMock.Setup(s => s.GetCustomerById(1)).Returns(customer);
            _loanServiceMock.Setup(s => s.GetLoansByCustomerId(1)).Returns(new List<Loan> { loan });

            // Act
            var result = _businessLogic.ProcessSingleReport(1);

            // Assert
            result.Should().NotBeNull();
            result.CustomerId.Should().Be(customer.CustomerId);
            result.CustomerName.Should().Be(customer.CustomerName);
            result.Loans.Should().HaveCount(1);
        }

        #endregion

        #region SettleLoan Tests

        [Fact]
        public void SettleLoan_InvalidInvoice_ReturnsFalse()
        {
            // Arrange
            _invoiceServiceMock.Setup(s => s.GetInvoiceByInvoiceNo(It.IsAny<string>()))
                .Returns(new List<Invoice>());

            // Act
            var result = _businessLogic.SettleLoan("INVALID-INV");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void SettleLoan_ValidInvoice_ReturnsTrue()
        {
            // Arrange
            var transaction = MockHelpers.CreateTestTransaction();
            var invoice = MockHelpers.CreateTestInvoice();
            invoice.Transaction = transaction;

            var loan = MockHelpers.CreateTestLoan();

            _invoiceServiceMock.Setup(s => s.GetInvoiceByInvoiceNo(It.IsAny<string>()))
                .Returns(new List<Invoice> { invoice });
            _loanServiceMock.Setup(s => s.GetAllLoans(It.IsAny<IDateTimeRange>()))
                .Returns(new List<Loan> { loan });

            // Act
            var result = _businessLogic.SettleLoan("INV-001");

            // Assert
            result.Should().BeTrue();
            _loanServiceMock.Verify(s => s.UpdateLoan(It.Is<Loan>(l => l.IsSettled)), Times.Once);
        }

        #endregion

        #region ProcessOverview Tests

        [Fact]
        public void ProcessOverview_ReturnsCorrectCounts()
        {
            // Arrange
            _loanServiceMock.Setup(s => s.GetActiveLoanCount()).Returns(10);
            _invoiceServiceMock.Setup(s => s.GetInvoiceCount()).Returns(50);
            _customerServiceMock.Setup(s => s.GetCustomerCount()).Returns(25);
            _itemServiceMock.Setup(s => s.GetInventoryCount()).Returns(100);
            _transactionServiceMock.Setup(s => s.GetRevenue()).Returns(500000m);

            // Act
            var result = _businessLogic.ProcessOverview();

            // Assert
            result.Should().NotBeNull();
            result.TotalActiveLoans.Should().Be(10);
            result.TotalInvoices.Should().Be(50);
            result.CustomerCount.Should().Be(25);
            result.InventoryCount.Should().Be(100);
            result.RevenueGenerated.Should().Be(500000m);
        }

        #endregion

        #region GetInvoicesByCustomer Tests

        [Fact]
        public void GetInvoicesByCustomer_ReturnsCorrectDTOs()
        {
            // Arrange
            var customer = MockHelpers.CreateTestCustomer();
            var transaction = MockHelpers.CreateTestTransaction();
            var invoice = MockHelpers.CreateTestInvoice();
            invoice.Transaction = transaction;

            _invoiceServiceMock.Setup(s => s.GetInvoicesByCustomerId(1))
                .Returns(new List<Invoice> { invoice });
            _loanServiceMock.Setup(s => s.GetLoansByCustomerId(1))
                .Returns(new List<Loan>());

            // Act
            var result = _businessLogic.GetInvoicesByCustomer(customer);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var dto = result.First();
            dto.CustomerNIC.Should().Be(customer.CustomerNIC);
            dto.PrincipleAmount.Should().Be(transaction.SubTotal);
        }

        #endregion
    }
}
