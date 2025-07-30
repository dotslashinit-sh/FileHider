using FileHider.Lib;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Hider
{
    public partial class HiderWindow : Window
    {
        Session contextSession
        {
            get => (Session)this.DataContext;
        }
        public HiderWindow()
        {
            this.DataContext = new Session();
        }

        /// <summary>
        /// Displays the given message in an error box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">(Optional) The title of the error box. Default balue is "Error".</param>
        private static void ShowError(string message, string caption = "Error")
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnBrowseBtnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new();
            dialog.Title = "Open file";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Filter = "All supported formats(*.mp3, *.mp4, *.jpg, *.png)|*.mp3;*.mp4;*.jpg;*.png";
            dialog.ShowDialog();
            contextSession.InputFile = dialog.FileName;
        }

        private void OnAddFilesBtnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new();
            dialog.Multiselect = true;
            dialog.CheckFileExists = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            var files = dialog.FileNames;
            foreach (string? file in files)
            {
                if (file != null)
                {
                    contextSession.AddFile(file);
                }
            }
        }

        private void OnAddDirBtnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            string dir = dialog.SelectedPath;
            contextSession.AddDirectory(dir);
        }

        private void OnRemoveBtnClick(object sender, RoutedEventArgs e)
        {
            object? selectedItem = filesListView.SelectedItem;
            if (selectedItem != null)
            {
                contextSession.Delete((IListItem)selectedItem);
            }
        }

        private void OnRemoveAllBtnClick(object sender, RoutedEventArgs e)
        {
            contextSession.DeleteAll();
        }

        private async void OnHideFilesBtnClick(object sender, RoutedEventArgs e)
        {
            // Check if the input file is set and it exists.
            if (inputFileBox.Text == "")
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
            if (contextSession.Files.IsEmpty())
            {
                ShowError("Please add atleast one item!");
                return;
            }

            string inputFile = contextSession.InputFile;
            string inputFileExt = Path.GetExtension(contextSession.InputFile);

            // Get the file to be saved.
            System.Windows.Forms.SaveFileDialog dialog = new();
            dialog.DefaultExt = inputFileExt;
            dialog.Title = "Save location";
            dialog.OverwritePrompt = true;
            dialog.Filter = $"Input file type(*{inputFileExt})|{inputFileExt}";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            string outputFile = dialog.FileName;

            if (inputFile == outputFile)
            {
                ShowError("Input and output files cannot be the same!");
                return;
            }

            hFilesBtn.IsEnabled = false;

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            contextSession.OutputFile = outputFile;
            await Task.Run(contextSession.ProcessFiles);

            hFilesBtn.IsEnabled = true;

            MessageBox.Show("File saved success fully! Open the saved file with an archive manager like " +
                "WinRAR or 7-Zip to view the files that were hidden!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnFilesListDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var current = filesListView.SelectedItem;
            if (current == null || current is not DirectoryItem) return;
            contextSession.Files.GoTo((DirectoryItem)current);
        }

        private void OnBackBtnClicked(object sender, RoutedEventArgs e)
        {
            var cd = contextSession.Files.CurrentDirectory;
            if(cd.ParentDirectory != null)
            {
                contextSession.Files.GoTo(cd.ParentDirectory);
            }
        }
    }
}
