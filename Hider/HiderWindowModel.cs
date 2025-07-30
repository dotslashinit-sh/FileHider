using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hider
{
    internal class HiderWindowModel
    {
        public FileHider.Lib.Session Session { get; set; }

        public HiderWindowModel()
        {
            Session = new();
        }
    }
}
