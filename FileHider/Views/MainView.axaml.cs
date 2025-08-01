using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FileHider.Lib;
using FileHider.Lib.FilesList;
using FileHider.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Schema;
using static System.Net.WebRequestMethods;

namespace FileHider.Views;

public partial class MainView : UserControl
{
    MainViewModel VM;

    public MainView()
    {
        InitializeComponent();

        // Attach the main view model
        VM = new MainViewModel();
        this.DataContext = VM;
        this.Loaded += LoadCompleted;
    }

    private void LoadCompleted(object? sender, RoutedEventArgs e)
    {
        var sp = TopLevel.GetTopLevel(this)?.StorageProvider;
        VM.StorageProvider = sp;
    }

    private void FilesList_DoubleTap(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if(VM.EnterDirCommand.CanExecute(null) == true)
        {
            VM.EnterDirCommand.Execute(null);
        }
    }
}

