using System.ComponentModel;

namespace FileHider.Lib
{
    public class ArchiveOptions : INotifyPropertyChanged
    {
        private System.IO.Compression.CompressionLevel compressionLevel;
        public System.IO.Compression.CompressionLevel CompressionLevel
        {
            get {  return compressionLevel; }
            set
            {
                compressionLevel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompressionLevel)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // To-do implement password protection.
        //private bool isPasswordProtected;
        //public bool IsPasswordProtected { get => isPasswordProtected;
        //    set
        //    {
        //        isPasswordProtected = value;
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPasswordProtected)));
        //    }
        //}

        //string? password;
        //public string? Password
        //{
        //    get { return password; }
        //    set
        //    {
        //        password = value;
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
        //    }
        //}
    }
}
