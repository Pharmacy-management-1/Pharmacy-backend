using Microsoft.AspNetCore.Http;

namespace PharmacyApi.Helpers;

public static class FileUploadHelper
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public static async Task<string> SavePrescriptionFileAsync(IFormFile file, string uploadsFolderPath)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file provided.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException($"File size exceeds {MaxFileSize / 1024 / 1024} MB limit.");

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Only .jpg, .jpeg, .png, .pdf files are allowed.");

        // Generate unique file name
        var uniqueFileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
        var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

        // Ensure directory exists
        Directory.CreateDirectory(uploadsFolderPath);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath; // Return absolute or relative path as needed
    }
}