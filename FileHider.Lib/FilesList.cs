using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;

namespace FileHider.Lib.FilesList;

public class FilesList : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private DirectoryNode _currentDirectory;
    public DirectoryNode CurrentDirectory {
        get => _currentDirectory;
        private set
        {
            _currentDirectory = value;
            TriggerPropertyChange(nameof(CurrentDirectory));
        }
    }

    public DirectoryNode RootDirectory { get; }
    private int itemsProcessed = 0;

    private int _fileCount;
    /// <summary>
    /// Total number of files in the list.
    /// </summary>
    public int FileCount
    {
        get => _fileCount;
        set
        {
            _fileCount = value;
            TriggerPropertyChange(nameof(FileCount));
        }
    }

    public FilesList()
    {
        _currentDirectory = DirectoryNode.RootDir;
        RootDirectory = CurrentDirectory;
    }

    private void TriggerPropertyChange(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Adds the specified file to the list of files.
    /// </summary>
    /// <param name="realPath">Path to the file</param>
    public void AddFile(string realPath)
    {
        if(File.Exists(realPath))
        {
            FileNode.Create(realPath, CurrentDirectory);
            FileCount++;
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
            var dir = DirectoryNode.Create(Path.GetFileName(dirPath), CurrentDirectory);
            var cd = CurrentDirectory;
            GoTo(dir);
            foreach (var file in Directory.EnumerateFiles(dirPath))
            {
                FileNode.Create(file, CurrentDirectory);
                FileCount++;
            }
            foreach(var d in Directory.EnumerateDirectories(dirPath))
            {
                AddDirectory(d);
            }
            GoTo(cd);

        }
    }

    /// <summary>
    /// Deletes the specified item and its contents from the list.
    /// </summary>
    /// <param name="item">Item to delete.</param>
    public void Delete(INode item)
    {
        CurrentDirectory.DeleteItem(item);
    }

    /// <summary>
    /// Deletes all the files and directories in the list.
    /// </summary>
    public void DeleteAll()
    {
        CurrentDirectory.Delete();
    }

    /// <summary>
    /// Navigates to the specified directory.
    /// </summary>
    /// <param name="directory">Directory to navigate to.</param>
    public void GoTo(DirectoryNode directory)
    {
        CurrentDirectory = directory;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentDirectory"));
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
    /// Finds the item with the given name in the current directory.
    /// </summary>
    /// <param name="name">Name of the item to return.</param>
    /// <returns></returns>
    public INode? FindItem(string name)
    {
        foreach(var item in CurrentDirectory.Children)
        {
            if (item.Name == name) return item;
        }
        return null;
    }


    /// <summary>
    /// Adds all the files to the archive.
    /// </summary>
    public void AddItemsToArchive(DirectoryNode directory, ZipArchive archive, IProgress<float> progress)
    {
        itemsProcessed = 0;
        _AddItemsToArchive(directory, archive, progress);
    }

    private void _AddItemsToArchive(DirectoryNode directory, ZipArchive archive, IProgress<float> progress)
    {
        foreach (var item in directory.Children)
        {
            if (item == null) continue;
            else if (item is DirectoryNode)
            {
                _AddItemsToArchive((DirectoryNode)item, archive, progress);
            }
            else
            {
                var fileItem = (FileNode)item;
                archive.CreateEntryFromFile(fileItem.RealPath, fileItem.GetVirtualPath());
                itemsProcessed += 1;
                progress.Report((float)itemsProcessed * 100.0f / FileCount);
            }
        }
    }

    /// <summary>
    /// Returns true if the root directory is empty.
    /// </summary>
    /// <returns>Whether the root directory is empty or not.</returns>
    public bool IsEmpty()
    {
        return RootDirectory.Children.Count == 0;
    }
}
