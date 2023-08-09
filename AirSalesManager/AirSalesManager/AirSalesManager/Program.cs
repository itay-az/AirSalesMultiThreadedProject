using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace AirSalesManager
{
    public class Program
    {
        private volatile static bool timeOut = false;  // מגדירים משתנה וולטיל (משתנה שחשוף לכל התהליכונים)
        private static Random rand = new Random(); // מגדירים משתנה אקראי גלובלי
        private const int numOfFlights = 3; // הגדרת מספר טיסות



        static void Main(string[] args)
        {
            System.Timers.Timer timer = new System.Timers.Timer(getRandomNumber(7000,17000)); // הגדרת טיימר שנעצר בין 7 ל17 שניות
            double WeAreTooRichMargin = getRandomNumber(110000, 125000);
            timer.Elapsed += handleTimerStop; // הכנסת פונקציה לסיום הטיימר
            timer.Start();



            try
            {


                // הגדרת משתנים לצורך ביסוס תקשורת TCP/IP
                IPAddress ipAddr = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 17000);
                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // יצירת חיבור
                    sender.Connect(localEndPoint);


                    bool stop = false; // יצירת משתנה שיעצור את התהליך

                    // פתיחת המממורי מאפד פייל
                    MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("AirSalesMMF");
                    MemoryMappedViewAccessor mmfv = mmf.CreateViewAccessor();


                    //לולאה שרצה עד כל עוד אחד התנאים שהוגדרו לעצירה לא מתקיימים
                    while (!stop)
                    {



                        //קריאה מהממורי מאפד פייל
                        byte[] responseBytes = new byte[mmfv.Capacity]; // יצירת מערך בתים
                        mmfv.ReadArray(0, responseBytes, 0, responseBytes.Length); // קריאה מהמערך
                        string responseJson = Encoding.UTF8.GetString(responseBytes); // המרה מבתים למחרוזת
                        dynamic obj = JsonConvert.DeserializeObject(responseJson); // יצירת אובייקט דינאמי



                        Console.WriteLine("Total income: " + obj.TotalMoney + ", Clientes served so far " + obj.ClientsServed);


                        // יצירת תנאי לעצירת התוכנית במידה ונגמר הזמן
                        if (timeOut)
                        {
                            byte[] Message = new byte[1];
                            Message[0] = 0x5D;
                            sender.Send(Message);
                            Console.WriteLine("Times up, Departuring");
                            stop = true;
                        }

                        // יצירת תנאי לעצירת התוכנית במידה וקיבלנו מספיק כסף
                        if (obj.TotalMoney >= WeAreTooRichMargin)
                        {
                            byte[] Message = new byte[1];
                            Message[0] = 0x7A;
                            sender.Send(Message);
                            Console.WriteLine("We are too rich");
                            stop = true;
                        }

                        // יצירת תנאי לעצירת התוכנית במידה ומכרנו את כל הכרטיסים
                        if (obj.ClientsServed == (numOfFlights * 132))
                        {
                            byte[] Message = new byte[1];
                            Message[0] = 0x9E;
                            sender.Send(Message);
                            Console.WriteLine("All Flights Sold Out");
                            stop = true;
                        }


                    }





                }


                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }  
        }

        //פונקציה לטיימר שתשנה את המשתנה הגלובלי לאמת
        private static void handleTimerStop(Object source, ElapsedEventArgs e)
        {
            timeOut = true;
            
        }

        //פונקציה שמקבלת מינימום ומקסימום ומחזירה מספר אקראי, בנוסף בטוחה לתהליכונים
        public static int getRandomNumber(int min, int max)
        {
            lock (rand)
            {
                return rand.Next(min, max);
            }
        }
    }
}

