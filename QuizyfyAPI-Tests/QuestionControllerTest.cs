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
    public class QuestionControllerTest
    {
        QuestionsController _questionsController;
        QuestionRepositoryFake _questionRepository;
        QuizRepositoryFake _quizRepository;
        ChoiceRepositoryFake _choiceRepository;
        IMapper _mapper;

        //TODO:test if includeChoices is working correctly

        public QuestionControllerTest()
        {
            _questionRepository = new QuestionRepositoryFake();
            _quizRepository = new QuizRepositoryFake();
            _choiceRepository = new ChoiceRepositoryFake();

            var configuration = new MapperConfiguration(config =>
            {
                config.AddProfile(new QuestionProfile());
            });


            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            var memoryCache = serviceProvider.GetService<IMemoryCache>();

            _mapper = configuration.CreateMapper();
            _questionsController = new QuestionsController(_questionRepository, _choiceRepository, _quizRepository ,_mapper, memoryCache);
        }
        
        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Returns_NoContent_If_Db_Is_Empty()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();
            _questionRepository.Empty<Question>();

            var quiz = new Quiz {Id = 1, DateAdded = DateTime.Now, Name = "Test", Questions = null };

            _quizRepository.Add(quiz);

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _questionsController.Get(1);

            //Assert
            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Returns_NotFound_If_Quiz_Is_Not_Existing()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();
            _questionRepository.Empty<Question>();

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _questionsController.Get(1);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
        
        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Return_Ok_If_Quiz_Contains_Questions()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var quiz = new Quiz()
            {
                Id = 100,
                Name = "Elo",
                DateAdded = DateTime.Now,
                Questions = null
            };

            var question = new Question() { Id = 1, QuizId = 100, Text = "Pytanko", Choices = null };

            _quizRepository.Add(quiz);
            _questionRepository.Add(question);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            //Act
            var result = await _questionsController.Get(100);

            //Assert
            Assert.IsType<QuestionModel[]>(result.Value);
        }

        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Returns_All_Questions()
        {

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };

            var question = new Question() { Id = 1, QuizId = 101, Text = "Pytanko", Choices = null };

            var question1 = new Question() { Id = 2, QuizId = 101, Text = "Pytanko1", Choices = null };

            //Arrange
            _quizRepository.Empty<Quiz>();
            _questionRepository.Empty<Question>();

            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(question);
            _questionRepository.Add(question1);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            //Act
            var result = await _questionsController.Get(101);

            //Assert
            Assert.True(result.Value.Length == 2);
        }
        
        [Fact]
        [Trait("Category", "GetId")]
        public async Task GetId_Returns_NotFound_If_Question_Was_Not_Found()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };

            _quizRepository.Add(tempQuiz);

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _questionsController.Get(101, 1, false);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "GetId")]
        public async Task GetId_Returns_Ok_And_Model_If_Question_Was_Found()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            var question = new Question() { Id = 1, QuizId = 101, Text = "pytanko", Choices = null};

            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(question);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            //Act
            var result = await _questionsController.Get(101, 1, false);

            //Assert
            Assert.IsType<QuestionModel>(result.Value);
        }

        [Fact]
        [Trait("Category", "GetId")]
        public async Task GetId_Returns_One_Question()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            var question = new Question() { Id = 1, QuizId = 101, Text = "pytanko", Choices = null };

            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(question);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            //Act
            var result = await _questionsController.Get(101, 1, false);

            //Assert
            Assert.True(result.Value.Text == question.Text);
        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Return_CreatedAtAction_If_Question_Was_Created()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            _quizRepository.Add(tempQuiz);

            var question = new QuestionCreateModel() { Text = "Pytanko1" };
                
            await _quizRepository.SaveChangesAsync();

            //Act
            //var result = await _questionsController.Post(101, question);

            //Assert
           // Assert.IsType<CreatedAtActionResult>(result.Result);

        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Return_BadRequest_If_Quiz_Does_Not_Exist()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var question = new QuestionCreateModel() { Text = "Pytanko1" };

            await _quizRepository.SaveChangesAsync();

            //Act
            //var result = await _questionsController.Post(101, question);

            //Assert
           // Assert.IsType<BadRequestObjectResult>(result.Result);

        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Return_BadRequest_If_No_Input_Was_Supplied()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            _quizRepository.Add(tempQuiz);

            var question = new QuestionCreateModel() { Text = "Pytanko1" };

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _questionsController.Post(101, null);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);

        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Creates_Question()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            _quizRepository.Add(tempQuiz);

            var question = new QuestionCreateModel() { Text = "Pytanko1" };

            await _quizRepository.SaveChangesAsync();

            //Act
            //var result = await _questionsController.Post(101, question);

            //Assert
           // Assert.IsType<CreatedAtActionResult>(result.Result);

            var getResult = await _questionRepository.GetQuestions(101);

            //Assert
            Assert.True(getResult.Length > 0);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_Ok_And_Updated_Model_If_Sucessfully_Updated()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });
            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytankoo", Choices = null });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            var result = await _questionsController.Put(1, 2, new QuestionCreateModel { Text = "Pytanie" });

            //Assert
            Assert.IsType<QuestionModel>(result.Value);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_NotFound_If_Question_Was_Not_Found()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            var result = await _questionsController.Put(1, 2, new QuestionCreateModel { Text = "Pytanie" });

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_BadRequest_If_Passed_Null()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });
            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytankoo", Choices = null });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            var result = await _questionsController.Put(1, 2, null);

            //Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Updates_Question()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });
            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytankoo", Choices = null });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            var newQuestion = new QuestionCreateModel { Text = "Pytanie na sniadanie"};
            
            var result = await _questionsController.Put(1, 2, newQuestion);

            //Assert
            Assert.True(result.Value.Text == newQuestion.Text);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Deletes_Question()
        {
            _quizRepository.Empty<Quiz>();

            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });

            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytanko", Choices = null });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            await _questionsController.Delete(1, 2);

            var result = await _questionsController.Get(1);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Return_Ok_If_Question_Was_Deleted()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });

            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytanko", Choices = null });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            var result = await _questionsController.Delete(1, 2);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Returns_NotFound_If_Question_Was_Not_Found()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _questionsController.Delete(1, 1);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
