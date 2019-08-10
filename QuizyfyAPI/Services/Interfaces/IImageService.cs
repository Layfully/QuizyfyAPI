using Microsoft.AspNetCore.Http;
using QuizyfyAPI.Domain;
using QuizyfyAPI.Models;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public interface IImageService : IService
    {
        Task<ObjectResult<ImageModel[]>> GetAll();
        Task<ObjectResult<ImageModel>> Get(int id);
        Task<ObjectResult<ImageModel>> Create(IFormFile file);
        Task<ObjectResult<ImageModel>> Update(int id, IFormFile file);
        Task<DetailedResult> Delete(int id);
    }
}
