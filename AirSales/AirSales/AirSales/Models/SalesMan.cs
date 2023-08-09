using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace AirSales.Models
{
    //מחלקה של איש מכירות
    public class SalesMan
    {
        private int id;
        private int sold;

        public SalesMan()
        {
            this.id = System.Environment.CurrentManagedThreadId;
            sold = 0;
        }

        public int GetId() { return id; }

        public int GetSold() { return sold;}


    }
}
