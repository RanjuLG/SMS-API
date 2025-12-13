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
    public class ItemServiceTests
    {
        private readonly Mock<IRepository> _repositoryMock;
        private readonly Mock<IReadOnlyRepository> _readOnlyRepositoryMock;
        private readonly ItemService _itemService;

        public ItemServiceTests()
        {
            _repositoryMock = MockHelpers.CreateMockRepository();
            _readOnlyRepositoryMock = new Mock<IReadOnlyRepository>();
            _itemService = new ItemService(_repositoryMock.Object, _readOnlyRepositoryMock.Object);
        }

        #region GetAllItems Tests

        [Fact]
        public void GetAllItems_WithValidDateRange_ReturnsAllNonDeletedItems()
        {
            // Arrange
            var items = MockHelpers.CreateTestItems(5);
            _repositoryMock.SetupGet(items);
            var dateRange = new DateTimeRange { From = DateTime.Now.AddDays(-30), To = DateTime.Now };

            // Act
            var result = _itemService.GetAllItems(dateRange);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }

        [Fact]
        public void GetAllItems_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock.SetupGet(new List<Item>());
            var dateRange = new DateTimeRange { From = DateTime.Now.AddDays(-30), To = DateTime.Now };

            // Act
            var result = _itemService.GetAllItems(dateRange);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetItemById Tests

        [Fact]
        public void GetItemById_ExistingItem_ReturnsItem()
        {
            // Arrange
            var item = MockHelpers.CreateTestItem(itemId: 1);
            _repositoryMock.SetupGet(new List<Item> { item });

            // Act
            var result = _itemService.GetItemById(1);

            // Assert
            result.Should().NotBeNull();
            result.ItemId.Should().Be(1);
        }

        [Fact]
        public void GetItemById_NonExistingItem_ReturnsNull()
        {
            // Arrange
            _repositoryMock.SetupGet(new List<Item>());

            // Act
            var result = _itemService.GetItemById(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateItem Tests

        [Fact]
        public void CreateItem_ValidItem_CallsRepository()
        {
            // Arrange
            var item = MockHelpers.CreateTestItem();

            // Act
            _itemService.CreateItem(item);

            // Assert
            _repositoryMock.Verify(r => r.Create<Item>(item, null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
            _repositoryMock.Verify(r => r.CommitTransaction(), Times.Once);
        }

        #endregion

        #region UpdateItem Tests

        [Fact]
        public void UpdateItem_ExistingItem_UpdatesFields()
        {
            // Arrange
            var existingItem = MockHelpers.CreateTestItem(itemId: 1);
            _repositoryMock.SetupGetById(existingItem, 1);

            var updatedItem = MockHelpers.CreateTestItem(itemId: 1, description: "Updated Description");

            // Act
            _itemService.UpdateItem(updatedItem);

            // Assert
            _repositoryMock.Verify(r => r.Update<Item>(It.IsAny<Item>(), null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region DeleteItem Tests

        [Fact]
        public void DeleteItem_ExistingItem_SoftDeletes()
        {
            // Arrange
            var item = MockHelpers.CreateTestItem(itemId: 1);
            _repositoryMock.SetupGetById(item, 1);

            // Act
            _itemService.DeleteItem(1);

            // Assert
            _repositoryMock.Verify(r => r.Update<Item>(It.Is<Item>(i => i.DeletedAt != null), null), Times.Once);
            _repositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region GetItemsByCustomerId Tests

        [Fact]
        public void GetItemsByCustomerId_ExistingCustomer_ReturnsItems()
        {
            // Arrange
            var items = MockHelpers.CreateTestItems(3, customerId: 1);
            _repositoryMock.SetupGet(items);

            // Act
            var result = _itemService.GetItemsByCustomerId(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
        }

        #endregion

        #region GetInventoryCount Tests

        [Fact]
        public void GetInventoryCount_ReturnsCountOfInStockItems()
        {
            // Arrange
            var items = new List<Item>
            {
                MockHelpers.CreateTestItem(itemId: 1, status: (int)ItemStatus.InStock),
                MockHelpers.CreateTestItem(itemId: 2, status: (int)ItemStatus.InStock),
                MockHelpers.CreateTestItem(itemId: 3, status: (int)ItemStatus.Redeemed),
            };
            _repositoryMock.SetupGet(items);

            // Act
            var result = _itemService.GetInventoryCount();

            // Assert
            result.Should().Be(2);
        }

        #endregion
    }
}
