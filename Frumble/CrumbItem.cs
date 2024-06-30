using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frumble
{
    public class CrumbItem
    {
        public string ItemName { get; set; }
        public string ItemPath { get; set; }

        public CrumbItem(string itemPath)
        {
            ItemPath = itemPath;
            ItemName = Path.GetFileName(itemPath);
        }

        public override string ToString()
        {
            return ItemName;
        }
    }
}
