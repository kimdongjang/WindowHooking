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
    /// LogEdit.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HelpDialog : Window
    {
        string fileupanddownuse = "\n파일 업로드 및 다운로드는 개인적 용도, 채널에서 사용하는 용도로\n"
            + "나뉘어져 있습니다.\n\n"
            + "1. 파일 업로드 방법 : '파일 업로드' 버튼을 누르신 후 자신이 업로드하길 원하는\n"
            + "파일을 선택하시면 파일이 업로드됩니다.\n\n"
            + "2. 파일 다운로드 방법 : '경로 찾기' 버튼을 통해 파일을 받기를 원하시는\n"
            + "경로를 지정하신 후, 파일명을 입력하시고 '파일 다운로드'버튼을 누르시면 됩니다,\n\n"
            + "3. 파일 삭제 방법 : 삭제하기를 원하시는 파일명을 입력하신 후,\n"
            + "'파일 삭제'버튼을 누르시면 파일이 삭제되며 리스트에서 사라집니다.\n\n"
            + "4. 그 외 : 개인적 용도로 사용하는 파일 리스트는\n"
            + "날짜별/파일명 별 정렬이 가능합니다.";

        string dbuse = "\nDataBase는 자신이 업로드 한 파일들의 목록을 관리해줍니다.\n"
            + "녹화기능을 종료하고 저장할 때 그 로그가 자동으로 DataBase의 목록에 추가됩니다.\n"
            + "또한 FTP서버를 사용하여 직접 파일을 업로드하거나 다운로드할 수 있고,\n"
            + "필요가 없어지면 파일을 삭제할 수도 있습니다.\n"
            + "업로드를 하게되면, 업로드한 날짜, 유저ID, 설명, 파일명 등이 리스트에 출력됩니다.\n\n"
            + "다운로드를 받기 위해서는 자신이 받고싶은 위치를 경로에 경로찾기를 통해\n"
            + "지정해준 뒤, 파일을 선택하고 다운로드 버튼을 누르면 다운로드를 할 수 있습니다.\n"
            + "'파일 삭제' 버튼을 통해 파일을 삭제하게 되면 리스트에서\n" + "목록이 사라지게 되며 복구가 불가능합니다.";

        string Recordinguse = "\n녹화 기능은 자신이 사용한 마우스와 키보드 사용 기록을 전부 저장하게 되며,\n"
            + "그 기록을 통해 반복적인 작업을 도와주거나, 다른 사람들에게 공유하고\n"
            + "자신이 작업했던 결과물을 복구하는 정도까지 응용이 가능합니다.\n\n"
            + "녹화를 하게 되면, 녹화된 기록들은 Xml이라는 확장자를 가진 파일로 저장되며\n"
            + "이 파일을 통해 녹화된 기록을 실행시킬 수 있습니다.\n\n"
            + "메인화면에 있는 '환경설정'을 클릭하게 되면 기능을\n"
            + "사용할 수 있는 단축키를 지정할 수 있습니다.\n\n"
            + "녹화시작키 : 자신이 사용하고 있는 마우스,키보드의 기록을 녹화합니다.\n"
            + "녹화정지키 : 녹화하고 있는 기록을 완전히 정지시키고 저장여부를 결정합니다.\n"
            + "녹화재생키 : 녹화한 기록을 재생합니다.\n"
            + "재생정지키 : 녹화한 기록을 재생하는 도중에 재생을 멈추어줍니다.\n"
            + "일시정지키 : 녹화하고 있는 기록을 임시로 멈춘 후 부가기능을 사용하게 됩니다.\n"
            + "모듈녹화키 : 녹화 시 원하는 부분씩 잘라서 저장할 수 있습니다.";


        string Channeluse = "\n채널이란 자신과 다른 사람들이 모여 대화를 나누거나\n"
            + "파일을 공유할 수 있도록 만들어진 공간입니다.\n"
            + "채널 내의 사람들끼리만 업로드한 파일을 공유할 수 있으며,\n"
            + "채널에 접속하지 않은 인원은 업로드 된 파일 목록을 볼 수 없습니다.\n\n"
            + "자신이 원하는 사람들끼리만 사용할 수 있도록 비밀번호 설정이 가능하며,\n"
            + "인원제한을 둘 수 있습니다.\n\n"
            + "채널을 만든 사용자만 채널을 삭제할 수 있습니다.";

        public HelpDialog()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void FileUpandDown(object sender, RoutedEventArgs e)
        {

            Help_Label.Content = fileupanddownuse;
        }

        private void DB(object sender, RoutedEventArgs e)
        {
            Help_Label.Content = dbuse;
        }

        private void Recording(object sender, RoutedEventArgs e)
        {
            Help_Label.Content = Recordinguse;
        }

        private void Channel(object sender, RoutedEventArgs e)
        {
            Help_Label.Content = Channeluse;
        }
    }
}