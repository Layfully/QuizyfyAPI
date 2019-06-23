﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace QuizyfyAPI.Data
{
    public class QuizRepository : IQuizRepository
    {

        private readonly QuizDbContext _context;
        private readonly ILogger<QuizRepository> _logger;

        public QuizRepository(QuizDbContext context, ILogger<QuizRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Add<T>(T entity) where T : class
        {
            _logger.LogInformation($"Adding an object of type {entity.GetType()} to the context.");
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _logger.LogInformation($"Removing an object of type {entity.GetType()} to the context.");
            _context.Remove(entity);
        }
        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");

            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
        }

        public async Task<Quiz[]> GetAllQuizzesAsync(bool includeQuestions = false)
        {
            _logger.LogInformation($"Getting all Camps");

            IQueryable<Quiz> query = _context.Quizzes;

            if (includeQuestions)
            {
                _logger.LogInformation($"With questions");
            }

            return await query.ToArrayAsync();
        }

        public Task<Quiz> GetQuizAsync(string name, bool includeTalks = false)
        {
            throw new NotImplementedException();
        }


    }
}
