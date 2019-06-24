using System;
using System.Collections.Generic;
using System.Linq;
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserModel>> Login(UserModel model)
        {
            var user = await _repository.Authenticate(model.Username, model.Password);

            if (user != null)
            {
                user = JWTHelper.RequestToken(user, _appSettings);
            }

            return _mapper.Map<UserModel>(user);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserModel>> Post(UserModel model)
        {
            // map dto to entity
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
                return CreatedAtAction(nameof(Login), "", model);
            }

            return BadRequest();
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<UserModel>> Put(int id, UserModel model)
        {
            var oldUser = await _repository.GetUserById(id);

            if (oldUser == null)
            {
                return NotFound($"Couldn't find user with id of {id}");
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


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var oldUser = await _repository.GetUserById(id);

            if(oldUser == null)
            {
                return NotFound();
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