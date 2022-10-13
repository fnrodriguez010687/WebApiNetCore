using WebApiNetCore.Models;

namespace WebApiNetCore.Dtos.Device
{
    public class GetDeviceDto
    {
        public int UId { get; set; }
        public string Vendor { get; set; }
        public DateTime DateCreated { get; set; }
        public string GatewayId { get; set; }
        public StatusDevice Status { get; set; }
    }
}