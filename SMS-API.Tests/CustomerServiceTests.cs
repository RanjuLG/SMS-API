using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using SMS.Services;
using SMS.Interfaces;
using SMS.Models;
using Microsoft.EntityFrameworkCore; // Needed for Include

namespace SMS.Tests
{
    public class CustomerServiceTests
    {
        private readonly Mock<IRepository> _mockRepo;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _mockRepo = new Mock<IRepository>();
            _customerService = new CustomerService(_mockRepo.Object);
        }

        [Fact]
        public void GetCustomerById_ReturnsCustomer_WhenExists()
        {
            // Arrange
            var customer = new Customer { CustomerId = 1, CustomerName = "Ranju", DeletedAt = null };
            var customers = new List<Customer> { customer }.AsQueryable();

            _mockRepo.Setup(r => r.Get<Customer>(It.IsAny<System.Linq.Expressions.Expression<System.Func<Customer, bool>>>()))
                     .Returns(customers);

            // Act
            var result = _customerService.GetCustomerById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ranju", result.CustomerName);
        }

        [Fact]
        public void GetCustomerById_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var customers = new List<Customer>().AsQueryable();

            _mockRepo.Setup(r => r.Get<Customer>(It.IsAny<System.Linq.Expressions.Expression<System.Func<Customer, bool>>>()))
                     .Returns(customers);

            // Act
            var result = _customerService.GetCustomerById(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAllCustomers_ReturnsCustomersWithinDateRange()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { CustomerId = 1, CustomerName = "Ranju", CreatedAt = new DateTime(2025, 01, 01), DeletedAt = null },
                new Customer { CustomerId = 2, CustomerName = "Kamal", CreatedAt = new DateTime(2025, 02, 01), DeletedAt = null },
                new Customer { CustomerId = 3, CustomerName = "Nimal", CreatedAt = new DateTime(2025, 03, 01), DeletedAt = null }
            }.AsQueryable();

            // Setup mock repo to always return these customers (ignores filter lambda for simplicity)
            _mockRepo.Setup(r => r.Get<Customer>(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
                     .Returns((System.Linq.Expressions.Expression<Func<Customer, bool>> predicate) => customers.Where(predicate));

            var dateRange = new Mock<IDateTimeRange>();
            dateRange.Setup(d => d.From).Returns(new DateTime(2025, 01, 15));
            dateRange.Setup(d => d.To).Returns(new DateTime(2025, 02, 15));

            // Act
            var result = _customerService.GetAllCustomers(dateRange.Object);

            // Assert
            Assert.Single(result); // Only "Kamal" should be in range
            Assert.Equal("Kamal", result[0].CustomerName);
        }
    }
}
