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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace Client
{
    /// <summary>
    /// LogSaveSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogSaveSetting : Window
    {
        SaveFunction save = SaveFunction.Instance;

        int img_count = 0;
        int pause_count = 0;

        public LogSaveSetting()
        {
            InitializeComponent();
        }

        private void FileSaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int imagecount = 0;
                string oldfilepathname = "";
                string newfilepathname = "";
                 
                if (FileName_text.Text == null)
                {
                    MessageBox.Show("파일명이 입력되지 않았습니다. 파일명을 입력해주세요");
                    return;
                }
                if (FileNameCheck() == false)
                {
                    MessageBox.Show("이미 같은 이름의 파일이 있습니다."); // 덮어씌우시겠습니까? yes / no 로 판단 
                    return;
                }
                Data.Instance.Rec_Comment = LogData_text.Text;
                Data.Instance.ftp_myfilename = FileName_text.Text;
                SaveFunction.Instance.XmlCreateRecordList(SaveFunction.Instance.g_filepath + FileName_text.Text + ".xml");

                string packet = "RECORDING_SAVE" + "$";
                packet += Data.Instance.myId;
                MyClient.Instance.SendDataOne(packet);

                #region 이전에 저장한 이미지파일 이름들을 지정된 xml파일 이름으로 변경
                DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog"));

                foreach (var item in di.GetFiles())
                {
                    if (item.Name.Contains(SaveFunction.Instance.g_filenameexceptxml) && item.Name.Contains(".jpg")) // 자동저장 파일이름, jpg확장자 구별
                    {
                        oldfilepathname = SaveFunction.Instance.g_filepath + SaveFunction.Instance.g_filenameexceptxml + "_image[" + imagecount + "].jpg";
                        newfilepathname = SaveFunction.Instance.g_filepath + FileName_text.Text + "_image[" + imagecount + "].jpg";
                        File.Move(oldfilepathname, newfilepathname);
                        imagecount++;
                    }
                }
                #endregion

                #region 자동저장 옵션이 꺼져있다면 미리 만들어진 log(index).xml을 삭제함
                if (RecordSetting.Instance.is_autosavechecked == false)
                    File.Delete(SaveFunction.Instance.autosave_filename_for_delete);

                SaveFunction.Instance.InitSaveFileLoad(); // 저장된 로그 목록 출력(갱신)
                MainRecording.Instance.ShowList_Dlg.Hide();
                PauseEvent.Instance.Hide();
                this.Hide();
                MainRecording.Instance.WindowStateChange("normal");
                MainRecording.Instance.StopEvent();
                #endregion

                //========================================================================================
                // 기존 자동저장되는 로그의 파일이름을 사용자에 의해 다른이름으로 저장되고
                // Xml의 <InitImage>태그안의 이름도 그 다른이름으로 덮어씌우기
                string EditFile = SaveFunction.Instance.g_filepath + FileName_text.Text + ".xml";
                string InitImg = SaveFunction.Instance.g_filepath + FileName_text.Text + "_image["+ 0 +"].jpg";
                int k = 1;

                XmlDocument doc = new XmlDocument();
                doc.Load(EditFile);
                XmlNode Node = doc.DocumentElement;
                XmlNode InitImgNode = Node.SelectSingleNode("InitImg");
                InitImgNode.InnerText = InitImg;

                this.CheckingCount(EditFile);

                for (int i = 0; i < pause_count / 2; i++)
                {
                    for (int j = 0; j < img_count / 2; j++)
                    { 
                        XmlNode PauseNode = Node.SelectSingleNode("Pause" + i.ToString());
                        XmlNode ImgNode = PauseNode.SelectSingleNode("Image" + j.ToString());
                        ImgNode.InnerText = SaveFunction.Instance.g_filepath + FileName_text.Text + "_image[" + k + "].jpg";
                        PauseNode.AppendChild(ImgNode);
                        k++;
                    } 
                }
                doc.Save(EditFile);
                Data.Instance.screenIndex = 0;  // 하나의 로그마다 스크린샷 인덱스가 0으로 초기화
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void CheckingCount(string logName)
        { 
            XmlTextReader reader = new XmlTextReader(logName); // 사용자가 선택한 로그 파일의 경로를 검색
            while (reader.Read())
            {
                if (reader.LocalName.Contains("Pause"))
                    pause_count++;
                else if (reader.LocalName.Contains("Image"))
                    img_count++;
            }
            reader.Close();
        }
        private bool FileNameCheck()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SLog\";

            DirectoryInfo di = new DirectoryInfo(path);

            foreach (var item in di.GetFiles())
            {
                if (item.Name == FileName_text.Text + ".xml")
                {
                    return false;
                }
            }
            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (RecordSetting.Instance.is_autosavechecked == false)
                File.Delete(SaveFunction.Instance.g_filepathname);

            if (System.Windows.Forms.MessageBox.Show("지금까지 녹화한 데이터를 전부 삭제하시겠습니까?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                MainRecording.Instance.StopEvent();
                PauseEvent.Instance.Hide();

                if (MainRecording.Instance.ShowList_Dlg == null)
                    return;
                MainRecording.Instance.ShowList_Dlg.Hide();
            }
            else
            {
                SaveFunction.Instance.GetSaveFileIndex();
            }
            MainRecording.Instance.WindowStateChange("normal");
            this.Hide();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainRecording.Instance.StopEvent();
            MainRecording.Instance.WindowState = WindowState.Normal;

            if (KeyboardHooking.Instance.memo_dlg != null) KeyboardHooking.Instance.memo_dlg.Close();
            if (MainRecording.Instance.ShowList_Dlg != null) MainRecording.Instance.ShowList_Dlg.Close();
            this.Close();
        }
    }
}