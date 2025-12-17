namespace QuizyfyAPI.Services.Interfaces;

internal interface IImageService : IService
{
    Task<ObjectResult<ImageResponse[]>> GetAll();
    Task<ObjectResult<ImageResponse>> Get(int id);
    Task<ObjectResult<ImageResponse>> Create(IFormFile file);
    Task<ObjectResult<ImageResponse>> Update(int id, IFormFile file);
    Task<DetailedResult> Delete(int id);
}
