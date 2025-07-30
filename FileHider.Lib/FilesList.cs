using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;

namespace FileHider.Lib
{
    public interface IListItem
    {
        string Name { get; }

        /// <summary>
        /// Returns the virtual realPath to the list item as a string.
        /// </summary>
        /// <returns>`string`: Virtual realPath to the file/parent.</returns>
        string GetVirtualPath();
        void Delete();
    }

    public class DirectoryItem : IListItem
    {
        public string Name { get; }
        public DirectoryItem? ParentDirectory { get; }

        static private DirectoryItem _root = new DirectoryItem("", null);
        static public DirectoryItem RootDir { get { return _root; } }
        public ObservableCollection<IListItem> Children { get; }


        private DirectoryItem(string name, DirectoryItem? parent)
        {
            Name = name;
            ParentDirectory = parent;
            Children = new();
            if(parent != null) 
            {
                parent.AddChild(this);
            }
        }

        /// <summary>
        /// Creates a new `DirectoryItem`.
        /// </summary>
        /// <param name="name">Name of the directory</param>
        /// <param name="parent">Parent directory</param>
        /// <returns>A new DirectoryItem</returns>
        public static DirectoryItem Create(string name, DirectoryItem parent)
        {
            return new(name, parent);
        }

        /// <summary>
        /// Returns the virtual realPath to the inside of the parent.
        /// </summary>
        /// <returns>`string`: Virtual realPath to the inside of the parent.</returns>
        public string GetVirtualPath() => (ParentDirectory == null) ? Name : Path.Combine(ParentDirectory.GetVirtualPath(), Name);

        /// <summary>
        /// Deletes all children inside the parent.
        /// </summary>
        public void Delete()
        {
            foreach(var child in Children)
            {
                child.Delete();
            }
            Children.Clear();
        }

        public void DeleteItem(IListItem item)
        {
            Children.Remove(item);
        }

        public void AddChild(IListItem item)
        {
            Children.Add(item);
        }
    }

    public class FileItem : IListItem
    {
        public string Name { get; }
        public string RealPath { get; }
        public DirectoryItem ParentDirectory { get; }

        private FileItem(string realPath, DirectoryItem parent)
        {
            RealPath = Path.GetFullPath(realPath);
            Name = Path.GetFileName(realPath);
            ParentDirectory = parent;
            parent.AddChild(this);
        }

        public static FileItem Create(string realPath, DirectoryItem parent) => new(realPath, parent);

        /// <summary>
        /// Returns the virtual realPath to the file.
        /// </summary>
        /// <returns>`string`: Virtual realPath to the file.</returns>
        public string GetVirtualPath() => Path.Combine(ParentDirectory.GetVirtualPath(), Name);

        /// <summary>
        /// Deletes the file (does nothing on a file).
        /// </summary>
        public void Delete()
        {
        }
    }

    public class FilesList : INotifyPropertyChanged
    {
        private DirectoryItem _currentDirectory;
        public DirectoryItem CurrentDirectory {
            get => _currentDirectory;
            private set
            {
                _currentDirectory = value;
                OnPropertyChanged(nameof(CurrentDirectory));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public FilesList()
        {
            _currentDirectory = DirectoryItem.RootDir;
        }

        /// <summary>
        /// Adds the specified file to the list of files.
        /// </summary>
        /// <param name="realPath">Path to the file</param>
        public void AddFile(string realPath)
        {
            if(File.Exists(realPath))
            {
                FileItem.Create(realPath, CurrentDirectory);
            }
        }

        /// <summary>
        /// Adds the specified directory and its contents to the list of files.
        /// </summary>
        /// <param name="dirPath">Path to the directory</param>
        public void AddDirectory(string dirPath)
        {
            if(Directory.Exists(dirPath))
            {
                var dir = DirectoryItem.Create(Path.GetFileName(dirPath), CurrentDirectory);
                var cd = CurrentDirectory;
                GoTo(dir);
                foreach (var file in Directory.EnumerateFiles(dirPath))
                {
                    FileItem.Create(file, CurrentDirectory);
                }
                foreach(var d in Directory.EnumerateDirectories(dirPath))
                {
                    Debug.WriteLine(d);
                    AddDirectory(d);
                }
                GoTo(cd);
            }
        }

        /// <summary>
        /// Deletes the specified item and its contents from the list.
        /// </summary>
        /// <param name="item">Item to delete.</param>
        public void Delete(IListItem item)
        {
            CurrentDirectory.DeleteItem(item);
        }

        /// <summary>
        /// Navigates to the specified directory.
        /// </summary>
        /// <param name="directory">Directory to navigate to.</param>
        public void GoTo(DirectoryItem directory)
        {
            CurrentDirectory = directory;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentDirectory"));
        }

        /// <summary>
        /// Finds the item with the given name in the current directory.
        /// </summary>
        /// <param name="name">Name of the item to return.</param>
        /// <returns></returns>
        public IListItem? FindItem(string name)
        {
            foreach(var item in CurrentDirectory.Children)
            {
                if (item.Name == name) return item;
            }
            return null;
        }

        /// <summary>
        /// Moves to the root of the virtual path.
        /// </summary>
        public void GoToRoot()
        {
            while (CurrentDirectory.ParentDirectory != null)
            {
                GoTo(CurrentDirectory.ParentDirectory);
            }
        }

        /// <summary>
        /// Adds all the files to the archive.
        /// </summary>
        public void AddItemsToArchive(ZipArchive archive)
        {
            foreach(var item in CurrentDirectory.Children)
            {
                if (item == null) continue;
                else if(item is DirectoryItem)
                {
                    var cd = CurrentDirectory;
                    GoTo((DirectoryItem)item);
                    AddItemsToArchive(archive);
                    GoTo(cd);
                } else
                {
                    var fileItem = (FileItem)item;
                    archive.CreateEntryFromFile(fileItem.RealPath, fileItem.GetVirtualPath());
                }
            }
        }

        /// <summary>
        /// Returns true if the root directory is empty.
        /// </summary>
        /// <returns>Whether the root directory is empty or not.</returns>
        public bool IsEmpty()
        {
            var cd = CurrentDirectory;
            GoToRoot();
            var c = CurrentDirectory.Children.Count;
            GoTo(cd);
            return c == 0;
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
