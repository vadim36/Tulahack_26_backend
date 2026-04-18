namespace backend.Services.Image
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile imageFile);
        void DeleteImage(string imagePath);
    }
}
