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
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/Quizzes/{quizId}/Questions/{questionId}/[controller]")]
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

        /// <summary>
        /// Get list of all choices for given quiz id and question id.
        /// </summary>
        /// <param name="quizId">Parameter which tells us to which quiz question belongs to.</param>
        /// <param name="questionId">Id of question we want to get choices from.</param>
        /// <returns>An ActionResult of ChoiceModel array type</returns>
        /// <remarks>
        /// Sample request (this request returns **array of choices**)  
        ///      
        ///     GET /quizzes/1/questions/1/choices
        ///     
        /// </remarks>
        /// <response code="200">Returns array of all choices</response>
        /// <response code="204">No questions exists so return nothing.</response>
        /// <response code="404">Quiz or question doesn't exist.</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<ChoiceModel[]>> Get(int quizId, int questionId)
        {
            var quiz = await _repository.GetQuiz(quizId);
            var question = await _repository.GetQuestion(quizId, questionId, false);

            if (quiz == null || question == null)
            {
                return NotFound();
            }

            var choices = await _repository.GetChoices(quizId, questionId);

            return _mapper.Map<ChoiceModel[]>(choices);
        }

        /// <summary>
        /// Get one choice for given quiz and question id.
        /// </summary>
        /// <param name="quizId">Parameter which tells us to which quiz question belongs to.</param>
        /// <param name="questionId">Id of question we want to get choice from.</param>
        /// <param name="choiceId">Id of choice we want to return.</param>
        /// <returns>An ActionResult of ChoiceModel type</returns>
        /// <remarks>
        /// Sample request (this request returns **one choice**)  
        ///      
        ///     GET /quizzes/1/questions/1/choices/1
        ///     
        /// </remarks>
        /// <response code="200">Returns one choice</response>
        /// <response code="204">No questions exists so return nothing.</response>
        /// <response code="404">Choice doesn't exist.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Creates choice for given quiz and question with provided data.
        /// </summary>
        /// <param name="quizId">Id of quiz you want to get question from.</param>
        /// <param name="questionId">Id of question we want to attach choice to.</param>
        /// <param name="model">Choice model.</param>
        /// <returns>>An ActionResult of Choice</returns>
        /// <remarks>
        /// Sample request (this request returns **created choice**)  
        ///      
        ///     POST /quizzes/1/questions/1/choices/
        ///     
        ///     {
        ///         "name":"another name"
        ///     }
        ///     
        /// </remarks>  
        /// <response code="201">Returns created choice.</response>
        /// <response code="422">One of validation errors occured.</response>
        /// <response code="400">Bad request not complete or corrupted data.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<ChoiceModel>> Post(int quizId, int questionId, ChoiceModel model)
        {
            var question = await _repository.GetQuestion(quizId, questionId);
            var quiz = await _repository.GetQuiz(quizId);
            if (question == null || quiz == null)
            {
                return BadRequest("Question or Quiz doesn't exists");
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

        /// <summary>
        /// Updates choice for given quiz and question with specified id and data.
        /// </summary>
        /// <param name="quizId">Id of quiz you want to update.</param>
        /// <param name="questionId">Id of question you want to update.</param>
        /// <param name="choiceId">Id of choice you want to update.</param>
        /// <param name="model">New data for question.</param>
        /// <returns>>An ActionResult of QuestionModel</returns>
        /// <remarks>
        /// Sample request (this request returns **updated choice**)  
        ///      
        ///     PUT /quizzes/1/questions/1/choices/1
        ///     
        ///     {
        ///         "name":"another name"
        ///     }
        ///     
        /// </remarks>  
        /// <response code="200">Returns choice with provided id and updated info.</response>
        /// <response code="404">Choice with provided id wasn't found.</response> 
        /// <response code="204">Probably should never return that but there is possibility that question isn't null but mapping result in null.</response> 
        /// <response code="422">One of validation errors occured.</response>
        /// <response code="400">Bad request not complete or corrupted data.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
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

        /// <summary>
        /// Deletes question for quiz and question with specified id.
        /// </summary>
        /// <param name="quizId">Id of quiz you want to delete question from.</param>
        /// <param name="questionId">Id of question you want to delete choice from.</param>
        /// <param name="choiceId">Id of choice you want to delete.</param>
        /// <returns>Response Code</returns>
        /// <remarks>
        /// Sample request (this request returns **response code only**)  
        ///      
        ///     DELETE /quizzes/1/questions/1/choices/1
        ///     
        /// </remarks> 
        /// <response code = "404" >Quiz or question or choice with provided id wasn't found.</response> 
        /// <response code = "200" >Choice was sucessfully deleted.</response> 
        /// <response code = "400" >Request data was not complete or corrupted.</response> 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
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