using System.Security.Claims;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApiNetCore.Data;
using WebApiNetCore.Dtos.Gateway;
using WebApiNetCore.Models;

namespace WebApiNetCore.Services.GatewayService
{
    public class GatewayService : IGatewayService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GatewayService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _mapper = mapper;

        }

        private int GetUserId()=>int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        public async Task<ServiceResponse<GetGatewayDto>> AddGateway(AddGatewayDto newGateway)
        {
            var regexAgencia = new Regex(@"^((25[0-5]|(2[0-4]|1[0-9]|[1-9]|)[0-9])(\.(?!$)|$)){4}$");
            ServiceResponse<GetGatewayDto> serviceResponse = new ServiceResponse<GetGatewayDto>(); 
            if(regexAgencia.IsMatch(newGateway.Ipv4_Address)){            
            Guid guid = Guid.NewGuid ();
            Gateway Gateway = _mapper.Map<Gateway>(newGateway);
            Gateway.User = await _context.Users.FirstOrDefaultAsync( u => u.Id == GetUserId());
            Gateway.SerialNumber = guid.ToString();


            await _context.Gateways.AddAsync(Gateway);
            await _context.SaveChangesAsync();
            Gateway dbGateway = await _context.Gateways
                                                       .FirstOrDefaultAsync(c => c.Human_ReadAble_Name == newGateway.Human_ReadAble_Name && c.User.Id == GetUserId());
            serviceResponse.Data =  _mapper.Map<GetGatewayDto>(dbGateway);
            }
            else{
                serviceResponse.Success = false;
                serviceResponse.Message = "IPv4 Wrong";
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetGatewayDto>>> DeleteGateway(string SerialNumber)
        {
            ServiceResponse<List<GetGatewayDto>> serviceResponse = new ServiceResponse<List<GetGatewayDto>>();
            try
            {
                Gateway Gateway = await _context.Gateways.
                FirstOrDefaultAsync(c => c.SerialNumber.Equals(SerialNumber)
                                         && c.User.Id == GetUserId());
                if(Gateway != null){
                _context.Gateways.Remove(Gateway);
                await _context.SaveChangesAsync();
                serviceResponse.Data = (_context.Gateways
                .Where(c => c.User.Id == GetUserId())
                .Select(c => _mapper.Map<GetGatewayDto>(c))).ToList();
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

        public async Task<ServiceResponse<List<GetGatewayDto>>> GetAllGateways()
        {
            ServiceResponse<List<GetGatewayDto>> serviceResponse = new ServiceResponse<List<GetGatewayDto>>();
            List<Gateway> dbGateways = await _context.Gateways.Where(c => c.User.Id == GetUserId()).ToListAsync();
            serviceResponse.Data = (dbGateways.Select(c => _mapper.Map<GetGatewayDto>(c))).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetGatewayDto>> GetGatewayById(string serialNumber)
        {
            ServiceResponse<GetGatewayDto> serviceResponse = new ServiceResponse<GetGatewayDto>();
            Gateway dbGateway = await _context.Gateways.Include(c=>c.Devices)
                                                       .FirstOrDefaultAsync(c => c.SerialNumber == serialNumber && c.User.Id == GetUserId());
            serviceResponse.Data = _mapper.Map<GetGatewayDto>(dbGateway);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetGatewayDto>> UpdateGateway(UpdateGatewayDto updateGateway)
        {
            ServiceResponse<GetGatewayDto> serviceResponse = new ServiceResponse<GetGatewayDto>();
            var regexAgencia = new Regex("(?:[0-9]{1,3}[.]){3}[0-9]{1,3}");
            if(regexAgencia.IsMatch(updateGateway.Ipv4_Address)){
            try
            {
                Gateway Gateway = await _context.Gateways.Include( c => c.User).FirstOrDefaultAsync(c => c.SerialNumber == updateGateway.SerialNumber);
                if(Gateway.User.Id == GetUserId()){
                Gateway.Human_ReadAble_Name = updateGateway.Human_ReadAble_Name;
                Gateway.Ipv4_Address = updateGateway.Ipv4_Address;                

                _context.Gateways.Update(Gateway);
                await _context.SaveChangesAsync();

                serviceResponse.Data = _mapper.Map<GetGatewayDto>(Gateway);
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
            }
            else{
                serviceResponse.Success = false;
                serviceResponse.Message = "IPv4 Wrong";
            }
            return serviceResponse;
        }
    }
}