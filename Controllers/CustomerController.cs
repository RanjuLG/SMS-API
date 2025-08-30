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
        public ActionResult<PaginatedResponse<GetCustomerDTO>> GetCustomers([FromQuery] CustomerSearchRequest request)
        {
            try
            {
                // For now, use the existing service method but we'll need to enhance it for pagination
                var dateRange = new SMS.Generic.DateTimeRange 
                { 
                    From = request.From, 
                    To = request.To 
                };
                
                var customers = _customerService.GetAllCustomers(dateRange);
                var customersDTO = _mapper.Map<IEnumerable<GetCustomerDTO>>(customers);
                
                // Apply search if provided
                if (!string.IsNullOrEmpty(request.Search))
                {
                    customersDTO = customersDTO.Where(c => 
                        (c.CustomerName != null && c.CustomerName.Contains(request.Search, StringComparison.OrdinalIgnoreCase)) ||
                        c.CustomerNIC.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                        (c.CustomerAddress != null && c.CustomerAddress.Contains(request.Search, StringComparison.OrdinalIgnoreCase)) ||
                        (c.CustomerContactNo != null && c.CustomerContactNo.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                    );
                }

                // Apply NIC filter if provided
                if (!string.IsNullOrEmpty(request.CustomerNIC))
                {
                    customersDTO = customersDTO.Where(c => c.CustomerNIC.Contains(request.CustomerNIC, StringComparison.OrdinalIgnoreCase));
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    customersDTO = request.SortBy.ToLower() switch
                    {
                        "customername" => request.SortOrder?.ToLower() == "desc" 
                            ? customersDTO.OrderByDescending(c => c.CustomerName) 
                            : customersDTO.OrderBy(c => c.CustomerName),
                        "customernic" => request.SortOrder?.ToLower() == "desc" 
                            ? customersDTO.OrderByDescending(c => c.CustomerNIC) 
                            : customersDTO.OrderBy(c => c.CustomerNIC),
                        "createdat" => request.SortOrder?.ToLower() == "desc" 
                            ? customersDTO.OrderByDescending(c => c.CreatedAt) 
                            : customersDTO.OrderBy(c => c.CreatedAt),
                        _ => customersDTO.OrderBy(c => c.CustomerName)
                    };
                }

                // Apply pagination manually (until we enhance the service layer)
                var totalItems = customersDTO.Count();
                var pagedCustomers = customersDTO
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var pagination = _paginationService.CreatePaginationMetadata(request.Page, request.PageSize, totalItems);
                var filters = _paginationService.CreateFilterMetadata(request, new { request.CustomerNIC });

                var response = new PaginatedResponse<GetCustomerDTO>
                {
                    Data = pagedCustomers,
                    Pagination = pagination,
                    Filters = filters
                };

                return Ok(response);
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
