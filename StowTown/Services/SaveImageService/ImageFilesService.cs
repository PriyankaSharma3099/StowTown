using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.Services.SaveImageService
{
    public static class ImageFilesService
    {

        public  static string CurrentFTPUser { get; set; } = "wca";
        public  static string CurrentFTPPassword { get; set; } = "Esh@len$1";
        public  static string GetImageUrl(string subfolder, string fileName)
        {
            if (string.IsNullOrEmpty(subfolder) || string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }
            // Ensure the subfolder and fileName are URL-safe
            string safeSubfolder = Uri.EscapeDataString(subfolder);
            string safeFileName = Uri.EscapeDataString(fileName);
            return $"https://wca.microlent.com/StowTown/assets/{safeSubfolder}/{safeFileName}";
        }
        public async static Task<string?> UploadImageToServer(FileResult file, string subfolder)
        {
            string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
            {
                  await Application.Current.MainPage.DisplayAlert("Invalid File Type", "Only PNG and JPG files are allowed.", "OK");
                Console.WriteLine("Invalid file type. Only PNG and JPG are allowed.");
                return null;
            }

            string ftpBase = "ftp://wca.microlent.com/StowTown/assets";
            string ftpFullPath = $"{ftpBase}/{subfolder}";
            string fileName = $"{Guid.NewGuid()}{extension}";
            string fileUrl = $"{ftpFullPath}/{fileName}";
            string httpImageUrl = $"https://wca.microlent.com/StowTown/assets/{subfolder}/{fileName}";

            string ftpUser = "wca";
            string ftpPass = "Esh@len$1";

            try
            {
                if (!FtpDirectoryExists(ftpFullPath, ftpUser, ftpPass))
                {
                    var dirRequest = (FtpWebRequest)WebRequest.Create(ftpFullPath);
                    dirRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                    dirRequest.Credentials = new NetworkCredential(ftpUser, ftpPass);
                    dirRequest.UsePassive = true;
                    dirRequest.UseBinary = true;
                    dirRequest.KeepAlive = false;

                    using var dirResponse = (FtpWebResponse)await dirRequest.GetResponseAsync();
                }

                byte[] fileBytes;
                using (var fileStream = await file.OpenReadAsync())
                using (var ms = new MemoryStream())
                {
                    await fileStream.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }

                var uploadRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
                uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
                uploadRequest.Credentials = new NetworkCredential(ftpUser, ftpPass);
                uploadRequest.UseBinary = true;
                uploadRequest.UsePassive = true;
                uploadRequest.KeepAlive = false;
                uploadRequest.ContentLength = fileBytes.Length;

                using (var requestStream = await uploadRequest.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                }

                using var uploadResponse = (FtpWebResponse)await uploadRequest.GetResponseAsync();
                if (uploadResponse.StatusCode == FtpStatusCode.ClosingData ||
                    uploadResponse.StatusCode == FtpStatusCode.CommandOK)
                {
                    return fileName;
                }

                Console.WriteLine("Unexpected FTP response: " + uploadResponse.StatusDescription);
            }
            catch (WebException wex) when (wex.Response is FtpWebResponse ftpResp)
            {
                Console.WriteLine($"FTP error: {ftpResp.StatusDescription}");
                if (ftpResp.StatusCode == FtpStatusCode.ClosingData || ftpResp.StatusCode == FtpStatusCode.CommandOK)
                {
                    return httpImageUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
            }

            return null;
        }

       

        private static bool FtpDirectoryExists(string directoryUrl, string user, string pass)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(directoryUrl);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(user, pass);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false;

                using var response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (WebException ex)
            {
                if (ex.Response is FtpWebResponse ftpResponse &&
                    ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return false;
                }
                throw;
            }
        }


        public static bool DeleteFtpImage(string subfolder, string fileName)
        {
            if (string.IsNullOrEmpty(subfolder) || string.IsNullOrEmpty(fileName))
                return false;

            try
            {
                // Build FTP URL safely
                string safeSubfolder = Uri.EscapeDataString(subfolder);
                string safeFileName = Uri.EscapeDataString(fileName);
                string ftpUrl = $"ftp://wca.microlent.com/StowTown/assets/{safeSubfolder}/{safeFileName}";

                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(CurrentFTPUser, CurrentFTPPassword);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false;

                using var response = (FtpWebResponse)request.GetResponse();
                Console.WriteLine($"FTP Delete Status: {response.StatusDescription}");
                return true;
            }
            catch (WebException ex)
            {
                if (ex.Response is FtpWebResponse ftpResponse)
                {
                    Console.WriteLine($"FTP Error: {ftpResponse.StatusDescription}");
                }
                else
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }

                return false;
            }
        }




    }
}
