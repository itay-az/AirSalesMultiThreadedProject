using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirSales.Models
{
    //מחלקת לקוח
    public class Client
    {
        private int id;
        private Flight flight;
        private Ticket ticket;

        public Client(int id,Flight flight, Ticket ticket) 
        {
            this.id = id;
            this.flight = flight;
            this.ticket = ticket;
        }

        public int GetId() { return id; }

        public Flight GetFlight() { return flight; }
        public Ticket GetTicket() { return ticket;}


    }
}
