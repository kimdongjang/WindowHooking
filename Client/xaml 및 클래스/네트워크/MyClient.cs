using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    class MyClient
    {
        private Socket client;
        private string ip = "192.168.0.8";
        private int port = 7000;

        NetworkStream ns; // 데이터를 읽고 전송하기 위한 소켓을 저장하는 클래스

        #region 싱글톤
        static MyClient single_control;
        public static MyClient Instance { get { return single_control; } }
        static MyClient() { single_control = new MyClient(); }
        private MyClient()
        {

        }
        #endregion
        public bool InitClient()
        {
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), port);

                client = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Stream, ProtocolType.Tcp);

                client.Connect(ipep);  // 127.0.0.1 서버 7000번 포트에 접속시도

                ns = new NetworkStream(client); // 소켓을 네트워크 클래스에 저장

                Thread th = new Thread(new ParameterizedThreadStart(ReavThread));
                th.IsBackground = true;
                th.Start(client);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        } // 서버 접속 함수

        private void ReavThread(object obj)
        {
            try
            {
                Socket sock = (Socket)obj;
                while (true)
                {
                    // 서버의 데이터를 읽는 코드 ====================================
                    StreamReader sr = new StreamReader(ns);
                    if (sr == null)
                        break;
                    string data = sr.ReadLine();
                    RecvClass.Instance.RecvData(data);

                    // ======================================================
                    //byte[] data = ReceiveData(sock);
                    //if (data == null)
                    //    break;
                    //string msg = Encoding.Default.GetString(data);
                    //Control.Singleton.RecvData(msg);
                }

                sock.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SendDataOne(string msg)
        {
            try
            {
                if (client.Connected)
                {
                    StreamWriter sw = new StreamWriter(ns);
                    sw.WriteLine(msg);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[데이터전송1]" + ex.Message);
            }
        }
    }
}