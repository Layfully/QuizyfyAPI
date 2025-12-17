using QuizyfyAPI.Data.Entities;

namespace QuizyfyAPI.Data.Repositories.Interfaces;

internal interface IImageRepository : IRepository
{
    Task<Image[]> GetImages();
    
    Task<Image?> GetImage(int imageId);
    
    Task<Image?> GetImageByUrl(string url);
}