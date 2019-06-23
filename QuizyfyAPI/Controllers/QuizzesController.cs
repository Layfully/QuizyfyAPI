using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using QuizyfyAPI.Data;
using QuizyfyAPI.Models;

namespace QuizyfyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizRepository _repository;
        private readonly IMapper _mapper;

        public QuizzesController(IQuizRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<QuizModel[]>> Get(bool includeQuestions = false)
        {
            try
            {
                var results = await _repository.GetAllQuizzesAsync(includeQuestions);

                return _mapper.Map<QuizModel[]>(results);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuizModel>> Get(int id, bool includeQuestions = false)
        {
            try
            {
                var result = await _repository.GetQuizAsync(id, includeQuestions);

                if (result == null)
                {
                    return NotFound();
                }

                return _mapper.Map<QuizModel>(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        public async Task<ActionResult<QuizModel>> Post(QuizModel model)
        {
            try
            {
                model.DateAdded = DateTime.Now.ToString();

                var quiz = _mapper.Map<Quiz>(model);

                _repository.Add(quiz);

                if (await _repository.SaveChangesAsync())
                {
                    return CreatedAtAction(nameof(Get), new { id = quiz.Id }, _mapper.Map<QuizModel>(quiz));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure" + ex.Message);
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<QuizModel>> Put(int id, QuizModel model)
        {
            try
            {
                var oldQuiz = await _repository.GetQuizAsync(id);
                if(oldQuiz == null)
                {
                    return NotFound($"Couldn't find quiz with id of {id}");
                }

                _mapper.Map(model, oldQuiz);

                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<QuizModel>(oldQuiz);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure" + ex.Message);
            }

            return BadRequest();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var oldQuiz = await _repository.GetQuizAsync(id);

                if(oldQuiz == null)
                {
                    return NotFound();
                }

                _repository.Delete(oldQuiz);

                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure" + ex.Message);
            }

            return BadRequest();
        }
    }
}
