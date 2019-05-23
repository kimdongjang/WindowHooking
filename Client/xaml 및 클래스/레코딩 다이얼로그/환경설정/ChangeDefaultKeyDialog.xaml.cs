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
    /// ChangeDefaultKeyDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChangeDefaultKeyDialog : Window
    {
        string optionlabel = ""; // 단축키 설명
        string key = ""; // 단축키

        bool isok = false;

        public ChangeDefaultKeyDialog(string s1, string s2)
        {
            InitializeComponent();
            OptionLabel.Content = optionlabel = s1;
            KeyLabel.Content = key = s2;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // 조합키 입력 제한 반복 제어문
            for (int i = 0; i < Data.Instance.limitationkey.Length; i++)
            {
                if (e.Key.ToString() == Data.Instance.limitationkey[i])
                    return;
            }
            for (int i = 0; i < Data.Instance.vKeyList.Count; i++)
            {
                if (e.Key.ToString() == Data.Instance.vKeyList[i].ToString())
                {
                    KeyLabel.Content = key = Data.Instance.vKeyList[i].ToString();
                }
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            // RecordSetting의 TextBox를 수정
            switch (optionlabel)
            {
                case "녹화시작키 : ":
                    if (CheckKeyState("녹화시작키 : ", key) == true)
                        RecordSetting.Instance.RecordStartKey_TextBox.Text = key;
                    break;
                case "녹화정지키 : ":
                    if (CheckKeyState("녹화정지키 : ", key) == true)
                        RecordSetting.Instance.RecordStopKey_TextBox.Text = key;
                    break;
                case "녹화재생키 : ":
                    if (CheckKeyState("녹화재생키 : ", key) == true)
                        RecordSetting.Instance.RecordPlayKey_TextBox.Text = key;
                    break;
                case "재생정지키 : ":
                    if (CheckKeyState("재생정지키 : ", key) == true)
                        RecordSetting.Instance.RecordPlayStopKey_TextBox.Text = key;
                    break;
                case "일시정지키 : ":
                    if (CheckKeyState("일시정지키 : ", key) == true)
                        RecordSetting.Instance.RecordPauseKey_TextBox.Text = key;
                    break;
                case "모듈녹화키 : ":
                    if (CheckKeyState("모듈녹화키 : ", key) == true)
                        RecordSetting.Instance.ModuleRecordKey_TextBox.Text = key;
                    break;
            }

            if (isok)
                this.Close();
            else
                MessageBox.Show("입력한 키값이 이미 쓰이고 있습니다."); return;
        }

        private bool CheckKeyState(string Option, string Key)
        {
            // 입력된 키가 다른 컨트롤키에 사용되고 있는지 필터링
            switch (Option)
            {
                case "녹화시작키 : ":
                    if (Key == RecordSetting.Instance.RecordStopKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordPlayKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordPlayStopKey_TextBox.Text ||
                        key == RecordSetting.Instance.RecordPauseKey_TextBox.Text ||
                        Key == RecordSetting.Instance.ModuleRecordKey_TextBox.Text)
                        return false;
                    else
                        isok = true; return true;

                case "녹화정지키 : ":
                    if (Key == RecordSetting.Instance.RecordStartKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordPlayKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordPlayStopKey_TextBox.Text ||
                        key == RecordSetting.Instance.RecordPauseKey_TextBox.Text ||
                        Key == RecordSetting.Instance.ModuleRecordKey_TextBox.Text)
                        return false;
                    else
                        isok = true; return true;

                case "녹화재생키 : ":
                    if (Key == RecordSetting.Instance.RecordStartKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordStopKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordPlayStopKey_TextBox.Text ||
                        key == RecordSetting.Instance.RecordPauseKey_TextBox.Text ||
                        Key == RecordSetting.Instance.ModuleRecordKey_TextBox.Text)
                        return false;
                    else
                        isok = true; return true;

                case "재생정지키 : ":
                    if (Key == RecordSetting.Instance.RecordStartKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordStopKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordPlayKey_TextBox.Text ||
                        key == RecordSetting.Instance.RecordPauseKey_TextBox.Text ||
                        Key == RecordSetting.Instance.ModuleRecordKey_TextBox.Text)
                        return false;
                    else
                        isok = true; return true;

                case "일시정지키 : ":
                    if (Key == RecordSetting.Instance.RecordStartKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordStopKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordPlayStopKey_TextBox.Text ||
                        key == RecordSetting.Instance.RecordPlayKey_TextBox.Text ||
                        Key == RecordSetting.Instance.ModuleRecordKey_TextBox.Text)
                        return false;
                    else
                        isok = true; return true;

                case "모듈녹화키 : ":
                    if (Key == RecordSetting.Instance.RecordStartKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordStopKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordPlayStopKey_TextBox.Text ||
                        key == RecordSetting.Instance.RecordPlayKey_TextBox.Text ||
                        Key == RecordSetting.Instance.RecordPauseKey_TextBox.Text)
                        return false;
                    else
                        isok = true; return true;

                default: return false;
            }
        }
    }
}