﻿using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;

namespace BugTracker.Services
{
    public class BTFileService : IBTFileService
    {
        private readonly string _defaultImage = "";
        private readonly string _defaultBTUserImageSrc = "/img/DefaultProfileImage";
        private readonly string _defaultCompanyImageSrc = "";
        private readonly string _defaultProjectImageSrc = "/img/DefaultProjectImage.png";

        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };

        public string ConvertByteArrayToFile(byte[] fileData, string extension, DefaultImage defaultImage)
        {
            if (fileData is null)
            {
                return defaultImage switch
                {
                    DefaultImage.BTUserImage => _defaultBTUserImageSrc,
                    DefaultImage.CompanyImage => _defaultCompanyImageSrc,
                    DefaultImage.ProjectImage => _defaultProjectImageSrc,
                    _ => _defaultImage,
                };
            }
            try
            {
                return string.Format($"data:{extension};base64,{Convert.ToBase64String(fileData)}");
            }
            catch
            {
                throw;
            }
        }
        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var byteFile = memoryStream.ToArray();
                memoryStream.Close();
                memoryStream.Dispose();
                return byteFile;
            }
            catch
            {
                throw;
            }
        }

        public string GetFileIcon(string file)
        {
            string ext = Path.GetExtension(file).Replace(".", "");
            return $"/img/contenttype/{ext}.png";
        }


        public string FormatFileSize(long bytes)
        {
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }
    }
}
