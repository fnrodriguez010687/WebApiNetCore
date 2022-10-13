using AutoMapper;
using WebApiNetCore.Dtos;
using WebApiNetCore.Dtos.Device;
using WebApiNetCore.Dtos.Gateway;
using WebApiNetCore.Models;

namespace WebApiNetCore
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Gateway, GetGatewayDto>();
            CreateMap<AddGatewayDto, Gateway>();
            CreateMap<PeripheralDevice, GetDeviceDto>();
            CreateMap<AddDeviceDto, PeripheralDevice>();
        }
    }
}