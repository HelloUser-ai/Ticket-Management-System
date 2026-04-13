using Microsoft.EntityFrameworkCore;
using TMS201.Models;

namespace TMS201.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets yaha add kar
        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<TicketBackup> TicketBackups { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Client>Clients { get; set; }

        public DbSet<TicketAttachment> TicketAttachments { get; set; }


    }
}