using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections;

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

        // List of files and entry names to be hidden.
        // The entries are stored in tuples as (string, string) with Item1 being the full location of the file,
        // and Item2 being the entry name, with relative path to the file.
        ArrayList filesList;

        public HiderWindow()
        {
            inputFileExt = "";
            outputFile = "";
            inputFile = "";
            filesList = new();
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
                    filesList.Add((file, Path.GetFileName(file)));
                }
            }
        }

        /// <summary>
        /// Event for when the 'Add from Folder' button is clicked.
        /// </summary>
        private void OnAddFromDirBtnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            string dir = dialog.SelectedPath;
            ArrayList entries = GetFilesInDir(dir, dir);
            foreach((string, string) entry in entries)
            {
                // Remove already existing files and re-add them with new entry names.
                if(filesListBox.Items.Contains(entry.Item1))
                {
                    filesListBox.Items.Remove(entry.Item1);

                    // Iterate through the list, check each elements and remove the element with the same file path.
                    for (int i = 0; i < filesList.Count; ++i)
                    {
                        object? fileEntryObj = filesList[i];
                        if (fileEntryObj != null)
                        {
                            var filesListEntry = ((string, string))fileEntryObj;
                            if(filesListEntry.Item1 == entry.Item1)
                            {
                                filesList.RemoveAt(i);
                            }
                        }
                    }
                }
                filesListBox.Items.Add(entry.Item1);
            }
        }

        /// <summary>
        /// Returns an array of files and their relative path to the current directory.
        /// </summary>
        /// <param name="path"> The folder where all the files exist. </param>
        /// <param name="currentDir"> The current directory. Default value is "". </param>
        /// <returns></returns>
        private ArrayList GetFilesInDir(string path, string currentDir = "")
        {
            ArrayList entries = new();
            foreach(string entry in Directory.GetFiles(path))
            {
                entries.Add((entry, Path.GetRelativePath(currentDir, entry)));
            }
            foreach (string dir in Directory.GetDirectories(path))
            {
                entries.AddRange(GetFilesInDir(dir, currentDir));
            }
            return entries;
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
                foreach((string, string) entry in filesList)
                {
                    if (entry.Item1 == (string)selectedItem)
                    {
                        filesList.Remove(entry);
                        break;
                    }
                }
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
                filesList.Clear();
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
            // Create a temporary memory stream where the archive file is written.
            MemoryStream archiveStream = new(); 
            ZipArchive archive = new(archiveStream, ZipArchiveMode.Create);
            foreach((string, string) entry in filesList)
            {
                if(File.Exists(entry.Item1))
                    archive.CreateEntryFromFile(entry.Item1, entry.Item2);
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
