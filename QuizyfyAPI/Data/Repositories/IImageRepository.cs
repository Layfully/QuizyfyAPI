using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data;
public interface IImageRepository : IRepository
{
    Task<Image[]> GetImages();
    Task<Image> GetImage(int imageId);
    Task<Image> GetImageByUrl(string url);
}
