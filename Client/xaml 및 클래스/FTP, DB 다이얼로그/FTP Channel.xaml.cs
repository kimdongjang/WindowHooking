using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FTP_Channel : Window
    {
        OpenFileDialog openFileDlg = null;
        //FileInfo fInfo = null;
        private string FTPid = "wb";
        private string FTPpw = "123";
        LogSaveSetting logsavesetting;
        private string myfilepath;
        string[] openedFilePath;

        private string FileName { get { return Name_list.SelectedItem.ToString(); } }
        public string ChannelIndex { get; set; }
        public string ChannelName { get; set; }
        public string ChannelAdmin { get; set; }

        public FTP_Channel(string cindex, string cname, string cadmin)
        {
            InitializeComponent();
            this.ChannelIndex = cindex;
            this.ChannelName = cname;
            this.ChannelAdmin = cadmin;

            Data.Instance.uploadfile_cidx = this.ChannelIndex; // 현재 채널정보를 데이터 클래스에 저장
            PathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // 파일 저장경로를 내문서로 초기화시킴
            FTPInit();
        }

        public FTP_Channel()
        {

        }

        private void FTPInit()
        {
            string packet = "FTP_INITLIST" + "$";
            packet += ChannelName + "#";
            packet += Data.Instance.myId;

            MyClient.Instance.SendDataOne(packet);
        } // 파일 리스트 요청

        public void FileList_Load(string id, string pw, string files)
        {
            this.FTPid = id;
            this.FTPpw = pw;

            string[] filename = files.Split('#');
            //채널이름 파일리스트
            Name_list.Items.Clear();
            for (int i = 0; i < filename.Length - 1; i++)
            {
                Name_list.Items.Add(filename[i]);
            }

            return;
        } // 파일 리스트 수신

        private void Download_Location_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            string selected = dialog.SelectedPath;

            PathBox.Text = selected;
        } // 파일 경로 지정

        public void MyFileDownload()
        {
            try
            {
                //파일 경로
                //string localPath = @"D:\";
                if (MainRecording.Instance.dblist_Dlg.DownPath.Text == "")
                {
                    System.Windows.Forms.MessageBox.Show("경로를 지정해 주세요.");
                    return;
                }
                FtpWebRequest requestFileDownload =
                (FtpWebRequest)WebRequest.Create("ftp://192.168.0.8:8000/home/" + Data.Instance.myId + "/" + MainRecording.Instance.dblist_Dlg.FileName.Text);

                requestFileDownload.Method = WebRequestMethods.Ftp.DownloadFile;

                // This example assumes the FTP site uses anonymous logon.
                requestFileDownload.Credentials = new NetworkCredential(FTPid, FTPpw);

                FtpWebResponse responseFileDownload = (FtpWebResponse)requestFileDownload.GetResponse();

                string text = @MainRecording.Instance.dblist_Dlg.DownPath.Text + "/" + MainRecording.Instance.dblist_Dlg.FileName.Text;

                Stream responseStream = responseFileDownload.GetResponseStream();
                FileStream writeStream = new FileStream(text, FileMode.Create);


                int Length = 2048;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);

                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                }
                System.Windows.Forms.MessageBox.Show("파일 다운로드 성공");

                responseStream.Close();
                writeStream.Close();

                requestFileDownload = null;
                responseFileDownload = null;

                FTPInit();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        } // 파일 다운 진행

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Name_list.SelectedItem == null)
                {
                    System.Windows.Forms.MessageBox.Show("파일이 선택되지 않았습니다. 파일을 선택해주세요.");
                }
                else
                {
                    string packet = "FILE_DOWNLOAD" + "$";
                    packet += Data.Instance.myId + "#";
                    packet += this.ChannelName + "#";
                    packet += FileName;

                    MyClient.Instance.SendDataOne(packet);
                }
            }
            catch (Exception)
            {
                return;
            }
        } // 파일 다운 요청

        public void FileDownload()
        {
            try
            {
                //파일 경로
                //string localPath = @"D:\";
                if (PathBox.Text == "")
                {
                    System.Windows.Forms.MessageBox.Show("경로를 지정해 주세요.");
                    return;
                }
                FtpWebRequest requestFileDownload =
                (FtpWebRequest)WebRequest.Create("ftp://192.168.0.8:8000/home/" + ChannelName + "/" + FileName);

                requestFileDownload.Method = WebRequestMethods.Ftp.DownloadFile;

                // This example assumes the FTP site uses anonymous logon.
                requestFileDownload.Credentials = new NetworkCredential(FTPid, FTPpw);

                FtpWebResponse responseFileDownload = (FtpWebResponse)requestFileDownload.GetResponse();

                string text = @PathBox.Text + "/" + FileName;

                Stream responseStream = responseFileDownload.GetResponseStream();
                FileStream writeStream = new FileStream(text, FileMode.Create);


                int Length = 2048;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);

                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                }
                System.Windows.Forms.MessageBox.Show("파일 다운로드 성공");

                responseStream.Close();
                writeStream.Close();

                requestFileDownload = null;
                responseFileDownload = null;

                FTPInit();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        } // 파일 다운 진행

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            openFileDlg = new OpenFileDialog();
            openFileDlg.Multiselect = true;

            if (openFileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                openedFilePath = new string[openFileDlg.FileNames.Length]; // 여러 이미지를 선택해 스트링 배열에 이미지 파일 경로+파일명을 가져옴

                for (int i = 0; i < openFileDlg.FileNames.Length; i++)
                {
                    openedFilePath[i] = openFileDlg.FileNames[i].ToString();
                    //openedFilePath[i] = 호환할 이미지 경로+파일이름
                }
            }

            string packet = "FILE_UPLOAD" + "$";
            packet += Data.Instance.myId;

            MyClient.Instance.SendDataOne(packet);
        } // 파일 업로드 요청

        public void FileUpload()
        {
            try
            {
                for (int i = 0; i < openedFilePath.Length; i++)
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://192.168.0.8:8000/home/" + ChannelName + "/" + System.IO.Path.GetFileName(openedFilePath[i]));
                    request.Method = WebRequestMethods.Ftp.UploadFile;

                    request.Credentials = new NetworkCredential(FTPid, FTPpw);


                    byte[] fileContents = new byte[2048];
                    fileContents = File.ReadAllBytes(openedFilePath[i]);

                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(fileContents, 0, fileContents.Length);
                    requestStream.Close();

                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                    response.Close();
                    Name_list.Items.Clear();

                    FTPInit();
                }
                System.Windows.Forms.MessageBox.Show("파일 업로드 성공");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        } // 파일 업로드 진행

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Name_list.SelectedItem == null)
                {
                    System.Windows.Forms.MessageBox.Show("파일이 선택되지 않았습니다. 파일을 선택해주세요.");
                }
                else
                {
                    string packet = "FILE_DELETE" + "$";
                    packet += Data.Instance.myId + "#";
                    packet += this.ChannelName + "#";
                    packet += FileName;

                    MyClient.Instance.SendDataOne(packet);
                }
            }
            catch (Exception)
            {
                return;
            }
        } // 파일 삭제 요청

        public void LogFileUpload()
        {
            try
            {
                myfilepath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog");
                logsavesetting = new LogSaveSetting();
                string filename = Data.Instance.ftp_myfilename + ".xml";

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://192.168.0.8:8000/home/" + Data.Instance.myId + "/" + filename);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(FTPid, FTPpw);


                byte[] fileContents = new byte[2048];
                fileContents = File.ReadAllBytes(myfilepath + "\\" + filename);

                request.ContentLength = fileContents.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                FileUploadSuccess();

                //System.Windows.Forms.MessageBox.Show("Upload File Complete, status {0}", response.StatusDescription);

                response.Close();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public void FileUploadSuccess()
        {
            string packet = "FILEUPLOAD_SUCCESS" + "$";
            packet += Data.Instance.myId + "#";
            packet += Data.Instance.myFileInfo;

            MyClient.Instance.SendDataOne(packet);
        }

        public void MyFileUpload()
        {
            try
            {
                string Path = Data.Instance.MyfilePath;

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://192.168.0.8:8000/home/" + Data.Instance.myId + "/" + Data.Instance.myFileInfo);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(FTPid, FTPpw);

                StreamReader sourceStream = new StreamReader(Path);

                byte[] fileContents = new byte[2048];
                fileContents = File.ReadAllBytes(Path);

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                System.Windows.Forms.MessageBox.Show("파일 업로드 성공");

                sourceStream.Close();
                response.Close();

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        } // 파일 업로드 진행

        public void FileDelete()
        {
            try
            {
                System.Net.FtpWebRequest ftp = (FtpWebRequest)WebRequest.Create("ftp://192.168.0.8:8000/home/" + ChannelName + "/" + FileName);

                ftp.Credentials = new NetworkCredential(FTPid, FTPpw);
                ftp.Method = System.Net.WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)ftp.GetResponse();

                System.Windows.Forms.MessageBox.Show("파일 삭제 성공");
                Name_list.Items.Clear();
                response.Close();

                FTPInit();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        } // 채널 파일 삭제

        public void MyFileDelete()
        {
            try
            {
                string URL = Data.Instance.myId + "/" + Data.Instance.DeleteFileName;
                System.Net.FtpWebRequest ftp = (FtpWebRequest)WebRequest.Create("ftp://192.168.0.8:8000/home/" + URL);

                ftp.Credentials = new NetworkCredential(FTPid, FTPpw);
                ftp.Method = System.Net.WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)ftp.GetResponse();

                //System.Windows.Forms.MessageBox.Show("파일 삭제 성공");

                response.Close();
            }
            catch
            {
            }
        } // 내 파일 삭제

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Data.Instance.ftpOn = false;
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}