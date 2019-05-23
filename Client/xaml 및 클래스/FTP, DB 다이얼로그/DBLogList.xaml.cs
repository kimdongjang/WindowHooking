using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// DBLogList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DBLogList : Window
    {
        OpenFileDialog fd = null;
        FTP_Channel ftpserver;
        public bool AscButton = false;
        public bool DescButton = false;

        public DBLogList()
        {
            InitializeComponent();
            DBLogListInit();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainRecording.Instance.InitDBlist = false;
            this.Close();
        }

        public void DBLogListInit()
        {
            string packet = "ID_LOGLISTINIT" + "$";
            packet += Data.Instance.myId;

            MyClient.Instance.SendDataOne(packet);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Data.Instance.DeleteFileName = FileName.Text;

            string packet = "MY_DBFILEDELETE" + "$";
            packet += Data.Instance.myId + "#";
            packet += FileName.Text;

            MyClient.Instance.SendDataOne(packet);

            FileName.Text = "";
        }

        private void MyFileUpload_Click(object sender, RoutedEventArgs e)
        {
            fd = new OpenFileDialog();

            DialogResult dr = fd.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            else
            {
                Data.Instance.myFileInfo = fd.SafeFileName;

                Data.Instance.MyfilePath = fd.FileName;

                string packet = "MYFILE_UPLOAD" + "$";
                packet += Data.Instance.myId;

                MyClient.Instance.SendDataOne(packet);
            }
        }

        private void ASC_Button(object sender, RoutedEventArgs e)
        {
            if (AscButton == false)
            {
                AscButton = true;
                string packet = "LIST_ASC" + "$";
                packet += Data.Instance.myId;

                MyClient.Instance.SendDataOne(packet);
            }
            else if (AscButton == true)
            {
                AscButton = false;
                string packet = "LIST_DESC" + "$";
                packet += Data.Instance.myId;

                MyClient.Instance.SendDataOne(packet);
            }

        }

        private void DESC_Button(object sender, RoutedEventArgs e)
        {
            if (DescButton == false)
            {
                DescButton = true;
                string packet = "LIST_ASC_FILENAME" + "$";
                packet += Data.Instance.myId;

                MyClient.Instance.SendDataOne(packet);
            }
            else if (DescButton == true)
            {
                DescButton = false;
                string packet = "LIST_DESC_FILENAME" + "$";
                packet += Data.Instance.myId;

                MyClient.Instance.SendDataOne(packet);
            }

        }

        private void MyFileDown_Click(object sender, RoutedEventArgs e)
        {
            ftpserver = new FTP_Channel();
            ftpserver.MyFileDownload();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            string selected = dialog.SelectedPath;

            DownPath.Text = selected;
        }

        private void InitMemoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitDBLogList Init = (InitDBLogList)InitMemoList.SelectedItems[0];

            FileName.Text = Init.FileName;
        }
    }

    public class InitDBLogList
    {
        public string Today { get; set; }
        public string ClientID { get; set; }
        public string UseProgram { get; set; }
        public string Comment { get; set; }
        public string FileName { get; set; }


        public InitDBLogList(string today, string id, string useprogram, string comment, string filename)
        {
            Today = today;
            ClientID = id;
            UseProgram = useprogram;
            Comment = comment;
            FileName = filename;

            Data.Instance.initdbloglist.Add(this);
        }

    }
}