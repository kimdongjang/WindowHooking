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
    public partial class CreateChannel : Window
    {
        MyClient client = MyClient.Instance;
        private bool IsUsedPassword = false;
        
        #region 싱글톤
        static CreateChannel instance = null;
        static readonly object padlock = new Object();
        public static CreateChannel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new CreateChannel();
                    }
                    return instance;
                }
            }
        }
        public CreateChannel()
        {
            instance = this;
            InitializeComponent();
        }
        #endregion

        private void RequestCreateChannelButton(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("채널명을 입력하세요.");
                return;
            }
            if (textBox2.Text == "")
            {
                MessageBox.Show("접속 가능한 인원수를 입력하세요.");
                return;
            }
            if (int.Parse(textBox2.Text) > 100)
            {
                MessageBox.Show("인원 제한은 최대 100명까지 가능합니다.");
                return;
            }
            if (Data.Instance.myId == "")
            {
                return;
            }

            string packet = "CHANNEL_ADD" + "$";
            if (IsUsedPassword == true) // 비밀번호
            {
                packet += "PASSTRUE" + "#";
                packet += textBox3.Text + "#";
            }
            else
            {
                packet += "PASSFALSE" + "#";
                packet += "0" + "#";
            }
            packet += textBox1.Text + "#"; // 채널명
            packet += textBox4.Text + "#"; // 채널설명
            packet += Data.Instance.myId + "#";// 로그인시 저장된 ID
            packet += textBox2.Text; // 인원제한

            Data.Instance.myChannelName.Add(textBox1.Text);
            client.SendDataOne(packet);

            string packet2 = "FTP_INITLIST" + "$";
            packet2 += textBox1.Text + "#";
            packet2 += Data.Instance.myId;

            client.SendDataOne(packet2);
        }

        private void textBox2_PreviewTextInput(object sender, TextCompositionEventArgs e) // textBox2에 숫자만 입력받음
        {
            foreach(char c in e.Text)
            {
                if(!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        private void textBox3_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) || e.Text == "0")
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        private void PWcheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (PWcheckBox.IsChecked == true)
            {
                textBox3.IsEnabled = true;
                IsUsedPassword = true;
            }
            else if (PWcheckBox.IsChecked == false)
            {
                textBox3.IsEnabled = false;
                IsUsedPassword = false;
            }
        }

    }
}
