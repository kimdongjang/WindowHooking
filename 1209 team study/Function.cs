using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _1209_team_study
{
    class Function
    {
        private Data data = Data.Instance;
        private DBControl db_con = DBControl.Instance;
        

        public void InitIndex() // 현재 생성되어 있는 제일 마지막 채널ID값을 가져옴
        {
            
            if(db_con.SearchChannel_AllIndex() == "")
            {
                return;
            } 
            else
                data.channel_index = int.Parse(db_con.SearchChannel_AllIndex());
            
        }

        public string Func_AddChannel(string packet)
        {
            string[] data = packet.Split('#'); // 0은 비번유무, 1은 비번, 2은 채널명, 3은 채널설명, 4는 채널관리자, 5는 접속인원수
            if (db_con.GetChannelName_Check(data[2])) // 채널명 중복 체크
            {
                if (db_con.AddChannel(data[0], data[1], data[2], data[3], data[4], data[5])) // 생성
                {
                    if (db_con.AddIndexInChannelList(data[2], data[4]))
                    {
                        InitIndex(); // 생성이 가능하면 마지막 채널 ID값을 갱신
                        return "가능";
                    }
                    else return "불가";
                }
                else return "불가";
            }
            else return "중복";
        }

        public string Func_ChannelAllList()
        {
            return db_con.ChannelAllList();
        }

        public bool Func_ChannelJoin(string packet) // 채널 접속
        {
            string[] data = packet.Split('#'); // 0은 채널이름, 1은 회원아이디

            bool check_channel = db_con.GetChannelName_Check(data[0]);
            if (!check_channel)
            {
                if (db_con.ChannelJoin_GetCount(data[0])) // 채널 이름을 기준으로 로그인 할 수 있는 접속인원이 있는지 확인
                {
                    if (db_con.ChannelJoin_IsJoin(data[0], data[1])) // 채널에 이미 접속해 있는지 확인
                    {
                        if (db_con.AddChannelLoginCount(data[0], data[1]))
                        {
                            return true;
                        }
                        else return false;
                    }
                    else return false;
                }
                else return false;
            }
            else return false;
        }

        public bool Func_ServerIsLogin(string packet) // 현재 서버에 사용자의 아이디가 있는지 없는지 확인
        {
            string[] data = packet.Split('#'); // 0은 아이디, 1은 패스워드, 2는 이름, 3은 전화번호

            if (db_con.IsServerLogin(data[0]))
            {
                if (db_con.AddServerLogin(data[0]))
                {
                    return true;
                }
                else return false;
            }
            else // 현재 접속 중이라면 접속중인 사람에게 "강제 로그아웃" 패킷을 전송
            {
                return false;
            }
        }

        public string Func_Logout(string packet)
        {
            if (db_con.Logout(packet))
            {
                return "성공";
            }
            else return "실패";
        }

        public string Func_ChannelExit(string packet) // 유저가 채널에서 나갈때 채널에서 유저의 아이디를 지우고 유저의 인덱스를 전체 채널로 바꿈
        {
            string[] data = packet.Split('#'); // 0은 채널이름, 1은 아이디
            if(db_con.ChannelExit(data[0], data[1]) == "삭제")
            {
                if (db_con.ClientUpdateCIndex(data[1]) == "성공")
                {
                    db_con.ChannelUpdateCLCount(data[0]);
                    return "성공";
                }
                else return "실패";
            }
            return "실패";
        }

        public string Func_ResetChannelUser(string packet) // 채널에 해당하는 유저 목록을 갱신함
        {
            string[] data = packet.Split('#'); // 0은 채널이름, 1은 아이디
            string str = "";
            str += db_con.GetChannelIndex(data[0]) + "@";
            str += db_con.InChannelGetMemberList(data[0]) + "@";
            str += data[1];
            return str;
        }

        public string Func_ChannelDelete(string cname) // cname은 채널 이름
        {
            string[] userlist = db_con.InChannelGetMemberList(cname).Split('#'); // 채널의 접속자 리스트를 가져옴
            for (int i = 0; i < userlist.Length - 1; i++)
            {
                db_con.ClientUpdateCIndex(userlist[i]); // 회원들의 인덱스를 전체채널로 변경
            }
            // =======================================================================
            if (db_con.ChannelListAllDelete(cname) == "성공") // 채널 삭제
            {
                if (db_con.ChannelDelete(cname) == "삭제") // 채널 접속자 리스트 삭제
                {
                    return "성공";
                }
                else return "실패";
            }
            else return "실패";
        }

        public string Func_GetChannelUserList(string cid)
        {
            string[] clist = db_con.ClientGetChannelIndex(cid).Split('#');
            string str = "";
            for (int i = 0; i < clist.Length - 1; i++)
            {
                str += db_con.InChannelGetMemberList2(clist[i]) + "&";
            }
            return str;
        }

        public string Func_XmlReadUpdate(string cid, string fname)
        {
            try
            {
                //db_con.DBLogDataAllClear(Data.Instance.ClientID);
                string sPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + @"\ftpdown" + "\\" + cid + "\\";
                string filename = "";

                DirectoryInfo di = new DirectoryInfo(sPath);
                foreach (var item in di.GetFiles())
                {
                    filename = item.Name;

                    if(fname == filename)
                    {
                        Data.Instance.playinit_filename = sPath + filename;
                        XmlTextReader reader = new XmlTextReader(Data.Instance.playinit_filename);

                        string read_string;

                        #region Xml 정보 읽음
                        while (reader.Read())
                        {
                            switch (reader.LocalName)
                            {
                                case "Today":
                                    read_string = reader.ReadElementContentAsString();
                                    Data.Instance.Today = read_string;
                                    break;
                                case "ClientID":
                                    read_string = reader.ReadElementContentAsString();
                                    Data.Instance.ClientID = read_string;
                                    break;
                                case "UseProgram":
                                    read_string = reader.ReadElementContentAsString();
                                    Data.Instance.UseProgram = read_string;

                                    break;
                                case "Comment":
                                    read_string = reader.ReadElementContentAsString();
                                    Data.Instance.Comment = read_string;
                                    break;
                                case "FileName":
                                    read_string = reader.ReadElementContentAsString();
                                    Data.Instance.FileName = read_string;
                                    break;
                            }

                        }
                        #endregion

                        db_con.LogDataAdd(
                            Data.Instance.Today,
                            Data.Instance.ClientID,
                            Data.Instance.UseProgram,
                            Data.Instance.Comment,
                            Data.Instance.FileName);
                    }
                }
                return "성공";
            }
            catch
            {
                return "실패";
            }           
        }
    }
}