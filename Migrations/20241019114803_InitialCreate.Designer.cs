﻿// <auto-generated />
using System;
using DotNetAssignment2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DotNetAssignment2.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241019114803_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("DotNetAssignment2.Models.TrainingForm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BatchSize")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Epoch")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FeatureSelector")
                        .HasColumnType("TEXT");

                    b.Property<string>("FilePath")
                        .HasColumnType("TEXT");

                    b.Property<string>("Label")
                        .HasColumnType("TEXT");

                    b.Property<string>("Layers")
                        .HasColumnType("TEXT");

                    b.Property<string>("Optimizer")
                        .HasColumnType("TEXT");

                    b.Property<string>("TypeOfTraining")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TrainingForms");
                });

            modelBuilder.Entity("DotNetAssignment2.Models.UploadedFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UploadTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("UploadedFiles");
                });
#pragma warning restore 612, 618
        }
    }
}
