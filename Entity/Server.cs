using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
   

    public class Server
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public ServerType Type { get; set; }

        [Required]
        public string Specifications { get; set; }

        [Required]
        public decimal DailyPrice { get; set; }

        [Required]
        public int Cores { get; set; }

        [Required]
        public int RamGB { get; set; }

        [Required]
        public int StorageGB { get; set; }

        [Required]
        public string GpuType { get; set; } = "None";

        [Required]
        public int GpuCount { get; set; } = 0;

        [Required]
        public string Bandwidth { get; set; } = "1 Gb/s";

        [Required]
        public bool HasFirewall { get; set; } = true;

        [Required]
        public bool HasRootAccess { get; set; } = true;

        [Required]
        public string SupportLevel { get; set; }

        [Required]
        public string Uptime { get; set; } = "99.9%";

        [Required]
        public string DeliveryTime { get; set; } = "کمتر از ۱۵ دقیقه";

        public bool IsPopular { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum ServerType
    {
        CPU,
        GPU
    }

    public class ServerCategory
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string IconClass { get; set; }

        public List<Server> Servers { get; set; }
    }
}
