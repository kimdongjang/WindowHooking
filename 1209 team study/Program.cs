using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace _1209_team_study
{
    class Program
    {
        private BitServer server; // 서버
        private DBControl db_con = DBControl.Instance;
        private Function func = new Function();
        private FTP ftp = new FTP();
        private Data data = Data.Instance;

        public bool is_login = false;
        public bool is_allsenddata = false;

        public Program(int port)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[서버 구동 시작]");
            db_con.Connect(); // DB 연결

            func.InitIndex(); 

            server = new BitServer(this, port); // ip, port
            server.ServerRun(); // 서버구동
        }

        public void LogData(String type, String ip, int port)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(type + ip + ":" + port.ToString());
        }

        public void RecvData(string msg, Socket s = null)
        {
            this.IsRecvDataFile(msg);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[수신] " + msg);
            string[] pack = msg.Split('$');
            string[] data = pack[1].Split('#');
            string sendpacket = "";
            string is_boolcheck = "";
            switch (pack[0].Trim())
            {
                #region 회원가입
                case "MEMBERADD":
                    if (db_con.AddMember(pack[1]) == true)
                    {
                        sendpacket += "ACK_MEMBERADD_S" + "$";
                        sendpacket += pack[1];
                    }
                    else
                    {
                        sendpacket += "ACK_MEMBERADD_F" + "$";
                        sendpacket += pack[1];
                    }
                    server.SendDataOne(s, sendpacket);
                    break;
                #endregion

                #region 로그인
                case "LOGIN":
                    if (db_con.Login(pack[1])) // 0은 아이디, 1은 비밀번호
                    {
                        if (func.Func_ServerIsLogin(pack[1]))
                        {
                            sendpacket += "ACK_LOGIN_S" + "$";
                            sendpacket += data[2] + "#";
                            sendpacket += db_con.ClientGetList(data[0]);
                            server.SendDataOne(s, sendpacket);
                            is_login = true;
                        } 
                        else
                        {
                            sendpacket += "NOW_LOGOUT" + "$";
                            sendpacket += data[0];
                            db_con.Logout(data[0]);
                            is_allsenddata = true;
                            is_login = true;
                        }
                    }
                    else
                    {
                        sendpacket += "ACK_LOGIN_F" + "$";
                        sendpacket += pack[1];
                        server.SendDataOne(s, sendpacket);
                        is_login = false;
                    }
                    break;
                #endregion

                #region 사용자 접속했을 때, 채널의 인덱스를 반환
                case "CHANNEL_INIT": // 패킷 $ 인덱스#인덱스# @ 유저목록#유저목록# & 유저목록#유저목록#
                    sendpacket += "CHANNEL_INIT_S" + "$";
                    sendpacket += db_con.ClientGetChannelIndex(data[0]) + "@";
                    sendpacket += func.Func_GetChannelUserList(data[0]);
                    server.SendDataOne(s, sendpacket);
                    break;
                #endregion

                #region 사용자 접속 채널 초기화 접속
                case "CHANNEL_INITJOIN":
                    sendpacket += "CHANNEL_INITJOIN_S" + "$";
                    sendpacket += func.Func_ResetChannelUser(pack[1]);
                    server.SendDataOne(s, sendpacket);
                    break;
                #endregion

                #region 로그아웃
                case "LOGOUT":
                    is_boolcheck = func.Func_Logout(pack[1]);
                    if(is_boolcheck == "성공")
                    {
                        sendpacket += "LOGOUT_S" + "$";
                    }
                    else
                    {

                    }
                    server.SendDataOne(s, sendpacket);
                    break;
                #endregion

                #region 아이디 체크
                case "IDCHECK":
                    if (db_con.IDCheck(pack[1]))
                    {
                        sendpacket += "ACK_IDCHECK_S" + "$";
                    }
                    else
                    {
                        sendpacket += "ACK_IDCHECK_F" + "$";
                    }                    
                    server.SendDataOne(s, sendpacket);
                    break;
                #endregion

                #region 채널 생성
                case "CHANNEL_ADD":
                    if (func.Func_AddChannel(pack[1]) == "가능")
                    {
                        sendpacket += "CHANNEL_ADD_S" + "$";
                        sendpacket += db_con.ChannelGetList(data[2]);

                        ftp.FTP_CreateDirectory(data[2]); // FTP 디렉토리 생성
                    }
                    else if (func.Func_AddChannel(pack[1]) == "중복")
                    {
                        sendpacket += "CHANNEL_ADD_CHECK_F";
                    }
                    else
                    {
                        sendpacket += "CHANNEL_ADD_F";
                    }
                    server.SendDataOne(s, sendpacket);
                    break;

                #endregion

                #region 생성된 채널 목록 출력
                case "CHANNEL_ALLLIST":
                    sendpacket += "CHANNEL_ALLLIST_S" + "$";
                    sendpacket += func.Func_ChannelAllList();
                    is_allsenddata = true;
                    break;

                #endregion

                #region 채널 접속
                case "CHANNEL_JOIN":
                    if (func.Func_ChannelJoin(pack[1]))
                    {
                        sendpacket += "CHANNEL_JOIN_S" + "$";
                        sendpacket += func.Func_ResetChannelUser(pack[1]);
                    }
                    else
                    {
                        sendpacket += "CHANNEL_JOIN_F" + "$";
                    }
                    server.SendDataOne(s, sendpacket);
                    break;
                #endregion

                #region 채널 접속 리스트 갱신
                case "CHANNEL_JOINLIST":
                    sendpacket += "CHANNEL_JOINLIST_S" + "$";
                    sendpacket += func.Func_ResetChannelUser(pack[1]);
                    is_allsenddata = true;
                    break;
                #endregion

                #region 채널 접속 해제 리스트 갱신
                case "CHANNEL_EXITLIST":
                    sendpacket += "CHANNEL_EXITLIST_S" + "$";
                    sendpacket += func.Func_ResetChannelUser(pack[1]);
                    is_allsenddata = true;
                    break;
                #endregion

                #region 전체 채널 메세지 전송
                case "ACHATTING_SEND":
                    {
                        sendpacket += "R_ACHATTING_SEND" + "$";
                        sendpacket += data[0] + "#" + "1000#" + data[1];
                        is_allsenddata = true;
                    }
                    break;
                #endregion

                #region 개별 채널 메세지 전송
                case "ICHATTING_SEND":
                    {
                        sendpacket += "R_ICHATTING_SEND" + "$";
                        sendpacket += data[0] + "#" + db_con.GetChannelIndex(data[1]) + "#" + data[2];
                        is_allsenddata = true;
                    }
                    break;
                #endregion

                #region 채널 로그아웃
                case "CHANNEL_EXIT":
                    if (func.Func_ChannelExit(pack[1]) == "성공")
                    {
                        sendpacket += "CHANNEL_EXIT_S" + "$";
                        sendpacket += func.Func_ResetChannelUser(pack[1]);
                        server.SendDataOne(s, sendpacket);
                    }
                    else
                    {
                        sendpacket += "CHANNEL_EXIT_F" + "$";
                        server.SendDataOne(s, sendpacket);
                    }
                    break;
                #endregion

                #region 관리자 위임
                case "CHANNEL_ADMIN_GIVEHANDLE": // 0은 채널이름, 2은 변경할 아이디
                    if(db_con.InChannelChangeAdmin(data[0], data[2]) == "성공")
                    {
                        sendpacket += "CHANNEL_ADMIN_GIVEHANDLE_S" + "$";
                        sendpacket += db_con.GetChannelIndex(data[0]) + "@" + db_con.InChannelGetMemberList(data[0]) + "@" + data[2];
                        is_allsenddata = true;
                    }
                    break;
                #endregion

                #region 채널 강퇴기능
                case "ADM_OUT":
                    if(db_con.InChannelBanUser(data[0], data[1]) == "성공")
                    {
                        sendpacket += "ADM_OUT_S" + "$";
                        sendpacket += db_con.GetChannelIndex(data[0]) + "@" + db_con.InChannelGetMemberList(data[0]) + "@" + data[1];
                        is_allsenddata = true;
                    }
                    break;
                #endregion

                #region 채널 삭제
                case "CHANNEL_DELETE":
                    if(func.Func_ChannelDelete(data[0]) == "성공")
                    {
                        sendpacket += "CHANNEL_DELETE_S" + "$";
                        server.SendDataOne(s, sendpacket);

                        ftp.FTP_DeleteDirectory(data[0]); // FTP 디렉토리 삭제
                    }
                    else
                    {
                        sendpacket += "CHANNEL_DELETE_F" + "$";
                        server.SendDataOne(s, sendpacket);
                    }
                    break;
                #endregion

                #region FTP 파일 리스트 갱신
                case "FTP_INITLIST":
                    // 받는 데이터 : 패킷 $ 채널이름 # 클라이언트 아이디
                    string ftp_data = ftp.FTP_Init(data[0]);
                    if (ftp_data == "실패")
                    {
                        sendpacket = "FTP_INITLIST_F" + "$";
                        is_allsenddata = true;
                    }
                    else
                    {
                        sendpacket = "FTP_INITLIST_S" + "$";
                        sendpacket += data[0] + "@";
                        sendpacket += ftp.FTPid + "@";
                        sendpacket += ftp.FTPpw + "@";
                        sendpacket += ftp_data;
                        is_allsenddata = true;
                    }
                    break;
                #endregion

                #region FTP 파일 업로드
                case "FILE_UPLOAD":
                    // 파일 중복 체킹 - 파일이 없다면 허용?
                    sendpacket = "FILE_UPLOAD_S" + "$";
                    server.SendDataOne(s, sendpacket);
                    break;
                #endregion

                #region FTP 파일 다운로드
                case "FILE_DOWNLOAD":
                    // 파일이 중복 체킹. 파일이 있다면 허용
                    if (ftp.FTP_Search(data[1], data[2]) == "성공")
                    {
                        sendpacket = "FILE_DOWNLOAD_S" + "$";
                        server.SendDataOne(s, sendpacket);
                    }
                    else
                    {
                        sendpacket = "FILE_DOWNLOAD_F" + "$";
                        server.SendDataOne(s, sendpacket);
                    }
                    
                    break;
                #endregion

                #region FTP 파일 삭제
                case "FILE_DELETE":
                    // 파일이 중복 체킹. 파일이 있다면 허용
                    if (ftp.FTP_Search(data[1], data[2]) == "성공")
                    {
                        sendpacket = "FILE_DELETE_S" + "$";
                        server.SendDataOne(s, sendpacket);
                    }
                    else
                    {
                        sendpacket = "FILE_DELETE_F" + "$";
                        server.SendDataOne(s, sendpacket);
                    }
                    
                    break;
                #endregion

                #region 녹화 종료 시 파일 수동저장
    
                case "RECORDING_SAVE":
                    // 회원의 ID를 받아서 판별해서 폴더 생성(폴더 있나 없나 판별해서 있으면 거기에 없으면 폴더생성)
                    //반환값 void
                    //예외처리
                        if(ftp.FTP_CreateDirectory(data[0]) == "성공")
                        {
                            sendpacket += "RECORDING_SAVE_S" + "$";
                        }
                        else
                        {
                            sendpacket += "RECORDING_SAVE_F" + "$";
                        }
                        server.SendDataOne(s, sendpacket);
                    // FTP 디렉토리 생성
                    //sendpacket_S + 폴더명
                    break;

                    
                #endregion

                #region 파일업로드 여부
                case "FILEUPLOAD_SUCCESS":
                    if (func.Func_XmlReadUpdate(data[0], data[1]) == "성공")
                    {
                        sendpacket += "FILEUPLOAD_SUCCESS_S" + "$";
                    }
                    else
                    {
                        sendpacket += "FILEUPLOAD_SUCCESS_F" + "$";
                    }
                    server.SendDataOne(s, sendpacket);
                        
                    break;
                #endregion

                #region 로그파일 자동저장
                case "AUTO_SAVE":
                    // 회원의 ID를 받아서 판별해서 폴더 생성(폴더 있나 없나 판별해서 있으면 거기에 없으면 폴더생성)
                    //반환값 void
                    //예외처리
                    if (ftp.FTP_CreateDirectory(data[1]) == "성공")
                    {
                        sendpacket += "AUTO_SAVE_S" + "$";
                        sendpacket += data[1];
                    }
                    else
                    {
                        sendpacket += "AUTO_SAVE_F" + "$";
                        sendpacket += data[1];
                    }
                    server.SendDataOne(s, sendpacket);
                    // FTP 디렉토리 생성
                    //sendpacket_S + 폴더명
                    break;
                #endregion

                #region 개인ID폴더 읽어서 ListView 출력
                case "ID_LOGLISTINIT":

                    sendpacket += "ID_LOGLISTINIT_S" + "$";
                    sendpacket += db_con.LogDataListView(data[0]);

                    server.SendDataOne(s, sendpacket);
                    break;
                #endregion

                #region 개인ID폴더 파일 삭제
                case "MY_DBFILEDELETE":
                    if(db_con.MyFileDelete(data[0], data[1]) == "삭제")
                    {
                        sendpacket += "MY_DBFILEDELETE_S" + "$";
                        server.SendDataOne(s, sendpacket);
                    }
                    else
                        sendpacket += "MY_DBFILEDELETE_F" + "$";
                        server.SendDataOne(s, sendpacket);
                    break;
                #endregion

                #region 개인 ID폴더 파일 업로드
                case "MYFILE_UPLOAD":
                    // 파일 중복 체킹 - 파일이 없다면 허용?
                        sendpacket = "MYFILE_UPLOAD_S" + "$";                      
                        server.SendDataOne(s, sendpacket);                     
                    break;
                #endregion

               

                #region 개인 ID폴더 날짜별, 파일명별 오름차순, 내림차순 정렬
                case "LIST_ASC":
                    if(db_con.MyFileListASC(data[0]) != null)
                    {
                        sendpacket += "LIST_ASC_S" + "$";
                        sendpacket += db_con.MyFileListASC(data[0]);

                        server.SendDataOne(s, sendpacket);
                    }
                    
                    break;

                case "LIST_DESC":
                    if (db_con.MyFileListDESC(data[0]) != null)
                    {
                        sendpacket += "LIST_DESC_S" + "$";
                        sendpacket += db_con.MyFileListDESC(data[0]);

                        server.SendDataOne(s, sendpacket);
                    }
                    break;

                case "LIST_ASC_FILENAME":
                    if (db_con.MyFileListNameASC(data[0]) != null)
                    {
                        sendpacket += "LIST_ASC_FILENAME_S" + "$";
                        sendpacket += db_con.MyFileListNameASC(data[0]);

                        server.SendDataOne(s, sendpacket);
                    }

                    break;

                case "LIST_DESC_FILENAME":
                    if (db_con.MyFileListNameDESC(data[0]) != null)
                    {
                        sendpacket += "LIST_DESC_FILENAME_S" + "$";
                        sendpacket += db_con.MyFileListNameDESC(data[0]);

                        server.SendDataOne(s, sendpacket);
                    }
                    break;
                #endregion

            }

            //=====================================================
            if (is_allsenddata)
            {
                server.SendDataAll(sendpacket);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[전송] " + sendpacket);
            is_allsenddata = false;
        }

        private void IsRecvDataFile(string msg)
        {
            if(msg == "<Root>")
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Log Arrived");
            }
        }

        static void Main(string[] args)
        {
            Program p = new Program(7000);
            Console.ReadKey();
        }
    }
}
