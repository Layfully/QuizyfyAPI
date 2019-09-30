using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Data;
using QuizyfyAPI.Services;
using reCAPTCHA.AspNetCore;
using SendGrid;

namespace QuizyfyAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class UsersController : ControllerBase
    {   
        private readonly IUserService _userService;
        private readonly IRecaptchaService _recaptchaService;

        public UsersController(IUserService userService, IRecaptchaService recaptchaService)
        {
            _userService = userService;
            _recaptchaService = recaptchaService;
        }

        /// <summary>
        /// Authenticate user by checking if he is in database.
        /// </summary>
        /// <param name="request">User credentials</param>
        /// <returns>Action Result with user model</returns>
        /// <remarks>
        /// Sample request (this request returns **user with token if authentication went well**)  
        ///      
        ///     POST /users/login
        ///     
        ///     {
        ///         "username": "test",
        ///         "password": "password"
        ///     }    
        ///     
        /// </remarks> 
        /// <response code="200">Returns user with JWT bearer token.</response>
        /// <response code="204">User with this name was not found so return nothing.</response>
        /// <response code="401">Provided login and password didn't match with any user.</response>
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponse>> Login(UserLoginRequest request)
        {
            var recaptcha = await _recaptchaService.Validate(request.RecaptchaToken);

            if (!recaptcha.success && recaptcha.score >= 0.8M)
            {
                return Ok("Recaptcha failed");
            }

            var loginResponse = await _userService.Login(request);

            if (!loginResponse.Success)
            {
                return Unauthorized(loginResponse.Errors);
            }
            return loginResponse.Object;
        }

        /// <summary>
        /// Create user with given credentials.
        /// </summary>
        /// <param name="request">User credentials</param>
        /// <returns>Action Result of user model</returns>
        /// <remarks>
        /// Sample request (this request returns **user with token**)  
        ///      
        ///     POST /users/register
        ///     
        ///     {
        ///         "username": "test",
        ///         "password": "password",
        ///         "role": "user"
        ///     }    
        ///     
        /// </remarks> 
        /// <response code="201">User sucessfully created. Now you can login on login action.</response>
        /// <response code="400">User is already taken or there was no records in db saved.</response>
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [AllowAnonymous]
        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponse>> Register(UserRegisterRequest request)
        {
            var recaptcha = await _recaptchaService.Validate(request.RecaptchaToken);

            if (!recaptcha.success && recaptcha.score >= 0.8M)
            {
                return Ok("Recaptcha failed");
            }

            var registerResponse = await _userService.Register(request);

            if (!registerResponse.Success)
            {
                return BadRequest(registerResponse.Errors);
            }
            return CreatedAtAction(nameof(Login), request);
        }

        /// <summary>
        /// Updates user credentials.
        /// </summary>
        /// <param name="id">Id of user you want to update.</param>
        /// <param name="request">New credentials.</param>
        /// <returns>Action Result with user model</returns>
        /// <remarks>
        /// Sample request (this request returns **user**)  
        ///      
        ///     PUT /users/1
        ///     
        ///     {
        ///         "username": "test",
        ///         "password": "password"
        ///     }    
        ///     
        /// </remarks> 
        /// <response code="200">Returns user with changed properties.</response>
        /// <response code="400">User with this name already exists or there was no records updated.</response>
        /// <response code="401">You aren't authenticated. Please authenticate first.</response>
        /// <response code="403">You aren't user with given id.</response>
        /// <response code="404">User with given id was not found.</response>
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponse>> Put(int id, UserUpdateRequest request)
        {
            if (!User.IsInRole(Role.Admin) && User.Claims.Single(x => x.Type == "Id").Value != id.ToString())
            {
                return Forbid("Only admin can change other users data.");
            }

            var updateResponse = await _userService.Update(id, request);

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

        [HttpPatch("{id}")]
        public async Task<ActionResult<UserResponse>> EmailVerification(int id, [FromQuery] string token)
        {
            var verificationResponse = await _userService.VerifyEmail(id, token);

            if (!verificationResponse.Success)
            {
                return BadRequest(verificationResponse.Errors);
            }

            return verificationResponse.Object;
        }

        /// <summary>
        /// Deletes user with given id.
        /// </summary>
        /// <param name="id">Id of user you want to delete.</param>
        /// <returns>Response Code</returns>
        /// <remarks>
        /// Sample request (this request returns **response code only**)  
        ///      
        ///     DELETE /users/1
        ///     
        /// </remarks> 
        /// <response code="200">Returns ok if user was deleted.</response>
        /// <response code="400">There was no affected records in db.</response>
        /// <response code="401">You aren't authenticated. Please authenticate first.</response>
        /// <response code="403">You aren't user with given id.</response>
        /// <response code="404">User with given id was not found.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole(Role.Admin) && id.ToString() != User.Claims.Single(x => x.Type == "Id").Value)
            {
                return Forbid("Only admin can delete other users.");
            }

            var deleteResponse = await _userService.Delete(id);

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

        /// <summary>
        /// Refresh JWT token of a user.
        /// </summary>
        /// <param name="request">Object with refresh token and JWT token</param>
        /// <returns>Action Result with user with refreshed JWT token</returns>
        /// <remarks>
        /// Sample request (this request returns **user with JWT token and refresh token**)  
        ///      
        ///     POST /users/1/refresh
        ///     
        ///     {
        ///         "JwtToken": "token",
        ///         "RefreshToken": "token"
        ///     }    
        ///     
        /// </remarks> 
        /// <response code="200">JWT sucessfully refreshed.</response>
        /// <response code="400">There were problems with tokens validation.</response>
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="422">Request data couldn't be processed.</response>
        /// <response code="500">Something threw exception on server.</response>
        [AllowAnonymous]
        [HttpPost("Refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponse>> Refresh(UserRefreshRequest request)
        {
            var refreshResponse = await _userService.RefreshTokenAsync(request);

            if (!refreshResponse.Success)
            {
                return BadRequest(refreshResponse.Errors);
            }
            return refreshResponse.Object;
        }
    }
}