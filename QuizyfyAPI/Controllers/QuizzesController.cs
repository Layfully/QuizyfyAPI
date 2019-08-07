using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using QuizyfyAPI.Models;
using QuizyfyAPI.Services;

namespace QuizyfyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
        }


        /// <summary>
        /// Get list of all quizes.
        /// </summary>
        /// <param name="pagingParams">Parameter which tells us about which quizzes to display.</param>
        /// <returns>An ActionResult of QuizModel array type</returns>
        /// <remarks>
        /// Sample request (this request returns **array of quizzes**)  
        ///      
        ///     GET /quizzes
        ///     
        /// </remarks>
        /// <response code="200">Returns array of all quizzes</response>
        /// <response code="204">No quizzes exists so return nothing.</response>
        /// <response code="404">Couldn't find any quizzes.</response>
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
        public async Task<ActionResult<QuizListModel>> Get([FromQuery]PagingParams pagingParams)
        {
            var getAllResponse = await _quizService.GetAll(pagingParams, Response, HttpContext);

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
        /// <response code="204">Probably should never return that but there is possibility that quiz isn't null but mapping result in this.</response> 
        /// <response code="404">Quiz with provided id wasn't found.</response>
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizModel>> Get(int id, bool includeQuestions = false)
        {
            var getResponse = await _quizService.Get(id, includeQuestions);

            if (!getResponse.Found)
            {
                return NotFound(getResponse.Errors);
            }
            return getResponse.Object;
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
        /// <response code="201">Quiz was created and you can access it.</response>
        /// <response code="400">Data provided was not complete or corrupted.</response>
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]        [HttpPost]
        public async Task<ActionResult<QuizModel>> Post(QuizCreateModel model  )
        {
            var createResponse = await _quizService.Create(model);

            if (!createResponse.Success)
            { 
                return BadRequest(createResponse.Errors);
            }
            return CreatedAtAction(nameof(Get), new { id = createResponse.Object.Id }, createResponse.Object);
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
        /// <response code="204">Probably should never return that but there is possibility that quiz isn't null but mapping result in this.</response> 
        /// <response code="400">Bad request not complete or corrupted data.</response>
        /// <response code="404">Quiz with provided id wasn't found.</response> 
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("{id}")]
        public async Task<ActionResult<QuizModel>> Put(int id, QuizCreateModel model)
        {
            var updateResponse = await _quizService.Update(id, model);

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
        /// <response code="200" >Quiz was sucessfully deleted.</response> 
        /// <response code="400" >Request data was not complete or corrupted.</response> 
        /// <response code="404" >Quiz with provided id wasn't found.</response> 
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteResponse = await _quizService.Delete(id);

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
