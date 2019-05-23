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
    /// LogCapture.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogCapture : Window
    {
        #region 싱글톤
        static LogCapture instance = null;
        static readonly object padlock = new Object();
        public static LogCapture Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new LogCapture();
                    }
                    return instance;
                }
            }
        }
        public LogCapture()
        {
            instance = this;
            InitializeComponent(); 
        }
        #endregion
        
        private void Button_Click(object sender, RoutedEventArgs e)
        { 
            this.Hide();
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
        }
    }
}
