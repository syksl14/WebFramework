using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SYuksel
{
   public class Schema : Attribute
    {
        string schema;
        public Schema(string schema)
        {
            this.schema = schema;
        }
        public string Schemas
        {
            get { return schema; }
        }
    }
}
