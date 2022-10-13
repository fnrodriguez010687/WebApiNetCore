using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiNetCore.Dtos.Gateway;
using WebApiNetCore.Models;
using WebApiNetCore.Services.GatewayService;

namespace WebApiNetCore.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class GatewayController : ControllerBase
    {
        private readonly IGatewayService _GatewayService;

        public GatewayController(IGatewayService GatewayService)
        {
            _GatewayService = GatewayService;

        }

        
        public async Task<IActionResult> Get()
        {
            return Ok( await _GatewayService.GetAllGateways());
        }

        [HttpGet("{serialnumber}")]
        public async Task<IActionResult> GetSingle(string SerialNumber)
        {
            return Ok( await _GatewayService.GetGatewayById(SerialNumber));
        }

        [HttpPost]
        public async Task<IActionResult> AddGateway(AddGatewayDto newGateway)
        {
            
            return Ok( await _GatewayService.AddGateway(newGateway));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateGateway(UpdateGatewayDto updateGateway)
        {
            ServiceResponse<GetGatewayDto> response = await _GatewayService.UpdateGateway(updateGateway);
            if(response.Data == null) {
                return NotFound(response);
            }
            
            return Ok(response);
        }

         [HttpDelete("{serialnumber}")]
        public async Task<IActionResult> DeleteGateway(string SerialNumber)
        {
            ServiceResponse<List<GetGatewayDto>> response = await _GatewayService.DeleteGateway(SerialNumber);
            if(response.Data == null) {
                return NotFound(response);
            }
            
            return Ok(response);
        }
    }
}