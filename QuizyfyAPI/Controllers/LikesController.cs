using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizyfyAPI.Data;
using QuizyfyAPI.Models;

namespace QuizyfyAPI.Controllers
{
    [Route("api/Quizzes/{quizId}/[controller]")]
    [Authorize]
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly IQuizRepository _quizRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IMapper _mapper;

        public LikesController(IQuizRepository quizRepository, ILikeRepository likeRepository, IMapper mapper)
        {
            _quizRepository = quizRepository;
            _likeRepository = likeRepository;
            _mapper = mapper;
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
        [HttpPut]
        public async Task<ActionResult<LikeModel>> Put(int quizId)
        {
            var userId = Int32.Parse(User.Identity.Name);

            var quiz = await _quizRepository.GetQuiz(quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            var like = await _likeRepository.GetLike(quizId, userId);

            if (like != null)
            {
                return Ok();
            }

            var model = new LikeModel
            {
                QuizId = quizId,
                UserId = userId
            };

            _mapper.Map(model, like);

            _likeRepository.Add(like);

            if (await _likeRepository.SaveChangesAsync())
            {
                return _mapper.Map<LikeModel>(like);
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
        public async Task<IActionResult> Delete(int quizId)
        {
            var quiz = await _quizRepository.GetQuiz(quizId);
            var userId = Int32.Parse(User.Identity.Name);

            if (quiz == null)
            {
                return NotFound();
            }

            var like = await _likeRepository.GetLike(quizId, userId);

            _likeRepository.Delete(like);

            if (await _likeRepository.SaveChangesAsync())
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}