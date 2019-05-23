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
    /// ChannelInformation.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChannelInformation : Window
    {
        #region 싱글톤
        static ChannelInformation instance = null;
        static readonly object padlock = new Object();
        public static ChannelInformation Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ChannelInformation();
                    }
                    return instance;
                }
            }
        }
        public ChannelInformation()
        {
            instance = this;
            InitializeComponent();
        }
        #endregion

        public void ChannelInformationTextInput(string text)
        {
            info_textBox.Text = text;
        }
    }
}
