using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApiNetCore.Data;
using WebApiNetCore.Dtos.Device;
using WebApiNetCore.Dtos.Gateway;
using WebApiNetCore.Models;

namespace WebApiNetCore.Services.PeripheralDeviceService
{
    public class PeripheralDeviceService : IPeripheralDeviceService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public PeripheralDeviceService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        private int GetUserId()=>int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<GetDeviceDto>> AddDevice(AddDeviceDto newDevice)
        {
            ServiceResponse<GetDeviceDto> response = new ServiceResponse<GetDeviceDto>();
            try{
                Gateway gateway = await _context.Gateways.FirstOrDefaultAsync(c => c.SerialNumber == newDevice.GatewayId &&
                c.User.Id == GetUserId());
                if(gateway == null){
                    response.Success = false;
                    response.Message = "Gateway not found.";
                    return response;
                }
                PeripheralDevice device = new PeripheralDevice{
                    Vendor = newDevice.Vendor,
                    DateCreated = DateTime.Now,
                    Status = (StatusDevice)Convert.ToInt32(newDevice.Status),
                    Gateway = gateway
                };
                //device=_mapper.Map<PeripheralDevice>(newDevice);
                await _context.PeripheralDevices.AddAsync(device);
                await _context.SaveChangesAsync();
            //PeripheralDevice dbDevice = await _context.PeripheralDevices
             //   .FirstOrDefaultAsync(c => c.Vendor == newDevice.Vendor && c.Gateway.User.Id == GetUserId());
            response.Data = _mapper.Map<GetDeviceDto>(device);
               // response.Data = (_context.PeripheralDevices.Where(c=>c.Gateway.User.Id == GetUserId()).Select(c => _mapper.Map<GetDeviceDto>(c))).ToList();
            }catch(Exception ex){
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<List<GetDeviceDto>>> DeleteDevice(int UID)
        {
            ServiceResponse<List<GetDeviceDto>> serviceResponse = new ServiceResponse<List<GetDeviceDto>>();
            try
            {
                PeripheralDevice Device = await _context.PeripheralDevices.
                FirstOrDefaultAsync(c => c.UId == UID);
                if(Device != null){
                _context.PeripheralDevices.Remove(Device);
                await _context.SaveChangesAsync();
                serviceResponse.Data = (_context.PeripheralDevices
                .Where(c => c.Gateway.User.Id == GetUserId())
                .Select(c => _mapper.Map<GetDeviceDto>(c))).ToList();
                }
                else{
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Gateway not found";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetDeviceDto>>> GetAllDeviceByGateway(string SerialNumber)
        {
             ServiceResponse<List<GetDeviceDto>> serviceResponse = new ServiceResponse<List<GetDeviceDto>>();
            List<PeripheralDevice> dbDevices = await _context.PeripheralDevices.Where(c =>c.GatewayId == SerialNumber && c.Gateway.User.Id == GetUserId()).ToListAsync();
            serviceResponse.Data = (dbDevices.Select(c => _mapper.Map<GetDeviceDto>(c))).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetDeviceDto>> GetDeviceById(int UID)
        {
           ServiceResponse<GetDeviceDto> serviceResponse = new ServiceResponse<GetDeviceDto>();
            PeripheralDevice dbDevice = await _context.PeripheralDevices
            .FirstOrDefaultAsync(c => c.UId == UID && c.Gateway.User.Id == GetUserId());
            serviceResponse.Data = _mapper.Map<GetDeviceDto>(dbDevice);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetDeviceDto>> UpdateDevice(UpdateDeviceDto updateDevice)
        {
            ServiceResponse<GetDeviceDto> serviceResponse = new ServiceResponse<GetDeviceDto>();
            try
            {
                PeripheralDevice device = await _context.PeripheralDevices.Include( b => b.Gateway).Include( c => c.Gateway.User).FirstOrDefaultAsync(c => c.UId == updateDevice.UId);
                
                    
                if(device.Gateway.User.Id == GetUserId()){
                    device.Vendor = updateDevice.Vendor;
                    device.Status = (StatusDevice)Convert.ToInt32(updateDevice.Status); 
                    if(device.GatewayId != updateDevice.GatewayId)
                        device.Gateway = await _context.Gateways.Where(x => x.SerialNumber == updateDevice.GatewayId).FirstOrDefaultAsync();
                _context.PeripheralDevices.Update(device);
                await _context.SaveChangesAsync();

                serviceResponse.Data = _mapper.Map<GetDeviceDto>(device);
                }
                else{
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Gateway not found";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }    
    }
}