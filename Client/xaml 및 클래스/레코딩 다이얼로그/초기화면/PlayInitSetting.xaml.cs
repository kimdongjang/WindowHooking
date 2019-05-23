using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace Client
{
    /// <summary>
    /// PlayInit.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PlayInitSetting : Window
    {
        KeyboardHooking kh_class = KeyboardHooking.Instance;
        LogCapture lc_class = LogCapture.Instance;
        string initImg;

        public PlayInitSetting()
        {
            InitializeComponent();
            PlayInitLoadHeader();
            Playing_WindowSize_TextBox.Text = MainRecording.Instance.Window_Width.ToString() + "/" + MainRecording.Instance.Window_Height.ToString();
        }

        public void PlayInitLoadHeader() // 헤더파일의 로그를 읽는다.
        {
            try
            {
                //string filepathname = Data.Instance.init_loglistview[MainRecording.Instance.xaml_LogList.SelectedIndex].FilePath;

                XmlTextReader reader = new XmlTextReader(SaveFunction.Instance.logplay_start_filename); // 선택한 로그 파일을 읽는다.

                string col1_string = "";
                string col2_string = "";
                string read_string;
                
                while (reader.Read())
                {
                    switch (reader.LocalName)
                    {
                        case "WindowSize":
                            read_string = reader.ReadElementContentAsString(); 
                            string[] winSize = read_string.Split('#'); // 윈도우 해상도를 가져옴
                            SavedLog_WindowSize_TextBox.Text = winSize[0] + "/" + winSize[1];
                            break;
                        case "col1":
                            col1_string = reader.ReadElementContentAsString();
                            break;
                        case "col2":
                            col2_string = reader.ReadElementContentAsString();
                            xaml_playinitlist.Items.Add(new InitPlaySetting(col1_string, col2_string));
                            break;
                        case "InitImg":
                            initImg = reader.ReadElementContentAsString(); 
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("로그 파일을 실행하시겠습니까?", "경고", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                lc_class.Hide();
                MainRecording.Instance.RecordingPlayButtonControl.Content = "■";
                MainRecording.Instance.LogXamlList.Items.Clear(); // 로그 리스트를 클리어
                MainRecording.Instance.PlayThread = new Thread(MainRecording.Instance.Play_Button);
                MainRecording.Instance.PlayThread.IsBackground = true;
                MainRecording.Instance.PlayThread.Start();
                MainRecording.Instance.UpShowListDlg(); // 실행중인 로그를 ShowList에 띄움
                this.Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InitImageCheck_Click(object sender, RoutedEventArgs e)
        {
            lc_class.Show();

            BitmapImage bi = new BitmapImage();      // BitmapImage로 우선 만들기
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bi.UriSource = new Uri(initImg);
            bi.EndInit();
            lc_class.LogImg.Source = bi;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }

    public class InitPlaySetting
    {
        public string Col1 { get; set; }
        public string Col2 { get; set; }
        public InitPlaySetting(string s1, string s2)
        {
            Col1 = s1;
            Col2 = s2;
            Data.Instance.initplaysetting.Add(this);
        }
    }
}
