using System.IO;
using System.IO.Compression;
using System.Collections;

namespace Hider
{
    class HiderSession
    {
        string inputFileExt;
        string inputFile;
        string outputFile;

        // List of files and entry names to be hidden.
        // The entries are stored in tuples as (string, string) with Item1 being the full location of the file,
        // and Item2 being the entry name, with relative path to the file.
        ArrayList filesList;

        public HiderSession()
        {
            inputFile = "";
            inputFileExt = "";
            outputFile = "";
            filesList = new();
        }

        public void AddFile(string path, string filename)
        {
            filesList.Add((path, filename));
        }

        public void AddDirectory(string path, string rootDir)
        {
            string[] files = Directory.GetFiles(path);
            foreach(string file in files)
            {
                filesList.Add((file, Path.GetRelativePath(rootDir, file)));
            }

            string[] directories = Directory.GetDirectories(path);
            foreach(string dir in directories)
            {
                this.AddDirectory(dir, rootDir);
            }
        }

        public void RemoveEntry(string entry)
        {
            foreach((string, string) e in filesList)
            {
                if(e.Item1 == entry)
                {
                    filesList.Remove(e);
                    break;
                }
            }
        }

        public void RemoveAll()
        {
            filesList.Clear();
        }

        public void SetInputFile(string file)
        {
            inputFile = file;
            if (File.Exists(inputFile))
            {
                inputFileExt = Path.GetExtension(inputFile);
            }
        }

        public void SetOutputFile(string file)
        {
            outputFile = file;
        }

        public string GetInputFile()
        {
            return inputFile;
        }

        public string GetInputFileExt()
        {
            return inputFileExt;
        }

        public string GetOutputFile()
        {
            return outputFile;
        }

        public void ProcessFiles()
        {
            MemoryStream archiveStream = new();
            ZipArchive archive = new(archiveStream, ZipArchiveMode.Create);

            foreach ((string, string) entry in filesList)
            {
                if (File.Exists(entry.Item1))
                    archive.CreateEntryFromFile(entry.Item1, entry.Item2);
            }

            byte[] baseFileData = File.ReadAllBytes(inputFile);
            byte[] archiveData = archiveStream.ToArray();
            byte[] outputData = new byte[baseFileData.Length + archiveData.Length];

            baseFileData.CopyTo(outputData, 0);
            archiveData.CopyTo(outputData, baseFileData.Length);
            File.WriteAllBytes(outputFile, outputData);
        }
    }
}