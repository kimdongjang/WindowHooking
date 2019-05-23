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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// AddChannel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChannelDialog : UserControl
    {
        public bool islockedchannel = false;
        public FTP_Channel ftp = null;

        #region 싱글톤
        static ChannelDialog instance = null;
        static readonly object padlock = new Object();
        public static ChannelDialog Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ChannelDialog();
                    }
                    return instance;
                }
            }
        }
        public ChannelDialog()
        {
            instance = this;
            InitializeComponent();
        }
        #endregion
        
        public string ChannelName { get; set; }
        public string ChannelIndex { get; set; }
        public string ChannelAdmin { get; set; }
        public string ChannelUserList { get; set; }

        public ChannelDialog(string name, string index, string admin, string userlist)
        {
            ChannelName = name;
            ChannelIndex = index;
            ChannelAdmin = admin;
            ChannelUserList = userlist;

            InitializeComponent();
        }

        public void ChannelInit(string users) // 채널 접속자 명단을 띄움
        {
            string[] memberlist = users.Split('#');

            xaml_JoinList.Items.Clear();
            for (int i = 0; i < memberlist.Length - 1; i++)
            {
                xaml_JoinList.Items.Add(memberlist[i]);
            }
        }

        public void InChannelUserListRefresh(string userlist, string user, int idx)
        {
            string[] memberlist = userlist.Split('#');

            xaml_JoinList.Items.Clear();
            for (int i = 0; i < memberlist.Length - 1; i++)
            {
                xaml_JoinList.Items.Add(memberlist[i]);
            }

            switch(idx)
            {
                case 1: xaml_Chatting.Text += user + "님께서 입장하셨습니다." + '\r'; break;
                case 2: xaml_Chatting.Text += user + "님께서 퇴장하셨습니다." + '\r'; break;
                case 3: xaml_Chatting.Text += "방장이 " + ChannelAdmin + "님에서 " + user + "님으로 변경되었습니다." + '\r'; 
                        ChannelAdmin = user; break;
                case 4: xaml_Chatting.Text += "방장이 " + user + "님을 강제퇴장하였습니다." + '\r';
                    ChannelAdmin = user; break;
            }
            return;
        } // 채널 갱신

        private void ChannelChat(string msg) // 채널 채팅 패킷
        {
            string packet = "ICHATTING_SEND" + "$";     // 전체채팅 패킷 메시지 
            packet += Data.Instance.myId + "#";         // 로그인한 아이디  
            packet += this.ChannelName + "#";       // 채널이름 
            packet += msg;                              // 채팅 내용

            MyClient.Instance.SendDataOne(packet);
        }

        public void ChannelChatting(string[] pack) // RecvClass.cs(서버)에서 채팅내용 받는 함수
        {
            string[] chat_data = pack[1].Split('#');
            MainRecording.Instance.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
            {
                string chatLine = "[" + chat_data[0] + "] :" + chat_data[2] + "\n";
                this.xaml_Chatting.Text += chatLine;
            });
        }

        private void xaml_ChattingTxt_KeyDown(object sender, KeyEventArgs e) // 엔터키로 메세지 전송
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ChannelChat(xaml_ChattingTxt.Text);
                xaml_ChattingTxt.Clear();
            }
            else
            {
                return;
            }
        }

        private void Admin_GiveHandle_Click(object sender, RoutedEventArgs e) // 채널 관리자 변경 메뉴 아이템
        {
            if (ChannelAdmin != Data.Instance.myId) // 채널 관리자가 아닌 경우
            {
                MessageBox.Show("해당 권한이 없습니다.");
                return;
            }
            else
            {
                if(xaml_JoinList.SelectedItem.ToString() == Data.Instance.myId) // 본인을 선택한 경우
                {
                    MessageBox.Show("잘못된 명령입니다.");
                    return;
                }

                string packet = "CHANNEL_ADMIN_GIVEHANDLE" + "$";
                packet += ChannelName + "#";
                packet += ChannelAdmin + "#";
                packet += xaml_JoinList.SelectedItem.ToString();

                MyClient.Instance.SendDataOne(packet);
            }
        }
        
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            ChannelChat(xaml_ChattingTxt.Text);
            xaml_ChattingTxt.Clear();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Channel_Exit();
        }

        public void Channel_Exit() // 채널 나가기
        {
            // =================================================================================
            for (int i = 0; i < Data.Instance.MyChannelList.Count; i++)
            {
                if (Data.Instance.MyChannelList[i].ChannelName == this.ChannelName)
                {
                    MainRecording.Instance.xaml_TabControl.Items.RemoveAt(2 + i);
                }
            }
            // =================================================================================
            string packet = "CHANNEL_EXIT" + "$";
            packet += this.ChannelName + "#";
            packet += Data.Instance.myId;
            Data.Instance.mybeforeChannel = this.ChannelName;

            MyClient.Instance.SendDataOne(packet);

            // 접속 해제가 되었으니 자신의 채널 헤더값을 -1빼기
            Data.Instance.myChannelheader--;
            // 접속 해제가 되었으니 채널 리스트에서 삭제
            Data.Instance.MyChannelList.Remove(this);
        }

        public void RecvChannelDelete()
        {
            if (Data.Instance.myId != Control.Instance.GetAddChannel(new TabItem().Content.ToString()).ChannelName)  // 삭제대상 채널에 관리자가 아닌 사용자가 있을 경우
            {
                MessageBox.Show("해당 채널이 삭제되었습니다.");
                ChannelDialog.Instance.Channel_Exit();
            }
            else    // 삭제대상 채널의 관리자일 경우
                return;
            ChannelDialog.Instance.Channel_Exit();
        }

        private void ChannelDelete_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChannelAdmin != Data.Instance.myId) // 채널 관리자가 아닌 경우
            {
                MessageBox.Show("해당 권한이 없습니다.");
                return;
            }
            else
            {
                string packet = "CHANNEL_DELETE" + "$";
                packet += this.ChannelName;

                MyClient.Instance.SendDataOne(packet);
                Channel_Exit();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Data.Instance.ftpOn == false)
            {
                Data.Instance.ftpOn = true;
                ftp = new FTP_Channel(this.ChannelIndex, this.ChannelName, this.ChannelAdmin);
                ftp.Show();
            }
            else
                return;
        }
    }
}
