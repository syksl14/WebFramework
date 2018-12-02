using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SYuksel.OracleDb
{
    public class Sequence : Attribute
    {
        string sequence;
        public Sequence(string sequence)
        {
            this.sequence = sequence;
        }
        public string Sequences
        {
            get { return sequence; }
        }
    }
}
