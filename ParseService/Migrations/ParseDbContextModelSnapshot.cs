﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParseService.Data;

#nullable disable

namespace ParseService.Migrations
{
    [DbContext(typeof(ParseDbContext))]
    partial class ParseDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("ParseService.Models.AnnouncementItem", b =>
                {
                    b.Property<int>("AnnId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AnnDesc")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AnnTitle")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AnnUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("CTime")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("AnnId");

                    b.ToTable("Announcements");
                });
#pragma warning restore 612, 618
        }
    }
}
