using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuizyfyAPI.Data;
using QuizyfyAPI.Helpers;
using QuizyfyAPI.Models;

namespace QuizyfyAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IQuizRepository _repository;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(IQuizRepository repository, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _repository = repository;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Authenticate user by checking if he is in database.
        /// </summary>
        /// <param name="model">User credentials</param>
        /// <returns>Action Result of user model</returns>
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
        /// <response code="406">Request data type is not in acceptable format.</response>
        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        public async Task<ActionResult<UserModel>> Login(UserLoginModel model)
        {
            var user = await _repository.Authenticate(model.Username, model.Password);

            if (user != null)
            {
                user = JWTHelper.RequestToken(user, _appSettings);
            }

            return _mapper.Map<UserModel>(user);
        }

        /// <summary>
        /// Create user with given credentials.
        /// </summary>
        /// <param name="model">User credentials</param>
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
        /// <response code="400">User is already taken or you didn't provide enough data.</response>
        /// <response code="201">User sucessfully created. Now you can login on login action.</response>
        /// <response code="406">Request data type is not in acceptable format.</response>
        [AllowAnonymous]
        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        public async Task<ActionResult<UserModel>> Register(UserRegisterModel model)
        {
            var user = _mapper.Map<User>(model);

            if (await _repository.GetUserByUsername(model.Username) != null)
            {
                return BadRequest("Username: " + user.Username + " is already taken");
            }

            byte[] passwordHash, passwordSalt;
            PasswordHash.Create(model.Password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _repository.Add(user);

            if (await _repository.SaveChangesAsync())
            {
                return CreatedAtAction(nameof(Login), model);
            }

            return BadRequest();
        }

        /// <summary>
        /// Updates user credentials.
        /// </summary>
        /// <param name="id">Id of user you want to update.</param>
        /// <param name="model">New credentials.</param>
        /// <returns>Action Result of user model</returns>
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
        /// <response code="404">User with given id was not found.</response>
        /// <response code="403">You aren't user with given id.</response>
        /// <response code="400">User with this name already exists or you didn't provide enough data.</response>
        /// <response code="200">Returns user with changed properties.</response>
        /// <response code="406">Request data type is not in acceptable format.</response>
        /// <response code="401">You aren't authenticated. Please authenticate first.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserModel>> Put(int id, UserRegisterModel model)
        {
            var oldUser = await _repository.GetUserById(id);

            if (oldUser == null)
            {
                return NotFound($"Couldn't find user with id of {id}");
            }


            if (!User.IsInRole(Role.Admin) && User.Identity.Name != id.ToString())
            {
                return Forbid("Only admin can change other users data.");
            }

            if (oldUser.Username != model.Username)
            {
                if (await _repository.GetUserByUsername(model.Username) != null)
                {
                    return BadRequest("User with this username already exists!");
                }
            }

            if (!string.IsNullOrEmpty(model.Password))
            {
                byte[] passwordHash, passwordSalt;
                PasswordHash.Create(model.Password, out passwordHash, out passwordSalt);

                oldUser.PasswordHash = passwordHash;
                oldUser.PasswordSalt = passwordSalt;
            }

            _mapper.Map(model, oldUser);

            if (await _repository.SaveChangesAsync())
            {
                return _mapper.Map<UserModel>(oldUser);
            }

            return BadRequest();
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
        /// <response code="404">User with given id was not found.</response>
        /// <response code="403">You aren't user with given id.</response>
        /// <response code="400">You didn't provide enough data.</response>
        /// <response code="200">Returns ok if user deleted.</response>
        /// <response code="401">You aren't authenticated. Please authenticate first.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var oldUser = await _repository.GetUserById(id);

            var user = User.Identity.Name;

            if (oldUser == null)
            {
                return NotFound();
            }

            if (!User.IsInRole(Role.Admin) && oldUser.Id.ToString() != user)
            {
                return Forbid("Only admin can delete other users.");
            } 

            _repository.Delete(oldUser);

            if (await _repository.SaveChangesAsync())
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}