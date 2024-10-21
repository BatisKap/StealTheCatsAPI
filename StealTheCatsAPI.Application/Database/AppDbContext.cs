using Microsoft.EntityFrameworkCore;
using StealTheCatsAPI.Application.Models;
using System;

namespace StealTheCatsAPI.Application.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
       : base(options)
        {
        }
        public DbSet<Cat> Cats { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cat>(entity =>
            {
                modelBuilder.Entity<Cat>()
                   .HasKey(c => c.Id);

                modelBuilder.Entity<Cat>()
                    .Property(c => c.Id)
                    .UseIdentityColumn();  
                                          
                entity.Property(e => e.CatId)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Width)
                    .IsRequired();

                entity.Property(e => e.Height)
                      .IsRequired();

                entity.Property(e => e.Image)
                      .IsRequired();

                entity.Property(e => e.Created)
                      .HasColumnType("datetime2(7)")
                      .HasDefaultValueSql("GETUTCDATE()")
                      .IsRequired();

                entity.HasIndex(e => e.CatId)
                      .IsUnique()
                      .HasDatabaseName("IX_Cat_CatId");

                entity.ToTable(tb => tb.HasCheckConstraint("CK_CatEntity_Width_Height", "[Width] >= 100 AND " +
                    "[Width] <= 9000 AND [Height] >= 100 AND [Height] <= 9000"));

            });
            modelBuilder.Entity<Tag>(entity =>
            {
                modelBuilder.Entity<Tag>()
                   .HasKey(c => c.Id);

                modelBuilder.Entity<Tag>()
                            .Property(c => c.Id)
                            .UseIdentityColumn();  // IDENTITY(1,1)
               
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Created)
                      .HasColumnType("datetime2(7)")
                      .HasDefaultValueSql("GETUTCDATE()")
                      .IsRequired();

                modelBuilder.Entity<Tag>()
                    .HasIndex(t => t.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Tag_Name_Unique");
            });

            // many-to-many relationship between Cat and Tag
            modelBuilder.Entity<Cat>()
                .HasMany(c => c.Tags)
                .WithMany(t => t.Cats)
                .UsingEntity(j => j.ToTable("CatTags"));

        }
    }
}
