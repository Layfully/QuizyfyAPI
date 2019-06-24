using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizyfyAPI.Data;
using QuizyfyAPI.Models;

namespace QuizyfyAPI.Controllers
{
    [Route("api/quizzes/{quizId}/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuizRepository _repository;
        private readonly IMapper _mapper;

        public QuestionsController(IQuizRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<QuestionModel[]>> Get(int quizId)
        {
            var questions = await _repository.GetQuestionsByIdAsync(quizId, true);

            return _mapper.Map<QuestionModel[]>(questions);
        }

        [HttpGet("{questionId}")]
        public async Task<ActionResult<QuestionModel>> Get(int quizId, int questionId)
        {
            var question = await _repository.GetQuestionByIdAsync(quizId, questionId, true);

            if (question == null)
            {
                return NotFound("Couldn't find question");
            }

            return _mapper.Map<QuestionModel>(question);
        }

        [HttpPost]
        public async Task<ActionResult<QuestionModel>> Post(int quizId, QuestionModel model)
        {
            var quiz = await _repository.GetQuizAsync(quizId);

            if (quiz == null)
            {
                return BadRequest("Camp doesn't exists");
            }

            var question = _mapper.Map<Question>(model);

            question.QuizId = quiz.Id;

            _repository.Add(question);

            if (await _repository.SaveChangesAsync())
            {
                return CreatedAtAction(nameof(Get), _mapper.Map<QuestionModel>(question));
            }

            return BadRequest("Failed to save new question");
        }

        [HttpPut("{questionId}")]
        public async Task<ActionResult<QuestionModel>> Put(int quizId, int questionId, QuestionModel model)
        {
            var oldQuestion = await _repository.GetQuestionByIdAsync(quizId, questionId, true);

            if (oldQuestion == null)
            {
                return NotFound();
            }

            _mapper.Map(model, oldQuestion);

            if (model.Choices != null)
            {
                var choices = await _repository.GetChoicesForQuestion(questionId);
                if (choices != null)
                {
                    oldQuestion.Choices = choices;
                }
            }

            if (await _repository.SaveChangesAsync())
            {
                return _mapper.Map<QuestionModel>(oldQuestion);
            }

            return BadRequest();
        }

        [HttpDelete("{questionId}")]
        public async Task<IActionResult> Delete(int quizId, int questionId)
        {
            var question = await _repository.GetQuestionByIdAsync(quizId, questionId, true);

            if (question == null)
            {
                return NotFound("Failed to find the question to delete");
            }

            _repository.Delete(question);

            if (await _repository.SaveChangesAsync())
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}