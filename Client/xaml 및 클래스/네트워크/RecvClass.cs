using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Client
{
    class RecvClass
    {
        #region 싱글톤
        static RecvClass single_control;
        public static RecvClass Instance { get { return single_control; } }
        static RecvClass()
        {
            single_control = new RecvClass();
        }
        private RecvClass()
        {

        }

        #endregion

        FTP_Channel ftpserver;


        public void RecvData(string msg)
        {
            string[] pack = msg.Split('$');
            string[] data = pack[1].Split('#');
            string[] pdata = pack[1].Split('@');

            switch (pack[0].Trim())
            {
                #region 회원가입
                case "ACK_MEMBERADD_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { MainWindow.Instance.Member_Dlg.Hide(); });
                    MessageBox.Show("회원가입 성공!"); break;
                case "ACK_MEMBERADD_F": MessageBox.Show("회원가입 실패!"); break;
                #endregion

                #region 아이디 중복확인
                case "ACK_IDCHECK_S":
                    MessageBox.Show("중복확인 성공!");
                    MainWindow.Instance.Member_Dlg.is_idcheck = true;
                    break;
                case "ACK_IDCHECK_F":
                    MessageBox.Show("중복된 아이디가 있습니다!");
                    MainWindow.Instance.Member_Dlg.check_iddata = "";
                    Data.Instance.myId = "";
                    break;
                #endregion

                #region 로그인
                case "ACK_LOGIN_S":
                    // ACK_LOGIN_S$ip#블라블라#####
                    // pack[0] = ACK_LOGIN_S, pack[1] = ip#등등등
                    // data[0] = ip;
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MainWindow.Instance.Hide();
                        MainRecording.Instance.Show();
                        Data.Instance.Init_Check = true;
                        Control.Instance.ChannelAllList();
                    });
                    break;
                case "ACK_LOGIN_F":
                    MessageBox.Show("로그인에 실패했습니다.");
                    Data.Instance.myId = "";
                    break;
                #endregion

                #region 채널 목록 불러오기
                case "CHANNEL_ALLLIST_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MainRecording.Instance.xamlChannelList.Items.Clear();
                        MainChannel.Instance.LoadChannel(pack);

                        if (Data.Instance.Init_Check == true)
                        {
                            Control.Instance.RequestMyChannelList(); // 해당 아이디로 접속되어있는 채널들을 받기위해 패킷 전송
                            Data.Instance.Init_Check = false;
                        }
                    });
                    break;
                #endregion

                #region 접속 채널 초기화
                case "CHANNEL_INIT_S":
                    //받는 데이터 : 채널 인덱스
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        Data.Instance.Init_IndexList = pdata[0].Split('#'); // 자신이 접속한 채널들의 인덱스를 배열에 보관
                        //Data.Instance.initchannellength = Data.Instance.initchannelindex.Length - 1;  // 현재 자신의 ID로 접속되어 있는 채널의 개수

                        //// =================================================================================
                        //// 접속된 채널들의 이름과 인덱스와 사용자들과 헤더 값을 추가
                        //for (int i = 0; i < Data.Instance.initchannelindex.Length - 1; i++)
                        //{
                        //    Data.Instance.myChannelName.Add(Data.Instance.SearchChannelName(Data.Instance.initchannelindex[i]));
                        //    Data.Instance.myChannelIndex.Add(Data.Instance.initchannelindex[i]);
                        //    Data.Instance.myChannelUsers.Add(pdatas[i]);
                        //    Data.Instance.myChannelheader++;
                        //}
                        //// =================================================================================

                        Data.Instance.Init_Check = true;
                        Control.Instance.Init_JoinChannel(); // 접속된 채널들 중에서 접속자 리스트를 불러옴
                    });
                    break;

                #endregion

                #region 초기화 채널 접속 성공
                case "CHANNEL_INITJOIN_S":
                    // 0 채널의 인덱스, 1 접속 유저 목록, 2 자신의 이름
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        // =================================================================================
                        // 탭 컨트롤의 아이템을 전부 Clear
                        try
                        {
                            for (int i = 0; i < Data.Instance.MyChannelList.Count; i++)
                            {
                                if (Data.Instance.MyChannelList.Count == 0)
                                    return;
                                MainRecording.Instance.xaml_TabControl.Items.RemoveAt(2);
                            }
                        }
                        catch
                        {

                        }
                        finally
                        {
                            this.InitChannelDialog(pdata[0], pdata[1], pdata[2]);
                        }
                        // =================================================================================

                    });
                    break;
                #endregion

                #region 로그아웃
                case "LOGOUT_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { MainWindow.Instance.Show(); });
                    break;
                case "NOW_LOGOUT": // 중복된 아이디가 접속해있을 경우 강제종료시킴 SendingLogoutPacket();
                    if (data[0] == Data.Instance.myId)
                    {
                        MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                        {
                            MessageBox.Show("현재 아이디가 접속 중입니다. 접속을 해제했으니 다시 한 번 시도해주십시오.");
                        });
                    }
                    break;
                #endregion

                #region 채널 생성
                case "CHANNEL_ADD_S": // 채널 리스트 - 0은 인덱스, 1은 채널이름
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        CreateChannel.Instance.Hide();
                        Control.Instance.ChannelAllList();

                        // =================================================================================
                        // 탭 컨트롤의 아이템을 전부 Clear
                        for (int i = 0; i < Data.Instance.MyChannelList.Count; i++)
                        {
                            if (Data.Instance.MyChannelList.Count == 0)
                                return;
                            MainRecording.Instance.xaml_TabControl.Items.RemoveAt(2);
                        }
                        // =================================================================================
                        Data.Instance.exam_cname = data[1];
                        this.ShowChannelDialog(data[0], data[3]);
                        MainRecording.Instance.xaml_TabControl.SelectedIndex = Data.Instance.count;
                        Data.Instance.count++;
                    });
                    break;
                case "CHANNEL_ADD_F":
                    MessageBox.Show("채널 생성에 실패했습니다.");
                    break;
                case "CHANNEL_ADD_CHECK_F":
                    MessageBox.Show("이미 생성된 채널명과 일치합니다.");
                    break;
                #endregion

                #region 채널 조인
                case "CHANNEL_JOIN_S":
                    // 조인이 되면 data클래스에 있는 cname을 사용
                    // 0 채널의 인덱스, 1 접속 유저 목록, 2 자신의 이름
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        try
                        {
                            // =================================================================================
                            // 탭 컨트롤의 아이템을 전부 Clear
                            for (int i = 0; i < Data.Instance.MyChannelList.Count; i++)
                            {
                                if (Data.Instance.MyChannelList.Count == 0)
                                    return;
                                MainRecording.Instance.xaml_TabControl.Items.RemoveAt(2);
                            }
                        }
                        catch
                        {

                        }
                        finally
                        {
                            MainRecording.Instance.xaml_TabControl.SelectedIndex = Data.Instance.count;
                            Data.Instance.count++;
                            // =================================================================================
                            this.ShowChannelDialog(pdata[0], pdata[1]);
                        }
                    });
                    break;
                #endregion

                #region 채널 조인 리스트
                case "CHANNEL_JOINLIST_S":
                    // 받는 데이터 : 채널 아이디, 멤버 리스트, 내 아이디
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                        {
                            if (ac.ChannelIndex == pdata[0]) // 채널 탭에서 일치하는 이름을 찾아서 갱신
                            {
                                ac.InChannelUserListRefresh(pdata[1], pdata[2], 1); // 접속자 새로 고침
                                // 새로 갱신된 접속자 리스트를 자신의 리스트에 추가
                                ac.ChannelUserList = pdata[1];

                                Control.Instance.ChannelAllList();
                            }
                        }
                    });
                    break;
                case "CHANNEL_JOIN_F":
                    MessageBox.Show("채널 접속에 실패했습니다.");
                    break;
                #endregion

                #region 채널 나가기
                case "CHANNEL_EXIT_S":
                    // 받는 데이터 : 채널 아이디, 멤버 리스트, 내 아이디
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        Control.Instance.ExitRequestList(); // 채널에서 나갔다면 리스트를 서버에게 요구함 
                        Control.Instance.ChannelAllList();

                        for (int i = 0; i < Data.Instance.myChannelName.Count; i++)
                        {
                            if (Data.Instance.myChannelName[i] == pdata[2])
                            {
                                Data.Instance.myChannelName.RemoveAt(i);
                            }
                        }
                        Data.Instance.count--;
                        if (Data.Instance.count < 2)
                        {
                            Data.Instance.count = 2;
                        }
                    });
                    break;
                case "CHANNEL_EXITLIST_S":
                    // 채널에서 누군가가 나갔을 때, 유저 리스트를 갱신
                    // 받는 데이터 : 채널 아이디, 멤버 리스트, 내 아이디
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                        {
                            if (ac.ChannelIndex == pdata[0])
                            {
                                ac.ChannelUserList = pdata[1];
                                ac.InChannelUserListRefresh(pdata[1], pdata[2], 2);
                            }
                        }

                    });
                    break;
                #endregion

                #region 채널 관리자 변경
                // 받는 데이터 : 채널 아이디, 멤버 리스트, 권한 이임받은 멤버 아이디
                case "CHANNEL_ADMIN_GIVEHANDLE_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                        {
                            if (ac.ChannelIndex == pdata[0])
                            {
                                ChannelDialog.Instance.InChannelUserListRefresh(pdata[1], pdata[2], 3);
                                Control.Instance.ChannelAllList();
                            }
                        }
                    });
                    break;
                #endregion

                #region 관리자-강퇴기능
                // 받는 데이터 : [0]채널인덱스, [1]접속자 리스트, [2]강퇴대상 아이디
                case "ADM_OUT_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                        {
                            if (ac.ChannelIndex == pdata[0])
                            {
                                ChannelDialog.Instance.InChannelUserListRefresh(pdata[1], pdata[2], 4);
                            }
                        }
                    });
                    break;
                #endregion

                #region 관리자-채널 삭제기능
                case "CHANNEL_DELETE_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        Control.Instance.ChannelAllList();       // 채널리스트 갱신
                    });
                    break;
                #endregion

                #region 채팅
                case "R_ACHATTING_SEND":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MainChannel.Instance.Chatting(pack);
                    });
                    break;
                case "R_ACHATTING_SEND_F":
                    MessageBox.Show("로그인이 되어있지 않거나, 다른채널에 접속중입니다.");
                    break;
                case "R_ICHATTING_SEND":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                        {
                            if (ac.ChannelIndex == data[1])
                            {
                                ac.ChannelChatting(pack);
                            }
                        }
                    });
                    break;
                #endregion

                #region FTP관련
                //채널접속 시 파일리스트 출력
                case "FTP_INITLIST_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                        {
                            if (ac.ChannelName == pdata[0])
                            {
                                if (pdata[3] == "파일없음")
                                {
                                    return;
                                }
                                else
                                    ac.ftp.FileList_Load(pdata[1], pdata[2], pdata[3]); // FTP ID, FTP PASSWORD, FILELIST 전송

                            }

                        }
                    });
                    break;

                #endregion

                #region FTP 파일 전송
                case "FILE_UPLOAD_S":
                    // FTPServer cs의 FileUpload()에 접근
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                        {
                            if (ac.ChannelIndex == Data.Instance.uploadfile_cidx)
                            {
                                ac.ftp.FileUpload();
                            }
                        }
                    });
                    break;
                #endregion

                #region FTP 파일 수신
                case "FILE_DOWNLOAD_S":
                    // FTPServer cs의 FileUpload()에 접근
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                        {
                            if (ac.ChannelIndex == Data.Instance.uploadfile_cidx)
                            {
                                ac.ftp.FileDownload();
                            }
                        }
                    });
                    break;
                case "FILE_DOWNLOAD_F":
                    MessageBox.Show("파일을 찾을 수 없습니다.");
                    break;
                #endregion

                #region FTP 파일 삭제
                case "FILE_DELETE_S":
                    // FTPServer cs의 FileUpload()에 접근
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                        {
                            if (ac.ChannelIndex == Data.Instance.uploadfile_cidx)
                            {
                                ac.ftp.FileDelete();
                            }
                        }
                    });
                    break;
                case "FILE_DELETE_F":
                    MessageBox.Show("파일을 찾을 수 없습니다.");
                    break;
                #endregion

                #region 파일 자동저장

                case "AUTO_SAVE_S":
                    //S를 받았으면 폴더이름을 기준으로 그 폴더에 파일 저장. 파일 이름이 동일할 경우 덮어씀
                    //FTP 업로드
                    ftpserver.LogFileUpload();
                    break;

                case "AUTO_SAVE_F":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MessageBox.Show("저장이 실패하였습니다. 다시 시도해주세요");
                    });
                    break;
                #endregion

                #region 로그 파일 수동 저장

                case "RECORDING_SAVE_S":
                    //S를 받았으면 폴더이름을 기준으로 그 폴더에 파일 저장. 파일 이름이 동일할 경우 덮어씀
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        ftpserver = new FTP_Channel();
                        ftpserver.LogFileUpload();
                    });

                    break;

                case "RECORDING_SAVE_F":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MessageBox.Show("저장이 실패하였습니다. 다시 시도해주세요");
                    });
                    break;
                #endregion

                #region 파일업로드 여부

                case "FILEUPLOAD_SUCCESS_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        //MessageBox.Show("저장되었습니다.");
                    });
                    break;
                case "FILEUPLOAD_SUCCESS_F":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MessageBox.Show("DB에 로그파일 업로드 실패");
                    });
                    break;
                #endregion

                #region 개인 ID폴더 읽어서 ListView 출력

                case "ID_LOGLISTINIT_S":
                    //로그 리스트 리스트뷰에 띄워줌
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MainRecording.Instance.dblist_Dlg.InitMemoList.Items.Clear();
                        for (int i = 0; i < pdata.Length - 1; i++)
                        {
                            string[] cdata = pdata[i].Split('#');

                            MainRecording.Instance.dblist_Dlg.InitMemoList.Items.Add(new InitDBLogList(cdata[0], cdata[1], cdata[2], cdata[3], cdata[4]));
                        }
                    });
                    break;
                #endregion

                #region 개인 ID폴더 파일 삭제
                case "MY_DBFILEDELETE_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        ftpserver = new FTP_Channel();
                        ftpserver.MyFileDelete();
                    });
                    break;
                case "MY_DBFILEDELETE_F":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MessageBox.Show("삭제할 파일이 없습니다.");
                    });
                    break;
                #endregion

                #region 개인 ID폴더 파일 업로드
                case "MYFILE_UPLOAD_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        ftpserver = new FTP_Channel();
                        ftpserver.MyFileUpload();
                        ftpserver.FileUploadSuccess();
                        MainRecording.Instance.dblist_Dlg.DBLogListInit();
                    });
                    break;
                #endregion

                #region 개인 ID폴더 날짜별 오름차순, 내림차순 정렬
                case "LIST_ASC_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MainRecording.Instance.dblist_Dlg.InitMemoList.Items.Clear();
                        for (int i = 0; i < pdata.Length - 1; i++)
                        {
                            string[] cdata = pdata[i].Split('#');
                            MainRecording.Instance.dblist_Dlg.InitMemoList.Items.Add(new InitDBLogList(cdata[0], cdata[1], cdata[2], cdata[3], cdata[4]));
                        }
                    });
                    break;

                case "LIST_DESC_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MainRecording.Instance.dblist_Dlg.InitMemoList.Items.Clear();
                        for (int i = 0; i < pdata.Length - 1; i++)
                        {
                            string[] cdata = pdata[i].Split('#');
                            MainRecording.Instance.dblist_Dlg.InitMemoList.Items.Add(new InitDBLogList(cdata[0], cdata[1], cdata[2], cdata[3], cdata[4]));
                        }
                    });
                    break;

                case "LIST_ASC_FILENAME_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MainRecording.Instance.dblist_Dlg.InitMemoList.Items.Clear();
                        for (int i = 0; i < pdata.Length - 1; i++)
                        {
                            string[] cdata = pdata[i].Split('#');
                            MainRecording.Instance.dblist_Dlg.InitMemoList.Items.Add(new InitDBLogList(cdata[0], cdata[1], cdata[2], cdata[3], cdata[4]));
                        }
                    });
                    break;

                case "LIST_DESC_FILENAME_S":
                    MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        MainRecording.Instance.dblist_Dlg.InitMemoList.Items.Clear();
                        for (int i = 0; i < pdata.Length - 1; i++)
                        {
                            string[] cdata = pdata[i].Split('#');
                            MainRecording.Instance.dblist_Dlg.InitMemoList.Items.Add(new InitDBLogList(cdata[0], cdata[1], cdata[2], cdata[3], cdata[4]));
                        }
                    });
                    break;
                #endregion
            }
        }

        public void ShowChannelDialog(string cindex, string userlist) // 채널 접속이 되었으면 채널 탭을 갱신하고 접속자, 채널 리스트를 갱신
        {
            MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
            {
                // 접속한 채널을 접속 채널 리스트에 추가
                Data.Instance.MyChannelList.Add(new ChannelDialog(Data.Instance.exam_cname,
                    Data.Instance.SearchChannelIndex(Data.Instance.exam_cname),
                    Data.Instance.SearchChannelAdmin(Data.Instance.exam_cname),
                    userlist));

                // 탭의 위치를 정리
                foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                {
                    MainRecording.Instance.tabitem = new TabItem();
                    MainRecording.Instance.tabitem.Header = ac.ChannelName;
                    MainRecording.Instance.tabitem.Content = ac;

                    ac.ChannelInit(ac.ChannelUserList); // 채널 접속자 명단을 갱신
                    MainRecording.Instance.xaml_TabControl.Items.Add(MainRecording.Instance.tabitem);
                }
                // 접속이 되었으니 자신의 채널 헤더값을 +1추가
                Data.Instance.myChannelheader++;
                // 선택 탭을 접속한 채널로 변경
                MainRecording.Instance.xaml_TabControl.SelectedIndex = Data.Instance.MyChannelList.Count + 2;

                // 해당 채널의 접속자 멤버 리스트를 요구함
                Control.Instance.RefreshJoinChannelList();
            });
        }

        public void InitChannelDialog(string cindex, string mlist, string myid) // 초기화 채널 접속
        {
            MainWindow.Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
            {
                try
                {
                    // 내 채널 리스트에 채널을 추가
                    Data.Instance.MyChannelList.Add(new ChannelDialog(Data.Instance.SearchChannelName(cindex),
                        cindex,
                        Data.Instance.SearchChannelAdmin_idx(cindex),
                        mlist));

                    foreach (ChannelDialog ac in Data.Instance.MyChannelList)
                    {
                        MainRecording.Instance.tabitem = new TabItem();
                        MainRecording.Instance.tabitem.Header = ac.ChannelName;
                        MainRecording.Instance.tabitem.Content = ac;

                        if (Data.Instance.SearchChannelName(cindex) == ac.ChannelName)
                        {
                            ac.ChannelInit(ac.ChannelUserList); // 채널 접속자 명단을 갱신
                            ac.xaml_Chatting.Text += myid + "님께서 입장하셨습니다." + '\r';
                        }
                        MainRecording.Instance.xaml_TabControl.Items.Add(MainRecording.Instance.tabitem);
                    }
                    // 접속이 되었으니 자신의 채널 헤더값을 +1추가
                    Data.Instance.myChannelheader++;
                    // 선택 탭을 접속한 채널로 변경
                    MainRecording.Instance.xaml_TabControl.SelectedIndex = Data.Instance.myChannelheader + 2;

                    Control.Instance.Init_JoinChannel();
                }
                catch
                {

                }
            });
        }
    }
}
