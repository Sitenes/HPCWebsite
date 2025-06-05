using Entities.Models.MainEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models.Workflows
{
    public class Workflow
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        #region relations
        public List<Workflow_User>? workflowUser { get; set; }
        #endregion
    }
}