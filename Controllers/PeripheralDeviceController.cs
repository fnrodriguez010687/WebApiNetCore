using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiNetCore.Dtos.Device;
using WebApiNetCore.Models;
using WebApiNetCore.Services.PeripheralDeviceService;

namespace WebApiNetCore.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PeripheralDeviceController : ControllerBase
    {
        private readonly IPeripheralDeviceService _deviceService;

        public PeripheralDeviceController(IPeripheralDeviceService deviceService )
        {
            _deviceService = deviceService;
        }

        

        
        [HttpGet("{uid}")]
        public async Task<IActionResult> GetSingle(int uid)
        {
            return Ok( await _deviceService.GetDeviceById(uid));
        }
        [HttpGet("getall/{serialnumber}")]
        public async Task<IActionResult> Get(string serialnumber)
        {
            return Ok( await _deviceService.GetAllDeviceByGateway(serialnumber));
        }

       [HttpPost]
        public async Task<IActionResult> AddDevice(AddDeviceDto newDevice){
            return Ok(await _deviceService.AddDevice(newDevice));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDevice(UpdateDeviceDto updateDevice)
        {
            ServiceResponse<GetDeviceDto> response = await _deviceService.UpdateDevice(updateDevice);
            if(response.Data == null) {
                return NotFound(response);
            }
            
            return Ok(response);
        }

         [HttpDelete("{uid}")]
        public async Task<IActionResult> DeleteDevice(int uid)
        {
            ServiceResponse<List<GetDeviceDto>> response = await _deviceService.DeleteDevice(uid);
            if(response.Data == null) {
                return NotFound(response);
            }
            
            return Ok(response);
        }
    }
}