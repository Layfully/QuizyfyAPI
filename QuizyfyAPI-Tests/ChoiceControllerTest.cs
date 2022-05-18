using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Controllers;
using QuizyfyAPI.Data;
using QuizyfyAPI_Tests.Fakes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace QuizyfyAPI_Tests
{
    public class ChoiceControllerTest
    {
        ChoicesController _choiceController;

        ChoiceRepositoryFake _choiceRepository;
        QuestionRepositoryFake _questionRepository;
        QuizRepositoryFake _quizRepository;

        IMapper _mapper;

        public ChoiceControllerTest()
        {
            _questionRepository = new QuestionRepositoryFake();
            _quizRepository = new QuizRepositoryFake();
            _choiceRepository = new ChoiceRepositoryFake();

            var configuration = new MapperConfiguration(config =>
            {
                config.AddProfile(new ChoiceProfile());
            });

            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            var memoryCache = serviceProvider.GetService<IMemoryCache>();

            _mapper = configuration.CreateMapper();
           // _choiceController = new ChoicesController(_choiceRepository, _quizRepository, _questionRepository, _mapper, memoryCache);
        }

        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Returns_NoContent_If_Db_Is_Empty()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();
            _questionRepository.Empty<Question>();

            var quiz = new Quiz { Id = 1, DateAdded = DateTime.Now, Name = "Test" };
            var question = new Question { Id = 2, QuizId = 1, Text = "Pytanko" };
            _quizRepository.Add(quiz);
            _questionRepository.Add(question);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Get(1, 2);

            //Assert
            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Returns_NotFound_If_Question_Or_Quiz_Is_Not_Existing()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();
            _questionRepository.Empty<Question>();

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Get(1, 2);

            //Assert 
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Return_Ok_If_Question_Contains_Choices()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var quiz = new Quiz()
            {
                Id = 100,
                Name = "Elo",
                DateAdded = DateTime.Now,
            };

            var question = new Question { Id = 5, QuizId = 100, Text = "Pytanko" };

            var choice = new Choice { Id = 2, QuestionId = 5, IsRight = true, Text = "Odp1." };

            _quizRepository.Add(quiz);
            _questionRepository.Add(question);
            _choiceRepository.Add(choice);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Get(100, 5);

            //Assert
            Assert.IsType<ChoiceResponse[]>(result.Value);
        }
        
        [Fact]
        [Trait("Category", "Get")]
        public async Task Get_Returns_All_Choices()
        {

            var quiz = new Quiz()
            {
                Id = 100,
                Name = "Elo",
                DateAdded = DateTime.Now,
            };

            var question = new Question { Id = 5, QuizId = 100, Text = "Pytanko" };

            var choice = new Choice { Id = 2, QuestionId = 5, IsRight = true, Text = "Odp1." };
            var choice1 = new Choice { Id = 3, QuestionId = 5, IsRight = true, Text = "Odp2." };

            //Arrange

            _quizRepository.Add(quiz);
            _questionRepository.Add(question);

            _choiceRepository.Add(choice);
            _choiceRepository.Add(choice1);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            ActionResult<ChoiceResponse[]> result = null;

            for (int i = 0; i < 10000; i++)
            {
                result = await _choiceController.Get(100, 5);

                //Assert
            }
            Assert.True(result.Value.Length == 2);

            //Act
        }

        [Fact]
        [Trait("Category", "GetId")]
        public async Task GetId_Returns_NotFound_If_Choice_Was_Not_Found()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            var tempQuestion = new Question { QuizId = 101, Id = 200, Text = "Gi" };

            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(tempQuestion);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Get(101, 200, 1);

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
            var question = new Question() { Id = 150, QuizId = 101, Text = "pytanko" };
            var choice = new Choice() {QuestionId = 150, Id = 200, Text = "asd"};

            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(question);
            _choiceRepository.Add(choice);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Get(101, 150, 200);

            //Assert
            Assert.IsType<ChoiceResponse>(result.Value);
        }

        [Fact]
        [Trait("Category", "GetId")]
        public async Task GetId_Returns_One_Choice()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            var question = new Question() { Id = 150, QuizId = 101, Text = "pytanko" };
            var choice = new Choice() { QuestionId = 150, Id = 200, Text = "asd" };

            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(question);
            _choiceRepository.Add(choice);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Get(101, 150, 200);

            //Assert
            Assert.True(result.Value.Text == choice.Text);
        }
        [Fact]
        [Trait("Category", "GetId")]
        public async Task GetId_Fails_If_Quiz_Does_Not_Exist()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            var question = new Question() { Id = 150, QuizId = 101, Text = "pytanko" };
            var choice = new Choice() { QuestionId = 150, Id = 200, Text = "asd" };

            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(question);
            _choiceRepository.Add(choice);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Get(100, 150, 200);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "GetId")]
        public async Task GetId_Fails_If_Question_Does_Not_Exist()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            var question = new Question() { Id = 150, QuizId = 101, Text = "pytanko" };
            var choice = new Choice() { QuestionId = 150, Id = 200, Text = "asd" };

            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(question);
            _choiceRepository.Add(choice);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Get(101, 151, 200);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Return_BadRequest_If_Quiz_Does_Not_Exist()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Post(101, 50, new ChoiceCreateRequest { Text = "Hello", IsRight = false });

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);

        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Return_BadRequest_If_No_Input_Was_Supplied()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            var question = new Question { Id = 2, Text = "asd", QuizId = 101 };
            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(question);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Post(101, 2, null);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);

        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Return_BadRequest_If_Question_Does_Not_Exist()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            _quizRepository.Add(new Quiz { DateAdded = DateTime.Now, Id = 1, Name = "Elo" });

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Post(1, 50, new ChoiceCreateRequest { Text = "Hello", IsRight = false });

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);

        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Returns_CreatedAtAction_If_Choice_Created()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            var tempQuestion = new Question() { Id = 102, QuizId = 101, Text = "Asd" };

            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(tempQuestion);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            var choice = new ChoiceCreateRequest { Text = "asd", IsRight = true };
            //Act
            var result = await _choiceController.Post(101, 102, choice);

            //Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Post")]
        public async Task Post_Creates_Choice()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            var tempQuiz = new Quiz() { Id = 101, Name = "Elo", DateAdded = DateTime.Now };
            var tempQuestion = new Question() { Id = 102, QuizId = 101, Text = "Asd"};

            _quizRepository.Add(tempQuiz);
            _questionRepository.Add(tempQuestion);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();

            var choice = new ChoiceCreateRequest { Text = "asd", IsRight = true };
            //Act
            var result = await _choiceController.Post(101, 102, choice);

            //Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);

            var getResult = await _choiceController.Get(101, 102);

            //Assert
            Assert.True(getResult.Value.Length > 0);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_Ok_And_Updated_Model_If_Sucessfully_Updated()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });
            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytankoo"});
            _choiceRepository.Add(new Choice { Id = 3, QuestionId = 2, Text = "Pytankoo"});

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            var result = await _choiceController.Put(1, 2, 3, new ChoiceUpdateRequest { Text = "Pytanie" });

            //Assert
            Assert.IsType<ChoiceResponse>(result.Value);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_NotFound_If_Quiz_Was_Not_Found()
        {
            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytankoo" });
            _choiceRepository.Add(new Choice { Id = 3, QuestionId = 2, Text = "Pytankoo" });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            var result = await _choiceController.Put(1, 2, 3, new ChoiceUpdateRequest { Text = "Pytanie" });

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_NotFound_If_Question_Was_Not_Found()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });
            _choiceRepository.Add(new Choice { Id = 3, QuestionId = 2, Text = "Pytankoo" });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            var result = await _choiceController.Put(1, 2, 3, new ChoiceUpdateRequest { Text = "Pytanie" });

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_NotFound_If_Choice_Was_Not_Found()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });
            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytankoo" });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            var result = await _choiceController.Put(1, 2, 3, new ChoiceUpdateRequest { Text = "Pytanie" });

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Returns_BadRequest_If_Passed_Null()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });
            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytankoo" });
            _choiceRepository.Add(new Choice { Id = 3, QuestionId = 2, Text = "Pytankoo" });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();


            var result = await _choiceController.Put(1, 2, 3, null);

            //Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Put")]
        public async Task Put_Updates_Question()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });
            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytankoo" });
            _choiceRepository.Add(new Choice { Id = 3, QuestionId = 2, Text = "Pytankoo" });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();


            var newChoice = new ChoiceUpdateRequest { Text = "Pytanie na sniadanie" };

            var result = await _choiceController.Put(1, 2, 3, newChoice);

            //Assert
            Assert.True(result.Value.Text == newChoice.Text);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Deletes_Choice()
        {
            _quizRepository.Add(new Quiz { Id = 1, Name = "asd", DateAdded = DateTime.Now });
            _questionRepository.Add(new Question { Id = 2, QuizId = 1, Text = "Pytanko" });
            _choiceRepository.Add(new Choice { Id = 3, QuestionId = 2, Text = "Odp", IsRight = true });

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            await _choiceController.Delete(1, 2, 3);

            var result = await _choiceController.Get(1, 2);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Return_Ok_If_Choice_Was_Deleted()
        {
            var quiz = (new Quiz { Id = 200, Name = "asd", DateAdded = DateTime.Now });

            var question = new Question
            {
                Id = 300,
                QuizId = quiz.Id,
                Text = "Pytanko"
            };

            var choice = new Choice { Id = 400, QuestionId = question.Id, Text = "Odp", IsRight = true };

            _quizRepository.Add(quiz);
            _questionRepository.Add(question);
            _choiceRepository.Add(choice);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            var result = await _choiceController.Delete(200, 300, 400);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Returns_NotFound_If_Choice_Was_Not_Found()
        {
            //Arrange
            _quizRepository.Empty<Quiz>();

            await _quizRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Delete(1, 1, 1);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Returns_NotFound_If_Quiz_Was_Not_Found()
        {

            var question = new Question
            {
                Id = 300,
                QuizId = 1,
                Text = "Pytanko"
            };

            var choice = new Choice { Id = 400, QuestionId = question.Id, Text = "Odp", IsRight = true };

            _questionRepository.Add(question);
            _choiceRepository.Add(choice);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Delete(1, 300, 400);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        [Trait("Category", "Delete")]
        public async Task Delete_Returns_NotFound_If_Question_Was_Not_Found()
        {

            var quiz = (new Quiz { Id = 200, Name = "asd", DateAdded = DateTime.Now });


            var choice = new Choice { Id = 400, QuestionId = 300, Text = "Odp", IsRight = true };

            _quizRepository.Add(quiz);
            _choiceRepository.Add(choice);

            await _quizRepository.SaveChangesAsync();
            await _questionRepository.SaveChangesAsync();
            await _choiceRepository.SaveChangesAsync();

            //Act
            var result = await _choiceController.Delete(200, 300, 400);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

    }
}
