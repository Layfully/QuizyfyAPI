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
    [Route("api/quizzes/{quizId}/questions/{questionId}/[controller]")]
    [ApiController]
    public class ChoicesController : ControllerBase
    {
        private readonly IQuizRepository _repository;
        private readonly IMapper _mapper;

        public ChoicesController(IQuizRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ChoiceModel[]>> Get(int quizId, int questionId)
        {
            var choices = await _repository.GetChoices(quizId, questionId);

            return _mapper.Map<ChoiceModel[]>(choices);
        }

        [HttpGet("{choiceId}")]
        public async Task<ActionResult<ChoiceModel>> Get(int quizId, int questionId, int choiceId)
        {
            var choice = await _repository.GetChoice(quizId, questionId, choiceId);

            if (choice == null)
            {
                return NotFound("Couldn't find choice");
            }

            return _mapper.Map<ChoiceModel>(choice);
        }

        public async Task<ActionResult<ChoiceModel>> Post(int quizId, int questionId, ChoiceModel model)
        {
            var question = await _repository.GetQuestion(quizId, questionId);

            if (question == null)
            {
                return BadRequest("Question doesn't exists");
            }

            var choice = _mapper.Map<Choice>(model);

            choice.QuestionId = question.Id;

            _repository.Add(choice);

            if (await _repository.SaveChangesAsync())
            {
                return CreatedAtAction(nameof(Get), _mapper.Map<ChoiceModel>(choice));
            }

            return BadRequest("Failed to save new choice");
        }

        [HttpPut("{choiceId}")]
        public async Task<ActionResult<ChoiceModel>> Put(int quizId, int questionId, int choiceId, ChoiceModel model)
        {
            var oldChoice = await _repository.GetChoice(quizId, questionId, choiceId);

            if (oldChoice == null)
            {
                return NotFound();
            }

            _mapper.Map(model, oldChoice);

            if (await _repository.SaveChangesAsync())
            {
                return _mapper.Map<ChoiceModel>(oldChoice);
            }

            return BadRequest();
        }

        [HttpDelete("{choiceId}")]
        public async Task<IActionResult> Delete(int quizId, int questionId, int choiceId)
        {
            var question = await _repository.GetChoice(quizId, questionId, choiceId);

            if (question == null)
            {
                return NotFound("Failed to find the choice to delete");
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