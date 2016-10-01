using System;
using System.Collections.Generic;

namespace Foundatio00 {
    public class CurrencyExchange {
        public string Base{get; set;}
        public DateTime Date{get; set;}
        public Dictionary <string, decimal> Rates{get; set;}
        public string Error{get; set;}
    }
}