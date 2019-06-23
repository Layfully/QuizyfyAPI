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
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizRepository _repository;
        private readonly IMapper _mapper;


        public QuizzesController(IQuizRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<QuizModel[]>> Get()
        {
            try
            {
                var results = await _repository.GetAllQuizzesAsync();

                return _mapper.Map<QuizModel[]>(results);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuizModel>> Get(int id)
        {
            try
            {
                var result = await _repository.GetQuizAsync(id);

                if(result == null)
                {
                    return NotFound();
                }

                return _mapper.Map<QuizModel>(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
