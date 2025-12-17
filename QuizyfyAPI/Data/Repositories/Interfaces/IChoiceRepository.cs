using QuizyfyAPI.Data.Entities;

namespace QuizyfyAPI.Data.Repositories.Interfaces;

internal interface IChoiceRepository : IRepository
{
    Task<Choice[]> GetChoices(int questionId);
    Task<Choice?> GetChoice(int questionId, int choiceId);
}