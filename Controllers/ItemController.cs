using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models.DTO;
using SMS.Models;
using SMS.Services;
using SMS.Generic;

namespace SMS.Controllers
{
    [Route("api/items")]
    [ApiController]
    public class ItemController : Controller
    {
        private readonly IItemService _ItemService;
        private readonly IMapper _mapper;
        private readonly ICustomerService _customerService;
        private readonly IPaginationService _paginationService;

        public ItemController(IItemService ItemService, IMapper mapper, ICustomerService customerService, IPaginationService paginationService)
        {
            _ItemService = ItemService;
            _mapper = mapper;
            _customerService = customerService;
            _paginationService = paginationService;
        }

        [HttpGet]
        [Route("")]
        public ActionResult<IEnumerable<GetItemDTO>> GetItems([FromQuery] DateTimeRange dataParams)
        {
            try
            {
                var items = _ItemService.GetAllItems(dataParams);
                var itemsDTO = items.Select(item => new GetItemDTO
                {
                    ItemId = item.ItemId,
                    CustomerId = item.CustomerId ?? 0,
                    CustomerName = item.Customer?.CustomerName,
                    CustomerNIC = item.Customer?.CustomerNIC,
                    ItemDescription = item.ItemDescription,
                    ItemRemarks = item.ItemRemarks,
                    ItemCaratage = item.ItemCaratage,
                    ItemWeight = item.ItemWeight,
                    ItemGoldWeight = item.ItemGoldWeight,
                    ItemValue = item.ItemValue,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt
                }).ToList();

                return Ok(itemsDTO);
            }
            catch (Exception)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("paginated")]
        public async Task<ActionResult<PaginatedResponse<GetItemDTO>>> GetItemsPaginated(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "desc",
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? customerNIC = null,
            [FromQuery] decimal? minValue = null,
            [FromQuery] decimal? maxValue = null)
        {
            try
            {
                var request = new ItemSearchRequest
                {
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    From = from,
                    To = to,
                    CustomerNIC = customerNIC,
                    MinValue = minValue,
                    MaxValue = maxValue
                };

                // Get queryable items
                var itemsQuery = _ItemService.GetItemsQueryable(request);

                // Project to DTO
                var itemsDTOQuery = itemsQuery.Select(item => new GetItemDTO
                {
                    ItemId = item.ItemId,
                    CustomerId = item.CustomerId ?? 0,
                    CustomerName = item.Customer != null ? item.Customer.CustomerName : null,
                    CustomerNIC = item.Customer != null ? item.Customer.CustomerNIC : null,
                    ItemDescription = item.ItemDescription,
                    ItemRemarks = item.ItemRemarks,
                    ItemCaratage = item.ItemCaratage,
                    ItemWeight = item.ItemWeight,
                    ItemGoldWeight = item.ItemGoldWeight,
                    ItemValue = item.ItemValue,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt
                });

                // Create applied filters object for metadata
                var appliedFilters = new
                {
                    customerNIC,
                    minValue,
                    maxValue,
                    DateRange = from.HasValue || to.HasValue 
                        ? new { From = from, To = to } 
                        : null
                };

                // Create paginated response
                var paginatedResponse = await _paginationService.CreatePaginatedResponseAsync(
                    itemsDTOQuery, 
                    request, 
                    appliedFilters);

                return Ok(paginatedResponse);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<PaginatedResponse<GetItemDTO>>> SearchItems([FromBody] ItemSearchRequest request)
        {
            try
            {
                // Get queryable items
                var itemsQuery = _ItemService.GetItemsQueryable(request);

                // Project to DTO
                var itemsDTOQuery = itemsQuery.Select(item => new GetItemDTO
                {
                    ItemId = item.ItemId,
                    CustomerId = item.CustomerId ?? 0,
                    CustomerName = item.Customer != null ? item.Customer.CustomerName : null,
                    CustomerNIC = item.Customer != null ? item.Customer.CustomerNIC : null,
                    ItemDescription = item.ItemDescription,
                    ItemRemarks = item.ItemRemarks,
                    ItemCaratage = item.ItemCaratage,
                    ItemWeight = item.ItemWeight,
                    ItemGoldWeight = item.ItemGoldWeight,
                    ItemValue = item.ItemValue,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt
                });

                // Create applied filters object for metadata
                var appliedFilters = new
                {
                    request.CustomerNIC,
                    request.MinValue,
                    request.MaxValue,
                    DateRange = request.From.HasValue || request.To.HasValue 
                        ? new { request.From, request.To } 
                        : null
                };

                // Create paginated response
                var paginatedResponse = await _paginationService.CreatePaginatedResponseAsync(
                    itemsDTOQuery, 
                    request, 
                    appliedFilters);

                return Ok(paginatedResponse);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }




        [HttpGet]
        [Route("{ItemId}/Item")]
        public ActionResult<GetItemDTO> GetItemById(int ItemId)
        {
            try
            {
                var item = _ItemService.GetItemById(ItemId);
                if (item == null)
                {
                    return NotFound();
                }

                var ItemDTO = new GetItemDTO 
                {
                    ItemId = item.ItemId,
                    CustomerId = item.CustomerId ?? 0,
                    CustomerName = item.Customer?.CustomerName,
                    CustomerNIC = item.Customer?.CustomerNIC,
                    ItemDescription = item.ItemDescription,
                    ItemRemarks = item.ItemRemarks,
                    ItemCaratage = item.ItemCaratage,
                    ItemWeight = item.ItemWeight,
                    ItemGoldWeight = item.ItemGoldWeight,
                    ItemValue = item.ItemValue,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt
                };
                return Ok(ItemDTO);
            }
            catch (Exception)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("")]
        public ActionResult<CreateItemDTO> CreateItem([FromBody] CreateItemDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CustomerNIC))
                {
                    return BadRequest("Customer NIC is required.");
                }

                var isCustomerExists = _customerService.GetCustomerByNIC(request.CustomerNIC);

                if (isCustomerExists == null)
                {
                    return NotFound("Customer not found.");
                }

                var Item = new Item
                {
                    ItemDescription = request.ItemDescription,
                    ItemRemarks = request.ItemRemarks,
                    ItemCaratage = request.ItemCaratage,
                    ItemWeight = request.ItemWeight,
                    ItemGoldWeight = request.ItemGoldWeight,
                    ItemValue = request.ItemValue,
                    Status = request.Status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CustomerId = isCustomerExists?.CustomerId
                };

                _ItemService.CreateItem(Item);
                //  var responseDTO = _mapper.Map<CreateItemDTO>(response);

                return Ok();
            }
            catch (Exception)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPut]
        [Route("{ItemId}/Item")]
        public ActionResult UpdateItem(int ItemId, [FromBody] UpdateItemDTO request)
        {
            try
            {
                // Fetch the existing item
                var existingItem = _ItemService.GetItemById(ItemId);
                if (existingItem == null)
                {
                    return NotFound();
                }

                if (string.IsNullOrWhiteSpace(request.CustomerNIC))
                {
                    return BadRequest("Customer NIC is required.");
                }

                // Fetch the customer by NIC
                var customer = _customerService.GetCustomerByNIC(request.CustomerNIC);
                if (customer == null)
                {
                    return BadRequest("Customer with the provided NIC does not exist.");
                }

                // Update properties
                existingItem.ItemDescription = request.ItemDescription;
                existingItem.ItemRemarks = request.ItemRemarks;
                existingItem.ItemCaratage = request.ItemCaratage;
                existingItem.ItemWeight = request.ItemWeight;
                existingItem.ItemGoldWeight = request.ItemGoldWeight;
                existingItem.ItemValue = request.ItemValue;
                existingItem.Status = request.Status;
                existingItem.CustomerId = customer.CustomerId; // Update the customer ID
                existingItem.UpdatedAt = DateTime.Now;

                // Save the updated item
                _ItemService.UpdateItem(existingItem);

                return Ok();
            }
            catch (Exception)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpDelete]
        [Route("{ItemId}/Item")]
        public ActionResult DeleteItem(int ItemId)
        {
            try
            {
                var existingItem = _ItemService.GetItemById(ItemId);
                if (existingItem == null)
                {
                    return NotFound();
                }

                _ItemService.DeleteItem(ItemId);

                return Ok();
            }
            catch (Exception)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete-multiple")]
        public IActionResult DeleteMultipleItems([FromBody] List<int> itemIds)
        {
            if (itemIds == null || itemIds.Count == 0)
            {
                return BadRequest("No item IDs provided.");
            }

            _ItemService.DeleteItems(itemIds);

            return Ok();
        }
        [HttpGet("customer/{customerNIC}")]
        public ActionResult<IEnumerable<GetItemDTO>> GetItemsByCustomerNIC(string customerNIC)
        {
            try
            {
                var customer = _customerService.GetCustomerByNIC(customerNIC);
                if (customer == null)
                {
                    return NotFound("Customer not found.");
                }

                var items = _ItemService.GetItemsByCustomerId(customer.CustomerId);
                var itemsDTO = items.Select(item => new GetItemDTO
                {
                    ItemId = item.ItemId,
                    CustomerId = item.CustomerId ?? 0,
                    CustomerName = item.Customer?.CustomerName,
                    CustomerNIC = item.Customer?.CustomerNIC,
                    ItemDescription = item.ItemDescription,
                    ItemRemarks = item.ItemRemarks,
                    ItemCaratage = item.ItemCaratage,
                    ItemWeight = item.ItemWeight,
                    ItemGoldWeight = item.ItemGoldWeight,
                    ItemValue = item.ItemValue,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt
                }).ToList();

                return Ok(itemsDTO);
            }
            catch (Exception)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
