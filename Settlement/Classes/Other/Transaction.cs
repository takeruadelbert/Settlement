using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement.Classes.Other
{
    public class Transaction
    {
        public string Result { get; set; }
        public string Amount { get; set; }
        public string TransactionDatetime { get; set; }
        public string Bank { get; set; }
        public string IPV4 { get; set; }
        public string Operator { get; set; }
        public string IDReader { get; set; }
    }
}
