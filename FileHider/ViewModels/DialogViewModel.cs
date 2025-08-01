using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FileHider.ViewModels
{
    public partial class DialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _message;

        public DialogViewModel()
        {
            Title = "";
            Message = "";
        }

        [RelayCommand]
        private void Ok()
        {
            CloseRequest?.Invoke();
        }

        public event System.Action? CloseRequest;
    }
}
