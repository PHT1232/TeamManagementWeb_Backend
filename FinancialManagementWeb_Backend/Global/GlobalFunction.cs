using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace TeamManagementProject_Backend.Global
{
    public static class GlobalFunction
    {
        private static byte[] GetAllBytes(Stream stream)
        {
            using MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }

        public static string SaveFile(string folderPath, IFormFile importFile)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string extension = Path.GetExtension(importFile.FileName);

            string trustedFileNameForFileStorage = string.Format("{0}" + extension, Path.GetRandomFileName().Replace(".", string.Empty));

            string uploadFilePath = Path.Combine(folderPath, trustedFileNameForFileStorage);

            using (FileStream fileStream = File.Create(uploadFilePath))
            {
                importFile.CopyTo(fileStream);
            }

            return trustedFileNameForFileStorage;
        }

        public static void DeleteFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            fileInfo.Delete();
        }
    }
}
