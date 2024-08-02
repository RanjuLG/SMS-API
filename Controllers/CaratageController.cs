using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMS.Controllers
{
    [Route("api/caratages")]
    [ApiController]
    public class CaratageController : Controller
    {
        private readonly ICaratageService _caratageService;
        private readonly IMapper _mapper;

        public CaratageController(ICaratageService caratageService, IMapper mapper)
        {
            _caratageService = caratageService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("")]
        public ActionResult<IEnumerable<GetCaratageDTO>> GetCaratages()
        {
            try
            {
                var caratages = _caratageService.GetAllCaratages();
                var caratagesDTO = _mapper.Map<IEnumerable<GetCaratageDTO>>(caratages);
                return Ok(caratagesDTO);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("{caratageId}")]
        public ActionResult<GetCaratageDTO> GetCaratageById(int caratageId)
        {
            try
            {
                var caratage = _caratageService.GetCaratageById(caratageId);
                if (caratage == null)
                {
                    return NotFound();
                }

                var caratageDTO = _mapper.Map<GetCaratageDTO>(caratage);
                return Ok(caratageDTO);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("")]
        public ActionResult<CreateCaratageDTO> CreateCaratage([FromBody] CreateCaratageDTO request)
        {
            try
            {
                var caratage = new GoldCaratage
                {
                    Caratage = request.Caratage,
                    //AmountPerCaratage = request.AmountPerCaratage,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _caratageService.CreateCaratage(caratage);

                var responseDTO = _mapper.Map<CreateCaratageDTO>(caratage);

                return Ok(responseDTO);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Route("{caratageId}")]
        public ActionResult UpdateCaratage(int caratageId, [FromBody] UpdateCaratageDTO request)
        {
            try
            {
                var existingCaratage = _caratageService.GetCaratageById(caratageId);
                if (existingCaratage == null)
                {
                    return NotFound();
                }

                existingCaratage.Caratage = request.Caratage;
               // existingCaratage.AmountPerCaratage = request.AmountPerCaratage;
                existingCaratage.UpdatedAt = DateTime.Now;

                _caratageService.UpdateCaratage(existingCaratage);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Route("{caratageId}")]
        public ActionResult DeleteCaratage(int caratageId)
        {
            try
            {
                var existingCaratage = _caratageService.GetCaratageById(caratageId);
                if (existingCaratage == null)
                {
                    return NotFound();
                }

                _caratageService.DeleteCaratage(caratageId);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework for this)
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete-multiple")]
        public IActionResult DeleteMultipleCaratages([FromBody] List<int> caratageIds)
        {
            if (caratageIds == null || caratageIds.Count == 0)
            {
                return BadRequest("No caratage IDs provided.");
            }

            _caratageService.DeleteCaratages(caratageIds);

            return Ok();
        }
    }
}
