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

        public DbSet<Item> Items { get; set; }
        public DbSet<LoginGameItem> LoginGameItems { get; set; }

        public DbSet<Champion> Champions { get; set; }

        public DbSet<Rune> Runes { get; set; }
        public DbSet<LoginGameRune> LoginGameRunes { get; set; }

        public DbSet<Spell> Spells { get; set; }
        public DbSet<LoginGameSpell> LoginGameSpells { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
