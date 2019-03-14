using System;
using System.Net;

namespace TraceConsole
{
    class Program
    {
        private static Trace traceroute;
        private static string lastHopIP;
        private static int hopsCount;
        private static int maxHops;

        static void Main(string[] args)
        {
            Console.Write("Введите IP-адрес или имя домена: ");
            string IPOrDomain = Console.ReadLine();           
            traceroute = new Trace(IPOrDomain);
            Console.Write("Введите максимальное количесвто переходов(прыжков): ");
            string hop = Console.ReadLine();

            if (hop != "")
            {
              maxHops = Convert.ToInt32(hop);
            }
            else
            {
                Console.WriteLine("Значение установлено по умолчанию(30).");
              maxHops = 30;
            }
            Console.WriteLine($"Маршрутизация к {traceroute.domain} [{traceroute.ip}]");
            hopsCount = 0;
            lastHopIP = "";

            for (int row = 1; row <= maxHops; row++) 
            {
                traceroute.InitTTL();

                Console.Write($"{row}");
                for (int i = 1; i <= 3; i++)
                {                  
                    Console.Write($" {traceroute.send()} ");
                }

                if (lastHopIP == traceroute.hopIP)
                    Console.Write("  Превышено время ожидания.");
                else
                {
                    string domain = "";
                    /*try
                    {
                         domain = Dns.GetHostEntry(Trace.HopIPadr).HostName.ToString();
                    }
                    catch (Exception ex)
                    {

                        Console.Write(ex.Message);
                    }*/
                    Console.Write($" {traceroute.hopIP}"); // {domain}
                    lastHopIP = traceroute.hopIP;
                }
                  Console.Write("\n");
                  hopsCount++;

                if (traceroute.hopIP == traceroute.ip.ToString())
                {
                    Console.WriteLine("Трассировка завершена.");                    
                    break;
                }
                else 
                 if (hopsCount >= maxHops)
                {
                    Console.WriteLine("Максимальное количество прыжков было достигнуто."); 
                    if (traceroute.hopIP != traceroute.ip.ToString())
                    {
                     Console.WriteLine("Конечная точка не достигнута.");
                    }
                }
            }
            Console.ReadKey();
        }
    }

   
}
