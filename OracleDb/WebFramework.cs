using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Data;
using System.Reflection;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;

namespace SYuksel.OracleDb
{
    public class WebFramework
    {
        private OracleConnection Connect()
        {
            string baglanti = WebConfigurationManager.ConnectionStrings[Config.connectionName].ConnectionString;
            OracleConnection con = new OracleConnection(baglanti);
            con.Open();
            return (con);
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
                //fakat parametreler : şeklinde ayarlanmamış ise çalışmasına izin verilmez.
                if (sql.Contains(":") || sql.Contains("=:") || sql.Contains("= :"))
                {
                }
                else
                {
                    calistir = false;
                }
            }
            if (calistir)
            {
                OracleConnection baglan = new OracleConnection(WebConfigurationManager.ConnectionStrings[Config.connectionName].ToString());

                DataSet ds = new DataSet();

                try
                {
                    OracleDataAdapter da = new OracleDataAdapter(sql, baglan);
                    MatchCollection matches = Regex.Matches(sql, @":(?=(?:'[^']*'|[^'\n])*$)\w+");
                    for (int i = 0; i < matches.Count; i++)
                    {
                        da.SelectCommand.Parameters.Add(new OracleParameter(matches[i].ToString().Split(':')[1], values[i]));
                    }
                    da.Fill(ds);
                }
                catch (Exception e)
                {
                    String hata = e.Message;
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
                //fakat parametreler : şeklinde ayarlanmamış ise çalışmasına izin verilmez.
                if (sql.Contains("= :") || sql.Contains("=:") || sql.Contains(":"))
                {
                }
                else
                {
                    calistir = false;
                }
            }
            if (calistir)
            {
                OracleConnection con = this.Connect();
                OracleDataAdapter adapter = new OracleDataAdapter(sql, con);
                MatchCollection matches = Regex.Matches(sql, @":(?=(?:'[^']*'|[^'\n])*$)\w+");
                for (int i = 0; i < matches.Count; i++)
                {
                    adapter.SelectCommand.Parameters.Add(new OracleParameter(matches[i].ToString(), values[i]));
                }
                DataTable dt = new DataTable();
                try
                {
                    adapter.Fill(dt);
                }
                catch (OracleException ex)
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
        private DataRow DataRowGetir(string sql)
        {
            DataTable tablo = DataTableGetir(sql);
            if (tablo.Rows.Count == 0) return null;
            return tablo.Rows[0];
        }
        /// <summary>
        /// Geri değer döndürmeyen direk sql sorguları çalıştırır. Geçerli: DELETE, UPDATE vs.'
        /// </summary>
        /// <param name="sql">Bir sql ifadesi girin.</param>
        /// <param name="values">sql parametre değerlerini girin.</param>
        public static Result ExecuteNonQuery(string sql, params object[] values)
        {
            Result sonuc = new Result();
            Boolean calistir = true;
            string setparams = "";
            try
            {
                //sql ifadesi where şartı içeriyorsa
                if (sql.ToLower().Contains("where"))
                {
                    //fakat parametreler : şeklinde ayarlanmamış ise çalışmasına izin verilmez.
                    if (sql.Contains(":") || sql.Contains("=:") || sql.Contains("= :"))
                    {
                    }
                    else
                    {
                        calistir = false;
                    }
                }
                if (calistir)
                {
                    OracleConnection baglan = new OracleConnection(WebConfigurationManager.ConnectionStrings[Config.connectionName].ToString());

                    if (baglan.State == ConnectionState.Open) // Sql Connection açıksa
                    {
                        baglan.Close(); // Kapat
                    }

                    baglan.Open(); // Aç

                    OracleCommand emir1 = new OracleCommand();

                    emir1.Connection = baglan;
                    emir1.CommandText = sql;
                    MatchCollection matches = Regex.Matches(sql, @":(?=(?:'[^']*'|[^'\n])*$)\w+");
                    for (int i = 0; i < matches.Count; i++)
                    {
                        if (matches[i].ToString() != "MI" || matches[i].ToString() != "SS" || matches[i].ToString() != "HH" || matches[i].ToString() != "MM")
                        {
                            emir1.Parameters.Add(new OracleParameter(matches[i].ToString(), values[i]));
                        }
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
        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        /// <summary>
        /// Veritabanındaki tabloya kayıt eklemek
        /// için kullanılır. Set edilmeyecek string, int veya null olabilecek alanlar public olabilir.
        /// Fakat tarih ve özel, primary key gibi alanlar sınıfta private olarak ayarlanmalıdır.
        /// </summary>
        /// <param name="tablo">Bir tablo örneği girin.</param>
        public static Result CommitInsert(Object tablo)
        {
            Result result = new Result();
            int sayac = 0;
            string sutunlar = "";
            string degerler = "";
            string values = "";

            String sema = "";
            Attribute[] attrs = Attribute.GetCustomAttributes(tablo.GetType());
            foreach (Attribute attr in attrs)
            {
                if (attr is Schema)
                {
                    Schema s = (Schema)attr;
                    if (s.Schemas != null || s.Schemas != "")
                    {
                        sema = s.Schemas + ".";
                    }
                }
            }

            foreach (PropertyInfo prop in tablo.GetType().GetProperties())
            {
                sayac++;
                if (GetPropValue(tablo, prop.Name.ToString()) != null)
                {
                    if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(String))
                    {
                        if (!prop.GetValue(tablo, null).Equals(0))
                        {
                            sutunlar += prop.Name + ",";
                            degerler += ":" + prop.Name + ",";
                        }
                    }
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long)
                        || prop.PropertyType == typeof(Int64) || prop.PropertyType == typeof(decimal))
                    {
                        if (!prop.GetValue(tablo, null).Equals(0))
                        {
                            sutunlar += prop.Name + ",";
                            degerler += ":" + prop.Name + ",";
                        }
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        if (prop.GetValue(tablo, null) != null)
                        {
                            sutunlar += prop.Name + ",";
                            degerler += ":" + prop.Name + ",";
                        }
                    }
                    else if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(double)
                        || prop.PropertyType == typeof(Double))
                    {
                        if (!prop.GetValue(tablo, null).Equals(0f) || !prop.GetValue(tablo, null).Equals(0d))
                        {
                            sutunlar += prop.Name + ",";
                            degerler += ":" + prop.Name + ",";
                        }
                    }
                }
            }

            sayac = 0;
            sutunlar = sutunlar.Remove(sutunlar.Length - 1);
            degerler = degerler.Remove(degerler.Length - 1);

            string query = "INSERT INTO " + sema + tablo.GetType().Name + " (" + sutunlar + ") " +
                  "VALUES (" + degerler + ")";
            try
            {

                using (OracleConnection baglanti = new OracleConnection(WebConfigurationManager.ConnectionStrings[Config.connectionName].ToString()))
                {
                    using (OracleCommand cmd = new OracleCommand(query, baglanti))
                    {
                        foreach (var prop in tablo.GetType().GetProperties())
                        {
                            if (GetPropValue(tablo, prop.Name.ToString()) != null)
                            {
                                String[] s = sutunlar.Split(',');
                                for (int i = 0; i < s.Length; i++)
                                {
                                    if (prop.Name.ToString() == s[i])
                                    {
                                        cmd.Parameters.Add(new OracleParameter(prop.Name.ToString(), prop.GetValue(tablo, null)));
                                    }
                                }
                            }
                            values += prop.GetValue(tablo, null) + ",";
                        }
                        baglanti.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        baglanti.Close();
                        values = values.Remove(values.Length - 1);
                    }
                }
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute: " + query + Environment.NewLine + "Values: " + values);
                }
                result.Message = "OK";
                return result;
            }
            catch (Exception hata)
            {
                if (Config.debug == true)
                {
                    FrameworkHandler.DebugLogger("SQL Execute Fail: " + query + Environment.NewLine + "Values: " + values + Environment.NewLine + hata.ToString());
                }
                result.Message = "Error: " + hata.Message + " Query: " + query + " Values: " + values;
                return result;
            }
        }


        /// <summary>
        /// Veritabanındaki tabloya kayıt ekler diğer methotdan farkı eklenen kayıdın primary idsini döndürür.
        /// </summary>
        /// <param name="tablo">Bir tablo örneği girin.</param>
        public static Result CommitInsertIdentity(Object tablo)
        {
            Result result = new Result();
            int sayac = 0;
            string sutunlar = "";
            string degerler = "";
            string values = "";
            String sema = "";
            String sequence = "";
            Attribute[] attrs = Attribute.GetCustomAttributes(tablo.GetType());
            foreach (Attribute attr in attrs)
            {
                if (attr is Schema)
                {
                    Schema s = (Schema)attr;
                    if (s.Schemas != null || s.Schemas != "")
                    {
                        sema = s.Schemas + ".";
                    }
                }
                else if (attr is Sequence)
                {
                    Sequence s2 = (Sequence)attr;
                    if (s2.Sequences != null || s2.Sequences != "")
                    {
                        sequence = s2.Sequences;
                    }
                }
            }

            foreach (PropertyInfo prop in tablo.GetType().GetProperties())
            {
                sayac++;
                if (GetPropValue(tablo, prop.Name.ToString()) != null)
                {
                    if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(String))
                    {
                        if (!prop.GetValue(tablo, null).Equals(0))
                        {
                            sutunlar += prop.Name + ",";
                            degerler += ":" + prop.Name + ",";
                        }
                    }
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long)
                        || prop.PropertyType == typeof(Int64) || prop.PropertyType == typeof(decimal))
                    {
                        if (!prop.GetValue(tablo, null).Equals(0))
                        {
                            sutunlar += prop.Name + ",";
                            degerler += ":" + prop.Name + ",";
                        }
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        if (prop.GetValue(tablo, null) != null)
                        {
                            sutunlar += prop.Name + ",";
                            degerler += ":" + prop.Name + ",";
                        }
                    }
                    else if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(double)
                        || prop.PropertyType == typeof(Double))
                    {
                        if (!prop.GetValue(tablo, null).Equals(0f) || !prop.GetValue(tablo, null).Equals(0d))
                        {
                            sutunlar += prop.Name + ",";
                            degerler += ":" + prop.Name + ",";
                        }
                    }
                }
            }
            sayac = 0;
            sutunlar = sutunlar.Remove(sutunlar.Length - 1);
            degerler = degerler.Remove(degerler.Length - 1);
            string query = "INSERT INTO " + sema + tablo.GetType().Name + " (" + sutunlar + ") " +
                  "VALUES (" + degerler + ")";
            try
            {
                Int64 sonId = 0;
                using (OracleConnection baglanti = new OracleConnection(WebConfigurationManager.ConnectionStrings[Config.connectionName].ToString()))
                {
                    using (OracleCommand cmd = new OracleCommand(query, baglanti))
                    {
                        foreach (var prop in tablo.GetType().GetProperties())
                        {
                            if (GetPropValue(tablo, prop.Name.ToString()) != null)
                            {
                                String[] s = sutunlar.Split(',');
                                for (int i = 0; i < s.Length; i++)
                                {
                                    if (prop.Name.ToString() == s[i])
                                    {
                                        cmd.Parameters.Add(new OracleParameter(prop.Name.ToString(), prop.GetValue(tablo, null)));
                                    }
                                }
                            }
                            values += prop.GetValue(tablo, null) + ",";
                        }
                        baglanti.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        values = values.Remove(values.Length - 1);
                    }
                    DataSet ds = new DataSet();
                    OracleDataAdapter da = new OracleDataAdapter("SELECT " + sema + sequence + ".CURRVAL FROM DUAL", baglanti);
                    da.Fill(ds);
                    sonId = Convert.ToInt64(ds.Tables[0].Rows[0].Field<Object>("CURRVAL"));
                    baglanti.Close();
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
                    DataSet ds = DataSetFill(sql, values);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < kolonlar.Length; i++)
                        {
                            verikumesi += ds.Tables[0].Rows[0].Field<Object>(kolonlar[i].Trim()) + Config.parse.ToString();
                        }
                    }
                    else
                    {
                        verikumesi = "" + Config.parse.ToString();
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
                if (Config.debug == true)
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
                    DataSet ds = DataSetFill(sql, values);
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
                object verikumesi = 0;
                DataSet ds = DataSetFill(sql, values);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    verikumesi = ds.Tables[0].Rows[0].Field<Object>(sonuc.Trim());
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
                    DataSet ds = DataSetFill(sql, values);
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
                            FrameworkHandler.DebugLogger("SQL Execute: " + sql + Environment.NewLine + "Count: " + kayit);
                        }
                        return tablo;
                    }
                    else
                    {
                        if (Config.debug == true)
                        {
                            FrameworkHandler.DebugLogger("SQL Execute: " + sql + Environment.NewLine + "Count: " + kayit);
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
                tablo.Add("Error:" + hata.Message);
                return tablo;
            }
        }

        /// <summary>
        /// JSON tipinde veritabanında tablodan kayıtlar döndürür.'
        /// </summary>
        /// <param name="sql">Bir sql ifadesi girin.</param>
        /// <param name="values">sql parametre değerlerini girin.</param>
        [Obsolete("GetJSONData method is deprecated, please not use.")]
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

                DataSet ds = DataSetFill(sql, values);

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

                DataSet ds = DataSetFill(sql, values);

                List<T> liste = new List<T>();

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    T obj = Activator.CreateInstance<T>();

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
                return liste[0];

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
