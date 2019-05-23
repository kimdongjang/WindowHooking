using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// OpenImage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageChecking : Window
    {
        string logfilename;
        public int image_checking_count = 0;
        public int pause_checking_count = 0;

        public ImageChecking()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {

        }

        public void SearchLogFileImage(string logname)
        {
            logfilename = logname;
            InitLogFileLoad(logname);
        }

        public void SearchIsImageChecked() // 이미지가 호환되어 있는지 여부를 검색
        {
            try
            {
                for (int i = 0; i < Data.Instance.initimage.Count; i++)
                {
                    if (Data.Instance.initimage[i].State == "X")
                    {
                        MainRecording.Instance.xaml_IsImageChecking.Content = "이미지 호환 여부 : X";
                        Data.Instance.Is_ImagePathChecked = false;
                        return;
                    }
                }
                MainRecording.Instance.xaml_IsImageChecking.Content = "이미지 호환 여부 : O";
                Data.Instance.Is_ImagePathChecked = true;
                return;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void InitLogFileLoad(string logname)
        {
            try
            {
                string initImgPath = "";
                //string ImgPath = "";

                bool IsInitImg = false;
                bool IsImg = false;

                XmlTextReader reader = new XmlTextReader(logname); // 사용자가 선택한 로그 파일의 경로를 검색

                while (reader.Read())
                {
                    if (reader.LocalName.Contains("InitImg"))
                    {
                        initImgPath = reader.ReadElementContentAsString();
                        IsInitImg = Is_ImageFile(initImgPath);
                        xaml_OpenImageList.Items.Add(new InitImage(System.IO.Path.GetFileName(initImgPath), IsInitImg));

                    }
                    else if (reader.LocalName.Contains("Image"))
                    {
                        string data = reader.ReadElementContentAsString();
                        IsImg = Is_ImageFile(data);
                        xaml_OpenImageList.Items.Add(new InitImage(System.IO.Path.GetFileName(data), IsImg));
                        image_checking_count++;
                    }
                }
                reader.Close();
                SearchIsImageChecked();
                Data.Instance.initimage.Clear();
            }
            catch
            {

            }
        }

        public bool Is_ImageFile(string path) // 파일이 그 폴더에 있는지 체크
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(System.IO.Path.GetDirectoryName(path));
                if (di.Exists == false)
                {
                    di.Create();
                }
                else
                {
                    foreach (var item in di.GetFiles())
                    {
                        string file = item.Name;

                        if (file == System.IO.Path.GetFileName(path))
                            return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateImageFile_Click(object sender, RoutedEventArgs e) // 여러 이미지를 선택해서 호환되어있지 않은 이미지를 검색해 이미지를 호환시킨다.
        {
            try
            {
                InitImage ImgState = (InitImage)xaml_OpenImageList.SelectedItems[0];    // 현재 xaml_OpenImageList 리스트 뷰에서 선택한 column 상태값 얻어오기
                if (xaml_OpenImageList.SelectedItem == null || ImgState.State == "O")
                {
                    System.Windows.MessageBox.Show("해당 이미지는 호환된 이미지입니다. 호환되지 않은 이미지를 선택해주세요");
                    return;
                }
                System.Windows.Forms.OpenFileDialog openFileDlg = new System.Windows.Forms.OpenFileDialog(); // 파일 찾아보기 다이얼로그
                openFileDlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                openFileDlg.Multiselect = true; // 파일 다중 선택
                openFileDlg.Filter = "JPEG(*.jpg)|*.jpg|BITMAP(*.bmp)|*.bmp|PNG(*.png)|*.png|모든파일(*.*)|*.*";
                openFileDlg.Title = "호환할 이미지를 선택하십시오.";
                //텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*"

                if (openFileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string[] openedFilePath = new string[openFileDlg.FileNames.Length]; // 여러 이미지를 선택해 스트링 배열에 이미지 파일 경로+파일명을 가져옴

                    for (int i = 0; i < openFileDlg.FileNames.Length; i++)
                    {
                        openedFilePath[i] = openFileDlg.FileNames[i].ToString();
                        //openedFilePath[i] = 호환할 이미지 경로+파일이름

                        if (openedFilePath[i].Contains(ImgState.Name))
                        {
                            ImgState.State = "O";

                            string Path = Data.Instance.filePath + ImgState.Name;
                            File.Move(openedFilePath[i], Path);
                            this.Close();
                            MainRecording.Instance.Refresh();
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class InitImage
    {
        public string Name { get; set; }
        public string State { get; set; }
        public InitImage(string s1, bool s2)
        {
            Name = s1;
            if (s2)
                State = "O";
            else
                State = "X";
            Data.Instance.initimage.Add(this);
        }
    }
}