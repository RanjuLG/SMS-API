using AutoMapper;
using Castle.Core.Resource;
using Microsoft.AspNetCore.Mvc;
using SMS.Enums;
using SMS.Generic;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using System;
using System.Collections.Generic;

namespace SMS.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public CustomerController(ICustomerService customerService, IMapper mapper, IConfiguration configuration)
        {
            _customerService = customerService;
            _mapper = mapper;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("")]
        public ActionResult<IEnumerable<GetCustomerDTO>> GetCustomers([FromQuery] DateTimeRange dataParams)
        {
            try
            {
                var customers = _customerService.GetAllCustomers(dataParams);
                var customersDTO = _mapper.Map<IEnumerable<GetCustomerDTO>>(customers);
                return Ok(customersDTO);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
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
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
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
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPost]
        [Route("")]
        public ActionResult<CreateCustomerDTO> CreateCustomer([FromForm] CreateCustomerDTO request, IFormFile? nicPhoto)
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
                    // Ensure the uploads directory exists
                    /*
                    var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "NICPhotos");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    */

                    // Get the directory path from configuration
                    var directoryPath = _configuration["FileSettings:NicUploadFolderPath"];

                    // Generate a unique filename to avoid conflicts
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(nicPhoto.FileName);
                    var filePath = Path.Combine(directoryPath, uniqueFileName);

                    // Save the file locally in wwwroot directory
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        nicPhoto.CopyTo(stream);
                    }

                    // Construct the public URL by removing the path up to the "uploads" folder
                    var uploadsFolderIndex = directoryPath.IndexOf("uploads", StringComparison.OrdinalIgnoreCase);
                    var relativePath = directoryPath.Substring(uploadsFolderIndex).Replace("\\", "/");
                    var publicUrl = $"/{relativePath}/{uniqueFileName}";
                    customer.NICPhotoPath = publicUrl;  // Store the URL, not the local path
                }

                _customerService.CreateCustomer(customer);
              //  var responseDTO = _mapper.Map<CreateCustomerDTO>(response);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("byNIC")]
        public ActionResult<IEnumerable<GetCustomerDTO>> GetCustomersByNIC([FromBody] string customerNIC)
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
                    return NotFound("Customer doesnot exist");
                }

                var customerDto = new GetCustomerDTO()
                {
                    CustomerName = customer.CustomerName,
                    CustomerAddress = customer.CustomerAddress,
                    CustomerId = customer.CustomerId,
                    CustomerContactNo = customer.CustomerContactNo,
                    CustomerNIC = customer.CustomerNIC,
                    NICPhotoPath = customer.NICPhotoPath,
                    CreatedAt = DateTime.Now,
                };

               // var customerDTOs = _mapper.Map<IEnumerable<GetCustomerDTO>>(customer);

                
                return Ok(customerDto);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut]
        [Route("{customerId}/customer")]
        public ActionResult UpdateCustomer(int customerId, [FromForm] CreateCustomerDTO request, IFormFile? nicPhoto)
        {
            try
            {
                var existingCustomer = _customerService.GetCustomerById(customerId);
                if (existingCustomer == null)
                {
                    return NotFound("Customer doesnot exist");
                }

                var isNewNicExists = _customerService.GetCustomerByNIC(request.CustomerNIC);

                if (isNewNicExists != null && request.CustomerNIC != existingCustomer.CustomerNIC)
                {
                    return BadRequest("Customer already exists!");
                }
                // Update properties
                existingCustomer.CustomerNIC = request.CustomerNIC;
                existingCustomer.CustomerName = request.CustomerName;
                existingCustomer.CustomerAddress = request.CustomerAddress;
                existingCustomer.CustomerContactNo = request.CustomerContactNo;
                if (nicPhoto != null && nicPhoto.Length > 0)
                {
                    // Ensure the uploads directory exists
                    /*
                    var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "NICPhotos");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    */

                    var directoryPath = _configuration["FileSettings:NicUploadFolderPath"];

                    // Generate a unique filename to avoid conflicts
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(nicPhoto.FileName);
                    var filePath = Path.Combine(directoryPath, uniqueFileName);

                    // Save the file locally in wwwroot directory
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        nicPhoto.CopyTo(stream);
                    }

                    // Save the web-accessible file path in the customer record
                    var uploadsFolderIndex = directoryPath.IndexOf("uploads", StringComparison.OrdinalIgnoreCase);
                    var relativePath = directoryPath.Substring(uploadsFolderIndex).Replace("\\", "/");
                    var publicUrl = $"/{relativePath}/{uniqueFileName}";
                    existingCustomer.NICPhotoPath = publicUrl;  // Store the URL, not the local path
                }
                else if (nicPhoto == null && nicPhoto.Length == 0)
                {
                    existingCustomer.NICPhotoPath = null;

                }
                _customerService.UpdateCustomer(existingCustomer);
               
                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
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

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete-multiple")]
        public IActionResult DeleteMultipleCustomers([FromBody] List<int> customerIds)
        {
            if (customerIds == null || customerIds.Count == 0)
            {
                return BadRequest("No customer IDs provided.");
            }

            _customerService.DeleteCustomers(customerIds);

            return Ok();
        }

    }
}
