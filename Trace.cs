using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;


namespace TraceConsole
{
    class Trace
    {
        private Socket sendSocket;
        private Socket recSocket;
        public string domain="";
        public IPAddress ip;
        private const int port = 0;
        private IPEndPoint ipEndPoint;

        private EndPoint endPoint;

        private const Byte type = 8;
        private const Byte code = 0;
        private const UInt16 checkSum = 0;
        private const UInt16 ID = 1;
        private const int TTLexpired = 11;
        private UInt16 SN;
        private UInt32 data = 123456789;
        private Byte[] ICMP;
        
        private Byte[] recBuffer;

        private Byte ttl;       
        public string hopIP;
        public static IPAddress HopIPadr;
        public Trace(string IPOrName)
        {
            sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            recSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);// сделать одним сокетом
            recSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);
            recSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
            endPoint = new IPEndPoint(IPAddress.Any, 0);
            
            IPAddress[] ips;
            try
            {
                ips = Dns.GetHostAddresses(IPOrName);
                ip = ips[0];
                domain = Dns.GetHostEntry(IPOrName).HostName.ToString();
                ipEndPoint = new IPEndPoint(ip, port);
            }
            catch (Exception)
            {
                Console.WriteLine("Невозможно подключиться к хосту.");
            }


            ttl = 1; SN = 1;
        }

        public void InitTTL()
        {
            sendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, ttl);
            ttl++;
        }

        public string send()
        {

            ICMP = new Byte[1004];

            Byte[] byteType = new Byte[] { type };
            Byte[] byteCode = new Byte[] { code };
            Byte[] byteCheckSum = BitConverter.GetBytes(checkSum);
            Byte[] byteID = BitConverter.GetBytes(ID);
            Byte[] byteSN = BitConverter.GetBytes(SN);
            Byte[] byteData = BitConverter.GetBytes(data);

            Array.Copy(byteType, 0, ICMP, 0, byteType.Length);
            Array.Copy(byteCode, 0, ICMP, 1, byteCode.Length);
            Array.Copy(byteCheckSum, 0, ICMP, 2, byteCheckSum.Length);
            Array.Copy(byteID, 0, ICMP, 4, byteID.Length);
            Array.Copy(byteSN, 0, ICMP, 6, byteSN.Length);
            Array.Copy(byteData, 0, ICMP, 8, byteData.Length);

            UInt32 tmpSum = 0;
            int size = ICMP.Length;
            int index = 0;
            while (size > 1)
            {
                tmpSum += Convert.ToUInt32(BitConverter.ToUInt16(ICMP, index));
                index += 2;
                size -= 2;
            }
            if (size == 1)
            {
                tmpSum += Convert.ToUInt32(ICMP[index]);
            }
            tmpSum = (tmpSum >> 16) + (tmpSum & 0xffff);// Если сумма не вмещается в двухбайтное число, старшее слово складывается с младшим
            tmpSum += (checkSum >> 16);//  После операции >>16 все равно может произойти переполнение резултата, поэтому еще раз

            byteCheckSum = BitConverter.GetBytes((UInt16)(~tmpSum));
            Array.Copy(byteCheckSum, 0, ICMP, 2, byteCheckSum.Length);


            int receiveSize;
            Byte recType;
            Byte recCode;
            UInt16 recID;
            UInt16 recSN;

            recBuffer = new Byte[1024];

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
           
                sendSocket.SendTo(ICMP, ipEndPoint);
           
            while (true)
            {
                try
                {
                    receiveSize = recSocket.ReceiveFrom(recBuffer, ref endPoint);
                }
                catch (SocketException)
                {
                    return "*";
                }
                // анализ датаграммы
                recType = recBuffer[20];// первые 20 байт используются для заголовка IP
                recCode = recBuffer[21];
                recID = BitConverter.ToUInt16(recBuffer, 52);// размер датаграммы = 52 байта
                recSN = BitConverter.ToUInt16(recBuffer, 54);// порядковый номер необходим для корректного определения отправителя
                if (recType == TTLexpired && recCode == 0 && recID == ID && recSN == SN ||
                    recType == 0 && recCode == 0)
                {
                    stopwatch.Stop();
                    break;
                }

            }

            IPAddress temp = new IPAddress(BitConverter.ToUInt32(recBuffer, 12));
             hopIP = temp.ToString();
             HopIPadr = temp; 
            
            SN++;
              long ms = stopwatch.ElapsedMilliseconds;
            return ms == 0 ? "<1мс" : $"{ms}мс";
        }
    }
}
