using System.IO;
using TechMove.GLMS.Interfaces;

namespace TechMove.GLMS.Services
{
    public class FileService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SavePdfAsync(IFormFile file)
        {
            // 1. Validate extension
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".pdf")
                throw new Exception("Only PDF files are allowed.");

            // 2. Create folder path (correct folder)
            var folder = Path.Combine(_env.WebRootPath, "Uploads", "Agreements");
            Directory.CreateDirectory(folder);

            // 3. Generate UUID filename
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            // 4. Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. Return relative path for DB
            return $"/Uploads/Agreements/{fileName}";
        }
    }
}
