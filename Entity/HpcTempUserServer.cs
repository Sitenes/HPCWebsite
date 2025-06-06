using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class HpcTempUserServer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public HpcUser? User { get; set; }

        public int ServerId { get; set; }
        [ForeignKey(nameof(ServerId))]
        public HpcServer? Server { get; set; }

        public int? WorkflowUserId { get; set; }
    }

}
