using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Frumble
{
    public class CrumbItem
    {
        public event EventHandler<TViewItem> ItemClicked;
        public string ItemName { get; set; }
        public string ItemPath { get; set; }

        public List<SubItem> ContainedItems = new();
        //public CrumbItem()
        //{
            
        //}
        public CrumbItem(TViewItem tViewItem)
        {
            ItemName = tViewItem.ItemName;
            ItemPath = tViewItem.ItemPath;
            foreach (TViewItem item in tViewItem.Items)
            {
                ContainedItems.Add(new SubItem(item));
            }
        }

        //private void TViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    ItemClicked?.Invoke(this, TViewItem);
        //}

        public override string ToString()
        {
            return ItemName;
        }
    }

    public class SubItem
    {
        public string ItemName { get; set; }
        public string ItemPath { get; set; }

        public SubItem(TViewItem item)
        {
            ItemName=item.ItemName;
            ItemPath = item.ItemPath;
        }

        public override string ToString()
        {
            return ItemName;
        }
    }
}
