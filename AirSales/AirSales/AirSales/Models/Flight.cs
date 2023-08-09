using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirSales.Models
{
    // מחלקה של טיסה
    public class Flight
    {
        private FirstClass firstSeats;
        private EconomyClass economySeats;

        public Flight()
        {
            firstSeats = new FirstClass();
            economySeats = new EconomyClass();
        }
        public FirstClass GetFirstClass()
        {
            return firstSeats;
        }

        public EconomyClass GetEcoClass()
        {
            return economySeats;
        }


        public bool SellFirstClass()
        {
            // נעילה של המשתנה לצורך קריאה תקינה עדכונו
            lock (firstSeats)
            {
                if(firstSeats.GetSeats() > 0 && !Program.stop)
                {
                    firstSeats.SellSeat();
                    return true;
                }
                return false;

            }
        }

        public bool SellEconomyClass()
        {
            // נעילה של המשתנה לצורך קריאה תקינה עדכונו
            lock (economySeats)
            {
                if(economySeats.GetSeats() > 0 && !Program.stop)
                {
                    economySeats.SellSeat();
                    return true;
                }
                return false;   
            }

        }
    }
}
