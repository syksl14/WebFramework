using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web.Configuration;
using System.Text.RegularExpressions;

namespace SYuksel
{
    /// <summary>
    /// Selahattin.cs framework ile daha kolay veritabanı yönetimi ve işlemleri yapmaktadır. Bu framework Selahattin YÜKSEL tarafından geliştirilmiştir.
    /// </summary>
    public class WebFramework
    {

        public SqlConnection Connect()
        {
            try
            {
                string baglanti = WebConfigurationManager.ConnectionStrings[Config.connectionName].ConnectionString;
                SqlConnection con = new SqlConnection(baglanti);
                con.Open();
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("OK! Connected database.");
                }
                return (con);
            }
            catch (Exception hata)
            {
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger(hata.ToString());
                }
                return null;
            }

        }
        /// <summary>
        /// Dataset nesnesini doldurur.'
        /// </summary>
        /// <param name="sql">Bir sql ifadesi girin.</param>
        /// <param name="values">sql parametre değerlerini girin.</param>
        public static DataSet DataSetFill(String sql, params object[] values)
        {
            Boolean calistir = true;
            if (sql.ToLower().Contains("where"))
            {
                //fakat parametreler @ şeklinde ayarlanmamış ise çalışmasına izin verilmez.
                if (sql.Contains("@") || sql.Contains("=@") || sql.Contains("= @"))
                {
                }
                else
                {
                    calistir = false;
                }
            }
            if (calistir)
            {
                SqlConnection baglan = new SqlConnection(WebConfigurationManager.ConnectionStrings[Config.connectionName].ToString());

                DataSet ds = new DataSet();

                try
                {
                    SqlDataAdapter da = new SqlDataAdapter(sql, baglan);
                    MatchCollection matches = Regex.Matches(sql, @"(@[A-Z, a-z])\w+");
                    for (int i = 0; i < matches.Count; i++)
                    {
                        da.SelectCommand.Parameters.AddWithValue(matches[i].ToString(), values[i]);
                    }
                    baglan.Open();
                    da.Fill(ds);
                    if (Config.debug == true)
                    {
                        FrameworkHandler.DebugLogger("Fill dataset: " + sql + "\n");
                        FrameworkHandler.DebugLogger("Return Data: " + ds.Tables[0].Rows.Count);
                    }
                }
                catch (Exception e)
                {
                    String hata = e.Message;
                    if (Config.debug == true)
                    {
                        FrameworkHandler.DebugLogger(e.ToString());
                    }
                    baglan.Close();
                }
                finally
                {
                    baglan.Close();
                }
                return ds;
            }
            else
            {
                if (Config.debug)
                {
                    FrameworkHandler.DebugLogger("Warning: Unsafe SQL was detected. " + sql);
                }
                return null;
            }
        }


        private DataTable DataTableGetir(string sql, params object[] values)
        {
            Boolean calistir = true;
            if (sql.ToLower().Contains("where"))
            {
                //fakat parametreler @ şeklinde ayarlanmamış ise çalışmasına izin verilmez.
                if (sql.Contains("= @") || sql.Contains("=@") || sql.Contains("@"))
                {
                }
                else
                {
                    calistir = false;
                }
            }
            if (calistir)
            {
                SqlConnection con = this.Connect();
                SqlDataAdapter adapter = new SqlDataAdapter(sql, con);
                MatchCollection matches = Regex.Matches(sql, @"(@[A-Z, a-z])\w+");
                for (int i = 0; i < matches.Count; i++)
                {
                    adapter.SelectCommand.Parameters.AddWithValue(matches[i].ToString(), values[i]);
                }
                DataTable dt = new DataTable();
                try
                {
                    adapter.Fill(dt);
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                adapter.Dispose();
                con.Close();
                return dt;
            }
            else
            {
                if (Config.debug)
                {
                    FrameworkHandler.DebugLogger("Warning: Unsafe SQL was detected. " + sql);
                }
                return null;
            }
        }


        /// <summary>
        /// Sql sorguları çalıştırır. Geçerli: DELETE, UPDATE vs.'
        /// </summary>
        /// <param name="sql">Bir sql ifadesi girin.</param>
        ///<param name="values">sql parametre değerlerini girin.</param>
        public static Result ExecuteNonQuery(string sql, params object[] values)
        {
            Boolean calistir = true;
            Result sonuc = new Result();
            string setparams = "";
            try
            {
                //sql ifadesi where şartı içeriyorsa
                if (sql.ToLower().Contains("where"))
                {
                    //fakat parametreler @ şeklinde ayarlanmamış ise çalışmasına izin verilmez.
                    if (sql.Contains("@") || sql.Contains("=@") || sql.Contains("= @"))
                    {
                    }
                    else
                    {
                        calistir = false;
                    }
                }
                if (calistir)
                {
                    SqlConnection baglan = new SqlConnection(WebConfigurationManager.ConnectionStrings[Config.connectionName].ToString());

                    if (baglan.State == ConnectionState.Open) // Sql Connection açıksa
                    {
                        baglan.Close(); // Kapat
                    }

                    baglan.Open(); // Aç

                    SqlCommand emir1 = new SqlCommand();
                    emir1.Connection = baglan;

                    /*  foreach (var value in values)
                      {
                          StringBuilder b = new StringBuilder(sql);
                          b.Replace("?", (string)"'" + value + "'");
                          sql = b.ToString();
                      }*/

                    emir1.CommandText = sql;

                    MatchCollection matches = Regex.Matches(sql, @"(@[A-Z, a-z])\w+");
                    for (int i = 0; i < matches.Count; i++)
                    {
                        setparams += matches[i].ToString() + "=" + values[i] + "\n";
                        emir1.Parameters.AddWithValue(matches[i].ToString(), values[i]);
                    }

                    int don = emir1.ExecuteNonQuery();
                    emir1.Dispose();
                    baglan.Close();
                    if (Config.debug == true)
                    {
                        FrameworkHandler.DebugLogger("SQL Execute: " + sql);
                    }
                    sonuc.ID = 0;
                    sonuc.Message = "OK";
                }
                else
                {
                    sonuc.Message = "Parameters not set correctly! Sample: UPDATE Users SET UserName= @UserName, Email= @Email WHERE ID= @ID";
                }

            }
            catch (Exception e)
            {
                string hata = e.ToString();
                sonuc.ID = 0;
                sonuc.Message = e.Message;
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger(sql + "\n" + "\n" + setparams + e.ToString());
                }
            }
            return sonuc;
        }


        public static void ProcedureExecute(string sql, params object[] values)
        {
            Boolean calistir = false;
            string setparams = "";
            try
            {
                //sql ifadesi where şartı içeriyorsa
                if (sql.ToLower().Contains("where"))
                {
                    //fakat parametreler @ şeklinde ayarlanmamış ise çalışmasına izin verilmez.
                    if (sql.Contains("@") || sql.Contains("=@") || sql.Contains("= @"))
                    {
                    }
                    else
                    {
                        calistir = false;
                    }
                }
                if (calistir)
                {
                    SqlConnection baglan = new SqlConnection(WebConfigurationManager.ConnectionStrings[Config.connectionName].ToString());

                    if (baglan.State == ConnectionState.Open) // Sql Connection açıksa
                    {
                        baglan.Close(); // Kapat
                    }

                    baglan.Open();

                    SqlCommand emir1 = new SqlCommand();
                    emir1.Connection = baglan;
                    emir1.CommandText = sql;
                    MatchCollection matches = Regex.Matches(sql, @"(@[A-Z, a-z])\w+");
                    for (int i = 0; i < matches.Count; i++)
                    {
                        setparams += matches[i].ToString() + "=" + values[i] + "\n";
                        emir1.Parameters.AddWithValue(matches[i].ToString(), values[i]);
                    }
                    emir1.ExecuteNonQuery();
                    baglan.Close();
                    if (Config.debug == true)
                    {
                        FrameworkHandler.DebugLogger("Procedure Call: " + sql);
                    }
                }
                else
                {
                    if (Config.debug)
                    {
                        FrameworkHandler.DebugLogger("Warning: Unsafe SQL was detected. " + sql);
                    }
                }
            }
            catch (Exception e)
            {
                string hata = e.ToString();
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger(e.ToString());
                }
            }
        }

        /// <summary>
        /// Veritabanındaki tabloya kayıt eklemek
        /// için kullanılır. Set edilmeyecek string, int veya null olabilecek alanlar public olabilir.
        /// Fakat tarih ve özel, primary key gibi alanlar sınıfta private olarak ayarlanmalıdır.
        /// </summary>
        /// <param name="tablo">Bir tablo örneği girin.</param>
        public static Result InsertData(Object tablo)
        {
            Result result = new Result();
            int sayac = 0;
            string sutunlar = "";
            string degerler = "";
            string values = "";
            String primary = null;
            Object[] dnAttribute = null;
            foreach (PropertyInfo prop in tablo.GetType().GetProperties())
            {
                sayac++;
                if (GetPropValue(tablo, prop.Name.ToString()) != null)
                {
                    dnAttribute = prop.GetCustomAttributes(true);
                    primary = null;
                    foreach (var item in dnAttribute)
                    {
                        Column pk = item as Column;
                        if (pk != null)
                        {
                            primary = prop.Name;
                        }
                    }
                    if (primary != prop.Name)
                    {
                        sutunlar += prop.Name + ",";
                        degerler += "@" + prop.Name + ",";
                    }
                }
            }
            sayac = 0;
            sutunlar = sutunlar.Remove(sutunlar.Length - 1);
            degerler = degerler.Remove(degerler.Length - 1);
            string query = "INSERT INTO " + tablo.GetType().Name + " (" + sutunlar + ") " +
                  "VALUES (" + degerler + ")";
            try
            {
                using (SqlConnection baglanti = new SqlConnection(WebConfigurationManager.ConnectionStrings[Config.connectionName].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, baglanti))
                    {
                        foreach (var prop in tablo.GetType().GetProperties())
                        {

                            if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(String))
                            {
                                if (GetPropValue(tablo, prop.Name.ToString()) != null)
                                {
                                    cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.NVarChar).Value = prop.GetValue(tablo, null);
                                }
                            }
                            else if (prop.PropertyType == typeof(int))
                            {
                                cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.Int).Value = prop.GetValue(tablo, null);
                            }
                            else if (prop.PropertyType == typeof(DateTime))
                            {
                                cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.DateTime).Value = prop.GetValue(tablo, null);
                            }
                            else if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(Double))
                            {
                                cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.Float).Value = prop.GetValue(tablo, null);
                            }
                            else if (prop.PropertyType == typeof(char))
                            {
                                cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.Char).Value = prop.GetValue(tablo, null);
                            }
                            else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(Boolean))
                            {
                                cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.Bit).Value = prop.GetValue(tablo, null);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@" + prop.Name.ToString(), prop.GetValue(tablo, null));
                            }
                            values += prop.GetValue(tablo, null) + ",";
                        }
                        baglanti.Open();
                        cmd.ExecuteNonQuery();
                        baglanti.Close();
                        values = values.Remove(values.Length - 1);
                    }
                }
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute: " + query + System.Environment.NewLine + "Values: " + values);
                }
                result.Message = "OK";
                return result;
            }
            catch (Exception hata)
            {
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute Fail: " + query + System.Environment.NewLine + "Values: " + values + System.Environment.NewLine + hata.ToString());
                }
                result.Message = "Error: " + hata.Message + " Query: " + query + " Values: " + values;
                return result;
            }


        }
        /// <summary>
        /// Veritabanındaki tabloya kayıt ekler diğer methotdan farkı eklenen kayıdın primary idsini döndürür.
        /// </summary>
        /// <param name="tablo">Bir tablo örneği girin.</param>
        public static Result InsertDataIdentity(Object tablo)
        {
            Result result = new Result();
            int sayac = 0;
            string sutunlar = "";
            string degerler = "";
            string values = "";
            String primary = null;
            Object[] dnAttribute = null;
            foreach (PropertyInfo prop in tablo.GetType().GetProperties())
            {
                sayac++;
                if (GetPropValue(tablo, prop.Name.ToString()) != null)
                {
                    dnAttribute = prop.GetCustomAttributes(true);
                    primary = null;
                    foreach (var item in dnAttribute)
                    {
                        Column pk = item as Column;
                        if (pk != null)
                        {
                            primary = prop.Name;
                        }
                    }
                    if (primary != prop.Name)
                    {
                        sutunlar += prop.Name + ",";
                        degerler += "@" + prop.Name + ",";
                    }
                }
            }
            sayac = 0;
            sutunlar = sutunlar.Remove(sutunlar.Length - 1);
            degerler = degerler.Remove(degerler.Length - 1);
            string query = "INSERT INTO " + tablo.GetType().Name + " (" + sutunlar + ") " +
                  "VALUES (" + degerler + "); SELECT @@IDENTITY AS 'Identity'";
            try
            {
                int sonId = 0;
                using (SqlConnection baglanti = new SqlConnection(WebConfigurationManager.ConnectionStrings[Config.connectionName].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, baglanti))
                    {
                        foreach (var prop in tablo.GetType().GetProperties())
                        {

                            if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(String))
                            {
                                if (GetPropValue(tablo, prop.Name.ToString()) != null)
                                {
                                    cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.NVarChar).Value = prop.GetValue(tablo, null);
                                }
                            }
                            else if (prop.PropertyType == typeof(int))
                            {
                                cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.Int).Value = prop.GetValue(tablo, null);
                            }
                            else if (prop.PropertyType == typeof(DateTime))
                            {
                                cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.DateTime).Value = prop.GetValue(tablo, null);
                            }
                            else if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(Double))
                            {
                                cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.Float).Value = prop.GetValue(tablo, null);
                            }
                            else if (prop.PropertyType == typeof(char))
                            {
                                cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.Char).Value = prop.GetValue(tablo, null);
                            }
                            else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(Boolean))
                            {
                                cmd.Parameters.Add("@" + prop.Name.ToString(), SqlDbType.Bit).Value = prop.GetValue(tablo, null);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@" + prop.Name.ToString(), prop.GetValue(tablo, null));
                            }
                            values += prop.GetValue(tablo, null) + ",";
                        }
                        values = values.Remove(values.Length - 1);
                        baglanti.Open();
                        sonId = Convert.ToInt32(cmd.ExecuteScalar());
                        baglanti.Close();
                    }
                }
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute: " + query + System.Environment.NewLine + "Values: " + values + System.Environment.NewLine + "Return ID: " + sonId);
                }
                result.ID = sonId;
                result.Message = "OK";
                return result;
            }
            catch (Exception hata)
            {
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute Fail: " + query + System.Environment.NewLine + "Values: " + values + System.Environment.NewLine + hata.ToString());
                }
                result.Message = "Error: " + hata.Message;
                return result;
            }
        }
        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        /// <summary>
        ///Tablodan tek satır veri getirir.  Seçili tablodan sql ifadesindeki alanları tek satır olarak getirir. Örnek: 'SELECT Ad,Soyad FROM Uyeler WHERE Id=3'
        /// </summary>
        /// <param name="sql">Bir sql ifadesi girin.</param>
        /// <param name="values">sql parametre değerlerini girin.</param>
        [Obsolete("GetSingleData method is deprecated, please not use.")]
        public static string GetSingleData(String sql, params object[] values)
        {
            try
            {
                int kayit = 0;
                int pFrom = sql.IndexOf("SELECT ") + "SELECT ".Length;
                int pTo = sql.LastIndexOf(" FROM ");
                String sonuc = sql.Substring(pFrom, pTo - pFrom).Trim();
                String verikumesi = null;
                String[] kolonlar = sonuc.Split(',');
                if (sonuc == "*")
                {
                    verikumesi = "An error occurred while rotating the line * instead of the column names.";
                }
                else
                {
                    DataSet ds = WebFramework.DataSetFill(sql, values);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < kolonlar.Length; i++)
                        {
                            verikumesi += ds.Tables[0].Rows[0].Field<Object>(kolonlar[i].Trim()) + Config.parse.ToString();
                        }
                    }
                    else
                    {
                        verikumesi = "";
                    }
                    kayit = ds.Tables[0].Rows.Count;
                    verikumesi = verikumesi.Remove(verikumesi.Length - 1);
                }
                if (Config.debug)
                {
                    FrameworkHandler.DebugLogger("SQL Execute: " + sql + System.Environment.NewLine + "Count: " + kayit);
                }
                return verikumesi;
            }

            catch (Exception hata)
            {
                if (Config.debug)
                {
                    FrameworkHandler.DebugLogger(hata.ToString());
                }
                return "Error: " + hata.Message;
            }

        }
        /// <summary>
        /// Tablodan birden fazla satır veri getirir. Seçili tablodan sql ifadesindeki alanları veya şarta göre bir veri kümesi getirir. Örnek: 'SELECT IL_ID,IL FROM iller'
        /// </summary>
        /// <param name="sql">Bir sql ifadesi girin.</param>
        /// <param name="values">sql parametre değerlerini girin.</param>
        [Obsolete("GetData method is deprecated, please not use.")]
        public static List<String> GetData(String sql, params object[] values)
        {

            List<String> tablo = new List<string>();
            try
            {
                int pFrom = sql.IndexOf("SELECT ") + "SELECT ".Length;
                int pTo = sql.LastIndexOf(" FROM ");
                String sonuc = sql.Substring(pFrom, pTo - pFrom).Trim();
                String[] kolonlar = sonuc.Split(',');
                int kayit = 0;
                if (sonuc == "*")
                {
                    tablo.Add("An error occurred while returning the dataset *, please specify the column names instead.");
                }
                else
                {
                    DataSet ds = WebFramework.DataSetFill(sql, values);
                    String verikumesi = null;
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            for (int j = 0; j < kolonlar.Length; j++)
                            {
                                verikumesi += ds.Tables[0].Rows[i].Field<Object>(kolonlar[j].Trim()) + Config.parse.ToString();
                            }
                            verikumesi = verikumesi.Remove(verikumesi.Length - 1);
                            tablo.Add(verikumesi);
                            verikumesi = null;
                        }
                        kayit = ds.Tables[0].Rows.Count;
                    }
                    else
                    {
                        tablo.Add("");
                    }
                }
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute: " + sql + System.Environment.NewLine + "Count: " + kayit);
                }
                return tablo;

            }
            catch (Exception hata)
            {
                tablo.Add("Error: " + hata.Message);
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger(hata.ToString());
                }
                return tablo;
            }

        }
        /// <summary>
        /// Tablodaki kayıt sayısını getirir. Örnek: 'SELECT COUNT(IL_ID) AS SAYI FROM iller'
        /// </summary>
        /// <param name="sql">Bir count sql ifadesi girin.</param>
        /// <param name="values">sql parametre değerlerini girin.</param>
        [Obsolete("GetCountData method is deprecated, please not use.")]
        public static string GetCountData(string sql, params object[] values)
        {
            try
            {
                int pFrom = sql.IndexOf("AS ") + "AS ".Length;
                int pTo = sql.LastIndexOf(" FROM ");
                String sonuc = sql.Substring(pFrom, pTo - pFrom).Trim();
                int verikumesi = 0;
                DataSet ds = WebFramework.DataSetFill(sql, values);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    verikumesi = ds.Tables[0].Rows[0].Field<int>(sonuc.Trim());
                }
                else
                {
                    verikumesi = 0;
                }
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute: " + sql + System.Environment.NewLine + "Return: " + verikumesi);
                }
                return verikumesi.ToString();

            }
            catch (Exception hata)
            {
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger(hata.ToString());
                }
                return "Error: " + hata.Message;
            }
        }
        /// <summary>
        /// Birden fazla tabloyu birleştirerek birden fazla kayıt getirir. Geçerli: INNER JOIN, LEFT JOIN'
        /// </summary>
        /// <param name="sql">Bir join sql ifadesi girin.</param>
        /// <param name="values">sql parametre değerlerini girin.</param>
        [Obsolete("GetJoinData method is deprecated, please not use.")]
        public static List<String> GetJoinData(string sql, params object[] values)
        {

            List<String> tablo = new List<string>();
            int kayit = 0;
            try
            {
                if (sql.IndexOf("INNER JOIN") != -1 || sql.IndexOf("LEFT JOIN") != -1 || sql.IndexOf("Inner Join") != -1 || sql.IndexOf("Left Join") != -1 || sql.IndexOf("inner join") != -1 || sql.IndexOf("left join") != -1)
                {
                    DataSet ds = WebFramework.DataSetFill(sql, values);
                    String verikumesi = null;
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                            {
                                verikumesi += ds.Tables[0].Rows[i][j] + Config.parse.ToString();
                            }
                            verikumesi = verikumesi.Remove(verikumesi.Length - 1);
                            tablo.Add(verikumesi);
                            verikumesi = null;
                        }
                        kayit = ds.Tables[0].Rows.Count;
                        if (Config.debug == true)
                        {
                            FrameworkHandler.DebugLogger("SQL Execute: " + sql + System.Environment.NewLine + "Count: " + kayit);
                        }
                        return tablo;
                    }
                    else
                    {
                        if (Config.debug == true)
                        {
                            FrameworkHandler.DebugLogger("SQL Execute: " + sql + System.Environment.NewLine + "Count: " + kayit);
                        }
                        tablo.Add("");
                        return tablo;
                    }

                }
                else
                {
                    if (Config.debug == true)
                    {
                        FrameworkHandler.DebugLogger(sql + System.Environment.NewLine + "Could not execute SQL query could not be found in the inner join and left join, please check your query.");
                    }
                    tablo.Add("Couldn't run Sql in question, please check your query.");
                    return tablo;
                }

            }
            catch (Exception hata)
            {
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger(hata.ToString());
                }
                tablo.Add("Error:" + hata.Message);
                return tablo;
            }

        }

        /// <summary>
        /// JSON tipinde veritabanında tablodan kayıtlar döndürür.'
        /// </summary>
        /// <param name="sql">Bir sql ifadesi girin.</param>
        /// <param name="values">sql parametre değerlerini girin.</param>
        [Obsolete("joinDataGetir method is deprecated, please not use.")]
        public static string GetJSONData(string sql, params object[] values)
        {
            try
            {
                WebFramework web = new WebFramework();
                DataTable dt = web.DataTableGetir(sql, values);
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute: " + sql + " \nCount: " + dt.Rows.Count);
                }

                return Utils.GetJson(dt);
            }
            catch (Exception hata)
            {
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger(hata.ToString());
                }
                return "Error: " + hata.Message;
            }
        }
        /// <summary>
        /// Bir tablodan obje değişkenlerinden kayıtlar getirir. Birden fazla kayıt veya tek satır kayıt gibi çok fonksiyonlu sql destekler.'
        /// </summary>
        /// <param name="sql">Bir sql ifadesi girin.</param>
        /// <param name="values">sql parametre değerlerini girin.</param>
        public static List<T> GetData<T>(String sql, params object[] values)
        {
            try
            {

                Type tip = typeof(T);
                PropertyInfo[] ps = tip.GetProperties();

                DataSet ds = WebFramework.DataSetFill(sql, values);

                List<T> liste = new List<T>();

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    var obj = Activator.CreateInstance<T>();

                    liste.Add(obj);

                    foreach (var properti in ps)
                    {
                        object deger = null;
                        try
                        {
                            deger = ds.Tables[0].Rows[i].Field<Object>(properti.Name);
                            Type t = Nullable.GetUnderlyingType(properti.PropertyType) ?? properti.PropertyType;
                            object safeValue = (deger == null) ? null : Convert.ChangeType(deger, t);
                            properti.SetValue(obj, safeValue, null);
                        }
                        catch
                        {
                        }
                    }
                }
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute: " + sql + System.Environment.NewLine + " Count: " + ds.Tables[0].Rows.Count);
                }
                return liste;

            }
            catch (Exception hata)
            {
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger(hata.ToString());
                }
                return null;
            }

        }

        /// <summary>
        /// Bir tablodan obje değişkenlerinden kayıt getirir. Sadece tek satır kayıt gibi çok fonksiyonlu sql destekler.'
        /// </summary>
        /// <param name="sql">Bir sql ifadesi girin.</param>
        /// <param name="values">sql parametre değerlerini girin.</param>
        public static T GetSingleData<T>(String sql, params object[] values)
        {
            try
            {
                Type tip = typeof(T);
                PropertyInfo[] ps = tip.GetProperties();

                DataSet ds = WebFramework.DataSetFill(sql, values);

                List<T> liste = new List<T>();


                var obj = Activator.CreateInstance<T>();

                liste.Add(obj);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (var properti in ps)
                    {
                        object deger = null;
                        try
                        {
                            deger = ds.Tables[0].Rows[0].Field<Object>(properti.Name);
                            Type t = Nullable.GetUnderlyingType(properti.PropertyType) ?? properti.PropertyType;
                            object safeValue = (deger == null) ? null : Convert.ChangeType(deger, t);
                            properti.SetValue(obj, safeValue, null);
                        }
                        catch (Exception hata)
                        {
                            if (Config.debug == true)
                            {
                                FrameworkHandler.DebugLogger(hata.ToString());
                            }
                        }
                    }
                }
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute: " + sql + System.Environment.NewLine + " Count: " + ds.Tables[0].Rows.Count);
                }
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return liste[0];
                }
                else
                {
                    return default(T);
                }

            }
            catch (Exception hata)
            {
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger(hata.ToString());
                }
                return default(T);
            }

        }
    }
}
