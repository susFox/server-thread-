using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;  // Process. ~ . ~ 생성에 필요함
using System.IO; //StreamReader,StreamWriter 생성에 필요함
using System.Threading; // Thread 생성에 필요함

namespace Server_Thread_
{
    class Receiver
    {
        NetworkStream NS = null;
        StreamReader SR = null;
        StreamWriter SW = null;
        TcpClient client;

        public void startClient(TcpClient clientSocket)
        {
            client = clientSocket;
            Thread echo_thread = new Thread(echo);
            echo_thread.Start();
        }

        public void echo()
        {
            NS = client.GetStream(); // 소켓에서 메시지를 가져오는 스트림
            SR = new StreamReader(NS, Encoding.UTF8); // Get message
            SW = new StreamWriter(NS, Encoding.UTF8); // Send message

            string GetMessage = string.Empty;

            try
            {
                while (client.Connected == true) //클라이언트 메시지받기
                {
                    GetMessage = SR.ReadLine();

                    SW.WriteLine("Server: {0} [{1}]", GetMessage, DateTime.Now); // 메시지 보내기
                    SW.Flush();
                    Console.WriteLine("Log: {0} [{1}]", GetMessage, DateTime.Now);
                }
            }
            catch (Exception ee)
            {

            }
            finally
            {

                SW.Close();
                SR.Close();
                client.Close();
                NS.Close();
            }
        }
    }


    class Program
    {
        
        static void Main(string[] args)
        {
            /*
            if (args.Length < 1)
            {
                Console.WriteLine("사용법 : {0} <Bind IP>",
                    Process.GetCurrentProcess().ProcessName);
                return;
            }
            */

            Console.WriteLine(args.ToString());
            string bindIp = "127.0.0.1";//args[0];
            const int bindPort = 5426;
            TcpListener server = null;

            try
            {
                IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse(bindIp), bindPort);
                //IPEndPoint 네트워크 끝점을 ip주소와 포트 번호로 나타낸다
                server = new TcpListener(localAddress);

                server.Start();
                //서버 시작
                Console.WriteLine("메아리 서버 시작...");
                
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine(" 클라이언트 접속 : {0}", ((IPEndPoint) client.Client.RemoteEndPoint).ToString());

                    NetworkStream stream = client.GetStream();

                    int length;
                    string data = "";
                    byte[] bytes = new byte[256];

                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = Encoding.Default.GetString(bytes, 0, length);
                        Console.WriteLine("수신: {0}", data);

                        byte[] msg = Encoding.Default.GetBytes(data);
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("송신 : {0}", data);
                    }

                    stream.Close();
                    client.Close();

                }

            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("서버를 종료합니다.");
        }


    }
}