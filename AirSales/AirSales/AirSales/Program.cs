using AirSales.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirSales
{
    public class Program
    {
        private static Mutex mut = new Mutex(); // יצירת מיוטקס
        public static volatile bool stop = false; // יצירת משתנה לצורך עצירה שהוא חשוף לכלל התהליכונים
        private const int numOfSalesman = 5; // משתנה כמות אנשי מכירות
        private const int numOfFlights = 3; // משתנה כמות טיסות
        private static Random rand = new Random(); // משתנה אקראי
        private static double totalMoney = 0; // משתנה כמות כסף
        private static int clientServed = 0; // משתנה כמות לקוחות
        private static MemoryMappedFile mmf; // משתנה ממורי מאפד פייל




        static void Main(string[] args)
        {

            // אתחול משתנה מממורי מאפד פייל עם שם וכמות בתים שיכול להכיל
            mmf = MemoryMappedFile.CreateNew("AirSalesMMF", 10000);


            // יצירת תהליכון שתפקידו להאזין לסוקט
            Thread t1 = new Thread(() =>
            {
                StartClient();
                
            });
            t1.Start();

            // יצירת תור של לקוחות דרך קנקורנט לצורך בטיחות תהליכונים
                ConcurrentQueue<Client> cq = new ConcurrentQueue<Client>();


            // יצירת אובייקט מכירות
            Sales sales = new Sales
            {
                TotalMoney = totalMoney,
                ClientsServed = clientServed
            };


            // לולאה ליצירת אנשי מכירות ומכירת כרטיסים
            for (int i = 0; i < numOfSalesman; i++)
            {
                Thread t = new Thread(() =>
                {
                    //יצירת משתנה לספירת כמות כרטיסים
                    int sold = 0;
                    // לולאה שרצה כל עוד לא עצרנו את התוכנית לפי התנאים שהוגדרו מראש
                    while (!stop)
                    {
                        // יצירת לקוח
                        Client client;
                        //הוצאת הלקוח מהתור לפי הסדר
                        if (cq.TryDequeue(out client))
                        {
                            // ניסיון מכירת כרטיס
                            if (Sell(client))
                            {
                                // במידה והצלחנו למכור כרטיס אז מעדכנים את האובייקט של המכירה
                                sales.TotalMoney = totalMoney;
                                sales.ClientsServed = clientServed;
                                mmfUsage(sales);
                                sold++;
                                PrintSale(client);
                            }
                        }
                    }
                });
                //הפעלת תהליכון הריצה
                t.Start();
            }

            // יצירת סוגי כרטיסים (מחלקה ראשונה ושנייה)
            Ticket[] tickets = new Ticket[2];
            tickets[0] = new FirstClass();
            tickets[1] = new EconomyClass();

            //יצירת כמות טיסות
            Flight[] flights = new Flight[numOfFlights];
            for(int i = 0; i < numOfFlights; i++)
            {
                flights[i] = new Flight();
            }


            int k = 0, j = 0;
            
            //לולאה שרצה ויוצרת לקוחות ומכניסה אותם לתור שהוגדר מעלה כל עוד לא התקיימו תנאים לעצירת התוכנית
            while (!stop)
            {
                Client client = new Client(k, flights[j], tickets[getRandomNumber(0,2)]);
                k++; j++;
                if(j % numOfFlights  == 0)
                {
                    j = 0;
                }
                cq.Enqueue(client);
                Thread.Sleep(getRandomNumber(5, 21));
            }
        }


        // פונקציה לתחילת התוכנית
        public static void StartClient()
        {
            //יצירת משתנים לביסוס תקשורת TCP/IP
            IPAddress ipAddr = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 17000);

            //יצירת סוקט להאזנה לתוכנית השנייה
            Socket listener = new Socket(ipAddr.AddressFamily,
                         SocketType.Stream, ProtocolType.Tcp);

            try
            {

                // קישור הסוקט לכתובת האייפי והפורט שהוגדר מעלה
                listener.Bind(localEndPoint);

                // תחילת האזנה לתקשורת שעוברת בסוקט שפתחנו
                listener.Listen(10);

                // לולאה שרצה עד שמסתיימת התקשורת בין המנגר לאיר סיילס
                while (true)
                {

                    Console.WriteLine("Waiting connection ... ");

                    Socket clientSocket = listener.Accept();

                    // יצירת מערך בתים להאזנה
                    byte[] bytes = new Byte[1024];
                    string data = null;

                    while (true)
                    {

                        int numByte = clientSocket.Receive(bytes);


                        // בדיקת איזה קוד קיבלנו והפעלת התנאים 
                        switch (bytes[0])
                        {
                            case 0x5D:
                                {
                                    stop = true;
                                    Console.WriteLine("SALES STOPPED BY MANAGER: DEPARTURE");
                                    break;
                                }
                            case 0x7A:
                                {
                                    stop = true;
                                    Console.WriteLine("SALES STOPPED BY MANAGER: RECEIVED WE_ARE_TOO_RICH_NOW CODE");
                                    break;
                                }
                            case 0x9E:
                                {
                                    stop = true;
                                    Console.WriteLine("SALES STOPPED BY MANAGER: ALL FLIGHTS SOLD OUT");
                                    break;
                                }
                        }
                    }




                    //סגירת התקשורת TCP/IP
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }

            // התמודדות עם חריגות
            catch (Exception e)
            {
                Console.WriteLine("Connection closed");
            }

            
        }

        //פונקציה לעדכון הממורי מאפד פייל
        public static void mmfUsage(Sales sales)
        {

            //שימוש בלוק כדי לוודא שרק תהליכון אחד כל פעם מבצע עדכון
            lock (mmf)
            {
                try
                {

                    // פתיחת הממורי מאפד פייל לצורך עדכון
                    using(MemoryMappedViewStream stream = mmf.CreateViewStream())
                    {
                        //הכנסת האובייקא הממעודכן לתוך הממורי מאפד פייל
                        string json = JsonConvert.SerializeObject(sales);
                        byte[] bytes = Encoding.UTF8.GetBytes(json);
                        MemoryMappedViewAccessor mmfv = mmf.CreateViewAccessor();
                        mmfv.WriteArray(0, bytes, 0, bytes.Length);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        public static int getRandomNumber(int min, int max)
        {
            // נעילת המשתנה לצורך גישה ועדכונו
            lock(rand)
            {
                return rand.Next(min,max);
            }
        }

        //עדכון כמות ההכנסות ועדכון כמות הלקוחות 
        public static void addToTotalMoney(double amount)
        {
            mut.WaitOne();
            
            {
                totalMoney += amount;
                clientServed++;
            }
            mut.ReleaseMutex();
        }


        // פונקציה למכירת כרטיסים
        public static bool Sell(Client client)
        {

            // בדיקה האם הכרטיס שהלקוח רוצה הוא מחלקה ראשונה
            if (client.GetTicket() is FirstClass)
            {

                // עדכון כמות ההכנסות
                addToTotalMoney(client.GetTicket().GetCost());
                // החזרת אמת או שקר במידה והצלחנו למכור כרטיס או לא
                return client.GetFlight().SellFirstClass();

            }
            else
            { // אותו דבר כמו התנאי למעלה רק למחלקה שנייה
                addToTotalMoney(client.GetTicket().GetCost());
                return client.GetFlight().SellEconomyClass();
             
            }
        }


        // פונקציית הדפסה
        public static void PrintSale(Client client)
        {
            Console.WriteLine("Worker " + System.Environment.CurrentManagedThreadId + " sold " + client.GetTicket().ToString() + " seat to client " + client.GetId());
        }

    }
}
