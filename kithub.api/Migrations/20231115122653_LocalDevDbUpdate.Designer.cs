﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using kithub.api.Data;

#nullable disable

namespace kithub.api.Migrations
{
    [DbContext(typeof(KithubDbContext))]
    [Migration("20231115122653_LocalDevDbUpdate")]
    partial class LocalDevDbUpdate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("kithub.api.Entities.Cart", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Carts");
                });

            modelBuilder.Entity("kithub.api.Entities.CartItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CartId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Qty")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("CartItems");
                });

            modelBuilder.Entity("kithub.api.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageURL")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Qty")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Products");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CategoryId = 1,
                            Description = "Best quality whole Jeera",
                            ImageURL = "/Images/SpicesW/Jeera1.png",
                            Name = "Cumin Seed Gold",
                            Price = 270m,
                            Qty = 500
                        },
                        new
                        {
                            Id = 2,
                            CategoryId = 1,
                            Description = "Best quality whole Fennel seeds",
                            ImageURL = "/Images/SpicesW/Fennel1.png",
                            Name = "Fennel Seeds Gold",
                            Price = 150m,
                            Qty = 500
                        },
                        new
                        {
                            Id = 3,
                            CategoryId = 1,
                            Description = "Premium quality whole Jeera seeds",
                            ImageURL = "/Images/SpicesW/Jeera2.png",
                            Name = "Cumin Seeds Premium",
                            Price = 250m,
                            Qty = 500
                        },
                        new
                        {
                            Id = 4,
                            CategoryId = 2,
                            Description = "Premium quality Jeera powder",
                            ImageURL = "/Images/SpicesP/Jeera1.png",
                            Name = "Cumin Powder",
                            Price = 200m,
                            Qty = 500
                        },
                        new
                        {
                            Id = 5,
                            CategoryId = 3,
                            Description = "Best quality Kasuri Methi",
                            ImageURL = "/Images/Herbs/MethiLeaves1.png",
                            Name = "Kasuri Methi",
                            Price = 150m,
                            Qty = 500
                        });
                });

            modelBuilder.Entity("kithub.api.Entities.ProductCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("IconCSS")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ProductCategories");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            IconCSS = "fas fa-clover",
                            Name = "Whole Spices"
                        },
                        new
                        {
                            Id = 2,
                            IconCSS = "fas fa-mortar-pestle",
                            Name = "Powder Spices"
                        },
                        new
                        {
                            Id = 3,
                            IconCSS = "fas fa-leaf",
                            Name = "Herbs"
                        });
                });

            modelBuilder.Entity("kithub.api.Entities.Product", b =>
                {
                    b.HasOne("kithub.api.Entities.ProductCategory", "ProductCategory")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProductCategory");
                });
#pragma warning restore 612, 618
        }
    }
}
