using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frumble
{
    public interface IViewItem
    {
        public string ItemName { get; set; }
        public string ItemPath { get; set; }

        public string ToString();
    }
}
