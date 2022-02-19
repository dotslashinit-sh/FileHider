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
        // Input file extension
        string inputFileExt;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            inputFileExt = "";
        }

        private void OnQuitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Even function to hide the files.
        /// </summary>
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

        /// <summary>
        /// Displays the given error message in an error box.
        /// </summary>
        private void ShowError(string errMsg)
        {
            MessageBox.Show(errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Displays the given message in an info box.
        /// </summary>
        private void ShowInformation(string msg)
        {
            MessageBox.Show(msg, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Browses for the input file and changes the value of the textbox.
        /// </summary>
        private void OnBrowseInputFile(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new();
            dialog.Title = "Please select an input file";
            dialog.Filter = "Supported Files(*.jpg, *.mp3, *.mp4)|*.jpg;*.mp3;*.mp4";
            dialog.ShowDialog();
            inputFileBox.Text = dialog.FileName;
            inputFileExt = Path.GetExtension(inputFileBox.Text); // Get the extension of the input file.
        }

        /// <summary>
        /// Browses for the input directory containing the files to hide and changes the value of the textbox.
        /// </summary>
        private void OnBrowseInputDir(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new();
            dialog.ShowNewFolderButton = false;
            dialog.ShowDialog();
            inputDirBox.Text = dialog.SelectedPath;
        }

        /// <summary>
        /// Browses for the output file and changes the value of the textbox.
        /// </summary>
        private void OnBrowseOutputFile(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dialog = new();
            dialog.Title = "Please select an output file";
            dialog.Filter = $"Input file type (*{inputFileExt})|*{inputFileExt}";
            dialog.ShowDialog();
            outputFileBox.Text = dialog.FileName;
        }
    }
}
