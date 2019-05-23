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

namespace Client
{
    /// <summary>
    /// SampleModule.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SampleModule : Window
    {
        #region 싱글톤
        static SampleModule instance = null;
        static readonly object padlock = new Object();
        public static SampleModule Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new SampleModule();
                    }
                    return instance;
                }
            }
        }
        public SampleModule()
        {
            instance = this;
            InitializeComponent();
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Window_MouseLeftButtonDown);
        }
        #endregion

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void mouduleName_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            mouduleName.Clear();
        }

        private void ModuleSaveButton_Click(object sender, RoutedEventArgs e)
        {
            ModuleSaveButtonEvent();
        }

        public void ModuleSaveButtonEvent()
        {
            string fileName = mouduleName.Text;     // 사용자가 지정한 파일 이름

            // 모듈 녹화본 저장경로
            string modRec_folderpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\sLogModule\" + fileName + ".xml";
            FileInfo fi = new FileInfo(modRec_folderpath);

            // 파일 이름 공백, 중복 예외처리
            if (fileName == "저장할 파일이름을 입력하세요." || fileName == "")
            {
                MessageBox.Show("저장할 파일이름을 입력하세요.");
                return;
            }
            if (fi.Exists)
            {
                MessageBox.Show("동일한 파일명이 이미 존재합니다.");
                return;
            }

            // 파일 xml으로 저장 
            SaveFunction.Instance.SaveModuleRecording();
            SaveFunction.Instance.XmlCreateMouduleRecList();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("모듈 녹화를 종료하시겠습니까?", "경고", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}