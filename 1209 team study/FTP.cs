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
    class FTP
    {
        public string FTPid = "wb";
        public string FTPpw = "123";
        public string playinit_filename;

        public string FTP_Init(string cname) // FTP 서버에서 가져온 파일 리스트를 str에 담아 반환해줌
        {
            List<string> list = GetFTPList(cname);
            string str = "";

            if (list == null)
            {
                return "실패";
            }
            else
            {
                foreach (string item in list)
                {
                    str += item + "#";
                }
                if (str == "")
                    str = "파일없음";
                return str; 
            }
        }

        public string FTP_Search(string cname, string filename)
        {
            List<string> list = GetFTPList(cname);
            string str = "";

            if (list == null)
            {
                return "실패";
            }
            else
            {
                foreach (string item in list)
                {
                    if (item == filename)
                    {
                        str += item;
                        break;
                    }
                    else
                    {
                        str = "파일없음";
                    }
                }
                return "성공";
            }
        } // 클라이언트로부터 채널 이름과 파일 이름을 받아 특정 FTP 디렉토리 안의 파일 이름을 검색

        

        private List<string> GetFTPList(string cname) // FTP 접속 후 Home Directory안의 파일 리스트를 가져온다.
        {
            try
            {
                // FTP 접속부분
                FtpWebRequest ftpWebRequest = WebRequest.Create("ftp://192.168.0.8:8000/home/" + cname) as FtpWebRequest;

                ftpWebRequest.Credentials = new NetworkCredential(FTPid, FTPpw);
                ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;


                StreamReader streamReader = new StreamReader(ftpWebRequest.GetResponse().GetResponseStream());

                // 파일 리스트 가져오기
                List<string> list = new List<string>();

                while (true)
                {
                    string fileName = streamReader.ReadLine();

                    if (string.IsNullOrEmpty(fileName))
                    {
                        break;
                    }
                    list.Add(fileName);
                }

                streamReader.Close();

                return list;
            }
            catch
            {
                return null;
            }
        }

        public string FTP_CreateDirectory(string cname) // 채널 생성시 FTP 디렉토리를 채널명으로 생성한다.
        {
            try
            {
                System.Net.FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create("ftp://192.168.0.8:8000/home/" + cname);
                request.Credentials = new NetworkCredential(FTPid, FTPpw);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.MakeDirectory;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("FTP Directory Created by channel name : " + response.StatusDescription);

                response.Close();

                return "성공";
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[FTP Directory Create Failed]");

                return "실패";
            }
        }

        public void FTP_DeleteDirectory(string cname) // 채널 삭제시 해당 채널 이름의 FTP 디렉토리가 삭제된다.
        {
            try
            {
                System.Net.FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create("ftp://192.168.0.8:8000/home/" + cname);
                request.Credentials = new NetworkCredential(FTPid, FTPpw);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("FTP Directory Deleted by channel name : " + response.StatusDescription);

                response.Close();
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[FTP Directory Delete Failed]");
            }
        }
    }
}
