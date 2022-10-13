using WebApiNetCore.Models;

namespace WebApiNetCore.Dtos.Gateway
{
    public class UpdateGatewayDto
    {
        public string SerialNumber{ get; set; }
        public string Human_ReadAble_Name { get; set; }
        public string Ipv4_Address { get; set; }
    }
}