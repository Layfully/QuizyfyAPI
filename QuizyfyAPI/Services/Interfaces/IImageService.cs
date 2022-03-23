using Microsoft.AspNetCore.Http;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Domain;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services;
public interface IImageService : IService
{
    Task<ObjectResult<ImageResponse[]>> GetAll();
    Task<ObjectResult<ImageResponse>> Get(int id);
    Task<ObjectResult<ImageResponse>> Create(IFormFile file);
    Task<ObjectResult<ImageResponse>> Update(int id, IFormFile file);
    Task<DetailedResult> Delete(int id);
}
