using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using System;
using System.Collections.Generic;

namespace SMS.Controllers
{
    [Route("api/installments")]
    [ApiController]
    public class InstallmentController : ControllerBase
    {
        private readonly IInstallmentService _installmentService;
        private readonly IMapper _mapper;

        public InstallmentController(IInstallmentService installmentService, IMapper mapper)
        {
            _installmentService = installmentService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("")]
        public ActionResult<IEnumerable<GetInstallmentDTO>> GetInstallments()
        {
            try
            {
                var installments = _installmentService.GetAllInstallments();
                var installmentDTOs = _mapper.Map<IEnumerable<GetInstallmentDTO>>(installments);
                return Ok(installmentDTOs);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("{installmentId}")]
        public ActionResult<GetInstallmentDTO> GetInstallmentById(int installmentId)
        {
            try
            {
                var installment = _installmentService.GetInstallmentById(installmentId);
                if (installment == null)
                {
                    return NotFound();
                }

                var installmentDTO = _mapper.Map<GetInstallmentDTO>(installment);
                return Ok(installmentDTO);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("transaction/{transactionId}")]
        public ActionResult<IEnumerable<GetInstallmentDTO>> GetInstallmentsByTransactionId(int transactionId)
        {
            try
            {
                var installments = _installmentService.GetInstallmentsByTransactionId(transactionId);
                if (installments == null || installments.Count == 0)
                {
                    return NotFound();
                }

                var installmentDTOs = _mapper.Map<IEnumerable<Installment>>(installments);
                return Ok(installmentDTOs);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("")]
        public ActionResult CreateInstallment([FromBody] CreateInstallmentDTO request)
        {
            try
            {
                var installment = _mapper.Map<Installment>(request);
                _installmentService.CreateInstallment(installment);
                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Route("{installmentId}")]
        public ActionResult UpdateInstallment(int installmentId, [FromBody] UpdateInstallmentDTO request)
        {
            try
            {
                var existingInstallment = _installmentService.GetInstallmentById(installmentId);
                if (existingInstallment == null)
                {
                    return NotFound();
                }

                _mapper.Map(request, existingInstallment);
                _installmentService.UpdateInstallment(existingInstallment);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Route("{installmentId}")]
        public ActionResult DeleteInstallment(int installmentId)
        {
            try
            {
                var existingInstallment = _installmentService.GetInstallmentById(installmentId);
                if (existingInstallment == null)
                {
                    return NotFound();
                }

                _installmentService.DeleteInstallment(installmentId);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete-multiple")]
        public IActionResult DeleteMultipleInstallments([FromBody] List<int> installmentIds)
        {
            if (installmentIds == null || installmentIds.Count == 0)
            {
                return BadRequest("No installment IDs provided.");
            }

            _installmentService.DeleteInstallments(installmentIds);

            return Ok();
        }


        [HttpGet("invoice/{invoiceNumber}")]
        public ActionResult<IEnumerable<GetInstallmentDTO>> GetInstallmentsByInvoiceNumber(string invoiceNumber)
        {
            try
            {
                var installments = _installmentService.GetInstallmentsByInitialInvoiceNumber(invoiceNumber);
                if (installments == null || installments.Count == 0)
                {
                    return NotFound("No installments found for the given invoice number.");
                }

                var installmentDTOs = _mapper.Map<IEnumerable<GetInstallmentDTO>>(installments);
                return Ok(installmentDTOs);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }



    }
}
