using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Models;
using SMS.Models.DTO;
using SMS.Interfaces;

namespace SMS.Controllers
{
    [Route("api/customers")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IPaginationService _paginationService;

        public CustomerController(
            ICustomerService customerService, 
            IMapper mapper, 
            IConfiguration configuration,
            IPaginationService paginationService)
        {
            _customerService = customerService;
            _mapper = mapper;
            _configuration = configuration;
            _paginationService = paginationService;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<PaginatedResponse<GetCustomerDTO>>> GetCustomers([FromQuery] CustomerSearchRequest request)
        {
            try
            {
                // Get the filtered and sorted query from the service
                var query = await _customerService.GetCustomersQueryAsync(request);
                
                // Convert to DTO query for pagination
                var dtoQuery = query.Select(c => new GetCustomerDTO
                {
                    CustomerId = c.CustomerId,
                    CustomerNIC = c.CustomerNIC,
                    CustomerName = c.CustomerName,
                    CustomerAddress = c.CustomerAddress,
                    CustomerContactNo = c.CustomerContactNo,
                    CreatedAt = c.CreatedAt,
                    NICPhotoPath = c.NICPhotoPath
                });

                // Apply pagination using the pagination service
                var appliedFilters = new 
                { 
                    CustomerNIC = request.CustomerNIC,
                    DateRange = new { From = request.From, To = request.To }
                };

                var response = await _paginationService.CreatePaginatedResponseAsync(
                    dtoQuery, 
                    request, 
                    appliedFilters
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet]
        [Route("search")]
        public async Task<ActionResult<IEnumerable<GetCustomerDTO>>> SearchCustomers([FromQuery] string query, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return BadRequest("Search query cannot be empty");
                }

                var searchRequest = new CustomerSearchRequest
                {
                    Search = query,
                    PageSize = limit,
                    Page = 1,
                    From = DateTime.MinValue,
                    To = DateTime.MaxValue
                };

                var customerQuery = await _customerService.GetCustomersQueryAsync(searchRequest);
                var customers = customerQuery.Take(limit).ToList();
                var customerDTOs = _mapper.Map<IEnumerable<GetCustomerDTO>>(customers);

                return Ok(customerDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet]
        [Route("count")]
        public ActionResult<object> GetCustomerCount()
        {
            try
            {
                var count = _customerService.GetCustomerCount();
                return Ok(new { totalCustomers = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet]
        [Route("{customerId}/customer")]
        public ActionResult<GetCustomerDTO> GetCustomerById(int customerId)
        {
            try
            {
                var customer = _customerService.GetCustomerById(customerId);
                if (customer == null)
                {
                    return NotFound();
                }

                var customerDTO = _mapper.Map<GetCustomerDTO>(customer);
                return Ok(customerDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        [Route("byIds")]
        public ActionResult<IEnumerable<GetCustomerDTO>> GetCustomersByIds([FromBody] int[] customerIds)
        {
            try
            {
                if (customerIds == null || customerIds.Length == 0)
                {
                    return BadRequest("Customer IDs cannot be null or empty");
                }

                var customers = _customerService.GetCustomersByIds(customerIds);
                if (customers == null || !customers.Any())
                {
                    return NotFound();
                }

                var customerDTOs = _mapper.Map<IEnumerable<GetCustomerDTO>>(customers);
                return Ok(customerDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        [Route("")]
        public ActionResult CreateCustomer([FromForm] CreateCustomerDTO request, IFormFile? nicPhoto)
        {
            try
            {
                var isCustomerExists = _customerService.GetCustomerByNIC(request.CustomerNIC);
                if (isCustomerExists != null)
                {
                    return BadRequest("Customer already exists!");
                }

                var customer = _mapper.Map<Customer>(request);
                if (nicPhoto != null && nicPhoto.Length > 0)
                {
                    var directoryPath = _configuration["FileSettings:NicUploadFolderPath"];
                    if (string.IsNullOrEmpty(directoryPath))
                    {
                        return StatusCode(500, new { message = "Upload directory not configured" });
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(nicPhoto.FileName);
                    var filePath = Path.Combine(directoryPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        nicPhoto.CopyTo(stream);
                    }

                    var uploadsFolderIndex = directoryPath.IndexOf("uploads", StringComparison.OrdinalIgnoreCase);
                    var relativePath = directoryPath.Substring(uploadsFolderIndex).Replace("\\", "/");
                    var publicUrl = $"/{relativePath}/{uniqueFileName}";
                    customer.NICPhotoPath = publicUrl;
                }

                _customerService.CreateCustomer(customer);
                return Ok(new { message = "Customer created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        [Route("byNIC")]
        public ActionResult<GetCustomerDTO> GetCustomerByNIC([FromBody] string customerNIC)
        {
            try
            {
                if (string.IsNullOrEmpty(customerNIC))
                {
                    return BadRequest("Customer NIC cannot be null or empty");
                }

                var customer = _customerService.GetCustomerByNIC(customerNIC);
                if (customer == null)
                {
                    return NotFound("Customer does not exist");
                }

                var customerDto = _mapper.Map<GetCustomerDTO>(customer);
                return Ok(customerDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut]
        [Route("{customerId}/customer")]
        public ActionResult UpdateCustomer(int customerId, [FromForm] UpdateCustomerDTO request, IFormFile? nicPhoto)
        {
            try
            {
                var existingCustomer = _customerService.GetCustomerById(customerId);
                if (existingCustomer == null)
                {
                    return NotFound("Customer does not exist");
                }

                var isNewNicExists = _customerService.GetCustomerByNIC(request.CustomerNIC);
                if (isNewNicExists != null && request.CustomerNIC != existingCustomer.CustomerNIC)
                {
                    return BadRequest("Customer with this NIC already exists!");
                }

                // Update properties
                existingCustomer.CustomerNIC = request.CustomerNIC;
                existingCustomer.CustomerName = request.CustomerName;
                existingCustomer.CustomerAddress = request.CustomerAddress;
                existingCustomer.CustomerContactNo = request.CustomerContactNo;
                existingCustomer.UpdatedAt = DateTime.Now;

                if (nicPhoto != null && nicPhoto.Length > 0)
                {
                    var directoryPath = _configuration["FileSettings:NicUploadFolderPath"];
                    if (string.IsNullOrEmpty(directoryPath))
                    {
                        return StatusCode(500, new { message = "Upload directory not configured" });
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(nicPhoto.FileName);
                    var filePath = Path.Combine(directoryPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        nicPhoto.CopyTo(stream);
                    }

                    var uploadsFolderIndex = directoryPath.IndexOf("uploads", StringComparison.OrdinalIgnoreCase);
                    var relativePath = directoryPath.Substring(uploadsFolderIndex).Replace("\\", "/");
                    var publicUrl = $"/{relativePath}/{uniqueFileName}";
                    existingCustomer.NICPhotoPath = publicUrl;
                }

                _customerService.UpdateCustomer(existingCustomer);
                return Ok(new { message = "Customer updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpDelete]
        [Route("{customerId}/customer")]
        public ActionResult DeleteCustomer(int customerId)
        {
            try
            {
                var existingCustomer = _customerService.GetCustomerById(customerId);
                if (existingCustomer == null)
                {
                    return NotFound();
                }

                _customerService.DeleteCustomer(customerId);
                return Ok(new { message = "Customer deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpDelete("delete-multiple")]
        public IActionResult DeleteMultipleCustomers([FromBody] List<int> customerIds)
        {
            try
            {
                if (customerIds == null || customerIds.Count == 0)
                {
                    return BadRequest("No customer IDs provided.");
                }

                _customerService.DeleteCustomers(customerIds);
                return Ok(new { message = "Customers deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
