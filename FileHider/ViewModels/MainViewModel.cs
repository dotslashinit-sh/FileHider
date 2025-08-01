using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using FileHider.Views;
using FileHider.Lib.FilesList;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System;
using CommunityToolkit.Mvvm.Input;
using FileHider.Services;

namespace FileHider.ViewModels;


public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputFile;

    [ObservableProperty]
    private string outputFile;

    [ObservableProperty]
    public FilesList files;

    public IStorageProvider? StorageProvider { get; set; }

    [ObservableProperty]
    private INode? selectedItem;

    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private float percentDone;

    public MainViewModel()
    {
        files = new();
        InputFile = "";
        OutputFile = "";
        IsRunning = false;
        percentDone = new();
        SelectedItem = null;
        StorageProvider = null;
    }

    [RelayCommand]
    private async void BrowseInputFile()
    {
        var options = new FilePickerOpenOptions();
        options.Title = "Open File";
        options.AllowMultiple = false;
        var filesFilter = new FilePickerFileType("Supported files");
        filesFilter.Patterns = ["*.png", "*.jpg", "*.mp3", "*.mp4"];
        options.FileTypeFilter = [filesFilter];
        if(StorageProvider == null)
        {
            throw new Exception("StorageProvider was not initialized!");
        }
        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(options);
        if (files.Count < 1) return;
        InputFile = files[0].Path.LocalPath;
    }

    [RelayCommand]
    private async void AddFiles()
    {
        var options = new FilePickerOpenOptions();
        options.Title = "Select files to add";
        options.AllowMultiple = true;
        if (StorageProvider == null)
        {
            throw new Exception("StorageProvider was not initialized!");
        }
        var files = await StorageProvider.OpenFilePickerAsync(options);
        foreach (var file in files)
        {
            if (file != null)
            {
                Files.AddFile(file.Path.LocalPath);
            }
        }
    }

    private async void ShowDialog(string message, string title="Error")
    {
        await DialogService.ShowDialogAsync(title, message);
    }

    [RelayCommand]
    private async void AddDir()
    {
        var options = new FolderPickerOpenOptions();
        options.Title = "Select folders to add";
        options.AllowMultiple = true;
        if (StorageProvider == null)
        {
            throw new Exception("StorageProvider was not initialized!");
        }
        var dirs = await StorageProvider.OpenFolderPickerAsync(options);
        foreach (var dir in dirs)
        {
            if (dir != null)
            {
                Files.AddDirectory(dir.Path.AbsolutePath);
            }
        }
    }

    [RelayCommand]
    private void Remove()
    {
        object? selectedItem = SelectedItem;
        if (selectedItem != null)
        {
            Files.Delete((INode)selectedItem);
        }
    }

    [RelayCommand]
    private void RemoveAll()
    {
        Files.DeleteAll();
    }

    [RelayCommand]
    private async Task HideFiles()
    {
        // Check if the input file is set and it exists.
        if (InputFile == "")
        {
            ShowDialog("Error! Please select a base file!");
            return;
        }
        if (!File.Exists(InputFile))
        {
            ShowDialog($"Error! File at \"{InputFile}\" does not exist, or it is not a file! Please select a base file that exists!");
            return;
        }

        // Check if atleast one item has been added to the list.
        if (Files.IsEmpty())
        {
            ShowDialog("Please add atleast one item!");
            return;
        }

        string inputFileExt = Path.GetExtension(InputFile);
        var options = new FilePickerSaveOptions();
        options.DefaultExtension = inputFileExt.Substring(1);
        options.Title = "Select a location for the output file";
        if (StorageProvider == null)
        {
            throw new Exception("StorageProvider was not initialized!");
        }
        var dialog = await StorageProvider.SaveFilePickerAsync(options);
        if (dialog == null) return;
        string outputFile = dialog.Path.AbsolutePath;

        if (InputFile == outputFile)
        {
            ShowDialog("Input and output files cannot be the same!");
            return;
        }

        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }

        Progress<float> progress = new(percent => {
            PercentDone = percent;
        });

        OutputFile = outputFile;
        IsRunning = true;
        try
        {
            await Task.Run(() => ProcessFiles(progress));
        }
        catch (Exception ex)
        {
            ShowDialog($"An unknown error occurred!\nError: {ex.Message}");
        }
        IsRunning = false;

        ShowDialog("File saved success fully! Open the saved file with an archive manager like " +
            "WinRAR or 7-Zip to view the files that were hidden!", "Confirmation");
    }

    [RelayCommand]
    private void EnterDir()
    {
        var current = SelectedItem;
        if (current == null || current is not DirectoryNode) return;
        Files.GoTo((DirectoryNode)current);
    }

    [RelayCommand]
    private void Back()
    {
        var cd = Files.CurrentDirectory;
        if (cd.ParentDirectory != null)
        {
            Files.GoTo(cd.ParentDirectory);
        }
    }

    /// <summary>
    /// Processes all the files and creates the output file.
    /// </summary>
    public void ProcessFiles(IProgress<float> progress)
    {
        FileStream outfile = new(OutputFile, FileMode.Create);
        byte[] buffer = File.ReadAllBytes(InputFile);
        outfile.Write(buffer);
        using (ZipArchive archive = new(outfile, ZipArchiveMode.Create))
        {
            Files.AddItemsToArchive(Files.RootDirectory, archive, progress);
        }
        outfile.Close();
    }
}
