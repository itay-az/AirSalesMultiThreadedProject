using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirSales.Models
{
    // מחלקה של כרטיס מחלקה שנייה שיורשת ממחלקת כרטיס
    public class EconomyClass : Ticket
    {
        private int cost;
        private int seats;


        public EconomyClass() { cost = 300; seats = 120; }

        public override int  GetCost() { return cost; }

        public int GetSeats() { return seats; }

        public void SellSeat()
        {
            if(seats == 0)
            {
                Console.WriteLine("Sold out");
                return;
            }
            seats--;
        }

        public override string ToString()
        {
            return "Economy class";
        }
    }
}
