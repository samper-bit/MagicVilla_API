using System.Net;
using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers.v1
{
    [Route("api/v{version:apiVersion}/VillaNumberAPI")]
    [ApiController]
    [ApiVersion("1.0", Deprecated = true)]
    public class VillaNumberAPIController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IVillaNumberRepository _dbVillaNumber;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;

        public VillaNumberAPIController(
            IVillaNumberRepository dbVillaNumber,
            IMapper mapper,
            IVillaRepository dbVilla)
        {
            _dbVillaNumber = dbVillaNumber;
            _mapper = mapper;
            _response = new();
            _dbVilla = dbVilla;
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
            try
            {
                IEnumerable<VillaNumber> villaNumberList = await _dbVillaNumber.GetAllAsync(includeProperties: "Villa");

                var responseResult = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);
                _response = CreateResponse(HttpStatusCode.OK, result: responseResult);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response = CreateResponse(HttpStatusCode.BadRequest, [ex.ToString()], isSuccess: false);
            }

            return Ok(_response);
        }

        [HttpGet("{id:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response = CreateResponse(HttpStatusCode.BadRequest, ["Villa Number ID is not valid!"], isSuccess: false);
                    return BadRequest(_response);
                }

                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);

                if (villaNumber == null)
                {
                    _response = CreateResponse(HttpStatusCode.NotFound, ["Villa Number not found!"], isSuccess: false);
                    return NotFound(_response);
                }

                var responseResult = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response = CreateResponse(HttpStatusCode.OK, result: responseResult);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response = CreateResponse(HttpStatusCode.BadRequest, [ex.ToString()], isSuccess: false);
            }

            return BadRequest(_response);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber(VillaNumberCreateDTO createDto)
        {
            try
            {
                if (await _dbVillaNumber.GetAsync(u => u.VillaNo == createDto.VillaNo) != null)
                {
                    _response = CreateResponse(HttpStatusCode.BadRequest, ["Villa Number ID is exists!"], isSuccess: false);
                    return BadRequest(_response);
                }

                if (await _dbVilla.GetAsync(u => u.Id == createDto.VillaId) == null)
                {
                    _response = CreateResponse(HttpStatusCode.BadRequest, ["Villa ID is not valid!"], isSuccess: false);
                    return BadRequest(_response);
                }

                if (createDto == null)
                {
                    _response = CreateResponse(HttpStatusCode.BadRequest, ["Villa Number model is null"], isSuccess: false);
                    return BadRequest(_response);
                }

                VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDto);

                await _dbVillaNumber.CreateAsync(villaNumber);

                var responseResult = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response = CreateResponse(HttpStatusCode.OK, result: responseResult);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response = CreateResponse(HttpStatusCode.BadRequest, [ex.ToString()], isSuccess: false);
            }

            return BadRequest(_response);
        }

        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response = CreateResponse(HttpStatusCode.BadRequest, ["Villa Number ID is not valid!"], isSuccess: false);
                    return BadRequest(_response);
                }

                VillaNumber villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);

                if (villaNumber == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response = CreateResponse(HttpStatusCode.NotFound, ["Villa Number not found!"], isSuccess: false);

                    return NotFound(_response);
                }

                await _dbVillaNumber.RemoveAsync(villaNumber);

                _response = CreateResponse(HttpStatusCode.OK);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response = CreateResponse(HttpStatusCode.BadRequest, [ex.ToString()], isSuccess: false);
            }

            return Ok(_response);
        }

        [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id,
            [FromBody] VillaNumberUpdateDTO updateDto)
        {
            try
            {
                if (id != updateDto.VillaNo)
                {
                    _response = CreateResponse(HttpStatusCode.BadRequest, ["Id does not match with Villa Number"], isSuccess: false);
                    return BadRequest(_response);
                }

                if (await _dbVilla.GetAsync(u => u.Id == updateDto.VillaId) == null)
                {
                    _response = CreateResponse(HttpStatusCode.BadRequest, ["Villa ID is invalid !"], isSuccess: false);
                    return BadRequest(_response);
                }

                VillaNumber villaNumber = _mapper.Map<VillaNumber>(updateDto);

                await _dbVillaNumber.UpdateAsync(villaNumber);

                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response = CreateResponse(HttpStatusCode.BadRequest, [ex.ToString()], isSuccess: false);
            }

            return Ok(_response);
        }

        private APIResponse CreateResponse(HttpStatusCode statusCode, IEnumerable<string> errorMessages = null,
            object result = null, bool isSuccess = true)
        {
            return new APIResponse
            {
                StatusCode = statusCode,
                ErrorMessages = errorMessages?.ToList() ?? [],
                Result = result,
                IsSuccess = isSuccess
            };
        }
    }
}
