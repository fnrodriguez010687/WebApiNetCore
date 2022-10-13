using System.ComponentModel.DataAnnotations;

namespace WebApiNetCore.Models
{
    public class Gateway
    {
        [Key]
        public string SerialNumber{ get; set; }
        public string Human_ReadAble_Name { get; set; }
        public string Ipv4_Address { get; set; }
        public User User { get; set; }
        public List<PeripheralDevice> Devices { get; set; }

    }
}