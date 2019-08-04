using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;
using QuizyfyAPI.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;

        public ImageService(IImageRepository imageRepository, IMemoryCache cache, IMapper mapper)
        {
            _imageRepository = imageRepository;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<ObjectResult<ImageModel[]>> GetAll()
        {
            if (!_cache.TryGetValue("Images", out ICollection<Image> images))
            {
                images = await _imageRepository.GetImages();
                _cache.Set("Images", images);
            }

            if (images == null || images.Count == 0)
            {
                return new ObjectResult<ImageModel[]> { Errors = new[] { "There are no images" } };
            }
            return new ObjectResult<ImageModel[]> { Object = _mapper.Map<ImageModel[]>(images).ToArray(), Found = true, Success = true };
        }

        public async Task<ObjectResult<ImageModel>> Get(int id)
        {
            if (!_cache.TryGetValue("$Image {id}", out Image image))
            {
                image = await _imageRepository.GetImage(id);
                _cache.Set("$Image {id}", image);
            }

            if (image == null)
            {
                return new ObjectResult<ImageModel> { Errors = new[] { "Couldn't find this image" } };
            }

            return new ObjectResult<ImageModel> { Object = _mapper.Map<ImageModel>(image), Found = true, Success = true };
        }
        public async Task<ObjectResult<ImageModel>> Create(IFormFile file)
        {
            if (file != null && file.Length > 0)
            { 
                var model = new ImageModel { ImageUrl = await UploadFile(file) };
                var image = _mapper.Map<Image>(model);

                _imageRepository.Add(image);

                if (await _imageRepository.SaveChangesAsync())
                {
                    return new ObjectResult<ImageModel> { Success = true, Object = model };
                }
            }

            return new ObjectResult<ImageModel> { Errors = new[] { "File couldn't be uploaded" } };
        }
        public async Task<ObjectResult<ImageModel>> Update(int id, IFormFile file)
        {
            var image = await _imageRepository.GetImage(id);

            if (image == null)
            {
                return new ObjectResult<ImageModel> { Errors = new[] { "Couldn't find the choice to update" } };
            }

            _imageRepository.Update(image);

            var path = image.Url;
            path = path.Replace('/', '\\');
            File.Delete(path);

            var model = new ImageModel { ImageUrl = await UploadFile(file) };

            image = _mapper.Map<Image>(model);

            if (await _imageRepository.SaveChangesAsync())
            {
                _cache.Set($"Image {image.Id}", image);
                return new ObjectResult<ImageModel> { Object = _mapper.Map<ImageModel>(image), Found = true, Success = true };
            }

            return new ObjectResult<ImageModel> { Found = true, Errors = new[] { "Action didn't affect any rows" } };
        }
        public async Task<DetailedResult> Delete(int id)
        {
            var image = await _imageRepository.GetImage(id);

            if (image == null)
            {
                return new DetailedResult { Errors = new[] { "Couldn't find the image to delete" } };
            }

            _imageRepository.Delete(image);

            if (await _imageRepository.SaveChangesAsync())
            {
                _cache.Remove($"Image {id}");
                return new DetailedResult { Found = true, Success = true };
            }

            return new DetailedResult { Found = true, Errors = new[] { "Action didn't affect any rows" } };
        }
        private async Task<string> UploadFile(IFormFile file)
        {
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\quizzes", fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            filePath = Path.Combine("https://localhost:5001/", "images\\quizzes", fileName);
            filePath = filePath.Replace('\\', '/');

            return filePath;
        }
    }
}
