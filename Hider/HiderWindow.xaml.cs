using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections;

namespace Hider
{
    public partial class HiderWindow : Window
    {
        HiderSession session;

        public HiderWindow()
        {
            session = new();
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

        private void OnInputBoxTxtChngd(object sender, TextChangedEventArgs e)
        {
            session.SetInputFile(inputFileBox.Text);
        }

        private void OnBrowseBtnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new();
            dialog.Title = "Open file";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Filter = "All supported formats(*.mp3, *.mp4, *.jpg, *.png)|*.mp3;*.mp4;*.jpg;*.png";
            dialog.ShowDialog();
            inputFileBox.Text = dialog.FileName;
        }

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
                    session.AddFile(file, Path.GetFileName(file));
                }
            }
        }

        private void OnAddDirBtnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            string dir = dialog.SelectedPath;
            session.AddDirectory(dir, dir);
            filesListBox.Items.Add(String.Format("{0}\\*", dir));
        }

        private void OnRemoveBtnClick(object sender, RoutedEventArgs e)
        {
            object? selectedItem = filesListBox.SelectedItem;
            if(selectedItem != null)
            {
                session.RemoveEntry((string)selectedItem);
                filesListBox.Items.Remove((string)selectedItem);
            }
        }

        private void OnRemoveAllBtnClick(object sender, RoutedEventArgs e)
        {
            filesListBox.Items.Clear();
            session.RemoveAll();
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
            if (filesListBox.Items.Count == 0)
            {
                ShowError("Please add atleast one item!");
                return;
            }

            string inputFile = session.GetInputFile();
            string inputFileExt = session.GetInputFileExt();

            // Get the file to be saved.
            System.Windows.Forms.SaveFileDialog dialog = new();
            dialog.DefaultExt = session.GetInputFileExt();
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
            
            session.SetOutputFile(outputFile);
            await Task.Run(session.ProcessFiles);

            hFilesBtn.IsEnabled = true;

            MessageBox.Show("File saved success fully! Open the saved file with an archive manager like " +
                "WinRAR or 7-Zip to view the files that were hidden!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
