using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// InvisibleWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DragCaptureWindow : Window
    {
        private bool m_isDown = false;
        private double m_StPosX = -1;
        private double m_StPosY = -1;
        private Rect tRect;

        public Rect SelRegon
        {
            get
            {
                return tRect;
            }
        }

        public DragCaptureWindow()
        {
            InitializeComponent();
        }

        private void Canvas_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            m_isDown = true;
            m_StPosX = e.GetPosition(this).X;
            m_StPosY = e.GetPosition(this).Y;
            selRect.Visibility = System.Windows.Visibility.Visible;
            e.Handled = true;
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_isDown = false;
            e.Handled = true;
            selRect.Visibility = System.Windows.Visibility.Collapsed;
            this.Close();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            tRect = new Rect(new Point(m_StPosX, m_StPosY), new Point(e.GetPosition(this).X, e.GetPosition(this).Y));

            if (m_isDown == true)
            {
                Canvas.SetLeft(selRect, tRect.Left);
                Canvas.SetTop(selRect, tRect.Top);
                selRect.Width = tRect.Width;
                selRect.Height = tRect.Height;
            }
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
        }
    }
}