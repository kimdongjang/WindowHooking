using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _1209_team_study
{
    class BitServer
    {
        private Program program;
        private Socket server;
        Socket client;
        private List<Socket> clientlist = new List<Socket>();


        public BitServer(Program p, int port)
        {
            program = p;
            InitServer(port);
        }

        public void ServerRun()
        {
            ServerStart();
        }

        private void ServerStart()
        {
            while (true)
            {
                Socket client = server.Accept();  // 클라이언트 접속 대기
                //소켓 연결================================================
                IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                program.LogData("[접속] ", ip.Address.ToString(), ip.Port);
                //Console.WriteLine("{0}주소, {1}포트 접속", ip.Address, ip.Port);
                //=========================================================
                clientlist.Add(client);

                Thread th = new Thread(new ParameterizedThreadStart(ReavThread));
                th.IsBackground = true;
                th.Start(client);

                //Thread ft_thread = new Thread(new ParameterizedThreadStart(ft_ReavThread));
                //ft_thread.IsBackground = true;
                //ft_thread.Start(client);
            }
        }

        public void SendDataAll(string msg)
        {
            try
            {
                foreach (Socket s in clientlist)
                {
                    NetworkStream ns = new NetworkStream(s); // 소켓에 대한 스트림 객체를 생성
                    if (s.Connected) // 소켓이 연결된 상태라면
                    {
                        StreamWriter sw = new StreamWriter(ns);
                        sw.WriteLine(msg);
                        sw.Flush();
                        // 스트림 객체 기준으로 데이터를 작성해서 전송
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("[데이터 전송1]" + ex.Message);
            }
        }

        public void SendDataOne(Socket s, string msg)
        {
            try
            {
                NetworkStream ns = new NetworkStream(s);

                if (s.Connected)
                {
                    StreamWriter sw = new StreamWriter(ns);
                    sw.WriteLine(msg);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[데이터 전송2]" + ex.Message);
            }
        }

        private void ReavThread(object obj)
        {
            Socket sock = (Socket)obj;
            try
            {
                while (true)
                {
                    NetworkStream ns = new NetworkStream(sock);
                    StreamReader sr = new StreamReader(ns);
                    if (sr == null)
                        break;
                    string data = sr.ReadLine();
                    program.RecvData(data, sock);
                }
            }
            catch
            {
                //소켓 연결해지=============================================
                IPEndPoint ip = (IPEndPoint)sock.RemoteEndPoint;
                program.LogData("[해제] ", ip.Address.ToString(), ip.Port);
                //=========================================================

                clientlist.Remove(sock);
                sock.Close();
                //Console.WriteLine(ex.Message);
            }
           
        }
        private void ft_ReavThread(object obj)
        {
            Socket sock = (Socket)obj;
            while (true)
            {
                byte[] data = ReceiveData(sock);
                if (data == null)
                    break;
                string msg = Encoding.Default.GetString(data);
                program.RecvData(msg, sock);
            }

            clientlist.Remove(sock);
            //소켓 연결해제=================================================================
            IPEndPoint ip = (IPEndPoint)sock.RemoteEndPoint;//getpearName과 같은함수
            program.LogData("[해제] ", ip.Address.ToString(), ip.Port);

            //=============================================================================
            clientlist.Remove(sock);
            sock.Close();
        }

        private byte[] ReceiveData(Socket sock)
        {
            try
            {
                int total = 0;
                int size = 0;
                int left_data = 0;
                int recv_data = 0;
                // 수신할 데이터 크기 알아내기   
                byte[] data_size = new byte[4];
                recv_data = sock.Receive(data_size, 0, 4, SocketFlags.None);
                size = BitConverter.ToInt32(data_size, 0);
                left_data = size;

                byte[] data = new byte[size];
                // 서버에서 전송한 실제 데이터 수신
                while (total < size)
                {
                    recv_data = sock.Receive(data, total, left_data, 0);
                    if (recv_data == 0) break;
                    total += recv_data;
                    left_data -= recv_data;
                }
                return data;
            }
            catch (Exception)
            {
                //Console.WriteLine("[수신에러] : "+ex.Message);
                return null;
            }
        }

        private void InitServer(int port)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
            server = new Socket(AddressFamily.InterNetwork,
                                                      SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ipep);
            server.Listen(20);
        }
    }
}
