using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Services;

namespace QuizyfyAPI.Controllers
{
    [Authorize]
    [Route("api/Quizzes/{quizId}/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private int UserId => int.Parse(User.Claims.Single(x => x.Type == "Id").Value);

        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        /// <summary>
        /// Likes quiz.
        /// </summary>
        /// <param name="quizId">Id of quiz you want to like.</param>
        /// <returns>Action Result with like model</returns>
        /// <remarks>
        /// Sample request (this request returns **like**)  
        ///      
        ///     PUT /quizzes/1/likes
        ///     
        /// </remarks> 
        /// <response code="200">Returns like which existed or newly created.</response>
        /// <response code="400">There was no records updated.</response>
        /// <response code="401">You aren't authenticated. Please authenticate first.</response>
        /// <response code="404">Quiz with given id was not found.</response>
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<ActionResult<LikeResponse>> Put(int quizId)
        {
            var likeResponse = await _likeService.Like(quizId, UserId);

            if (!likeResponse.Success)
            {
                if (!likeResponse.Found)
                {
                    return NotFound(likeResponse.Errors);
                }
                return BadRequest(likeResponse.Errors);
            }
            return likeResponse.Object;
        }

        /// <summary>
        /// Unlikes quiz.
        /// </summary>
        /// <param name="quizId">Id of quiz you want to unlike.</param>
        /// <returns>Response Code</returns>
        /// <remarks>
        /// Sample request (this request returns **response code only**)  
        ///      
        ///     DELETE /quizzes/1/likes
        ///     
        /// </remarks> 
        /// <response code="200">Returns ok if like was deleted.</response>
        /// <response code="400">There was no affected records in db.</response>
        /// <response code="401">You aren't authenticated. Please authenticate first.</response>
        /// <response code="404">Quiz with given id was not found.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int quizId)
        {
            var deleteResponse = await _likeService.Delete(quizId, UserId);

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