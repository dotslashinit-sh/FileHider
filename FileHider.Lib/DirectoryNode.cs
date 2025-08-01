using FileHider.Lib.FilesList;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHider.Lib.FilesList;

public class DirectoryNode : INode
{
    public string Name { get; }
    public DirectoryNode? ParentDirectory { get; }

    static private DirectoryNode _root = new DirectoryNode("", null);
    static public DirectoryNode RootDir { get { return _root; } }
    public ObservableCollection<INode> Children { get; }

    private DirectoryNode(string name, DirectoryNode? parent)
    {
        Name = name;
        ParentDirectory = parent;
        Children = new();
        if (parent != null)
        {
            parent.AddChild(this);
        }
    }

    /// <summary>
    /// Creates a new `DirectoryNode`.
    /// </summary>
    /// <param name="name">Name of the directory</param>
    /// <param name="parent">Parent directory</param>
    /// <returns>A new DirectoryNode</returns>
    public static DirectoryNode Create(string name, DirectoryNode parent)
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
        foreach (var child in Children)
        {
            child.Delete();
        }
        Children.Clear();
    }

    public void DeleteItem(INode item)
    {
        Children.Remove(item);
    }

    public void AddChild(INode item)
    {
        Children.Add(item);
    }
}
