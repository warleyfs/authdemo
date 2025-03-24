using AuthorizationServer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationServer.API.Data;

// Constructor accepting DbContextOptions and passing them to the base class.
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // Override OnModelCreating to configure entity properties and relationships.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure the UserRole entity as a join table for User and Role.
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId }); // Composite primary key.
        
        // Defines the many-to-many relationship between User and Role.
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);
        
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
        
        // Configure the UserSession entity as a join table for User, SignKey and Client.
        modelBuilder.Entity<UserSession>()
            .HasKey(us => new { us.UserId, us.KeyId, us.ClientId }); // Composite primary key.
        
        modelBuilder.Entity<UserSession>()
            .HasKey(us => new { us.Id, us.UserId, us.KeyId, us.ClientId });
        
        // Defines the one-to-many relationship between User and UserSession.
        modelBuilder.Entity<UserSession>()
            .HasOne(us => us.User)
            .WithMany(u => u.UserSessions)
            .HasForeignKey(us => us.UserId);
        
        // Defines the one-to-many relationship between SigningKey and UserSession.
        modelBuilder.Entity<UserSession>()
            .HasOne(us => us.Key)
            .WithMany(u => u.UserSessions)
            .HasForeignKey(us => us.KeyId);
        
        // Defines the one-to-many relationship between Client and UserSession.
        modelBuilder.Entity<UserSession>()
            .HasOne(us => us.Client)
            .WithMany(u => u.UserSessions)
            .HasForeignKey(us => us.ClientId);
        
        // Seed initial data for Roles, Users, Clients, and UserRoles.
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Admin Role" },
            new Role { Id = 2, Name = "Editor", Description = "Editor Role" },
            new Role { Id = 3, Name = "User", Description = "User Role" }
        );
        
        modelBuilder.Entity<Client>().HasData(
            new Client
            {
                Id = 1,
                ClientId = "Client1",
                Name = "Client Application 1",
                ClientURL = "https://client1.com"
            },
            new Client
            {
                Id = 2,
                ClientId = "Client2",
                Name = "Client Application 2",
                ClientURL = "https://client2.com"
            }
        );
    }

    // DbSet representing the Users table.
    public DbSet<User> Users { get; set; }

    // DbSet representing the Roles table.
    public DbSet<Role> Roles { get; set; }

    // DbSet representing the Clients table.
    public DbSet<Client> Clients { get; set; }

    // DbSet representing the UserRoles join table.
    public DbSet<UserRole> UserRoles { get; set; }

    // DbSet representing the SigningKeys table.
    public DbSet<SigningKey> SigningKeys { get; set; }
    
    // DbSet representing the User Sessions table.
    public DbSet<UserSession> UserSessions { get; set; }
}