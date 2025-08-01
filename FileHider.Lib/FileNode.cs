using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHider.Lib.FilesList;

public class FileNode : INode
{
    public string Name { get; }
    public string RealPath { get; }
    public DirectoryNode ParentDirectory { get; }

    private FileNode(string realPath, DirectoryNode parent)
    {
        RealPath = Path.GetFullPath(realPath);
        Name = Path.GetFileName(realPath);
        ParentDirectory = parent;
        parent.AddChild(this);
    }

    public static FileNode Create(string realPath, DirectoryNode parent) => new(realPath, parent);

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