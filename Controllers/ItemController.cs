using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models.DTO;
using SMS.Models;
using SMS.Services;

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
        public ActionResult<IEnumerable<GetItemDTO>> GetItems()
        {
            try
            {
                var items = _ItemService.GetAllItems();
                var itemsDTO = _mapper.Map<IEnumerable<GetItemDTO>>(items);
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
                var Item = _ItemService.GetItemById(ItemId);
                if (Item == null)
                {
                    return NotFound();
                }

                var ItemDTO = _mapper.Map<GetItemDTO>(Item);
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

                if (isCustomerExists != null)
                {
                    return NotFound();
                }

                var Item = new Item
                {
                    ItemDescription = request.ItemDescription,
                    ItemCaratage = request.ItemCaratage,
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
    }
}
