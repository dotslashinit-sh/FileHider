using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHider.Lib.FilesList
{
    public interface INode
    {
        string Name { get; }

        /// <summary>
        /// Returns the virtual realPath to the list item as a string.
        /// </summary>
        /// <returns>`string`: Virtual realPath to the file/parent.</returns>
        string GetVirtualPath();
        void Delete();
    }
}
