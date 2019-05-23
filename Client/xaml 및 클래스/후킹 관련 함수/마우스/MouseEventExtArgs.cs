using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    class MouseEventExtArgs : MouseEventArgs
    {
        public MouseEventExtArgs(MouseButtons buttons, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta)
        {

        }

        internal MouseEventExtArgs(MouseEventArgs e)
            : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {

        }

        private bool m_Handled;

        public bool Handled
        {
            get { return m_Handled; }
            set { m_Handled = value; }
        }
    }
}
