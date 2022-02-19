using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Threading.Tasks;

namespace Hider
{
    /// <summary>
    /// Main logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        // Input file extension
        string inputFileExt;

        // Paths of all the files and directories. The values for these are extracted from the textboxes.
        String inputFilePath;
        String inputDirPath;
        String outputFilePath;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            inputFileExt = "";

            inputFilePath = "";
            inputDirPath = "";
            outputFilePath = "";
        }

        private void OnQuitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Even function to hide the files.
        /// </summary>
        private async void OnHideFiles(object sender, RoutedEventArgs e)
        {
            // Get the path values from the input boxes.
            inputFilePath = inputFileBox.Text.Trim();
            inputDirPath = inputDirBox.Text.Trim();
            outputFilePath = outputFileBox.Text.Trim();

            // Remove the temporary file if it exists.
            if (File.Exists("./temp.zip"))
            {
                File.Delete("./temp.zip");
            }

            if (!File.Exists(inputFilePath))
            {
                ShowError("Cannot open input file to read!");
                return;
            }

            if (File.Exists(outputFilePath))
            {
                ShowError("Please select a different output file!");
                return;
            }

            if (!Directory.Exists(inputDirPath))
            {
                ShowError("The given input directory does not exist!");
                return;
            }

            // Disable the buttons until the opeartion is completed.
            hideButton.IsEnabled = false;
            quitButton.IsEnabled = false;

            // Do the file operations in the background.
            await Task.Run(ProcessFileOp);

            // Enable the buttons and then print the confirmation box.
            hideButton.IsEnabled = true;
            quitButton.IsEnabled = true;
            ShowInformation("The file was saved successfully! You can open the output file" +
                "with an archive manager(WinRAR, WinZIP, 7-Zip etc.) to view the files that were hidden.");
        }

        /// <summary>
        /// Processes the files and generates and writes to the output file.
        /// </summary>
        private void ProcessFileOp()
        {
            ZipFile.CreateFromDirectory(inputDirPath, "./temp.zip");
            byte[] zipBuffer = File.ReadAllBytes("./temp.zip");
            byte[] inputFileBuffer = File.ReadAllBytes(inputFilePath);
            File.Delete("./temp.zip"); // Delete temporary file after reading.
            byte[] buffer = new byte[zipBuffer.Length + inputFileBuffer.Length];
            inputFileBuffer.CopyTo(buffer, 0);
            zipBuffer.CopyTo(buffer, inputFileBuffer.Length);
            File.WriteAllBytes(outputFilePath, buffer);
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
