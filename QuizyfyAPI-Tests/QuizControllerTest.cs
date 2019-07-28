using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using QuizyfyAPI.Controllers;
using QuizyfyAPI.Data;
using QuizyfyAPI.Models;
using QuizyfyAPI_Tests.Fakes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace QuizyfyAPI_Tests
{
    public class QuizControllerTest
    {/*
        QuizzesController _quizzesController;
        QuizRepositoryFake _quizRepository;
        IMapper _mapper;


        //TODO:test if includeQuestions is working correctly


        public QuizControllerTest()
        {
            _quizRepository = new QuizRepositoryFake();

            var configuration = new MapperConfiguration(config =>
            {
                config.AddProfile(new QuizProfile());
            });

            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            var memoryCache = serviceProvider.GetService<IMemoryCache>();

            _mapper = configuration.CreateMapper();
            _quizzesController = new QuizzesController(_quizRepository, _mapper, memoryCache);
        }

        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Returns_NoContent_If_Db_Is_Empty()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            await _quizRepository.SaveChangesAsync();

            var result = await _quizzesController.Get();
            //Assert
            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Return_Ok_If_Db_Contains_Quiz()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            _quizRepository.Add(new Quiz()
            {
                Id = 100,
                Name = "Elo",
                DateAdded = DateTime.Now,
                Questions = null
            });

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _quizzesController.Get();

            //Assert
            Assert.IsType<QuizModel[]>(result.Value);
        }

        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Returns_All_Quizes()
        {

            Quiz tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            Quiz tempQuiz2 = new Quiz() { Id = 102, Name = "Elo", DateAdded = DateTime.Now };
            //Arrange
            _quizRepository.Empty<Quiz>();
            _quizRepository.Add(tempQuiz);
            _quizRepository.Add(tempQuiz2);

            await _quizRepository.SaveChangesAsync();

            //Act

            ActionResult<QuizModel[]> result = null;
            //Act
            for (int i = 0; i < 1000000; i++)
            {
                result = await _quizzesController.Get();
            }
            //Assert
            Assert.True(result.Value.Length == 2);
        }

        [Fact]
        [Trait("Category", "GetId")]
        public async Task GetId_Returns_Not_Found_If_Quiz_Was_Not_Found()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _quizzesController.Get(1);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "GetId")]
        public async Task GetId_Returns_Ok_If_Quiz_Was_Found()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var quiz = new Quiz() { Id = 102, DateAdded = DateTime.Now, Name = "w/e" };

            _quizRepository.Add(quiz);

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _quizzesController.Get(102);

            //Assert
            Assert.IsType<QuizModel>(result.Value);
        }

        [Fact]
        [Trait("Category", "GetId")]
        public async Task GetId_Returns_One_Quiz()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var quiz = new Quiz() { Id = 102, DateAdded = DateTime.Now, Name = "w/e" };

            _quizRepository.Add(quiz);

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _quizzesController.Get(102);

            //Assert
            Assert.True(result.Value.Name == quiz.Name);
        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Return_CreatedAtAction_If_Quiz_Was_Created()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _quizzesController.Post(new QuizCreateModel { Name = "Nowy", Questions = null });

            //Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);

        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Return_BadRequest_If_No_Input_Was_Supplied()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _quizzesController.Post(null);

            //Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Creates_Quiz()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            await _quizRepository.SaveChangesAsync();

            var quizModel = new QuizCreateModel { Name = "Nowy", Questions = null };

            //Act
            await _quizzesController.Post(quizModel);

            var getResult = await _quizRepository.GetQuizzes();

            //Assert
            Assert.True(getResult.Length > 0);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_Ok_And_Updated_Model_If_Sucessfully_Updated()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });

            await _quizRepository.SaveChangesAsync();

            var result = await _quizzesController.Put(1, new QuizCreateModel { Name = "Dsa", Questions = null });

            //Assert
            Assert.IsType<QuizModel>(result.Value);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_NotFound_If_Quiz_Was_Not_Found()
        {
            _quizRepository.Empty<Quiz>();

            await _quizRepository.SaveChangesAsync();

            var result = await _quizzesController.Put(1, new QuizCreateModel { Name = "Dsa", Questions = null });

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_BadRequest_If_Passed_Null()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });

            await _quizRepository.SaveChangesAsync();

            var result = await _quizzesController.Put(1, null);

            //Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Updates_Quiz()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });

            await _quizRepository.SaveChangesAsync();
            var newQuiz = new QuizCreateModel { Name = "New", Questions = null };

            var result = await _quizzesController.Put(1, newQuiz);

            //Assert
            Assert.True(result.Value.Name == newQuiz.Name);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Returns_NotFound_If_Quiz_Was_Not_Found()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _quizzesController.Delete(1);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Return_Ok_If_Quiz_Was_Deleted()
        {
            //Arrange
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _quizzesController.Delete(1);

            //Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Deletes_Quiz()
        {
            _quizRepository.Empty<Quiz>();

            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });

            await _quizRepository.SaveChangesAsync();

            await _quizzesController.Delete(1);

            var result = await _quizzesController.Get();

            Assert.IsType<NoContentResult>(result.Result);
        }*/
    }
}
