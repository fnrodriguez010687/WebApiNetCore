using WebApiNetCore.Models;

namespace WebApiNetCore.Dtos.Device
{
    public class UpdateDeviceDto
    {
        public int UId { get; set; }
        public string Vendor { get; set; }       
        public string GatewayId { get; set; }
        public string Status { get; set; }
    }
}