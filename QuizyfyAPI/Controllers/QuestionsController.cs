using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Services;

namespace QuizyfyAPI.Controllers;
[Route("api/Quizzes/{quizId}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiController]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
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
    /// <response code="406">Request data type is not in acceptable format.</response>
    /// <response code="422">Request data couldn't be processed.</response>
    /// <response code="500">Something threw exception on server.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<ActionResult<QuestionResponse[]>> Get(int quizId, bool includeChoices = false)
    {
        var getAllResponse = await _questionService.GetAll(quizId, includeChoices);

        if (!getAllResponse.Success)
        {
            if (!getAllResponse.Found)
            {
                return NotFound(getAllResponse.Errors);
            }
            return NoContent();
        }
        return getAllResponse.Object;
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
    /// <response code="404">Quiz doesn't exist.</response>
    /// <response code="406">Request data type is not in acceptable format.</response>
    /// <response code="422">Request data couldn't be processed.</response>
    /// <response code="500">Something threw exception on server.</response>
    [HttpGet("{questionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuestionResponse>> Get(int quizId, int questionId, bool includeChoices)
    {
        var getResponse = await _questionService.Get(quizId, questionId, includeChoices);

        if (!getResponse.Found)
        {
            return NotFound(getResponse.Errors);
        }
        return getResponse.Object;
    }


    /// <summary>
    /// Creates question for given quiz with provided data.
    /// </summary>
    /// <param name="quizId">Id of quiz you want to attach question to.</param>
    /// <param name="request">Question model.</param>
    /// <returns>An ActionResult of Question</returns>
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
    /// <response code="400">Bad request not complete or corrupted data.</response>
    /// <response code="406">Request data type is not in acceptable format.</response>
    /// <response code="422">Request data couldn't be processed.</response>
    /// <response code="500">Something threw exception on server.</response>
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> Post(int quizId, QuestionCreateRequest request)
    {
        var createResponse = await _questionService.Create(quizId, request);

        if (!createResponse.Success)
        {
            return BadRequest(createResponse.Errors);
        }
        return CreatedAtAction(nameof(Get), createResponse.Object);
    }

    /// <summary>
    /// Updates question for given quiz with specified id and data.
    /// </summary>
    /// <param name="quizId">Id of quiz you want to update.</param>
    /// <param name="questionId">Id of question you want to update.</param>
    /// <param name="request">New data for question.</param>
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
    /// <response code="204">Probably should never return that but there is possibility that question isn't null but mapping result in null.</response> 
    /// <response code="404">Question with provided id wasn't found.</response> 
    /// <response code="400">Bad request not complete or corrupted data.</response>
    /// <response code="406">Request data type is not in acceptable format.</response>
    /// <response code="422">Request data couldn't be processed.</response>
    /// <response code="500">Something threw exception on server.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{questionId}")]
    public async Task<ActionResult<QuestionResponse>> Put(int quizId, int questionId, QuestionUpdateRequest request)
    {
        var updateResponse = await _questionService.Update(quizId, questionId, request);

        if (!updateResponse.Success)
        {
            if (!updateResponse.Found)
            {
                return NotFound(updateResponse.Errors);
            }
            return BadRequest(updateResponse.Errors);
        }
        return updateResponse.Object;
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
    /// <response code="200" >Question was sucessfully deleted.</response> 
    /// <response code="400" >Request data was not complete or corrupted.</response>
    /// <response code="404" >Quiz or question with provided id wasn't found.</response> 
    /// <response code="406">Request data type is not in acceptable format.</response>
    /// <response code="422">Request data couldn't be processed.</response>
    /// <response code="500">Something threw exception on server.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{questionId}")]
    public async Task<IActionResult> Delete(int quizId, int questionId)
    {
        var deleteResponse = await _questionService.Delete(quizId, questionId);

        if (!deleteResponse.Success)
        {
            if (!deleteResponse.Found)
            {
                return NotFound(deleteResponse.Errors);
            }
            return BadRequest(deleteResponse.Errors);
        }
        return Ok();
    }
}
