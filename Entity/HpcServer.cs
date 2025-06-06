using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{

    public class HpcServer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ServerType Type { get; set; } // CPU یا GPU
        public string Specifications { get; set; }
        public decimal DailyPrice { get; set; }
        public int Cores { get; set; }
        public int RamGB { get; set; }
        public int StorageGB { get; set; }
        public string GpuType { get; set; }
        public int GpuCount { get; set; }
        public string Bandwidth { get; set; }
        public bool HasFirewall { get; set; }
        public bool HasRootAccess { get; set; }
        public string SupportLevel { get; set; }
        public string Uptime { get; set; }
        public string OpenstackFlavorName { get; set; }
        public string DeliveryTime { get; set; }
        public bool IsPopular { get; set; }
        public string SuitableFor { get; set; } // فیلد جدید
        public string ProcessingSpeed { get; set; } // فیلد جدید
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? WorkflowUserId { get; set; }
    }

    public enum ServerType
    {
        CPU = 0,
        GPU = 1
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

        public List<HpcServer> Servers { get; set; }

        public int? WorkflowUserId { get; set; }
    }
}
