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
    /// RecentListEdit.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecentListEdit : Window
    {
        // 편집 창을 클릭했을 때 오픈
        // 최근 불러온 목록을 관리할 수 있는 다이얼로그
        public bool is_opened_dlg = false;
        public RecentListEdit()
        {
            if (is_opened_dlg == false)
            {
                InitializeComponent();
                is_opened_dlg = true;
                Init_xaml_RecentList();
            }
        }
        public void Init_xaml_RecentList()
        {
            //xaml_RecentList.Items.Clear();
            //for (int i = 0; i < Data.Instance.xmlLog_list.Count; i++)
            //{
            //    string path = Data.Instance.xmlLog_list[i];
            //    xaml_RecentList.Items.Add(new InitRecentList(System.IO.Path.GetFileName(path), File.GetCreationTime(path).ToString(), path));
            //}
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            is_opened_dlg = false;
            SaveFunction.Instance.GetLoadListName();
            this.Close();
        }

        private void SelectDelete_Click(object sender, RoutedEventArgs e) // 선택 항목 삭제
        {
            //if (xaml_RecentList.SelectedIndex != -1)
            //{
            //    int sindex = xaml_RecentList.SelectedIndex;
            //    if (MessageBox.Show("이 항목을 삭제하시겠습니까?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //    {
            //        xaml_RecentList.Items.RemoveAt(sindex);
            //        //Data.Instance.xmlLog_list.RemoveAt(sindex);

            //        SaveFunction.Instance.GetLoadListName();
            //        SaveFunction.Instance.RecentList_UpdateList();
            //    }
            //}
        }

        private void AllDelete_Click(object sender, RoutedEventArgs e)
        {
            //if (xaml_RecentList == null)
            //{
            //    MessageBox.Show("삭제할 항목이 없습니다!");
            //}
            //else
            //{
            //    if (MessageBox.Show("최근 불러온 모든 리스트를 삭제하시겠습니까?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //    {
            //        xaml_RecentList.Items.Clear();
            //        //Data.Instance.xmlLog_list.Clear();

            //        SaveFunction.Instance.GetLoadListName();
            //        SaveFunction.Instance.RecentList_UpdateList();
            //    }
            //}

        }

        private void FileAdd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDlg = new System.Windows.Forms.OpenFileDialog(); // 파일 찾아보기 다이얼로그
            openFileDlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            openFileDlg.Filter = "XML(*.xml)|*.xml|모든파일(*.*)|*.*";
            openFileDlg.Title = "추가할 로그를 선택하십시오.";

            if (openFileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Data.Instance.xmlLog_list.Add(openFileDlg.FileName);
                SaveFunction.Instance.RecentLog_UpdateToXml();
                Init_xaml_RecentList();
            }
        }

        private void Up_Update_Click(object sender, RoutedEventArgs e)  // 선택 자멜 자리 위치 변경
        {
            //try
            //{
            //    // 아무것도 선택되어있지 않으면 리턴
            //    if (xaml_RecentList.SelectedIndex == -1)
            //    {
            //        return;
            //    }
            //    else
            //    {
            //        if (xaml_RecentList.SelectedIndex - 1 != -1)
            //        {
            //            // 4번 "abc" 5번 "def"
            //            int sindex = xaml_RecentList.SelectedIndex; // 5번 선택

            //            var temp = xaml_RecentList.Items[sindex]; // temp 변수에 자리를 변경할 데이터 저장 "abc"
            //            //string path = Data.Instance.xmlLog_list[sindex];

            //            xaml_RecentList.Items.Insert(sindex - 1, temp);
            //            // 선택한 인덱스 자리 -1(4번)에 아이템을 추가
            //            // 4번자리 "def", 5번자리 "abc"생성 6번자리 "def"
            //            xaml_RecentList.Items.RemoveAt(sindex + 1);
            //            // 6번 자리에 def를 삭제
            //            // 4번 def 5번 abc
            //            // ===============================================================================

            //            //var temp2 = Data.Instance.xmlLog_list[sindex]; // 5번 선택 
            //            //Data.Instance.xmlLog_list[sindex] = Data.Instance.xmlLog_list[sindex - 1]; // 5번 자리에 4번을 넣고
            //            //Data.Instance.xmlLog_list[sindex - 1] = temp2; // 4번 자리에 5번을 넣음

            //        }
            //        SaveFunction.Instance.GetLoadListName();
            //        SaveFunction.Instance.RecentList_UpdateList();
            //    }

            //}
            //catch
            //{

            //}
        }

        private void Down_Update_Click(object sender, RoutedEventArgs e) // 선택 자멜 자리 위치 변경
        {
        //    try
        //    {
        //        // 아무것도 선택되어있지 않으면 리턴
        //        if (xaml_RecentList.SelectedIndex == -1)
        //        {
        //            return;
        //        }
        //        else
        //        {
        //            if (xaml_RecentList.SelectedIndex + 1 != -1)
        //            {
        //                // 5번 "abc" 6번 "def
        //                int sindex = xaml_RecentList.SelectedIndex;

        //                var temp = xaml_RecentList.Items[sindex + 1]; // temp 변수에 자리를 변경할 데이터 저장 "def"
        //                //string path = Data.Instance.xmlLog_list[sindex]; //5번 선택

        //                xaml_RecentList.Items.Insert(sindex, temp);
        //                // 선택한 인덱스 자리(5번)에 아이템을 추가
        //                // 5번자리 "def", 6번자리 "abc"생성 7번자리 "def"
        //                xaml_RecentList.Items.RemoveAt(sindex + 2);
        //                // 7번 자리에 def를 삭제
        //                // 5번 abc 6번 def
        //                // ===============================================================================

        //                //var temp2 = Data.Instance.xmlLog_list[sindex];
        //                //Data.Instance.xmlLog_list[sindex] = Data.Instance.xmlLog_list[sindex + 1];
        //                //Data.Instance.xmlLog_list[sindex + 1] = temp2;

        //            }
        //            SaveFunction.Instance.GetLoadListName();
        //            SaveFunction.Instance.RecentList_UpdateList();
        //        }

        //    }
        //    catch
        //    {

        //    }
        }
    }

    public class InitRecentList
    {
        public string FileName { get; set; }
        public string Date { get; set; }
        public string FilePath { get; set; }
        public InitRecentList(string name, string date, string path)
        {
            FileName = name;
            Date = date;
            FilePath = path;
            //Data.Instance.init_recent.Add(this);
        }
    }
}