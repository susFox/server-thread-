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
using System.Data.SqlClient; // mysql 필요함
using System.Data;

namespace Server_Thread_
{
    class Receiver
    {
        NetworkStream NS = null;
        StreamReader SR = null;
        StreamWriter SW = null;
        TcpClient client;

        private object locker = new object();
        private void dbConnect()
        {

            lock (locker)
            {
                string serverDomain = "wnsfox@192.168.153.1";
                string schema = "mydb";

                string strDBConn = String.Format("Server={0};Database={1};Uid=mellorin;Pwd=tjqjtjqj;", serverDomain, schema);
                MySqlConnection dbConn = new MySqlConnection(strDBConn);
                dbConn.Open();

                string dbQuery = String.Format("select pw from temp where id = '{0}'", "asdf");

                DataSet dbDataSet = new DataSet();
                MySqlDataAdapter dbAdapter = new MySqlDataAdapter(dbQuery, dbConn);
                dbAdapter.Fill(dbDataSet, "temp");

                if (dbDataSet.Tables[0].Rows.Count != 0)
                {
                    string pw = (string)dbDataSet.Tables[0].Rows[0]["pw"];
                    Console.WriteLine("값 찾음 pw = {0}", pw);
                }
                else
                {
                    string insertQuery = String.Format("insert into data (uuid,token) values ('{0}','{1}');", uuid, token);
                    MySqlCommand cmd = new MySqlCommand(insertQuery, dbConn);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("못 찾음");

                }

                dbConn.Close();
            }
        }

        public string CheckByte(string b1) // 첫번째 바이트 검사
        {
            string temp;
            char []tempS = b1.ToArray();

            if (tempS[0] == '0')
                return temp = b1.Substring(1);

            else if (tempS[1] == '1')
                return temp = b1.Substring(1);

            else return "";
     
        }

        
        public void startClient(TcpClient clientSocket)
        {
                client = clientSocket;
                Thread receive_thread = new Thread(receive_f);
                receive_thread.Start();
        }

        public void receive_f()
        {
            NS = client.GetStream(); // 소켓에서 메시지를 가져오는 스트림
            SR = new StreamReader(NS, Encoding.UTF8); // Get message
            SW = new StreamWriter(NS, Encoding.UTF8); // Send message

            string GetMessage = string.Empty;
            string SendMessage = string.Empty;

            try
            { 
                while (client.Connected == true) //클라이언트 메시지받기
                {
                    GetMessage = SR.ReadLine();

                    CheckByte(GetMessage);
          
                    //SW.WriteLine("Server: {0} [{1}]", GetMessage, DateTime.Now); // 메시지 보내기
                    //SW.Flush();
                    Console.WriteLine("Client : {0} [{1}]", GetMessage, DateTime.Now);

                    Console.Write("Server -> client한테 보낼 메세지 : ");
                    SendMessage = Console.ReadLine();
                    SW.WriteLine("Server: {0} [{1}]", SendMessage, DateTime.Now);
                    SW.Flush();
                    Console.WriteLine();

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
            const int bindPort = 5427;
            TcpListener server = null;
            TcpClient client = null;

            try
            {
                IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse(bindIp), bindPort);
                //IPEndPoint 네트워크 끝점을 ip주소와 포트 번호로 나타낸다
                server = new TcpListener(localAddress);

                server.Start();
                //서버 시작
                Console.WriteLine("서버 시작...");
                
                while (true)
                {
                    client = server.AcceptTcpClient();

                    Console.WriteLine(" 클라이언트 접속 : {0}", ((IPEndPoint) client.Client.RemoteEndPoint).ToString());

                    Receiver r = new Receiver();
                    r.startClient(client);

                    /*
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
                    */
                }

            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                //server.Stop();
            }

            Console.WriteLine("서버를 종료합니다.");
        }


    }
}