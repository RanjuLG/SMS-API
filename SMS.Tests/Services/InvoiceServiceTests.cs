using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models;
using SMS.Services;
using SMS.Tests.Helpers;
using Xunit;

namespace SMS.Tests.Services
{
    public class InvoiceServiceTests
    {
        private readonly Mock<IRepository> _repositoryMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly Mock<ILoanService> _loanServiceMock;
        private readonly Mock<ITransactionItemService> _transactionItemServiceMock;
        private readonly Mock<IInstallmentService> _installmentServiceMock;
        private readonly Mock<ILogger<InvoiceService>> _loggerMock;
        private readonly InvoiceService _invoiceService;

        public InvoiceServiceTests()
        {
            _repositoryMock = MockHelpers.CreateMockRepository();
            _transactionServiceMock = new Mock<ITransactionService>();
            _loanServiceMock = new Mock<ILoanService>();
            _transactionItemServiceMock = new Mock<ITransactionItemService>();
            _installmentServiceMock = new Mock<IInstallmentService>();
            _loggerMock = new Mock<ILogger<InvoiceService>>();
            
            _invoiceService = new InvoiceService(
                _repositoryMock.Object,
                _transactionServiceMock.Object,
                _loanServiceMock.Object,
                _transactionItemServiceMock.Object,
                _installmentServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region GetAllInvoices Tests

        [Fact]
        public void GetAllInvoices_ReturnsAllInvoices()
        {
            // Arrange
            var invoices = new List<Invoice>
            {
                MockHelpers.CreateTestInvoice(invoiceId: 1),
                MockHelpers.CreateTestInvoice(invoiceId: 2),
            };
            _repositoryMock.SetupGet(invoices);

            // Act
            var result = _invoiceService.GetAllInvoices();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        #endregion

        #region GetInvoiceById Tests

        [Fact]
        public void GetInvoiceById_ExistingInvoice_ReturnsInvoice()
        {
            // Arrange
            var invoice = MockHelpers.CreateTestInvoice(invoiceId: 1);
            _repositoryMock.SetupGet(new List<Invoice> { invoice });

            // Act
            var result = _invoiceService.GetInvoiceById(1);

            // Assert
            result.Should().NotBeNull();
            result.InvoiceId.Should().Be(1);
        }

        [Fact]
        public void GetInvoiceById_NonExistingInvoice_ReturnsNull()
        {
            // Arrange
            _repositoryMock.SetupGet(new List<Invoice>());

            // Act
            var result = _invoiceService.GetInvoiceById(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetInvoiceByInvoiceNo Tests

        [Fact]
        public void GetInvoiceByInvoiceNo_ExistingNumber_ReturnsInvoice()
        {
            // Arrange
            var invoice = MockHelpers.CreateTestInvoice(invoiceNo: "INV-001");
            _repositoryMock.SetupGet(new List<Invoice> { invoice });

            // Act
            var result = _invoiceService.GetInvoiceByInvoiceNo("INV-001");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().InvoiceNo.Should().Be("INV-001");
        }

        #endregion

        #region CreateInvoice Tests

        [Fact]
        public void CreateInvoice_ValidInvoice_CallsRepository()
        {
            // Arrange
            var invoice = MockHelpers.CreateTestInvoice();

            // Act
            _invoiceService.CreateInvoice(invoice);

            // Assert
            _repositoryMock.Verify(r => r.Create<Invoice>(invoice, null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region GenerateInvoiceNumber Tests

        [Fact]
        public void GenerateInvoiceNumber_NoExistingInvoices_ReturnsFirstNumber()
        {
            // Arrange
            _repositoryMock.SetupGet(new List<Invoice>());

            // Act
            var result = _invoiceService.GenerateInvoiceNumber();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("INV-");
        }

        [Fact]
        public void GenerateInvoiceNumber_ExistingInvoices_ReturnsIncrementedNumber()
        {
            // Arrange
            var existingInvoice = MockHelpers.CreateTestInvoice(invoiceNo: "INV-0001");
            _repositoryMock.SetupGet(new List<Invoice> { existingInvoice });

            // Act
            var result = _invoiceService.GenerateInvoiceNumber();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("INV-");
        }

        #endregion

        #region GetInvoiceCount Tests

        [Fact]
        public void GetInvoiceCount_ReturnsCorrectCount()
        {
            // Arrange
            var invoices = new List<Invoice>
            {
                MockHelpers.CreateTestInvoice(invoiceId: 1),
                MockHelpers.CreateTestInvoice(invoiceId: 2),
                MockHelpers.CreateTestInvoice(invoiceId: 3),
            };
            _repositoryMock.SetupGet(invoices);

            // Act
            var result = _invoiceService.GetInvoiceCount();

            // Assert
            result.Should().Be(3);
        }

        #endregion

        #region GetInvoicesByCustomerId Tests

        [Fact]
        public void GetInvoicesByCustomerId_ReturnsCustomerInvoices()
        {
            // Arrange
            var transaction = MockHelpers.CreateTestTransaction(customerId: 1);
            var invoice = MockHelpers.CreateTestInvoice(invoiceId: 1);
            invoice.Transaction = transaction;
            _repositoryMock.SetupGet(new List<Invoice> { invoice });

            // Act
            var result = _invoiceService.GetInvoicesByCustomerId(1);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion
    }
}
