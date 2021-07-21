using DeviceLogLoader.Network;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace DeviceLogLoader
{
    public partial class Form1 : Form
    {
        private IHttpClient _httpClient;
        
        public Form1()
        {
            InitializeComponent();
            Logger.Configure();
            LogStartWork();
            _httpClient = new Http();
        }
        
        private static void LogStartWork()
        {
            Log.Information(new string('=', 70));
            Log.Information($"[Main] - DeviceLogLoader started, version: {Assembly.GetExecutingAssembly().GetName().Version}");
            Log.Information(new string('=', 70));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Input_text_box.Text))
            {
                MessageBox.Show("Inputed identifier couldn't be empty!", "Attention", MessageBoxButtons.OK);
                Log.Information("[LoadButton] - Inputed identifier is empty.");
                return;
            }
            var (statusCode, content) = _httpClient.Get(Input_text_box.Text + ".zip").GetAwaiter().GetResult();
            if (ValidateResponseStatusCode(statusCode) is false)
            {
                MessageBox.Show($"Server status code - {statusCode}", "Operation failed", MessageBoxButtons.OK);
                ClearTextBox();
                return;
            }
            var stream = content.ReadAsStreamAsync().GetAwaiter().GetResult();
            string fileName = content.Headers.ContentDisposition.FileName;
            Log.Information($"[LoadButton] - File '{fileName}' loaded.");
            SaveStreamAsFileAndOpen(stream, fileName);
            ClearTextBox();
        }

        private static bool ValidateResponseStatusCode(HttpStatusCode? statusCode) => statusCode == HttpStatusCode.OK;
        
        private static void SaveStreamAsFileAndOpen(Stream inputStream, string fileName) 
        {
            try 
            { 
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                if (File.Exists(path))
                {
                    MessageBox.Show($"File '{fileName}' already exist in '{Path.GetDirectoryName(path)}' folder", "Operation cancelled", MessageBoxButtons.OK);
                    OpenFolder(path);
                    return;
                }
                using var outputFileStream = new FileStream(path, FileMode.Create);
                inputStream.CopyTo(outputFileStream);
                Log.Information($"[LoadButton:SaveStreamAsFileAndOpen] - File '{fileName}' saved to {Path.GetDirectoryName(path)}.");
                MessageBox.Show("File saved to desktop", "Success", MessageBoxButtons.OK);
                OpenFolder(path);

                static void OpenFolder(string folderPath)
                {
                    var explorer = new ProcessStartInfo
                    {
                        Arguments = folderPath,
                        FileName = "explorer.exe"
                    };
                    Process.Start(explorer);
                    Log.Information($"[LoadButton:OpenFolder] - Folder {folderPath} was opened.");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "[LoadButton:SaveStreamAsFileAndOpen] - Catch exception:");
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Log.Information(new string('=', 70));
            Log.Information("[Main] - DeviceLogLoader shutting down...");
            Log.Information(new string('=', 70));
            base.OnClosing(e);
        }

        private void Input_text_box_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                Log.Information("[TextBox] - Enter key handled");
                button1_Click(this, e);
            }
        }

        private void ClearTextBox() => Input_text_box.Text = null;
    }
}