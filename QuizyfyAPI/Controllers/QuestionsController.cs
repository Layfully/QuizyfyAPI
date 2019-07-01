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
    [Route("api/Quizzes/{quizId}/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IMapper _mapper;

        public QuestionsController(IQuestionRepository questionnRepository, IQuizRepository quizRepository, IMapper mapper)
        {
            _questionRepository = questionnRepository;
            _quizRepository = quizRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list of all questions for given quiz id.
        /// </summary>
        /// <param name="quizId">Parameter which tells us to which quiz question belongs to.</param>
        /// <param name="includeChoices">Parameter which tells us wheter to include choices for question or not.</param>
        /// <returns>An ActionResult of QuestionModel array type</returns>
        /// <remarks>
        /// Sample request (this request returns **array of questions**)  
        ///      
        ///     GET /quizzes/1/questions/
        ///     
        /// </remarks>
        /// <response code="200">Returns array of all questions</response>
        /// <response code="204">No questions exists so return nothing.</response>
        /// <response code="404">Quiz doesn't exist.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<QuestionModel[]>> Get(int quizId, bool includeChoices = false)
        {
            var quiz = await _quizRepository.GetQuiz(quizId);

            if(quiz == null)
            {
                return NotFound();
            }

            var questions = await _questionRepository.GetQuestions(quizId, includeChoices);

            if (questions.Length == 0)
            {
                return NoContent();
            }

            return _mapper.Map<QuestionModel[]>(questions);
        }

        /// <summary>
        /// Get one question for given quiz id.
        /// </summary>
        /// <param name="quizId">Parameter which tells us to which quiz question belongs to.</param>
        /// <param name="questionId">Id of question we want to return.</param>
        /// <param name="includeChoices">Parameter which tells us wheter to include choices for question or not.</param>
        /// <returns>An ActionResult of QuestionModel array type</returns>
        /// <remarks>
        /// Sample request (this request returns **one question**)  
        ///      
        ///     GET /quizzes/1/questions/1
        ///     
        /// </remarks>
        /// <response code="200">Returns one question</response>
        /// <response code="204">No questions exists so return nothing.</response>
        /// <response code="404">Quiz doesn't exist.</response>
        [HttpGet("{questionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<QuestionModel>> Get(int quizId, int questionId, bool includeChoices)
        {
            var question = await _questionRepository.GetQuestion(quizId, questionId, includeChoices);

            if (question == null)
            {
                return NotFound("Couldn't find question");
            }

            return _mapper.Map<QuestionModel>(question);
        }


        /// <summary>
        /// Creates question for given quiz with provided data.
        /// </summary>
        /// <param name="quizId">Id of quiz you want to attach question to.</param>
        /// <param name="model">Question model.</param>
        /// <returns>>An ActionResult of Question</returns>
        /// <remarks>
        /// Sample request (this request returns **created question**)  
        ///      
        ///     POST /quizzes/1/questions
        ///     
        ///     {
        ///         "name":"another name"
        ///     }
        ///     
        /// </remarks>  
        /// <response code="201">Returns created question with.</response>
        /// <response code="422">One of validation errors occured.</response>
        /// <response code="400">Bad request not complete or corrupted data.</response>
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost]
        public async Task<ActionResult<QuestionModel>> Post(int quizId, QuestionCreateModel model)
        {
            var quiz = await _quizRepository.GetQuiz(quizId);

            if (quiz == null)
            {
                return BadRequest("Quiz doesn't exists");
            }

            var question = _mapper.Map<Question>(model);

            if(question != null)
            {
                question.QuizId = quiz.Id;

                _questionRepository.Add(question);
            }

            if (await _questionRepository.SaveChangesAsync())
            {
                return CreatedAtAction(nameof(Get), new { quizId = quiz.Id }, _mapper.Map<QuestionModel>(question));
            }

            return BadRequest("Failed to save new question");
        }

        /// <summary>
        /// Updates question for given quiz with specified id and data.
        /// </summary>
        /// <param name="quizId">Id of quiz you want to update.</param>
        /// <param name="questionId">Id of question you want to update.</param>
        /// <param name="model">New data for question.</param>
        /// <returns>>An ActionResult of QuestionModel</returns>
        /// <remarks>
        /// Sample request (this request returns **updated question**)  
        ///      
        ///     PUT /quizzes/1/questions/1
        ///     
        ///     {
        ///         "name":"another name"
        ///     }
        ///     
        /// </remarks>  
        /// <response code="200">Returns question with provided id and updated info.</response>
        /// <response code="404">Question with provided id wasn't found.</response> 
        /// <response code="204">Probably should never return that but there is possibility that question isn't null but mapping result in null.</response> 
        /// <response code="422">One of validation errors occured.</response>
        /// <response code="400">Bad request not complete or corrupted data.</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{questionId}")]
        public async Task<ActionResult<QuestionModel>> Put(int quizId, int questionId, QuestionCreateModel model)
        {
            var oldQuestion = await _questionRepository.GetQuestion(quizId, questionId, true);

            if (oldQuestion == null)
            {
                return NotFound();
            }

            _mapper.Map(model, oldQuestion);

            if (await _questionRepository.SaveChangesAsync())
            {
                return _mapper.Map<QuestionModel>(oldQuestion);
            }

            return BadRequest();
        }

        /// <summary>
        /// Deletes question for quiz with specified id.
        /// </summary>
        /// <param name="quizId">Id of quiz you want to delete question from.</param>
        /// <param name="questionId">Id of question you want to delete</param>
        /// <returns>Response Code</returns>
        /// <remarks>
        /// Sample request (this request returns **response code only**)  
        ///      
        ///     DELETE /quizzes/1/questions/1
        ///     
        /// </remarks> 
        /// <response code = "404" >Quiz or question with provided id wasn't found.</response> 
        /// <response code = "200" >Question was sucessfully deleted.</response> 
        /// <response code = "400" >Request data was not complete or corrupted.</response> 
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{questionId}")]
        public async Task<IActionResult> Delete(int quizId, int questionId)
        {
            var question = await _questionRepository.GetQuestion(quizId, questionId, true);

            if (question == null)
            {
                return NotFound("Failed to find the question to delete");
            }

            _questionRepository.Delete(question);

            if (await _questionRepository.SaveChangesAsync())
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}