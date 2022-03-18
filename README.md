# File Hider

## Introduction
A simple application to hide files inside JPG, MP3 and MP4 files.

## Usage
 1) Launch Hider.exe
 2) Click on browse to select a base file which is used to hide the other files.
 3) Click on the `Add Files` button and add as many files as you want to add. If you accidently added file which you didn't intend to,
 you can select it from the list and click on the `Remove Item` button.
 4) Click on the `Hide File!` button to hide the files. A save file dialog appears.
 5) Save the new file somewhere. (**NOTE: The saved file will have the extension of the base file.**)
 6) Right-click and open the saved file with an archive manager like WinRAR or 7-Zip to view the hidden files.

## Using the output file
Open the output file with a zip archive application like WinRAR, WinZIP or 7-Zip and then extract the files.

## Downloads
The latest releases can be found [here](https://github.com/dotslashinit-sh/FileHider/releases). Running these will require .NET 6.0 runtime which you can download from [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.3-windows-x64-installer).

## Compiling
You can build the project using any one of these following methods.

### Using .NET 6.0 SDK
1) Download and install .NET 6.0 SDK from [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.201-windows-x64-installer).
2) Clone the repo and go to the root folder.
3) Run `dotnet build`.

### Using Visual Studio 2022
1) Download and install Visual Studio Community Edition 2022 with the option "Desktop development with .NET" selected.
2) Open FileHider.sln
3) Build and run the project.

## License
[MIT License](./LICENSE.md)
