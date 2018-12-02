using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SYuksel
{
    public class Column : Attribute
    {
        string column;
        public Column(string column)
        {
            this.column = column;
        }
        public string Columns
        {
            get { return column; }
        }
     
    }
}