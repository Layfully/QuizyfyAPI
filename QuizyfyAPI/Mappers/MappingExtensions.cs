using QuizyfyAPI.Data.Entities;

namespace QuizyfyAPI.Mappers;

internal static class MappingExtensions
{
    // --- CHOICE EXTENSIONS ---
    
    extension(Choice choice)
    {
        public ChoiceResponse ToResponse()
        {
            return new ChoiceResponse
            {
                Id = choice.Id,
                Text = choice.Text,
                IsRight = choice.IsRight
            };
        }
        
        public void UpdateFrom(ChoiceUpdateRequest request)
        {
            if (request.Text is not null)
            {
                choice.Text = request.Text;
            }
            
            if (request.IsRight.HasValue)
            {
                choice.IsRight = request.IsRight.Value;
            }
        }
    }
    
    extension(ChoiceCreateRequest request)
    {
        public Choice ToEntity()
        {
            return new Choice
            {
                Text = request.Text,
                IsRight = request.IsRight
            };
        }
    }
    
    // --- IMAGE EXTENSIONS ---
    extension(Image image)
    {
        public ImageResponse ToResponse()
        {
            return new ImageResponse
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl
            };
        }
    }
    
    // --- LIKE EXTENSIONS ---

    extension(Like like)
    {
        public LikeResponse ToResponse()
        {
            return new LikeResponse
            {
                QuizId = like.QuizId,
                UserId = like.UserId
            };
        }
    }
    
    // --- QUESTION EXTENSIONS ---

    extension(Question question)
    {
        public QuestionResponse ToResponse()
        {
            return new QuestionResponse
            {
                Id = question.Id,
                Text = question.Text,
                ImageUrl = question.Image?.ImageUrl,
                Choices = question.Choices.Select(c => c.ToResponse()).ToList()
            };
        }
        
        public void UpdateFrom(QuestionUpdateRequest request)
        {
            if (request.Text is not null)
            {
                question.Text = request.Text;
            }
        }
    }
    
    extension(QuestionCreateRequest request)
    {
        public Question ToEntity()
        {
            return new Question
            {
                Text = request.Text
            };
        }
    }
    
    // --- QUIZ EXTENSIONS ---

    extension(Quiz quiz)
    {
        public QuizResponse ToResponse()
        {
            return new QuizResponse
            {
                Id = quiz.Id,
                Name = quiz.Name,
                Description = quiz.Description,
                DateAdded = quiz.DateAdded,
                ImageUrl = quiz.Image?.ImageUrl,
                Questions = quiz.Questions.Select(q => q.ToResponse()).ToList()
            };
        }
        
        public void UpdateFrom(QuizUpdateRequest request)
        {
            if (request.Name is not null)
            {
                quiz.Name = request.Name;
            }
            
            if (request.Description is not null)
            {
                quiz.Description = request.Description;
            }
        }
    }

    extension(QuizCreateRequest request)
    {
        public Quiz ToEntity()
        {
            return new Quiz
            {
                Name = request.Name,
                Description = request.Description
            };
        }
    }

    
    // --- USER EXTENSIONS ---

    extension(User user)
    {
        public UserResponse ToResponse()
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                JwtToken = user.JwtToken,
                RefreshToken = user.RefreshToken?.Token
            };
        }

        public void UpdateFrom(UserUpdateRequest request)
        {
            if (request.Username is not null)
            {
                user.Username = request.Username;
            }

            if (request.Email is not null)
            {
                user.Email = request.Email;
            }

            if (request.FirstName is not null)
            {
                user.FirstName = request.FirstName;
            }

            if (request.LastName is not null)
            {
                user.LastName = request.LastName;
            }
        }
    }

    extension(UserRegisterRequest request)
    {
        public User ToEntity()
        {
            return new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = request.Role,
                PasswordHash = [],
                PasswordSalt = []
            };
        }
    }
}