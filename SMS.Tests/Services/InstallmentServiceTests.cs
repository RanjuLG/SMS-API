using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using SMS.Interfaces;
using SMS.Models;
using SMS.Services;
using SMS.Tests.Helpers;
using Xunit;

namespace SMS.Tests.Services
{
    public class InstallmentServiceTests
    {
        private readonly Mock<IRepository> _repositoryMock;
        private readonly InstallmentService _installmentService;

        public InstallmentServiceTests()
        {
            _repositoryMock = MockHelpers.CreateMockRepository();
            _installmentService = new InstallmentService(_repositoryMock.Object);
        }

        #region GetAllInstallments Tests

        [Fact]
        public void GetAllInstallments_ReturnsAllNonDeletedInstallments()
        {
            // Arrange
            var installments = new List<Installment>
            {
                MockHelpers.CreateTestInstallment(installmentId: 1),
                MockHelpers.CreateTestInstallment(installmentId: 2),
            };
            _repositoryMock.SetupGet(installments);

            // Act
            var result = _installmentService.GetAllInstallments();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        #endregion

        #region GetInstallmentById Tests

        [Fact]
        public void GetInstallmentById_ExistingInstallment_ReturnsInstallment()
        {
            // Arrange
            var installment = MockHelpers.CreateTestInstallment(installmentId: 1);
            _repositoryMock.SetupGet(new List<Installment> { installment });

            // Act
            var result = _installmentService.GetInstallmentById(1);

            // Assert
            result.Should().NotBeNull();
            result.InstallmentId.Should().Be(1);
        }

        [Fact]
        public void GetInstallmentById_NonExistingInstallment_ReturnsNull()
        {
            // Arrange
            _repositoryMock.SetupGet(new List<Installment>());

            // Act
            var result = _installmentService.GetInstallmentById(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetInstallmentsByTransactionId Tests

        [Fact]
        public void GetInstallmentsByTransactionId_ExistingTransaction_ReturnsInstallments()
        {
            // Arrange
            var installments = new List<Installment>
            {
                MockHelpers.CreateTestInstallment(installmentId: 1, transactionId: 1),
                MockHelpers.CreateTestInstallment(installmentId: 2, transactionId: 1),
            };
            _repositoryMock.SetupGet(installments);

            // Act
            var result = _installmentService.GetInstallmentsByTransactionId(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        #endregion

        #region CreateInstallment Tests

        [Fact]
        public void CreateInstallment_ValidInstallment_CallsRepository()
        {
            // Arrange
            var installment = MockHelpers.CreateTestInstallment();

            // Act
            _installmentService.CreateInstallment(installment);

            // Assert
            _repositoryMock.Verify(r => r.Create<Installment>(installment, null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
            _repositoryMock.Verify(r => r.CommitTransaction(), Times.Once);
        }

        #endregion

        #region UpdateInstallment Tests

        [Fact]
        public void UpdateInstallment_ValidInstallment_UpdatesFields()
        {
            // Arrange
            var installment = MockHelpers.CreateTestInstallment(installmentId: 1, amountPaid: 2000m);

            // Act
            _installmentService.UpdateInstallment(installment);

            // Assert
            _repositoryMock.Verify(r => r.Update<Installment>(It.IsAny<Installment>(), null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region DeleteInstallment Tests

        [Fact]
        public void DeleteInstallment_ExistingInstallment_SoftDeletes()
        {
            // Arrange
            var installment = MockHelpers.CreateTestInstallment(installmentId: 1);
            _repositoryMock.SetupGetById(installment, 1);

            // Act
            _installmentService.DeleteInstallment(1);

            // Assert
            _repositoryMock.Verify(r => r.Update<Installment>(It.Is<Installment>(i => i.DeletedAt != null), null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        [Fact]
        public void DeleteInstallment_NonExistingInstallment_DoesNotUpdate()
        {
            // Arrange
            _repositoryMock.SetupGetById<Installment>(null!, 999);

            // Act
            _installmentService.DeleteInstallment(999);

            // Assert
            _repositoryMock.Verify(r => r.Update<Installment>(It.IsAny<Installment>(), null), Times.Never);
        }

        #endregion

        #region DeleteInstallments Tests

        [Fact]
        public void DeleteInstallments_MultipleIds_SoftDeletesAll()
        {
            // Arrange
            var installments = new List<Installment>
            {
                MockHelpers.CreateTestInstallment(installmentId: 1),
                MockHelpers.CreateTestInstallment(installmentId: 2),
            };
            _repositoryMock.SetupGet(installments);

            // Act
            _installmentService.DeleteInstallments(new[] { 1, 2 });

            // Assert
            _repositoryMock.Verify(r => r.Update<Installment>(It.IsAny<Installment>(), null), Times.Exactly(2));
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region GetInstallmentsByInitialInvoiceNumber Tests

        [Fact]
        public void GetInstallmentsByInitialInvoiceNumber_ExistingInvoice_ReturnsInstallments()
        {
            // Arrange
            var invoice = MockHelpers.CreateTestInvoice(invoiceNo: "INV-001");
            var transaction = MockHelpers.CreateTestTransaction();
            transaction.Invoice = invoice;
            var loan = MockHelpers.CreateTestLoan();
            loan.Transaction = transaction;
            
            var installment = MockHelpers.CreateTestInstallment(installmentId: 1);
            installment.Loan = loan;
            
            _repositoryMock.SetupGet(new List<Installment> { installment });

            // Act
            var result = _installmentService.GetInstallmentsByInitialInvoiceNumber("INV-001");

            // Assert
            result.Should().NotBeNull();
        }

        #endregion

        #region GetInstallmentsForCustomer Tests

        [Fact]
        public void GetInstallmentsForCustomer_ExistingCustomer_ReturnsInstallments()
        {
            // Arrange
            var transaction = MockHelpers.CreateTestTransaction(customerId: 1);
            var installment = MockHelpers.CreateTestInstallment(installmentId: 1);
            installment.Transaction = transaction;
            
            _repositoryMock.SetupGet(new List<Installment> { installment });

            // Act
            var result = _installmentService.GetInstallmentsForCustomer(1);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion
    }
}
