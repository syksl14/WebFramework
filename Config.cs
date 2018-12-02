using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SYuksel
{
    public class Config
    {
        public static String connectionName = "conStr";
        public static String keywords = "";
        public static String description = "";
        public static char parse = '~';
        public static bool debug = false;
        public static Objects.Provider provider = Objects.Provider.MSSQL;
        public static List<String> minifyPagesUrl = new List<string>();
        public static void libStart() {
            string kaynak1 = "SYuksel.libs.HtmlAgilityPack.dll";
            EmbeddedAssembly.Load(kaynak1, "HtmlAgilityPack.dll");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }
        public static long sqlparamsSize = 1000;
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
        //      private static bool veriSikistirma = false;
        //    private static bool veriSifreleme = false;
        /* public static string getAciklama()
        {
            return Config.aciklama;
        }
        public static string getAnahtarKelimeler()
        {
            return Config.anahtarKelimeler;
        }*/
    }
}
