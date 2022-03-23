namespace QuizyfyAPI.Data;
public interface IImageRepository : IRepository
{
    Task<Image[]> GetImages();
    Task<Image> GetImage(int imageId);
    Task<Image> GetImageByUrl(string url);
}
