using Entities.Models.MainEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models.Workflows
{
    public class Workflow_User
    {
        public int Id { get; set; }
        public string? WorkflowState { get; set; }
        
        #region relations
        public int UserId { get; set; }

        public int WorkflowId { get; set; }
        [ForeignKey(nameof(WorkflowId))]
        public Workflow? Workflow { get; set; }
        #endregion
    }
}
