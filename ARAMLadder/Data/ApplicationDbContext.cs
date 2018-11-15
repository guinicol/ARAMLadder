using System;
using System.Collections.Generic;
using System.Text;
using ARAMLadder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ARAMLadder.Data
{
    public class ApplicationDbContext : IdentityDbContext<AramIdentityUser, IdentityRole, string>
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<LoginGame> LoginGames { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
