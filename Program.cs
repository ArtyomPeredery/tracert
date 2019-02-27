using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;


namespace ConsoleApplication8
{
    public class TraceLocation
    {
        public int Hop { get; set; }        
        public long Time1 { get; set; }
        public long Time2 { get; set; }
        public long Time3 { get; set; }
        public String IpAddress { get; set; }
    }



    public class Trace
    {
        public static List<TraceLocation> Traceroute(string ipAddressOrHostName, int maximumHops, bool end)
        {
           if (maximumHops < 1 || maximumHops > 100)
            {
                maximumHops = 30;
            }
            IPAddress ipAddress = Dns.GetHostEntry(ipAddressOrHostName).AddressList[0];
            List<TraceLocation> traceLocations = new List<TraceLocation>();
            using (Ping pingSender = new Ping())
            {
                PingOptions pingOptions = new PingOptions();
                Stopwatch stopWatch = new Stopwatch();
                byte[] bytes = new byte[32];
                pingOptions.DontFragment = true;
                pingOptions.Ttl = 1;

                for (int i = 1; i <= maximumHops; i++)
                {
                    TraceLocation traceLocation = new TraceLocation();
                    traceLocation.Hop = i;
                    

                 for (int j = 1; j <= 3; j++)
                 { 
                        
                    stopWatch.Reset();
                    stopWatch.Start();
                    PingReply pingReply = pingSender.Send(ipAddress, 5000, new byte[32], pingOptions);

                    stopWatch.Stop();
                    
                     switch (j)
                        {
                            case 1: traceLocation.Time1 = stopWatch.ElapsedMilliseconds;  break;
                            case 2: traceLocation.Time2 = stopWatch.ElapsedMilliseconds; break;
                            case 3: traceLocation.Time3 = stopWatch.ElapsedMilliseconds; break;

                        }                                                                                                           

                    if (pingReply.Status == IPStatus.Success)
                    {
                        break; 
                    }
                 }

                    PingReply pingReplyforaddr = pingSender.Send(ipAddress, 5000, new byte[32], pingOptions);
                    if (pingReplyforaddr.Address != null)
                    {
                        traceLocation.IpAddress = pingReplyforaddr.Address.ToString();
                    }
                    if  (end)
                    {
                        break;
                    }

                    traceLocations.Add(traceLocation);
                    traceLocation = null;
                    pingOptions.Ttl++;
                    
                }
            }        
            return traceLocations;
        }    
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<string> domainNames = new List<string>();

            Console.WriteLine("Введите IP или доменное имя:");
            string str = Console.ReadLine();
            domainNames.Add(str);        

            foreach (String domainName in domainNames)
            {                           
                IPAddress ipaddress = Dns.GetHostEntry(domainName).AddressList[0] ;
                Console.WriteLine("маршрутизация к " + domainName + "["+ipaddress+"]  ");
                bool ending= false;
                foreach (TraceLocation traceLocation in Trace.Traceroute(domainName, 100, ending))
                {                                                        
                    if (!ending)
                        Console.Write(traceLocation.Hop + " " + traceLocation.Time1 + "мс  " + traceLocation.Time2 + "мс  "+ traceLocation.Time3 + "мс  " + traceLocation.IpAddress + "   " );




                    if (!String.IsNullOrWhiteSpace(traceLocation.IpAddress) && !traceLocation.IpAddress.StartsWith("10.") && !traceLocation.IpAddress.StartsWith("192."))
                    {

                        try
                        {
                            if (!ending)
                            Console.WriteLine(Dns.GetHostEntry(traceLocation.IpAddress).HostName.ToString());
                            if ((ipaddress.ToString() == traceLocation.IpAddress.ToString()) && (!ending))
                            {

                                Console.WriteLine("Маршрутизация осуществлена успешно!");
                                ending = true;
                            }
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(ex.Message);
                            if ((ipaddress.ToString() == traceLocation.IpAddress.ToString()) && (!ending) )
                            {

                                Console.WriteLine("Маршрутизация осуществлена успешно!");
                                ending = true;
                            }
                        }
                    }
                    else
                    {                                            
                        Console.WriteLine();
                    }                   
                }
                Console.ReadKey();
            }

        }

    }
}
