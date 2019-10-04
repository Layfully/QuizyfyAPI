using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Services;

namespace QuizyfyAPI.Controllers
{
    [Route("api/Quizzes/{quizId}/Questions/{questionId}/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class ChoicesController : ControllerBase
    {
        private readonly IChoiceService _choiceService;

        public ChoicesController(IChoiceService choiceService)
        {
            _choiceService = choiceService;
        }

        /// <summary>
        /// Get list of all choices for given quiz id and question id.
        /// </summary>
        /// <param name="quizId">Parameter which tells us to which quiz question belongs to.</param>
        /// <param name="questionId">Id of question we want to get choices from.</param>
        /// <returns>An ActionResult of ChoiceModel array type</returns>
        /// <remarks>
        /// Sample request (this request returns an **array of choices**)  
        ///      
        ///     GET /quizzes/1/questions/1/choices
        ///     
        /// </remarks>
        /// <response code="200">Returns array of all choices</response>
        /// <response code="204">No questions exists so return nothing.</response>
        /// <response code="404">Quiz or question doesn't exist.</response>
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
        public async Task<ActionResult<ChoiceResponse[]>> Get(int quizId, int questionId)
        {
            var getAllResponse = await _choiceService.GetAll(quizId, questionId);

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
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{choiceId}")]
        public async Task<ActionResult<ChoiceResponse>> Get(int quizId, int questionId, int choiceId)
        {
            var getResponse = await _choiceService.Get(quizId, questionId, choiceId);

            if (!getResponse.Found)
            {
                return NotFound(getResponse.Errors);
            }
            return getResponse.Object;
        }

        /// <summary>
        /// Creates choice for given quiz and question with provided data.
        /// </summary>
        /// <param name="quizId">Id of quiz you want to get question from.</param>
        /// <param name="questionId">Id of question we want to attach choice to.</param>
        /// <param name="request">Choice model.</param>
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
        /// <response code="400">Bad request not complete or corrupted data.</response>
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<ActionResult<ChoiceResponse>> Post(int quizId, int questionId, ChoiceCreateRequest request)
        {
            var createResponse = await _choiceService.Create(quizId, questionId, request);

            if (!createResponse.Success)
            {
                return BadRequest(createResponse.Errors);
            }

            return CreatedAtAction(nameof(Get), createResponse.Object);
        }

        /// <summary>
        /// Updates choice for given quiz and question with specified id and data.
        /// </summary>
        /// <param name="quizId">Id of quiz you want to update.</param>
        /// <param name="questionId">Id of question you want to update.</param>
        /// <param name="choiceId">Id of choice you want to update.</param>
        /// <param name="request">New data for question.</param>
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
        /// <response code="204">Probably should never return that but there is possibility that question isn't null but mapping result in null.</response> 
        /// <response code="400">Bad request not complete or corrupted data.</response>
        /// <response code="404">Choice with provided id wasn't found.</response> 
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        [HttpPut("{choiceId}")]
        public async Task<ActionResult<ChoiceResponse>> Put(int quizId, int questionId, int choiceId, ChoiceUpdateRequest request)
        {
            var updateResponse = await _choiceService.Update(quizId, questionId, choiceId, request);

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
        /// <response code="200" >Choice was sucessfully deleted.</response> 
        /// <response code="400" >Request data was not complete or corrupted.</response> 
        /// <response code="404" >Quiz or question or choice with provided id wasn't found.</response> 
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{choiceId}")]
        public async Task<IActionResult> Delete(int quizId, int questionId, int choiceId)
        {
            var deleteResponse = await _choiceService.Delete(quizId, questionId, choiceId);

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
}