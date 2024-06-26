﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StaveBi.Database;

#nullable disable

namespace SpellingBee.Migrations
{
    [DbContext(typeof(GameContext))]
    [Migration("20240509143101_RemoveWordsTable")]
    partial class RemoveWordsTable
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
#pragma warning restore 612, 618
        }
    }
}
