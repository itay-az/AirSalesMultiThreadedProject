using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirSales.Models
{
    // מחלקת אבסטרקטית של כרטיסים
    public abstract class Ticket
    {
        public abstract int GetCost();
        public abstract override string ToString();
        
    }
}
