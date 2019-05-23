using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace _1209_team_study
{
    class DBControl
    {
        SqlConnection conn = null;
        Data data = Data.Instance;

        #region 싱글톤
        static DBControl single_control;
        public static DBControl Instance { get { return single_control; } }
        static DBControl() { single_control = new DBControl(); }
        private DBControl()
        {

        }
        #endregion

        public void Connect()
        {
            conn = new SqlConnection();
            conn.ConnectionString = @"Server=192.168.0.8;database=WB24;uid=chh;pwd=123;";
            try
            {
                conn.Open();    //  데이터베이스 연결
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[데이터베이스 연결 성공]");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[데이터베이스 연결 실패]");
            }
        }
        public bool DisConnect()
        {
            if (conn != null)
            {
                conn.Close();       //  연결 해제
                conn = null;
                return true;
            }

            else return false;
        }
        public bool AddMember(string p)
        {
            int channel_index = 1000;
            string sql = "INSERT INTO Client (ClientID, ClientPW, ClientName, ClientPhone, ChannelIndex) VALUES (@ClientID, @ClientPW, @ClientName, @ClientPhone, @ChannelIndex)";
        
            String[] temp = p.Split('#');

            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", temp[0]);
            cmd.Parameters.Add(param);

            param = new SqlParameter("@ClientPW", temp[1]);
            cmd.Parameters.Add(param);

            param = new SqlParameter("@ClientName", temp[2]);
            cmd.Parameters.Add(param);

            param = new SqlParameter("@ClientPhone", temp[3]);
            cmd.Parameters.Add(param);

            param = new SqlParameter("@ChannelIndex", channel_index);
            param.SqlDbType = System.Data.SqlDbType.Int;
            cmd.Parameters.Add(param);

            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool IDCheck(string id)
        {
            string sql = "SELECT ClientID from Client where ClientID = @ClientID";
            bool check = false;

            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", id);
            cmd.Parameters.Add(param);

            SqlDataReader myDataReader;
            myDataReader = cmd.ExecuteReader();

            check = myDataReader.Read();
           
            cmd.Dispose();
            myDataReader.Close();

            if (check == true) return false;
            return true;

        }
        public bool Login(string p)
        {
            string sql = "SELECT * FROM Client WHERE ClientID = @ClientID AND ClientPW = @ClientPW ";

            String[] temp = p.Split('#');

            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", temp[0]);
            myCommand.Parameters.Add(param);

            param = new SqlParameter("@ClientPW", temp[1]);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader;
            myDataReader = myCommand.ExecuteReader();

            if (myDataReader.Read())
            {
                myDataReader.Close();
                myCommand.Dispose();
                return true;
            }
            else
            {
                myDataReader.Close();
                myCommand.Dispose();
                return false;
            }
        }
        public string ClientGetList(string cid) // 해당 회원 정보(아이디, 이름, 전화번호, 채널 인덱스)를 가져옴
        {
            string sql = "SELECT * FROM Client WHERE ClientID = @ClientID";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", cid);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader;
            myDataReader = myCommand.ExecuteReader();

            try
            {
                string str = "";
                myDataReader.Read();

                str += myDataReader[0].ToString() + "#";
                str += myDataReader[2].ToString() + "#";
                str += myDataReader[3].ToString() + "#";
                str += myDataReader[4].ToString();

                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }
        public string ClientGetChannelIndex(string cid)
        {
            string sql = "SELECT ChannelIndex FROM ChannelMem WHERE ChannelMemID = @ChannelMemID";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelMemID", cid);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader;
            myDataReader = myCommand.ExecuteReader();

            try
            {
                string str = "";
                while (myDataReader.Read())
                {
                    str += myDataReader[0].ToString() + "#";
                }
                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }
        public bool Logout(string id)
        {
            string sql = "DELETE FROM LoginMember WHERE ServerLoginMember = @ServerLoginMember";
            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ServerLoginMember", id);
            cmd.Parameters.Add(param);

            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool IsServerLogin(string id) // 현재 접속 중인 서버에 아이디가 사용중인지 확인
        {
            try
            {
                string sql = "SELECT ServerLoginMember FROM LoginMember WHERE ServerLoginMember = @ServerLoginMember";
                bool check = false;

                SqlCommand cmd = new SqlCommand(sql, conn);

                SqlParameter param = new SqlParameter("@ServerLoginMember", id);
                cmd.Parameters.Add(param);

                SqlDataReader myDataReader;
                myDataReader = cmd.ExecuteReader();

                check = myDataReader.Read();

                cmd.Dispose();
                myDataReader.Close();

                if (check == true)
                    return false;
                else return true; // 사용할 수 있음. 겹치지 않기 때문에.
            }
            catch
            {
                return false; // 해당된 채널명은 사용할 수 없음.
            }
        }
        public bool AddServerLogin(string id) // 서버에 해당 아이디를 등록
        {
            string sql = "INSERT INTO LoginMember (ServerLoginMember) VALUES (@ServerLoginMember)";
            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ServerLoginMember", id);
            cmd.Parameters.Add(param);

            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string SearchChannel_AllIndex() // 현재 생성된 모든 채널의 ID를 가져옴
        {
            string sql = "SELECT ChannelIndex FROM Channel";

            SqlCommand myCommand = new SqlCommand(sql, conn);
            SqlDataReader myDataReader = myCommand.ExecuteReader();

            try
            {
                string str = "";
                while (myDataReader.Read())
                {
                    str = myDataReader[0].ToString();
                }

                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }
        public bool GetChannelName_Check(string cname) // 채널 존재 유무 확인
        {
            try
            {
                string sql = "SELECT ChannelName FROM Channel WHERE ChannelName = @ChannelName";
                bool check = false;

                SqlCommand cmd = new SqlCommand(sql, conn);

                SqlParameter param = new SqlParameter("@ChannelName", cname);
                cmd.Parameters.Add(param);

                SqlDataReader myDataReader;
                myDataReader = cmd.ExecuteReader();

                check = myDataReader.Read();

                cmd.Dispose();
                myDataReader.Close();

                if (check == true)
                    return false;
                else return true; // 사용할 수 있음. 겹치지 않기 때문에.
            }
            catch
            {
                return false; // 해당된 채널명은 사용할 수 없음.
            }
        }
        public bool AddChannel(string ispw, string cpw, string cname, string ctitle, string cadmin, string cmember) // 채널 생성
        {
            string sql = "INSERT INTO Channel (ChannelIndex, ChannelName, ChannelTitle, ChannelAdmin, ChannelMember, ChannelLoginCount, ChannelIsPW, ChannelPW) values "
                + "(@CIndex, @CName, @ChannelTitle, @CAdmin, @CMem, @CLoginCount, @ChannelIsPW, @ChannelPW)";
            int clcount = 1;
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@CIndex", data.channel_index + 4);
            param.SqlDbType = System.Data.SqlDbType.Int;
            myCommand.Parameters.Add(param);
            param = new SqlParameter("@CName", cname);
            myCommand.Parameters.Add(param);
            param = new SqlParameter("@ChannelTitle", ctitle);
            myCommand.Parameters.Add(param);
            param = new SqlParameter("@CAdmin", cadmin);
            myCommand.Parameters.Add(param);
            param = new SqlParameter("@CMem", cmember);
            param.SqlDbType = System.Data.SqlDbType.Int;
            myCommand.Parameters.Add(param);
            param = new SqlParameter("@CLoginCount", clcount);
            param.SqlDbType = System.Data.SqlDbType.Int;
            myCommand.Parameters.Add(param);

            if (ispw == "PASSTRUE")
            {
                int one = 1;
                param = new SqlParameter("@ChannelIsPW", one);
                param.SqlDbType = System.Data.SqlDbType.Int;
                myCommand.Parameters.Add(param);
                param = new SqlParameter("@ChannelPW", cpw);
                param.SqlDbType = System.Data.SqlDbType.Int;
                myCommand.Parameters.Add(param);
            }

            else
            {
                int zero = 0;
                param = new SqlParameter("@ChannelIsPW", zero);
                param.SqlDbType = System.Data.SqlDbType.Int;
                myCommand.Parameters.Add(param);
                param = new SqlParameter("@ChannelPW", zero);
                param.SqlDbType = System.Data.SqlDbType.Int;
                myCommand.Parameters.Add(param);
            }
            
            try
            {
                myCommand.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool AddIndexInChannelList(string cname, string cadmin) // 채널 리스트에 채널 아이디와 멤버 아이디를 추가
        {
            string client_name = GetClientName(cadmin);
            string cindex = GetChannelIndex(cname);
            // ================================================================================
            // 해당 채널에 접속자 등록 시킴
            string sql3 = "INSERT INTO ChannelMem (ChannelIndex, ChannelMemID, ChannelName, ChannelMemName) VALUES (@CIndex, @CMemID, @ChannelName, @ChannelMemName)";
            SqlCommand myCommand3 = new SqlCommand(sql3, conn);

            SqlParameter param3 = new SqlParameter("@CIndex", cindex);
            myCommand3.Parameters.Add(param3);

            param3 = new SqlParameter("@ChannelName", cname);
            myCommand3.Parameters.Add(param3);

            param3 = new SqlParameter("@ChannelMemName", client_name);
            myCommand3.Parameters.Add(param3);

            param3 = new SqlParameter("@CMemID", cadmin);
            myCommand3.Parameters.Add(param3);

            try
            {
                myCommand3.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string ChannelAllList() // 모든 채널 리스트를 가져옴
        {
            string sql = "SELECT * from Channel";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlDataReader myDataReader;
            myDataReader = myCommand.ExecuteReader();

            try
            {
                string str = "";
                while (myDataReader.Read())
                {
                    str += myDataReader[0].ToString() + "#";
                    str += myDataReader[1].ToString() + "#";
                    str += myDataReader[2].ToString() + "#";
                    str += myDataReader[3].ToString() + "#";
                    str += myDataReader[4].ToString() + "#";
                    str += myDataReader[5].ToString() + "#";
                    str += myDataReader[6].ToString() + "#";
                    str += myDataReader[7].ToString() + "@";
                }
                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }
        public string ChannelGetList(string cname) // 해당 채널 리스트를 가져옴
        {
            string sql = "SELECT * FROM Channel WHERE ChannelName = @ChannelName";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelName", cname);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader;
            myDataReader = myCommand.ExecuteReader();

            try
            {
                string str = "";
                myDataReader.Read();
                
                str += myDataReader[0].ToString() + "#";
                str += myDataReader[1].ToString() + "#";
                str += myDataReader[2].ToString() + "#";
                str += myDataReader[3].ToString() + "#";
                str += myDataReader[4].ToString() + "#";
                str += myDataReader[5].ToString() + "#";
                str += myDataReader[6].ToString() + "#";
                str += myDataReader[7].ToString();
                
                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }
        public bool ChannelJoin_GetCount(string cname) // 채널 이름을 기준으로 로그인 할 수 있는 접속인원이 있는지 확인
        {
            string sql = "SELECT ChannelMember, ChannelLoginCount FROM Channel WHERE ChannelName = @ChannelName";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelName", cname);
            myCommand.Parameters.Add(param);
            
            SqlDataReader myDataReader = myCommand.ExecuteReader();
            try
            {
                myDataReader.Read();

                int cmember = int.Parse(myDataReader[0].ToString());
                int clogincount = int.Parse(myDataReader[1].ToString());

                myDataReader.Close();
                myCommand.Dispose();

                if (cmember > clogincount)
                {
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return false;
            }
        }

        public bool ChannelJoin_IsJoin(string cname, string cid) // 채널에 접속할 때, 이미 접속해있는지 확인
        {
            try
            {
                string channel_index = GetChannelIndex(cname);
                bool ischeck = false;
                string sql = "SELECT ChannelName FROM ChannelMem WHERE ChannelMemID = @ChannelMemID AND ChannelIndex = @ChannelIndex";
                SqlCommand myCommand = new SqlCommand(sql, conn);

                SqlParameter param = new SqlParameter("@ChannelMemID", cid);
                myCommand.Parameters.Add(param);

                param = new SqlParameter("@ChannelIndex", channel_index);
                myCommand.Parameters.Add(param);

                SqlDataReader myDataReader = myCommand.ExecuteReader();
            
                ischeck = myDataReader.Read();

                myDataReader.Close();
                myCommand.Dispose();

                if (ischeck) // 일치하는 게 있다면
                {
                    return false;
                }
                else return true;
            }
            catch
            {
                return false;
            }
        }
        public bool AddChannelLoginCount(string cname, string cid)
        {
            string client_name = GetClientName(cid);
            string cindex = GetChannelIndex(cname);
            // ================================================================================
            // 해당 채널에 접속자 등록 시킴
            string sql3 = "INSERT INTO ChannelMem (ChannelIndex, ChannelMemID, ChannelName, ChannelMemName) VALUES (@CIndex, @CMemID, @ChannelName, @ChannelMemName)";
            SqlCommand myCommand3 = new SqlCommand(sql3, conn);

            SqlParameter param3 = new SqlParameter("@CIndex", cindex);
            myCommand3.Parameters.Add(param3);

            param3 = new SqlParameter("@ChannelName", cname);
            myCommand3.Parameters.Add(param3);

            param3 = new SqlParameter("@ChannelMemName", client_name);
            myCommand3.Parameters.Add(param3);

            param3 = new SqlParameter("@CMemID", cid);
            myCommand3.Parameters.Add(param3);

            try
            {
                myCommand3.ExecuteNonQuery();
            }
            catch
            {
                return false;
            }
            // ==================================================================================
            ChannelUpdateCLCount(cname); // 접속자 카운트 업데이트
            // ==================================================================================
            // 사용자에게 해당 채널의 인덱스를 줌
            string sql4 = "UPDATE Client SET ChannelIndex = @ChannelIndex WHERE ClientID = @ClientID";
            SqlCommand myCommand4 = new SqlCommand(sql4, conn);

            SqlParameter param4 = new SqlParameter("@ChannelIndex", cindex);
            myCommand4.Parameters.Add(param4);

            param4 = new SqlParameter("@ClientID", cid);
            myCommand4.Parameters.Add(param4);

            try
            {
                myCommand4.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string InChannelGetMemberList(string cname) // 채널의 아이디에 해당하는 모든 접속한 멤버들의 목록을 출력함
        {
            string channelindex = GetChannelIndex(cname); // 채널의 아이디를 가져옴

            string sql = "SELECT ChannelMemID FROM ChannelMem WHERE ChannelIndex = @ChannelIndex";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelIndex", channelindex);
            myCommand.Parameters.Add(param);
            
            SqlDataReader myDataReader = myCommand.ExecuteReader();
            try
            {
                string str = "";
                while(myDataReader.Read())
                {
                    str += myDataReader[0].ToString() + "#";
                }

                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }

        }
        public string InChannelGetMemberList2(string cid) // 채널의 아이디에 해당하는 모든 접속한 멤버들의 목록을 출력함
        {
            string channelname = GetChannelName(cid); // 채널의 이름을 가져옴

            string sql = "SELECT ChannelMemID FROM ChannelMem WHERE ChannelName = @ChannelName";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelName", channelname);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader = myCommand.ExecuteReader();
            try
            {
                string str = "";
                while (myDataReader.Read())
                {
                    str += myDataReader[0].ToString() + "#";
                }

                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }
        public string GetChannelName(string cid) // 채널의 이름을 가져옴
        {
            string channelname;
            string sql = "SELECT ChannelName FROM Channel WHERE ChannelIndex = @ChannelIndex";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelIndex", cid);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader = myCommand.ExecuteReader();
            try
            {
                myDataReader.Read();

                channelname = myDataReader[0].ToString();

                myDataReader.Close();
                myCommand.Dispose();
                return channelname;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }
        public string GetChannelIndex(string cname) // 채널의 아이디를 가져옴
        {
            string channelindex;
            string sql = "SELECT ChannelIndex FROM Channel WHERE ChannelName = @ChannelName";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelName", cname);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader = myCommand.ExecuteReader();
            try
            {
                myDataReader.Read();

                channelindex = myDataReader[0].ToString();

                myDataReader.Close();
                myCommand.Dispose();
                return channelindex;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }

        public string GetClientName(string cid) // 사용자의 이름을 가져옴
        {
            string clinet_name;
            string sql = "SELECT ClientName FROM Client WHERE ClientID = @ClientID";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", cid);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader = myCommand.ExecuteReader();
            try
            {
                myDataReader.Read();

                clinet_name = myDataReader[0].ToString();

                myDataReader.Close();
                myCommand.Dispose();
                return clinet_name;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }
        public string ChannelExit(string cname, string cid) // 채널에서 사용자가 나갈 때 채널 접속자 목록에서 아이디를 제거
        {
            string channel_index = GetChannelIndex(cname);

            string sql = "DELETE FROM ChannelMem WHERE ChannelMemID = @ChannelMemID AND ChannelIndex = @ChannelIndex";
            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelMemID", cid);
            cmd.Parameters.Add(param);

            param = new SqlParameter("@ChannelIndex", channel_index);
            cmd.Parameters.Add(param);

            try
            {
                cmd.ExecuteNonQuery();
                return "삭제";
            }
            catch
            {
                return "실패";
            }
        }
        public string ClientUpdateCIndex(string cid) // 클라이언트 채널 인덱스를 전체채널로 변경
        {
            string sql2 = "UPDATE Client SET ChannelIndex = @ChannelIndex WHERE ClientID = @ClientID";
            SqlCommand myCommand2 = new SqlCommand(sql2, conn);

            SqlParameter param2 = new SqlParameter("@ClientID", cid);
            myCommand2.Parameters.Add(param2);

            int cindex = 1000;
            param2 = new SqlParameter("@ChannelIndex", cindex);
            myCommand2.Parameters.Add(param2);
            try
            {
                myCommand2.ExecuteNonQuery();
                return "성공";
            }
            catch
            {
                return "실패";
            }
        }
        public void ChannelUpdateCLCount(string cname) // 클라이언트 채널의 접속자 수를 갱신시킴
        {
            string cindex = GetChannelIndex(cname);
            int clogincount = 0;

            // 로그인 카운트를 먼저 DB에서 가져옴
            string sql = "SELECT COUNT(ChannelMemID) FROM ChannelMem WHERE ChannelIndex = @ChannelIndex";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelIndex", cindex);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader = myCommand.ExecuteReader();
            try
            {
                myDataReader.Read();

                clogincount = int.Parse(myDataReader[0].ToString());

                myDataReader.Close();
                myCommand.Dispose();
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
            }
            // ================================================================================
            // 가져온 로그인 카운트를 적용 시킴

            string sql2 = "UPDATE Channel SET ChannelLoginCount = @CLoginCount WHERE ChannelName = @ChannelName";
            SqlCommand myCommand2 = new SqlCommand(sql2, conn);

            SqlParameter param2 = new SqlParameter("@ChannelName", cname);
            myCommand2.Parameters.Add(param2);

            param2 = new SqlParameter("@CLoginCount", clogincount);
            myCommand2.Parameters.Add(param2);
            try
            {
                myCommand2.ExecuteNonQuery();
            }
            catch
            {
                return;
            }
        }
        public string IsAdminUser(string cname, string cid)
        {
            string channel_admin;
            string sql = "SELECT ChannelAdmin FROM Channel WHERE ChannelName = @ChannelName";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelName", cname);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader = myCommand.ExecuteReader();
            try
            {
                myDataReader.Read();

                channel_admin = myDataReader[0].ToString();

                myDataReader.Close();
                myCommand.Dispose();
                if (channel_admin == cid)
                {
                    return "맞음";
                }
                else return "틀림";
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return "틀림";
            }
        }
        public string InChannelBanUser(string cname, string cid) // 강제 퇴장 기능
        {
            try
            {
                ChannelExit(cname, cid);
                ClientUpdateCIndex(cid);
                ChannelUpdateCLCount(cname);
                return "성공";
            }
            catch
            {
                return "실패";
            }
        }
        public string InChannelChangeAdmin(string cname, string cid)
        {
            string channel_index = GetChannelIndex(cname);

            string sql2 = "UPDATE Channel SET ChannelAdmin = @ChannelAdmin WHERE ChannelIndex = @ChannelIndex";
            SqlCommand myCommand2 = new SqlCommand(sql2, conn);

            SqlParameter param2 = new SqlParameter("@ChannelAdmin", cid);
            myCommand2.Parameters.Add(param2);

            param2 = new SqlParameter("@ChannelIndex", channel_index);
            myCommand2.Parameters.Add(param2);
            try
            {
                myCommand2.ExecuteNonQuery();
                return "성공";
            }
            catch
            {
                return "실패";
            }
        }
        public string ChannelDelete(string cname) // 채널 삭제
        {
            string channel_index = GetChannelIndex(cname);

            string sql = "DELETE FROM Channel WHERE ChannelIndex = @ChannelIndex";
            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelIndex", channel_index);
            cmd.Parameters.Add(param);

            try
            {
                cmd.ExecuteNonQuery();
                return "삭제";
            }
            catch
            {
                return "실패";
            }
        }
        public string ChannelListAllDelete(string cname) // 채널 접속자 리스트 모두 삭제
        {
            string channel_index = GetChannelIndex(cname);

            string sql = "DELETE FROM ChannelMem WHERE ChannelIndex = @ChannelIndex";
            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ChannelIndex", channel_index);
            cmd.Parameters.Add(param);

            try
            {
                cmd.ExecuteNonQuery();
                return "성공";
            }
            catch
            {
                return "실패";
            }
        }

        public bool LogDataAdd(string today, string id, string program, string comm, string name)
        {
            string sql = "INSERT INTO LogData (Today, ClientID, UseProgram, Comment, FileName) VALUES (@Today, @ClientID, @UseProgram, @Comment, @FileName)";


            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@Today", today);
            cmd.Parameters.Add(param);

            param = new SqlParameter("@ClientID", id);
            cmd.Parameters.Add(param);

            param = new SqlParameter("@UseProgram", program);
            cmd.Parameters.Add(param);

            param = new SqlParameter("@Comment", comm);
            cmd.Parameters.Add(param);

            param = new SqlParameter("@FileName", name);
            cmd.Parameters.Add(param);

            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string LogDataListView(string cid)
        {
            string sql = "SELECT *from LogData WHERE ClientID = @ClientID";
            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", cid);
            myCommand.Parameters.Add(param);

            SqlDataReader myDataReader;
            myDataReader = myCommand.ExecuteReader();

            try
            {
                string str = "";
                while (myDataReader.Read())
                {
                    str += myDataReader[0].ToString() + "#";
                    str += myDataReader[1].ToString() + "#";
                    str += myDataReader[2].ToString() + "#";
                    str += myDataReader[3].ToString() + "#";
                    str += myDataReader[4].ToString() + "@";
                }
                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }
        public bool DBLogDataAllClear(string cid)
        {
            string sql = "Delete from LogData where ClientID = @ClientID";

            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", cid);
            myCommand.Parameters.Add(param);

            try
            {
                myCommand.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string MyFileDelete(string cid, string cfname) // 채널 삭제
        {
            string sql = "DELETE FROM LogData WHERE ClientID = @ClientID AND FileName = @FileName";
            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", cid);
            cmd.Parameters.Add(param);

            param = new SqlParameter("@FileName", cfname);
            cmd.Parameters.Add(param);

            try
            {
                cmd.ExecuteNonQuery();
                return "삭제";
            }
            catch
            {
                return "실패";
            }
        }

        public string MyFileListASC(string cid) // DB에 저장된 목록을 날짜별로 정렬
        {
            string sql = "SELECT *FROM LogData WHERE ClientID = @ClientID ORDER BY Today ASC";

            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", cid);
            myCommand.Parameters.Add(param);


            SqlDataReader myDataReader;
            myDataReader = myCommand.ExecuteReader();

            try
            {
                string str = "";
                while (myDataReader.Read())
                {
                    str += myDataReader[0].ToString() + "#";
                    str += myDataReader[1].ToString() + "#";
                    str += myDataReader[2].ToString() + "#";
                    str += myDataReader[3].ToString() + "#";
                    str += myDataReader[4].ToString() + "@";
                }

                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }

        public string MyFileListDESC(string cid) // DB에 저장된 목록을 날짜별로 정렬
        {
            string sql = "SELECT *FROM LogData WHERE ClientID = @ClientID ORDER BY Today DESC";

            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", cid);
            myCommand.Parameters.Add(param);


            SqlDataReader myDataReader;
            myDataReader = myCommand.ExecuteReader();

            try
            {
                string str = "";
                while (myDataReader.Read())
                {
                    str += myDataReader[0].ToString() + "#";
                    str += myDataReader[1].ToString() + "#";
                    str += myDataReader[2].ToString() + "#";
                    str += myDataReader[3].ToString() + "#";
                    str += myDataReader[4].ToString() + "@";
                }

                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }

        public string MyFileListNameASC(string cid) // DB에 저장된 목록을 파일명으로 정렬
        {
            string sql = "SELECT *FROM LogData WHERE ClientID = @ClientID ORDER BY FileName ASC";

            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", cid);
            myCommand.Parameters.Add(param);


            SqlDataReader myDataReader;
            myDataReader = myCommand.ExecuteReader();

            try
            {
                string str = "";
                while (myDataReader.Read())
                {
                    str += myDataReader[0].ToString() + "#";
                    str += myDataReader[1].ToString() + "#";
                    str += myDataReader[2].ToString() + "#";
                    str += myDataReader[3].ToString() + "#";
                    str += myDataReader[4].ToString() + "@";
                }

                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }

        public string MyFileListNameDESC(string cid) // DB에 저장된 목록을 파일명으로 정렬
        {
            string sql = "SELECT *FROM LogData WHERE ClientID = @ClientID ORDER BY FileName DESC";

            SqlCommand myCommand = new SqlCommand(sql, conn);

            SqlParameter param = new SqlParameter("@ClientID", cid);
            myCommand.Parameters.Add(param);


            SqlDataReader myDataReader;
            myDataReader = myCommand.ExecuteReader();

            try
            {
                string str = "";
                while (myDataReader.Read())
                {
                    str += myDataReader[0].ToString() + "#";
                    str += myDataReader[1].ToString() + "#";
                    str += myDataReader[2].ToString() + "#";
                    str += myDataReader[3].ToString() + "#";
                    str += myDataReader[4].ToString() + "@";
                }

                myDataReader.Close();
                myCommand.Dispose();
                return str;
            }
            catch
            {
                myDataReader.Close();
                myCommand.Dispose();
                return null;
            }
        }
    }
}

