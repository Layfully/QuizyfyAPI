using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public class QuizDbContext : DbContext
    {
        public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
        {
        }

        public DbSet<Quiz> Quizzes { get; set; }

        protected override void OnModelCreating(ModelBuilder bldr)
        {
            bldr.Entity<Quiz>()
              .HasData(new
              {
                Id = 1,
                Name = "Quizzserser",
                DateAdded = DateTime.Now
              });

            bldr.Entity<Question>()
              .HasData(new
              {
                  Id = 1,
                  Text = "Entity Framework From Scratch",
                  QuizId = 1
              },
              new
              {
                  Id = 2,
                  Text = "Writing Sample Data Made Easy",
                  QuizId = 1
              });

            bldr.Entity<Choice>()
              .HasData(new
              {
                  Id = 1,
                  Text = "Entity Framework From Scratch",
                  IsRight = false,
                  QuestionId = 1
              },
              new
              {
                  Id = 2,
                  Text = "Writing Sample Data Made Easy",
                  IsRight = true,
                  QuestionId = 1
              },
              new
              {
                  Id = 3,
                  Text = "Writing Sample Data Made Easy",
                  IsRight = false,
                  QuestionId = 2
              },
              new
              {
                  Id = 4,
                  Text = "ANSWER",
                  IsRight = true,
                  QuestionId = 2
              });
        }
    }
}
