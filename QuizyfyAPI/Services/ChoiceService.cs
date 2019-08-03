using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;
using QuizyfyAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public class ChoiceService : IChoiceService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IChoiceRepository _choiceRepository;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;

        public ChoiceService(IQuestionRepository questionRepository,
                             IChoiceRepository choiceRepository,
                             IMemoryCache cache,
                             IMapper mapper)
        {
            _questionRepository = questionRepository;
            _choiceRepository = choiceRepository;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<ObjectResult<ChoiceModel[]>> GetAll(int quizId, int questionId)
        {
            var question = await _questionRepository.GetQuestion(quizId, questionId, true);

            if (question == null)
            {
                return new ObjectResult<ChoiceModel[]> { Errors = new[] { "Failed to find the choice" } };
            }

            if (!_cache.TryGetValue("Choices", out ICollection<Choice> choices))
            {
                choices = question.Choices;
                _cache.Set("Choices", question.Choices);
            }

            if (choices.Count == 0)
            {
                return new ObjectResult<ChoiceModel[]>{ Found = true, Errors = new[] { "This question doesn't have any choices" } };
            }

            return new ObjectResult<ChoiceModel[]> { Object = _mapper.Map<ChoiceModel[]>(choices).ToArray(), Found = true, Success = true };
        }

        public async Task<ObjectResult<ChoiceModel>> Get(int quizId, int questionId, int choiceId)
        {
            var question = await _questionRepository.GetQuestion(quizId, questionId);

            if (!_cache.TryGetValue("$Choice {choiceId}", out Choice choice))
            {
                choice = await _choiceRepository.GetChoice(questionId, choiceId);
                _cache.Set("$Choice {choiceId}", choice);
            }

            if (choice == null || question == null)
            {
                return new ObjectResult<ChoiceModel> { Errors = new[] { "Failed to find the choice to update" } };
            }

            return new ObjectResult<ChoiceModel> { Object = _mapper.Map<ChoiceModel>(choice), Found = true, Success = true };
        }

        public async Task<ObjectResult<ChoiceModel>> Create(int quizId, int questionId, ChoiceModel model)
        {
            var question = await _questionRepository.GetQuestion(quizId, questionId);

            if (question == null)
            {
                return new ObjectResult<ChoiceModel> { Errors = new[] { "Didn't find the question" } };
            }

            var choice = _mapper.Map<Choice>(model);

            if (choice != null)
            {
                choice.QuestionId = question.Id;

                _choiceRepository.Add(choice);
            }

            if (await _choiceRepository.SaveChangesAsync())
            {
                _cache.Set($"Choice {choice.Id}", choice);
                return new ObjectResult<ChoiceModel> { Object = _mapper.Map<ChoiceModel>(choice), Found = true, Success = true };
            }

            return new ObjectResult<ChoiceModel> { Found = true, Errors = new[] { "Action didn't affect any rows" } };
        }

        public async Task<ObjectResult<ChoiceModel>> Update(int quizId, int questionId, int choiceId, ChoiceModel model)
        {
            var choice = await _choiceRepository.GetChoice(questionId, choiceId);
            var question = await _questionRepository.GetQuestion(quizId, questionId);

            if (choice == null || question == null)
            {
                return new ObjectResult<ChoiceModel> { Errors = new[] { "Failed to find the choice to update" } };
            }

            _choiceRepository.Update(choice);

            choice = _mapper.Map<Choice>(model);

            if (await _choiceRepository.SaveChangesAsync())
            {
                _cache.Set($"Choice {choice.Id}", choice);
                return new ObjectResult<ChoiceModel> { Object = _mapper.Map<ChoiceModel>(choice), Found = true, Success = true };
            }

            return new ObjectResult<ChoiceModel> { Found = true, Errors = new[] { "Action didn't affect any rows" } };
        }

        public async Task<DetailedResult> Delete(int quizId, int questionId, int choiceId)
        {
            var choice = await _choiceRepository.GetChoice(questionId, choiceId);
            var question = await _questionRepository.GetQuestion(quizId, questionId);

            if (question == null || choice == null)
            {
                return new DetailedResult { Errors = new[] { "Failed to find the choice to delete" } };
            }

            _choiceRepository.Delete(choice);

            if (await _choiceRepository.SaveChangesAsync())
            {
                _cache.Remove($"Choice {choiceId}");
                return new DetailedResult { Found = true, Success = true };
            }

            return new DetailedResult { Found = true, Errors = new[] { "Action didn't affect any rows" } };
        }
    }
}
