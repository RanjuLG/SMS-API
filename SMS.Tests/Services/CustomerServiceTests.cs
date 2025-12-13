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
    public class CustomerServiceTests
    {
        private readonly Mock<IRepository> _repositoryMock;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _repositoryMock = MockHelpers.CreateMockRepository();
            _customerService = new CustomerService(_repositoryMock.Object);
        }

        #region GetAllCustomers Tests

        [Fact]
        public void GetAllCustomers_WithValidDateRange_ReturnsAllNonDeletedCustomers()
        {
            // Arrange
            var customers = MockHelpers.CreateTestCustomers(5);
            _repositoryMock.SetupGet(customers);
            var dateRange = new DateTimeRange { From = DateTime.Now.AddDays(-30), To = DateTime.Now };

            // Act
            var result = _customerService.GetAllCustomers(dateRange);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }

        [Fact]
        public void GetAllCustomers_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock.SetupGet(new List<Customer>());
            var dateRange = new DateTimeRange { From = DateTime.Now.AddDays(-30), To = DateTime.Now };

            // Act
            var result = _customerService.GetAllCustomers(dateRange);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetCustomerById Tests

        [Fact]
        public void GetCustomerById_ExistingCustomer_ReturnsCustomer()
        {
            // Arrange
            var customer = MockHelpers.CreateTestCustomer(customerId: 1);
            _repositoryMock.SetupGet(new List<Customer> { customer });

            // Act
            var result = _customerService.GetCustomerById(1);

            // Assert
            result.Should().NotBeNull();
            result.CustomerId.Should().Be(1);
        }

        [Fact]
        public void GetCustomerById_NonExistingCustomer_ReturnsNull()
        {
            // Arrange
            _repositoryMock.SetupGet(new List<Customer>());

            // Act
            var result = _customerService.GetCustomerById(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetCustomerByNIC Tests

        [Fact]
        public void GetCustomerByNIC_ExistingNIC_ReturnsCustomer()
        {
            // Arrange
            var customer = MockHelpers.CreateTestCustomer(customerNIC: "123456789V");
            _repositoryMock.SetupGet(new List<Customer> { customer });

            // Act
            var result = _customerService.GetCustomerByNIC("123456789V");

            // Assert
            result.Should().NotBeNull();
            result.CustomerNIC.Should().Be("123456789V");
        }

        [Fact]
        public void GetCustomerByNIC_NonExistingNIC_ReturnsNull()
        {
            // Arrange
            _repositoryMock.SetupGet(new List<Customer>());

            // Act
            var result = _customerService.GetCustomerByNIC("INVALID");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateCustomer Tests

        [Fact]
        public void CreateCustomer_ValidCustomer_CallsRepository()
        {
            // Arrange
            var customer = MockHelpers.CreateTestCustomer();

            // Act
            _customerService.CreateCustomer(customer);

            // Assert
            _repositoryMock.Verify(r => r.Create<Customer>(customer, null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
            _repositoryMock.Verify(r => r.CommitTransaction(), Times.Once);
        }

        #endregion

        #region UpdateCustomer Tests

        [Fact]
        public void UpdateCustomer_ExistingCustomer_UpdatesFields()
        {
            // Arrange
            var existingCustomer = MockHelpers.CreateTestCustomer(customerId: 1);
            _repositoryMock.SetupGetById(existingCustomer, 1);

            var updatedCustomer = MockHelpers.CreateTestCustomer(customerId: 1, customerName: "Updated Name");

            // Act
            _customerService.UpdateCustomer(updatedCustomer);

            // Assert
            _repositoryMock.Verify(r => r.Update<Customer>(It.IsAny<Customer>(), null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region DeleteCustomer Tests

        [Fact]
        public void DeleteCustomer_ExistingCustomer_SoftDeletes()
        {
            // Arrange
            var customer = MockHelpers.CreateTestCustomer(customerId: 1);
            _repositoryMock.SetupGetById(customer, 1);

            // Act
            _customerService.DeleteCustomer(1);

            // Assert
            _repositoryMock.Verify(r => r.Update<Customer>(It.Is<Customer>(c => c.DeletedAt != null), null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region GetCustomerCount Tests

        [Fact]
        public void GetCustomerCount_ReturnsCorrectCount()
        {
            // Arrange
            var customers = MockHelpers.CreateTestCustomers(10);
            _repositoryMock.SetupGet(customers);

            // Act
            var result = _customerService.GetCustomerCount();

            // Assert
            result.Should().Be(10);
        }

        #endregion
    }
}
