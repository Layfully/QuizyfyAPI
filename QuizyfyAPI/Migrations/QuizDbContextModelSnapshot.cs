﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuizyfyAPI.Data;

namespace QuizyfyAPI.Migrations
{
    [DbContext(typeof(QuizDbContext))]
    partial class QuizDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("QuizyfyAPI.Data.Choice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsRight");

                    b.Property<int?>("QuestionId");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("Choice");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            IsRight = false,
                            QuestionId = 1,
                            Text = "Entity Framework From Scratch"
                        },
                        new
                        {
                            Id = 2,
                            IsRight = true,
                            QuestionId = 1,
                            Text = "Writing Sample Data Made Easy"
                        },
                        new
                        {
                            Id = 3,
                            IsRight = false,
                            QuestionId = 2,
                            Text = "Writing Sample Data Made Easy"
                        },
                        new
                        {
                            Id = 4,
                            IsRight = true,
                            QuestionId = 2,
                            Text = "ANSWER"
                        });
                });

            modelBuilder.Entity("QuizyfyAPI.Data.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("QuizId");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.HasIndex("QuizId");

                    b.ToTable("Question");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            QuizId = 1,
                            Text = "Entity Framework From Scratch"
                        },
                        new
                        {
                            Id = 2,
                            QuizId = 1,
                            Text = "Writing Sample Data Made Easy"
                        });
                });

            modelBuilder.Entity("QuizyfyAPI.Data.Quiz", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateAdded");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Quizzes");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            DateAdded = new DateTime(2019, 6, 23, 22, 9, 5, 101, DateTimeKind.Local).AddTicks(9527),
                            Name = "Quizzserser"
                        });
                });

            modelBuilder.Entity("QuizyfyAPI.Data.Choice", b =>
                {
                    b.HasOne("QuizyfyAPI.Data.Question")
                        .WithMany("Choices")
                        .HasForeignKey("QuestionId");
                });

            modelBuilder.Entity("QuizyfyAPI.Data.Question", b =>
                {
                    b.HasOne("QuizyfyAPI.Data.Quiz")
                        .WithMany("Questions")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
