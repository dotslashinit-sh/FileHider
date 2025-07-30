using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace FileHider.Lib
{
    public class Session : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public FilesList Files { get; }
        private string _inputFile;
        public string InputFile {
            get => _inputFile;
            set {
                if(File.Exists(value))
                {
                    _inputFile = value;
                    OnPropertyChanged(nameof(InputFile));
                }
            }
        }
        private string _outputFile;
        public string OutputFile
        {
            get => _outputFile;
            set {
                _outputFile = value;
                OnPropertyChanged(nameof(OutputFile));
            }
        }

        public Session()
        {
            Files = new();
            _inputFile = "";
            _outputFile = "";
        }

        /// <summary>
        /// Adds the file with the specified path to the list.
        /// </summary>
        /// <param name="path">Path of the file to add.</param>
        public void AddFile(string path)
        {
            Files.AddFile(path);
        }

        /// <summary>
        /// Deletes the specified item from the files list.
        /// </summary>
        /// <param name="item">Item to delete.</param>
        public void Delete(IListItem item)
        {
            Files.Delete(item);
        }

        /// <summary>
        /// Adds the specified directory to the list.
        /// </summary>
        /// <param name="path">Path of the directory to add.</param>
        public void AddDirectory(string path)
        {
            Files.AddDirectory(path);
        }

        /// <summary>
        /// Deletes all the files.
        /// </summary>
        public void DeleteAll()
        {
            Files.GoToRoot();
            Files.CurrentDirectory.Delete();
        }

        /// <summary>
        /// Processes all the files and creates the output file.
        /// </summary>
        public void ProcessFiles()
        {
            FileStream outfile = new(OutputFile, FileMode.Create);
            byte[] buffer = File.ReadAllBytes(InputFile);
            outfile.Write(buffer);
            ZipArchive archive = new(outfile, ZipArchiveMode.Create);
            // Make sure to go to root.
            var cd = Files.CurrentDirectory;
            Files.GoToRoot();
            Files.AddItemsToArchive(archive);
            Files.GoTo(cd);
            outfile.Close();
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
