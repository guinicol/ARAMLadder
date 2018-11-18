﻿// <auto-generated />
using System;
using ARAMLadder.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ARAMLadder.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20181118115330_AddRunesAndSpells")]
    partial class AddRunesAndSpells
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ARAMLadder.Models.AramIdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.Property<long>("riotId");

                    b.Property<string>("riotName");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("ARAMLadder.Models.Champion", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Icon");

                    b.Property<string>("Key");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Champions");
                });

            modelBuilder.Entity("ARAMLadder.Models.Game", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("GameCreation");

                    b.Property<int>("GameDuration");

                    b.Property<long>("GameId");

                    b.Property<int>("KillAlly");

                    b.Property<int>("KillEnnemy");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("ARAMLadder.Models.Item", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Icon");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("ARAMLadder.Models.LoginGame", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AramIdentityUserId");

                    b.Property<int>("Assists");

                    b.Property<int?>("ChampionId");

                    b.Property<int>("Deaths");

                    b.Property<int>("DoubleKills");

                    b.Property<bool>("FirstBloodKill");

                    b.Property<long>("GamesId");

                    b.Property<int>("Kills");

                    b.Property<int>("PentaKills");

                    b.Property<int>("QuadraKills");

                    b.Property<int>("TripleKills");

                    b.Property<bool>("Win");

                    b.HasKey("Id");

                    b.HasIndex("AramIdentityUserId");

                    b.HasIndex("ChampionId");

                    b.HasIndex("GamesId");

                    b.ToTable("LoginGames");
                });

            modelBuilder.Entity("ARAMLadder.Models.LoginGameItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ItemId");

                    b.Property<int>("LoginGameId");

                    b.HasKey("Id");

                    b.HasIndex("ItemId");

                    b.HasIndex("LoginGameId");

                    b.ToTable("LoginGameItems");
                });

            modelBuilder.Entity("ARAMLadder.Models.LoginGameRune", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("LoginGameId");

                    b.Property<int>("Position");

                    b.Property<int>("RuneId");

                    b.HasKey("Id");

                    b.HasIndex("LoginGameId");

                    b.HasIndex("RuneId");

                    b.ToTable("LoginGameRunes");
                });

            modelBuilder.Entity("ARAMLadder.Models.LoginGameSpell", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("LoginGameId");

                    b.Property<int>("Position");

                    b.Property<int>("SpellId");

                    b.HasKey("Id");

                    b.HasIndex("LoginGameId");

                    b.HasIndex("SpellId");

                    b.ToTable("LoginGameSpells");
                });

            modelBuilder.Entity("ARAMLadder.Models.Rune", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Icon");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Runes");
                });

            modelBuilder.Entity("ARAMLadder.Models.Spell", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Icon");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Spells");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .HasMaxLength(128);

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("ARAMLadder.Models.LoginGame", b =>
                {
                    b.HasOne("ARAMLadder.Models.AramIdentityUser", "AramIdentityUser")
                        .WithMany()
                        .HasForeignKey("AramIdentityUserId");

                    b.HasOne("ARAMLadder.Models.Champion", "Champion")
                        .WithMany()
                        .HasForeignKey("ChampionId");

                    b.HasOne("ARAMLadder.Models.Game", "Games")
                        .WithMany("LoginGames")
                        .HasForeignKey("GamesId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ARAMLadder.Models.LoginGameItem", b =>
                {
                    b.HasOne("ARAMLadder.Models.Item", "Item")
                        .WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ARAMLadder.Models.LoginGame", "LoginGame")
                        .WithMany("Items")
                        .HasForeignKey("LoginGameId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ARAMLadder.Models.LoginGameRune", b =>
                {
                    b.HasOne("ARAMLadder.Models.LoginGame", "LoginGame")
                        .WithMany("Runes")
                        .HasForeignKey("LoginGameId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ARAMLadder.Models.Rune", "Rune")
                        .WithMany()
                        .HasForeignKey("RuneId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ARAMLadder.Models.LoginGameSpell", b =>
                {
                    b.HasOne("ARAMLadder.Models.LoginGame", "LoginGame")
                        .WithMany("Spells")
                        .HasForeignKey("LoginGameId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ARAMLadder.Models.Spell", "Spell")
                        .WithMany()
                        .HasForeignKey("SpellId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("ARAMLadder.Models.AramIdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("ARAMLadder.Models.AramIdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ARAMLadder.Models.AramIdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("ARAMLadder.Models.AramIdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}