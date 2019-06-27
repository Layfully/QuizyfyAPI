using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using QuizyfyAPI.Data;
using QuizyfyAPI.Models;

namespace QuizyfyAPI.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
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

        /// <summary>
        /// Get list of all quizes.
        /// </summary>
        /// <param name="includeQuestions">Parameter which tells us wheter to include questions for quiz or not.</param>
        /// <returns>An ActionResult of QuizModel array type</returns>
        /// <remarks>
        /// Sample request (this request returns **array of quizzes**)  
        ///      
        ///     GET /quizzes
        ///     
        /// </remarks>
        /// <response code="200">Returns array of all quizzes</response>
        /// <response code="204">No quizzes exists so return nothing.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpGet]
        public async Task<ActionResult<QuizModel[]>> Get(bool includeQuestions = false)
        {
            var results = await _repository.GetQuizzes(includeQuestions);

            if(results.Length == 0)
            {
                return NoContent();
            }

            return _mapper.Map<QuizModel[]>(results);
        }

        /// <summary>
        /// Get one quiz by id.
        /// </summary>
        /// <param name="id">This is id of the quiz you want to get.</param>
        /// <param name="includeQuestions">Parameter which tells us wheter to include questions for quiz or not.</param>
        /// <returns>>An ActionResult of QuizModel</returns>
        /// <remarks>
        /// Sample request (this request returns **one quiz**)  
        ///      
        ///     GET /quizzes/1
        ///     
        /// </remarks>   
        /// <response code="200">Returns one quiz with provided id</response>
        /// <response code="404">Quiz with provided id wasn't found.</response>
        /// <response code="204">Probably should never return that but there is possibility that quiz isn't null but mapping result in this.</response> 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizModel>> Get(int id, bool includeQuestions = false)
        {

            var result = await _repository.GetQuiz(id, includeQuestions);

            if (result == null)
            {
                return NotFound();
            }

            return _mapper.Map<QuizModel>(result);

        }

        /// <summary>
        /// Create quiz with provided info.
        /// </summary>
        /// <param name="model">This is json representation of quiz you want to create.</param>
        /// <returns>>An ActionResult of QuizModel</returns>
        /// <remarks>
        /// Sample request (this request returns **created quiz**)  
        ///      
        ///     POST /quizzes
        ///     
        ///     {
        ///         "name":"quizname"
        ///     }
        ///     
        /// </remarks>  
        /// <response code="400">Data provided was not complete or corrupted.</response>
        /// <response code="201">Quiz was created and you can access it.</response>
        /// <response code="422">One of validation errors occured.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost]
        public async Task<ActionResult<QuizModel>> Post(QuizCreateModel model)
        {
            var quiz = _mapper.Map<Quiz>(model);

            quiz.DateAdded = DateTime.Now;

            _repository.Add(quiz);

            if (await _repository.SaveChangesAsync())
            {
                return CreatedAtAction(nameof(Get), new { id = quiz.Id }, _mapper.Map<QuizModel>(quiz));
            }

            return BadRequest();
        }

        /// <summary>
        /// Updates quiz with specified id and data.
        /// </summary>
        /// <param name="id">Id of quiz you want to update.</param>
        /// <param name="model">New data for quiz.</param>
        /// <returns>>An ActionResult of QuizModel</returns>
        /// <remarks>
        /// Sample request (this request returns **updated quiz**)  
        ///      
        ///     PUT /quizzes
        ///     
        ///     {
        ///         "name":"another name"
        ///     }
        ///     
        /// </remarks>  
        /// <response code="200">Returns quiz with provided id and updated info.</response>
        /// <response code="404">Quiz with provided id wasn't found.</response> 
        /// <response code="204">Probably should never return that but there is possibility that quiz isn't null but mapping result in this.</response> 
        /// <response code="422">One of validation errors occured.</response>
        /// <response code="400">Bad request not complete or corrupted data.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id}")]
        public async Task<ActionResult<QuizModel>> Put(int id, QuizCreateModel model)
        {
            var oldQuiz = await _repository.GetQuiz(id);
            if (oldQuiz == null)
            {
                return NotFound($"Couldn't find quiz with id of {id}");
            }

            _mapper.Map(model, oldQuiz);

            if (await _repository.SaveChangesAsync())
            {
                return _mapper.Map<QuizModel>(oldQuiz);
            }

            return BadRequest();
        }

        /// <summary>
        /// Deletes quiz with specified id.
        /// </summary>
        /// <param name="id">Id of quiz you want to delete.</param>
        /// <returns>Response Code</returns>
        /// <remarks>
        /// Sample request (this request returns **response code only**)  
        ///      
        ///     DELETE /quizzes/1
        ///     
        /// </remarks> 
        /// <response code = "404" >Quiz with provided id wasn't found.</response> 
        /// <response code = "200" >Quiz was sucessfully deleted.</response> 
        /// <response code = "400" >Request data was not complete or corrupted.</response> 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var oldQuiz = await _repository.GetQuiz(id);

            if (oldQuiz == null)
            {
                return NotFound();
            }

            _repository.Delete(oldQuiz);

            if (await _repository.SaveChangesAsync())
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}
