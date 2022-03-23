using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Controllers;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services;
public class QuestionService : IQuestionService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly IChoiceService _choiceService;

    public QuestionService(IQuizRepository quizRepository, IQuestionRepository questionRepository, IMemoryCache cache, IMapper mapper, IChoiceService choiceService)
    {
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
        _cache = cache;
        _mapper = mapper;
        _choiceService = choiceService;
    }

    public async Task<ObjectResult<QuestionResponse[]>> GetAll(int quizId, bool includeChoices)
    {
        if (!_cache.TryGetValue("Questions", out Question[] questions))
        {
            questions = await _questionRepository.GetQuestions(quizId, includeChoices);
            _cache.Set("Questions", questions);
        }

        if (questions.Length == 0)
        {
            if (await _quizRepository.GetQuiz(quizId) == null)
            {
                return new ObjectResult<QuestionResponse[]> { Errors = new[] { "Quiz with this id doesn't exist" } };
            }
            return new ObjectResult<QuestionResponse[]> { Found = true };
        }
        return new ObjectResult<QuestionResponse[]> { Found = true, Success = true, Object = _mapper.Map<QuestionResponse[]>(questions) };
    }

    public async Task<ObjectResult<QuestionResponse>> Get(int quizId, int questionId, bool includeChoices = false)
    {
        if (!_cache.TryGetValue("$Choice {choiceId}", out Question question))
        {
            question = await _questionRepository.GetQuestion(quizId, questionId, includeChoices);
            _cache.Set("$Question {questionId}", question);
        }

        if (question == null)
        {
            return new ObjectResult<QuestionResponse> { Errors = new[] { "Couldn't find this question" } };
        }
        return new ObjectResult<QuestionResponse> { Object = _mapper.Map<QuestionResponse>(question), Found = true, Success = true };
    }

    public async Task<ObjectResult<QuestionResponse>> Create(int quizId, QuestionCreateRequest request)
    {
        var quiz = await _quizRepository.GetQuiz(quizId);

        if (quiz == null)
        {
            return new ObjectResult<QuestionResponse> { Errors = new[] { "Couldn't find this quiz" } };
        }

        var question = _mapper.Map<Question>(request);

        if (question != null)
        {
            question.QuizId = quiz.Id;

            _questionRepository.Add(question);
        }

        await _questionRepository.SaveChangesAsync();

        var choicesController = new ChoicesController(_choiceService);

        foreach (var choice in request.Choices)
        {
            await choicesController.Post(quiz.Id, question.Id, choice);
        }

        _cache.Set($"Question {question.Id}", question);

        if (await _questionRepository.SaveChangesAsync())
        {
            return new ObjectResult<QuestionResponse> { Success = true, Found = true, Object = _mapper.Map<QuestionResponse>(question)  };
        }
        return new ObjectResult<QuestionResponse> { Found = true, Errors = new[] { "Action didn't affect any rows" } };
    }

    public async Task<ObjectResult<QuestionResponse>> Update(int quizId, int questionId, QuestionUpdateRequest request)
    {
        var question = await _questionRepository.GetQuestion(quizId, questionId, true);

        if (question == null)
        {
            return new ObjectResult<QuestionResponse> { Errors = new[] { "Couldn't find question" } };
        }

        _questionRepository.Update(question);

        question = _mapper.Map<Question>(request);

        if (await _questionRepository.SaveChangesAsync())
        {
            _cache.Set($"Question {question.Id}", question);
            return new ObjectResult<QuestionResponse> { Object = _mapper.Map<QuestionResponse>(question), Found = true, Success = true };
        }
        return new ObjectResult<QuestionResponse> { Found = true, Errors = new[] { "Action didn't affect any rows" } };
    }

    public async Task<DetailedResult> Delete(int quizId, int questionId)
    {
        var question = await _questionRepository.GetQuestion(quizId, questionId, false);

        if (question == null)
        {
            return new DetailedResult { Errors = new[] { "Failed to find the question to delete" } };
        }

        _questionRepository.Delete(question);

        if (await _questionRepository.SaveChangesAsync())
        {
            _cache.Remove($"Question {questionId}");
            return new DetailedResult { Success = true, Found = true };
        }

        return new DetailedResult { Found = true, Errors = new[] { "Action didn't affect any rows" } };
    }
}
