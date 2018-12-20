using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapeToSQLDatabase
{
    public class Stock
    {
        public string Symbol { get; set; }
        public string Price { get; set; }
        public string Change { get; set; }
        public string PChange { get; set; }
        public string Volume { get; set; }
        public string MarketCap { get; set; }
    }
}
