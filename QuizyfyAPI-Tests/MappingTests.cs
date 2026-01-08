using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Data.Entities;
using QuizyfyAPI.Mappers;
using Xunit;

namespace QuizyfyAPI_Tests;

public class MappingTests
{
    // --- QUIZ ---
    
    [Fact]
    public void Quiz_ToResponse_MapsCorrectly()
    {
        Quiz quiz = new()
        { 
            Id = 1, 
            Name = "Test", 
            Description = "Desc", 
            DateAdded = DateTime.Parse("2025-01-01", System.Globalization.CultureInfo.InvariantCulture),
            Image = new Image { ImageUrl = "http://img.com/1.png" }
        };

        QuizResponse result = quiz.ToResponse();

        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.Equal("http://img.com/1.png", result.ImageUrl);
        Assert.Equal(quiz.DateAdded, result.DateAdded);
    }
    
    [Fact]
    public void Quiz_ToResponse_HandlesNullCollections()
    {
        Quiz quizEmpty = new() { Name = "Test", Description = "D", Questions = [] };
        QuizResponse response = quizEmpty.ToResponse();

        Assert.NotNull(response.Questions);
        Assert.Empty(response.Questions);
    }

    [Fact]
    public void QuizCreateRequest_ToEntity_MapsCorrectly()
    {
        QuizCreateRequest request = new()
        {
            Name = "New",
            Description = "Desc",
            ImageUrl = "null",
            Questions = []
        };
        
        Quiz entity = request.ToEntity();

        Assert.Equal("New", entity.Name);
        Assert.Equal("Desc", entity.Description);
        Assert.Empty(entity.Questions);
    }
    
    [Fact]
    public void Quiz_UpdateFrom_PartialUpdate_RespectsNulls()
    {
        Quiz quiz = new() { Name = "Old", Description = "Old Desc" };
        QuizUpdateRequest request = new() { Name = "New Name", Description = null };

        quiz.UpdateFrom(request);

        Assert.Equal("New Name", quiz.Name);
        Assert.Equal("Old Desc", quiz.Description);
    }
    
    // --- USER ---

    [Fact]
    public void User_ToResponse_HidesSecrets()
    {
        User user = new()
        {
            Id = 99,
            Username = "admin",
            Email = "admin@test.com",
            PasswordHash = [1, 2, 3],
            PasswordSalt = [4, 5, 6],
            RefreshToken = new RefreshToken { Token = "secret_refresh", JwtId = "j1" },
            Role = Role.User
        };

        UserResponse response = user.ToResponse();

        Assert.Equal("admin", response.Username);
        Assert.Equal("secret_refresh", response.RefreshToken);
        Assert.Equal(99, response.Id);
    }

    [Fact]
    public void User_ToResponse_HandlesNullRefreshToken()
    {
        User user = new()
        { 
            Username = "u", Email = "e", Role = "r", 
            PasswordHash = [], PasswordSalt = [],
            RefreshToken = null
        };

        UserResponse response = user.ToResponse();

        Assert.Null(response.RefreshToken);
    }
    
    [Fact]
    public void UserRegister_ToEntity_SetsDefaults()
    {
        UserRegisterRequest request = new()
        {
            Username = "user",
            Email = "a@b.com",
            Password = "pw",
            RecaptchaToken = "token",
            Role = "null",
        };

        User entity = request.ToEntity();

        Assert.Empty(entity.PasswordHash);
        Assert.Empty(entity.PasswordSalt);
    }
    
    [Fact]
    public void User_UpdateFrom_PartialUpdate_RespectsNulls()
    {
        User user = new()
        {
            FirstName = "Old",
            LastName = "Old",
            Username = "null",
            Email = "null",
            Role = "null",
            PasswordHash = [],
            PasswordSalt = []
        };
        
        UserUpdateRequest request = new() { FirstName = "New", LastName = null };

        user.UpdateFrom(request);

        Assert.Equal("New", user.FirstName);
        Assert.Equal("Old", user.LastName);
    }
    
    // --- QUESTION ---

    [Fact]
    public void Question_ToResponse_MapsChoices()
    {
        Question question = new()
        {
            Id = 5,
            Text = "Q?",
            Choices = 
            [
                new Choice { Id = 1, Text = "A", IsRight = true },
                new Choice { Id = 2, Text = "B", IsRight = false }
            ]
        };

        QuestionResponse response = question.ToResponse();

        Assert.Equal(2, response.Choices.Count);
        Assert.True(response.Choices.First().IsRight);
    }
    
    [Fact]
    public void QuestionCreateRequest_ToEntity_MapsCorrectly()
    {
        QuestionCreateRequest request = new() { Text = "Q1", ImageId = 10, Choices = [] };
        Question entity = request.ToEntity();
        Assert.Equal("Q1", entity.Text);
    }

    [Fact]
    public void Question_UpdateFrom_UpdatesText()
    {
        Question question = new() { Text = "Old" };
        QuestionUpdateRequest request = new() { Text = "New" };

        question.UpdateFrom(request);

        Assert.Equal("New", question.Text);
    }

    // --- CHOICE ---

    [Fact]
    public void ChoiceCreateRequest_ToEntity_MapsCorrectly()
    {
        ChoiceCreateRequest request = new() { Text = "C1", IsRight = true };
        Choice entity = request.ToEntity();
        Assert.Equal("C1", entity.Text);
        Assert.True(entity.IsRight);
    }

    [Fact]
    public void Choice_UpdateFrom_HandlesBooleans()
    {
        Choice choice = new() { Text = "A", IsRight = true };
        
        ChoiceUpdateRequest request1 = new() { Text = "B", IsRight = null };
        choice.UpdateFrom(request1);
        Assert.Equal("B", choice.Text);
        Assert.True(choice.IsRight);

        ChoiceUpdateRequest request2 = new() { Text = null, IsRight = false };
        choice.UpdateFrom(request2);
        Assert.Equal("B", choice.Text);
        Assert.False(choice.IsRight);
    }
    
    // --- OTHERS ---

    [Fact]
    public void Image_ToResponse_MapsCorrectly()
    {
        Image image = new() { Id = 10, ImageUrl = "/test.jpg" };
        ImageResponse response = image.ToResponse();
        Assert.Equal(10, response.Id);
        Assert.Equal("/test.jpg", response.ImageUrl);
    }

    [Fact]
    public void Like_ToResponse_MapsCorrectly()
    {
        Like like = new() { QuizId = 1, UserId = 2 };
        LikeResponse response = like.ToResponse();
        Assert.Equal(1, response.QuizId);
        Assert.Equal(2, response.UserId);
    }
}