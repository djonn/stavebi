﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StaveBi.Database;

#nullable disable

namespace SpellingBee.Migrations
{
    [DbContext(typeof(GameContext))]
    [Migration("20240511101612_AddLemmaFrequency")]
    partial class AddLemmaFrequency
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("StaveBi.Model.Game", b =>
                {
                    b.Property<string>("Letters")
                        .HasColumnType("TEXT");

                    b.Property<int>("TotalScore")
                        .HasColumnType("INTEGER");

                    b.HasKey("Letters");

                    b.HasIndex("Letters");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("StaveBi.Model.WordDetails", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Conjugation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Example")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FullForm")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Lemma")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double?>("LemmaFrequency")
                        .HasColumnType("REAL");

                    b.Property<bool>("Standardized")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.ToTable("Words");
                });
#pragma warning restore 612, 618
        }
    }
}
