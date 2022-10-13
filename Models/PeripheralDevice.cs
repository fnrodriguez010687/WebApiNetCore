using System.ComponentModel.DataAnnotations;

namespace WebApiNetCore.Models
{
    public class PeripheralDevice {
        [Key]
        public int UId { get; set; }
        public string Vendor { get; set; }
        public DateTime DateCreated { get; set; }
        public Gateway Gateway { get; set; }
        public string GatewayId { get; set; }
        public StatusDevice Status { get; set; }
    }
}