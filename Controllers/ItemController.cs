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

        public ItemController(IItemService ItemService, IMapper mapper,ICustomerService customerService)
        {
            _ItemService = ItemService;
            _mapper = mapper;
            _customerService = customerService;
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
                    ItemDescription = item.ItemDescription,
                    ItemCaratage = item.ItemCaratage,
                    ItemWeight = item.ItemWeight,
                    ItemGoldWeight = item.ItemGoldWeight,
                    ItemValue = item.ItemValue,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt,
                    CustomerNIC = item.Customer != null ? item.Customer.CustomerNIC : null // Assuming Customer has an NIC property
                }).ToList();

                return Ok(itemsDTO);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
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
                    ItemDescription = item.ItemDescription,
                    ItemCaratage = item.ItemCaratage,
                    ItemWeight = item.ItemWeight,
                    ItemGoldWeight = item.ItemGoldWeight,
                    ItemValue = item.ItemValue,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt,
                    CustomerNIC = item.Customer != null ? item.Customer.CustomerNIC : null


                };
                return Ok(ItemDTO);
            }
            catch (Exception ex)
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
                var isCustomerExists = _customerService.GetCustomerByNIC(request.CustomerNIC);

                if (isCustomerExists == null)
                {
                    return NotFound();
                }

                var Item = new Item
                {
                    ItemDescription = request.ItemDescription,
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
            catch (Exception ex)
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

                // Fetch the customer by NIC
                var customer = _customerService.GetCustomerByNIC(request.CustomerNIC);
                if (customer == null)
                {
                    return BadRequest("Customer with the provided NIC does not exist.");
                }

                // Update properties
                existingItem.ItemDescription = request.ItemDescription;
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
            catch (Exception ex)
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
            catch (Exception ex)
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
                    ItemDescription = item.ItemDescription,
                    ItemCaratage = item.ItemCaratage,
                    ItemWeight = item.ItemWeight,
                    ItemGoldWeight = item.ItemGoldWeight,
                    ItemValue = item.ItemValue,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt,
                    CustomerNIC = item.Customer != null ? item.Customer.CustomerNIC : null // Assuming Customer has an NIC property
                }).ToList();

                return Ok(itemsDTO);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
