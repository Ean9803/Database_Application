using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Control
{
    class ItemBtn : GroupBox
    {
        private EventHandler? ClickItem;
        public event EventHandler OnItemClick
        {
            add
            {
                ClickItem += value;
                Click += value;
            }
            remove
            {
                ClickItem -= value;
                Click -= value;
            }
        }

        public void ActivateClickItem()
        {
            ClickItem?.Invoke(this, EventArgs.Empty);
        }
    }
}
