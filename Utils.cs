using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SYuksel
{
    public class Utils
    {
        /// <summary>
        /// Türkçe karakter ve özel karakterleri temizler bir FriendlyURL dönüştürür.'
        /// </summary>
        /// <param name="text">Türkçe karakter içeren metin girin.</param>
        public static string ClearTurkishLetter(string text)
        {
            string Temp = "";
            Temp = text.ToLower();
            Temp = Temp.Replace("-", ""); Temp = Temp.Replace(" ", "-");
            Temp = Temp.Replace("ç", "c"); Temp = Temp.Replace("ğ", "g");
            Temp = Temp.Replace("ı", "i"); Temp = Temp.Replace("ö", "o");
            Temp = Temp.Replace("ş", "s"); Temp = Temp.Replace("ü", "u");
            Temp = Temp.Replace("\"", ""); Temp = Temp.Replace("/", "");
            Temp = Temp.Replace("(", ""); Temp = Temp.Replace(")", "");
            Temp = Temp.Replace("{", ""); Temp = Temp.Replace("}", "");
            Temp = Temp.Replace("%", ""); Temp = Temp.Replace("&", "");
            Temp = Temp.Replace("+", ""); Temp = Temp.Replace(".", "-");
            Temp = Temp.Replace("?", ""); Temp = Temp.Replace(",", "");
            Temp = Temp.Replace("#", "-"); Temp = Temp.Replace("*", "-");
            Temp = Temp.Replace("_", "-"); Temp = Temp.Replace("'", "");
            Temp = Temp.Replace(":", ""); Temp = Temp.Replace("?", "");
            Temp = Temp.Replace(",", ""); Temp = Temp.Replace(";", "");
            Temp = Temp.Replace("\"", ""); Temp = Temp.Replace("’", "");
            return Temp;
        }
        /// <summary>
        /// Kategori ismi, özel isim gibi dosyaları tekrar adlandıran aynı zamanda Türkçe karakter ve özel karakterleri temizler. (Örnek: Resimler-0js091183kc_guzel_manzara.jpg)'
        /// </summary>
        /// <param name="CodeName">Bir takma isim girin.</param>
        /// <param name="FileName">Bir dosya adı girin.</param>
        public static string FileRename(string CodeName, string FileName)
        {
            string yeni_dosyadi = FileName;
            yeni_dosyadi = yeni_dosyadi.ToLower();
            yeni_dosyadi = yeni_dosyadi.Replace('ö', 'o');
            yeni_dosyadi = yeni_dosyadi.Replace('ü', 'u');
            yeni_dosyadi = yeni_dosyadi.Replace('ğ', 'g');
            yeni_dosyadi = yeni_dosyadi.Replace('ş', 's');
            yeni_dosyadi = yeni_dosyadi.Replace('ı', 'i');
            yeni_dosyadi = yeni_dosyadi.Replace('ç', 'c');
            yeni_dosyadi = yeni_dosyadi.Replace(' ', '_');
            yeni_dosyadi = yeni_dosyadi.Replace('!', '&');
            string sonuc = CodeName + "-" + yeni_dosyadi;
            return sonuc;
        }
        /// <summary>
        /// İstenilen kullanıcı adında özel bir benzersiz anahtar üretir. (Bkz: GUID)'
        /// </summary>
        /// <param name="UserName">Bir kullanıcı adı girin.</param>
        public string GenerateKey(String UserName)
        {
            return MD5Hash(UserName) + CodeGenerator(50) + UserName + CodeGenerator(25) + "_" + UserName.Substring(3) + CodeGenerator(70) + CodeGenerator(10).ToLower();
        }
        private static string NewFileName(string CodeName, string Extension, int Length)
        {
            string yeni_dosyadi = CodeGenerator(Length);
            string sonuc = CodeName + "-" + yeni_dosyadi + Extension;
            return sonuc;
        }
        /// <summary>
        /// Dizinde istenilen dosya adı ve uzantıda dosya oluşturma
        /// </summary>
        ///<param name="path">Dosyanın oluşturulacağı klasörün yolu.</param>
        ///<param name="fileName">Dosyanın adı.</param>
        ///<param name="content">Dosyanın içeriği.</param>
        public static string FileCreate(string path, string fileName, string content)
        {
            try
            {
                string dosyaYolu = HttpContext.Current.Server.MapPath(path + fileName);
                StreamWriter w;
                w = File.CreateText(dosyaYolu);
                w.WriteLine(content);
                w.Flush();
                w.Close();
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("Path: " + path + fileName + " File Create!");
                }
                return "OK";
            }
            catch (Exception e)
            {
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger(e.ToString());
                }
                return e.Message;
            }

        }
        /// <summary>
        /// String tipinde bir değişkeni veya veriyi md5 olarak bir string değişkene ya da alana geri döndürür.
        /// </summary>
        ///<param name="text">Bir string veri girin.</param>
        public static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
            byte[] result = md5.Hash;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));
            }
            return strBuilder.ToString();
        }

        /// <summary>
        /// A-Z ve 0-9 arasında karmaşık string üretir.'
        /// </summary>
        /// <param name="codeLength">Uzunluk girin.</param>
        public static string CodeGenerator(int codeLength)
        {
            StringBuilder sb = new StringBuilder();

            Random objRandom = new Random();

            string[] strChars = { "A","B","C","D","E","F","G","H","I",

                            "J","K","L","M","N","O","P","Q","R",

                            "S","T","U","V","W","X","Y","Z",

                            "1","2","3","4","5","6","7","8","9","0"};

            int maxRand = strChars.GetUpperBound(0);

            for (int i = 0; i < codeLength; i++)
            {
                int rndNumber = objRandom.Next(maxRand);

                sb.Append(strChars[rndNumber]);
            }

            return sb.ToString();
        }

        public static string GetJson(DataTable dt)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row = null;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName.Trim(), dr[col]);
                }
                rows.Add(row);
            }

            return serializer.Serialize(rows);
        }

       
    }
}
