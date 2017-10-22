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
using MySql.Data.MySqlClient; //mysql

namespace Server_Thread_
{
    
    class Receiver
    {
        NetworkStream NS = null;
        StreamReader SR = null;
        StreamWriter SW = null;
        TcpClient client;

        int tablenum = 0; // 0 이면 user Table, 1이면 ui Table
        int dbid = 1000;
        int photonum = 1;

        private void dbConnect(string dataq)
        {
            dbid=idProduce(); // DB id난수로 생성후 사용
            switch  (tablenum)
                {
                    case 0: // 사용자 등록(device->server)
                    {
                        Console.WriteLine("진입실패");
                        MySqlCommand cmd = new MySqlCommand(String.Format("INSERT INTO user VALUES ('{0}','{1}','{2}')"
                            ,dbid, dataq, photonum),Program.CONN);
                        cmd.ExecuteNonQuery();
                        dbid++;
                        photonum++;
                         break;
                    }

                case 1: // 판별 (device -> 서버(중개) -> openCV)
                    {
                        Console.WriteLine("진입1");
                        char d1; // id
                        char d2; // photonum
                        dataq.Split('/');
                        d1 = dataq[1]; // id
                        d2 = dataq[2]; // photonum
     

                        MySqlCommand cmd2 = new MySqlCommand
                            (String.Format("select {0} from user where id='{1}' ", d2, d1), Program.CONN);

                        MySqlDataReader reader = cmd2.ExecuteReader();
                        
                        string send = string.Empty;
                        Console.WriteLine("진입2");
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {

                                Console.Write(reader["id"]);
                                Console.Write(reader["name"]);
                                Console.WriteLine(reader["photonum"]);
                                string temp;
                                temp = d2.ToString(); // photonum 비교
                                if (temp == reader.GetString(1))
                                    send = "true";

                            }
                        }
                        else
                        {
                            Console.WriteLine("본인이 아닙니다.");
                            send = "false";
                        }
                        reader.Close();     
                     
                        SW.WriteLine("1{0}", send); // 1번 tablenum 실행했으니 그대로 보내줌
                        SW.Flush();

                        /*
                         SqlDataAdapter datainput = new SqlDataAdapter
                           (String.Format("select {0} from user where id='{1}' ", d1, d2), strConn);
                        
                        DataSet dbDataSet = new DataSet();
                        datainput.Fill(dbDataSet);
                        */

                        break;
                    }
                    

                    default:
                    {
                        break;
                    }

            }
            // string dbQuery = String.Format("select id from user = '{0}'", "asdf");
            //  string dbQuery = String.Format("select pw from temp where id = '{0}'", "asdf");

           // DataSet dbDataSet = new DataSet();

            
            //cmd.ExecuteNonQuery();
                
            
        }
       
        public int idProduce() // id 난수생성 (1001 ~ 4999)
        {
           
            Random idnum = new Random();
            return idnum.Next(1001, 5000);
        }

        public string CheckByte(string b1) // 첫번째 바이트 검사
        {
            
            string temp;
            char []tempS = b1.ToArray();           
            if (tempS[0] == '0')
            {
               
                tablenum = 0;
                return temp = b1.Substring(1);
            }

            else if (tempS[1] == '1')
            { 
                tablenum = 1;
                return temp = b1.Substring(1);
            }

            else
            { 
                tablenum = 2;
                return "";
            }

        }

        public char ByteSplit(string b1) // 데이터 쪼개기
        {
            b1.Split('/');
            char temp = b1[0];
            return temp; 
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

                    dbConnect(CheckByte(GetMessage));

                    //SW.WriteLine("Server: {0} [{1}]", GetMessage, DateTime.Now); // 메시지 보내기
                    //SW.Flush();
                    Console.WriteLine("Client : {0} [{1}]", GetMessage, DateTime.Now);

                    

                    /* 임시로 통신확인할때 
                    Console.Write("Server -> client한테 보낼 메세지 : ");
                    SendMessage = Console.ReadLine();
                    SW.WriteLine("Server: {0} [{1}]", SendMessage, DateTime.Now);
                    SW.Flush();
                    Console.WriteLine();
                    */

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
        public static MySqlConnection CONN;
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
           
            string strConn = "Server=localhost;Database=mydb;Uid=root;Pwd=s8727080;";

            MySqlConnection conn = new MySqlConnection(strConn);
            try
            {
                conn.Open();
            }
            catch(NotImplementedException e)
            {
                Console.WriteLine(e);
            }
            
            CONN = conn;

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
            CONN.Close();
        }


    }
}