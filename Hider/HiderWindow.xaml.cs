using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace Hider
{
    /// <summary>
    /// Main logic for MainWindow
    /// </summary>
    public partial class HiderWindow : Window
    {
        // Input file extension
        string inputFileExt;

        // Output file location
        string outputFile;

        // Input file location.
        string inputFile;

        public HiderWindow()
        {
            inputFileExt = "";
            outputFile = "";
            inputFile = "";
        }

        /// <summary>
        /// Displayss the given message in an error box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">(Optional) The title of the error box. Default balue is "Error".</param>
        private void ShowError(string message, string caption = "Error")
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Actions.

        /// <summary>
        /// Event for when the text in the base file input box has changed.
        /// </summary>
        private void OnInputBoxTxtChngd(object sender, TextChangedEventArgs e)
        {
            inputFile = inputFileBox.Text;
            if(File.Exists(inputFile))
            {
                inputFileExt = Path.GetExtension(inputFile);
            }
        }

        /// <summary>
        /// Event for when the browse button is clicked.
        /// </summary>
        private void OnBrowseBtnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new();
            dialog.Title = "Open file";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Filter = "All supported formats(*.mp3, *.mp4, *.jpg)|*.mp3;*.mp4;*.jpg";
            dialog.ShowDialog();
            inputFileBox.Text = dialog.FileName;
        }

        /// <summary>
        /// Event for when the add files button is clicked.
        /// </summary>
        private void OnAddFilesBtnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new();
            dialog.Multiselect = true;
            dialog.CheckFileExists = true;
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            var files = dialog.FileNames;
            foreach (string? file in files)
            {
                if (file != null && !filesListBox.Items.Contains(file))
                {
                    filesListBox.Items.Add(file);
                }
            }
        }

        /// <summary>
        /// Event for when the remove button is clicked.
        /// </summary>
        private void OnRemoveBtnClick(object sender, RoutedEventArgs e)
        {
            object? selectedItem = filesListBox.SelectedItem;
            if(selectedItem != null)
            {
                filesListBox.Items.Remove(selectedItem);
            }
        }

        /// <summary>
        /// Event for when the Remove All button is clicked.
        /// </summary>
        private void OnRemoveAllBtnClick(object sender, RoutedEventArgs e)
        {
            var files = filesListBox.Items;
            if(!files.IsEmpty)
            {
                files.Clear();
            }
        }

        /// <summary>
        /// Event for when the hide files button is clicked.
        /// </summary>
        private async void OnHideFilesBtnClick(object sender, RoutedEventArgs e)
        {
            // Check if the input file is set and it exists.
            if(inputFileBox.Text == "")
            {
                ShowError("Error! Please select a base file!");
                return;
            }
            if (!File.Exists(inputFileBox.Text))
            {
                ShowError($"Error! File at \"{inputFileBox.Text}\" does not exist, or it is not a file! Please select a base file that exists!");
                return;
            }

            // Check if atleast one item has been added to the list.
            if(filesListBox.Items.Count == 0)
            {
                ShowError("Please add atleast one item!");
                return;
            }

            // Get the file to be saved.
            System.Windows.Forms.SaveFileDialog dialog = new();
            dialog.DefaultExt = inputFileExt;
            dialog.Title = "Save location";
            dialog.OverwritePrompt = true;
            dialog.Filter = $"Input file type(*{inputFileExt})|{inputFileExt}";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            outputFile = dialog.FileName;
            if (inputFile == outputFile)
            {
                ShowError("Input and output files cannot be the same!");
                return;
            }
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            // Update the hide files button to be blanked out until the task is completed.
            hFilesBtn.IsEnabled = false;
            await Task.Run(ProcessFiles);
            hFilesBtn.IsEnabled = true;
            MessageBox.Show("File saved success fully! Open the saved file with an archive manager like " +
                "WinRAR or 7-Zip to view the files that were hidden!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Process the files in the list, create an archive and then write to the output file.
        /// </summary>
        private void ProcessFiles()
        {
            System.Collections.ArrayList filesList = new();
            foreach (object? item in filesListBox.Items)
            {
                if (item != null)
                {
                    filesList.Add((String)item);
                }
            }

            // Create a temporary memory stream where the archive file is written.
            MemoryStream archiveStream = new(); 
            ZipArchive archive = new(archiveStream, ZipArchiveMode.Create);
            foreach(string file in filesList)
            {
                archive.CreateEntryFromFile(file, Path.GetFileName(file));
            }
            byte[] baseFileData = File.ReadAllBytes(inputFile);
            byte[] archiveData = archiveStream.ToArray();
            byte[] outputData = new byte[baseFileData.Length + archiveData.Length];
            baseFileData.CopyTo(outputData, 0);
            archiveData.CopyTo(outputData, baseFileData.Length);
            File.WriteAllBytes(outputFile, outputData);
        }
    }
}
