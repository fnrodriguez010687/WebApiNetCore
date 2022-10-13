using WebApiNetCore.Dtos.Gateway;
using WebApiNetCore.Models;

namespace WebApiNetCore.Services.GatewayService
{
    public interface IGatewayService
    {
          Task<ServiceResponse<List<GetGatewayDto>>> GetAllGateways();
          Task<ServiceResponse<GetGatewayDto>> GetGatewayById(string SerialNumber);
          Task<ServiceResponse<GetGatewayDto>> AddGateway(AddGatewayDto newGateway);
          Task<ServiceResponse<GetGatewayDto>> UpdateGateway(UpdateGatewayDto  updateGateway);
          Task<ServiceResponse<List<GetGatewayDto>>> DeleteGateway(string SerialNumber);
    }
}