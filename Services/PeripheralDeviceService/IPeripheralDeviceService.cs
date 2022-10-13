using WebApiNetCore.Dtos.Device;
using WebApiNetCore.Dtos.Gateway;
using WebApiNetCore.Models;

namespace WebApiNetCore.Services.PeripheralDeviceService{
    public interface IPeripheralDeviceService{
        Task<ServiceResponse<List<GetDeviceDto>>> GetAllDeviceByGateway(string SerialNumber);
        //Task<ServiceResponse<GetGatewayDto>> AddDevice(AddDeviceDto newDevice);
        Task<ServiceResponse<GetDeviceDto>> AddDevice(AddDeviceDto newDevice);
        Task<ServiceResponse<GetDeviceDto>> GetDeviceById(int UID);
        
        Task<ServiceResponse<GetDeviceDto>> UpdateDevice(UpdateDeviceDto  updateDevice);
        Task<ServiceResponse<List<GetDeviceDto>>> DeleteDevice(int UID);
    }
}