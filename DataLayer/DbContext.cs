using Entities.Models.Workflows;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.DbContext
{
    public class Context : Microsoft.EntityFrameworkCore.DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }

        public DbSet<Workflow> Workflow { get; set; }
        public DbSet<Workflow_User> Workflow_User { get; set; }
       
    }
}