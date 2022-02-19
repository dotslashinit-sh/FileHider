using System;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace Hider
{
    /// <summary>
    /// Main logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        string inputFileExt;

        public MainWindow()
        {
            inputFileExt = "";
        }

        private void OnQuitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnHideFiles(object sender, RoutedEventArgs e)
        {
            // Get the path values from the input boxes.
            String inputFilePath = inputFileBox.Text.Trim();
            String inputDirPath = inputDirBox.Text.Trim();
            String outputFilePath = outputFileBox.Text.Trim();

            // Remove the temporary file if it exists.
            if (File.Exists("./temp.zip"))
            {
                File.Delete("./temp.zip");
            }

            byte[] inputFileBuffer;
            if (File.Exists(inputFilePath))
            {
                inputFileBuffer = File.ReadAllBytes(inputFilePath);
            }
            else
            {
                ShowError("Cannot open input file to read!");
                return;
            }

            FileStream outputFile;
            if(File.Exists(outputFilePath))
            {
                ShowError("Please select a different output file!");
                return;
            }
            else
            {
                outputFile = File.Create(outputFilePath);
            }

            if (!Directory.Exists(inputDirPath))
            {
                ShowError("The given input directory does not exist!");
                return;
            }

            ZipFile.CreateFromDirectory(inputDirPath, "./temp.zip");
            byte[] zipBuffer = File.ReadAllBytes("./temp.zip");
            byte[] buffer = new byte[zipBuffer.Length+inputFileBuffer.Length];
            inputFileBuffer.CopyTo(buffer, 0);
            zipBuffer.CopyTo(buffer, inputFileBuffer.Length);
            outputFile.Write(buffer);
            outputFile.Close();
            File.Delete("./temp.zip");

            ShowInformation("The file has been saved successfully! Open the output file using an archive manager(WinRAR, 7-Zip, WinZIP etc.) to view the hidden files!");
        }

        private void ShowError(string errMsg)
        {
            MessageBox.Show(errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowInformation(string msg)
        {
            MessageBox.Show(msg, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnBrowseInputFile(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new();
            dialog.Title = "Please select an input file";
            dialog.Filter = "Supported Files(*.jpg, *.mp3, *.mp4)|*.jpg;*.mp3;*.mp4";
            dialog.ShowDialog();
            inputFileBox.Text = dialog.FileName;
            inputFileExt = Path.GetExtension(inputFileBox.Text); // Get the extension of the input file.
        }

        private void OnBrowseInputDir(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new();
            dialog.ShowNewFolderButton = false;
            dialog.ShowDialog();
            inputDirBox.Text = dialog.SelectedPath;
        }

        private void OnBrowseOutputFile(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dialog = new();
            dialog.Title = "Please select an output file";
            dialog.Filter = $"Input file type (*{inputFileExt})|*{inputFileExt}"; // Remove the . from the input file extension and set is as filter.
            dialog.ShowDialog();
            outputFileBox.Text = dialog.FileName;
        }
    }
}
