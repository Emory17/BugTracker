using BugTracker.Models.Enums;

namespace BugTracker.Services.Interfaces
{
    public interface IBTFileService
    {
        string ConvertByteArrayToFile(byte[] fileData, string extension, DefaultImage defaultImage);
        Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);
        public string GetFileIcon(string file);
        public string FormatFileSize(long bytes);
    }
}
