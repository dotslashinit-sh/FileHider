using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using FileHider.ViewModels;
using FileHider.Views;

namespace FileHider.Services
{
    /// <summary>
    /// Implements methods to show dialogs.
    /// </summary>
    public class DialogService
    {
        public static async Task ShowDialogAsync(string title, string message)
        {
            var desktopLifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (desktopLifetime?.MainWindow is not Window owner)
            {
                return;
            }

            var dialogVM = new DialogViewModel();
            dialogVM.Title = title;
            dialogVM.Message = message;
            var dialogWindow = new DialogWindow
            {
                DataContext = dialogVM
            };
            dialogVM.CloseRequest += () =>
            {
                dialogWindow.Close();
            };

            await dialogWindow.ShowDialog(owner);
        }
    }
}
