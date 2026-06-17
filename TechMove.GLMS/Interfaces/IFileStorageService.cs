namespace TechMove.GLMS.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SavePdfAsync(IFormFile file);
    }
}
