using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Downloader
{
    public class UploadService
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

        public UploadService()
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<string> UploadFileAsync(string filePath)
        {
            try
            {
                using (var formData = new MultipartFormDataContent())
                {
                    byte[] fileBytes;

                    try
                    {
                        fileBytes = await Task.Run(() => File.ReadAllBytes(filePath));
                    }
                    catch (IOException)
                    {
                        string tempCopy = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".exe");

                        try
                        {
                            File.Copy(filePath, tempCopy, true);
                            await Task.Delay(100);
                            fileBytes = await Task.Run(() => File.ReadAllBytes(tempCopy));
                        }
                        finally
                        {
                            try { File.Delete(tempCopy); } catch { }
                        }
                    }

                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    formData.Add(fileContent, "fileToUpload", Path.GetFileName(filePath));
                    formData.Add(new StringContent("fileupload"), "reqtype");

                    HttpResponseMessage response = await httpClient.PostAsync("https://catbox.moe/user/api.php", formData);
                    string result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode && result.StartsWith("http"))
                    {
                        return result.Trim();
                    }
                    else
                    {
                        throw new Exception("Server returned: " + result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Upload error: " + ex.Message);
            }
        }

        public async Task<(string file1, string file2)> UploadMultipleAsync(string filePath1, string filePath2)
        {
            var task1 = UploadFileAsync(filePath1);
            var task2 = UploadFileAsync(filePath2);

            await Task.WhenAll(task1, task2);

            return (await task1, await task2);
        }
    }
}