using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirSales.Models
{
    //מחלקת כרטיס ממחלקה ראשונה שיורשת מהמחלקה האבסטרקטית של כרטיס
    public class FirstClass : Ticket
    {
        private int cost;
        private int seats;

        public FirstClass() { cost = 800; seats = 12; }

        public override int GetCost() { return cost; }
        public int GetSeats() { return seats; }

        public void SellSeat()
        {
            if (seats == 0)
            {
                Console.WriteLine("Sold out");
                return;
            }
            seats--;
        }

        public override string ToString()
        {
            return "First class";
        }
    }
}
