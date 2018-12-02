using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
namespace SYuksel
{
    public class UIController : Attribute
    {
        private Boolean required;
        private int maxLength;
        private string label;
        private Objects.Type type;
        private string className;
        private string placeHolder;
        private string SQL;
        private string displayValue;
        private string value;
        private String sections;
        ///<summary>Şunları kapsar: HIDDEN.</summary>
        /// <param name="type">HTML5 Nesne tipi</param>
        /// <param name="sections">HTML5 nesnelerin hangi bölümlerde oluşturulacağını belirler birden fazla yerde oluşturulacaksa virgül ile ayırın.</param>
        /// <param name="required">Nesne zorunlu alan mı?</param>
        public UIController(Objects.Type type, String sections, Boolean required)
        {
            this.type = type;
            this.sections = sections;
            this.required = required;
        }
        ///<summary>Şunları kapsar: CHECKBOX, FILEUPLOAD.</summary>
        /// <param name="type">HTML5 Nesne tipi</param>
        /// <param name="sections">HTML5 nesnelerin hangi bölümlerde oluşturulacağını belirler birden fazla yerde oluşturulacaksa virgül ile ayırın.</param>
        /// <param name="Class">HTML5 nesnesinin class özelliği</param>
        /// <param name="label">Nesnenin etiketi</param>
        /// <param name="required">Nesne zorunlu alan mı?</param>
        /// <param name="defaultValue">Checkbox nesnesinin işaretlendikten sonraki değeri yazar. (Örnek: 'E', 'true')</param>
        public UIController(Objects.Type type, String sections, Boolean required, String Class, String label, string defaultValue)
        {
            this.label = label;
            this.required = required;
            this.type = type;
            this.className = Class;
            this.sections = sections;
            this.value = defaultValue;
        }
        /// <summary>
        /// Şunları kapsar: TEXTBOX, PASSWORD, TEXTAREA 
        /// </summary>
        /// <param name="type">HTML5 Nesne tipi</param>
        /// <param name="sections">HTML5 nesnelerin hangi bölümlerde oluşturulacağını belirler birden fazla yerde oluşturulacaksa virgül ile ayırın.</param>
        /// <param name="Class">HTML5 nesnesinin class özelliği</param>
        /// <param name="label">Nesnenin etiketi</param>
        /// <param name="required">Nesne zorunlu alan mı?</param>
        /// <param name="maxLength">HTML5 nesnelerinden TEXTBOX, TEXTAREA, PASSWORD maksimum kaç karakter girileceğini belirler.</param>
        /// <param name="placeholder">HTML5 nesnelerin kullanıcıya verilecek referans bilgi girilir.</param>
        public UIController(Objects.Type type, String sections, Boolean required, String Class, String label, int maxLength, String placeholder)
        {
            this.label = label;
            this.required = required;
            this.maxLength = maxLength;
            this.type = type;
            this.className = Class;
            this.placeHolder = placeholder;
            this.sections = sections;
        }

        ///<summary>Şunları kapsar: SELECT. SQL Sorgusu, gösterilecek alan ve value değerlerleri yazılır.</summary>
        /// <param name="type">HTML5 Nesne tipi</param>
        /// <param name="sections">HTML5 nesnelerin hangi bölümlerde oluşturulacağını belirler birden fazla yerde oluşturulacaksa virgül ile ayırın.</param>
        /// <param name="Class">HTML5 nesnesinin class özelliği</param>
        /// <param name="label">Nesnenin etiketi</param>
        /// <param name="required">Nesne zorunlu alan mı?</param>
        /// <param name="SQL">SELECT nesnesini dolduracak sql sorgusunu girin. (Örnek: SELECT KULLANICI_ID, ADSOYAD FROM [tablo])</param>
        /// <param name="displayValue">Select nesnesinin option alanlarında gözükecek değerler. (Örnek: ADSOYAD)</param>
        /// <param name="Value">Select nesnesinin option alanlarında value değerler. (Örnek: KULLANICI_ID)</param>
        public UIController(Objects.Type type, String sections, Boolean required, String Class, String label, String SQL, String displayValue, String Value)
        {
            this.label = label;
            this.required = required;
            this.type = type;
            this.className = Class;
            this.SQL = SQL;
            this.displayValue = displayValue;
            this.value = Value;
            this.sections = sections;
        }
        public String Sections
        {
            get { return sections; }
        }
        public Objects.Type Tip
        {
            get { return type; }
        }

        public Boolean Required
        {
            get { return required; }
        }
        public int MaxLength
        {
            get
            {
                int temp = 0;
                if (Tip != Objects.Type.SELECT || Tip != Objects.Type.FILEUPLOAD || Tip != Objects.Type.CHECKBOX)
                {
                    temp = maxLength;
                }
                return temp;
            }
        }

        public string Label
        {
            get { return label; }
        }
        public string Class
        {
            get { return className; }
        }
        public string PlaceHolder
        {
            get
            {
                string temp = "";
                if (Tip != Objects.Type.SELECT || Tip != Objects.Type.FILEUPLOAD || Tip != Objects.Type.CHECKBOX)
                {
                    temp = placeHolder;
                }
                return temp;
            }
        }
        ///<summary>ASP.NET taglarını referans ederek UIControl nesnelerine erişerek nesneler oluşturabilirsiniz.
        ///Örnek: Response.Write(getUI(tablo, sql)) veya Literal1.Text = getUI(tablo, sql) daha fazlası için dökümana bakınız.</summary>
        ///<param name="tablo">Tablonun UIControl nesnelerine erişmek için bu alan girilmek zorundadır. </param>
        ///<param name="sql">Tabloyu veritabanından doldurmak istiyorsanız bir sql giriniz eğer doldurmak istemiyorsanız bu alanı boş bırakabilirsiniz.</param>
        ///<param name="section">UIControl alanlarının oluşturulması için gereken bölüm ismi bu bölüm UIControl özelliklerinde sections parametresinde verdiğiniz bir değeri içerir. (Bu alan doldurulmak zorundadır.)</param>
        public static string getUI(Type tablo, String sql, String section, params object[] sqlparametreler)
        {
            String html = "";
            try
            {
                DataSet ds = null;
                if (sql != "")
                {
                    string prov = Config.provider.ToString();
                    if (prov == "MSSQL")
                    {
                        ds = WebFramework.DataSetFill(sql, sqlparametreler);
                    }
                    else if (prov == "MYSQL")
                    {
                        ds = MySQLDb.WebFramework.DataSetFill(sql, sqlparametreler);
                    }
                    else if (prov == "ACCESS")
                    {
                        ds = AccessDb.WebFramework.DataSetFill(sql, sqlparametreler);
                    }
                    else if (prov == "ORACLE")
                    {
                        ds = OracleDb.WebFramework.DataSetFill(sql, sqlparametreler);
                    }
                }
                PropertyInfo[] props = tablo.GetProperties();
                string propName = "", label = "", tip = "", sinif = "", placeholder = "", selectSQL = "";
                int maks = 0;
                Boolean zorunlu = false;
                foreach (PropertyInfo prop in props)
                {
                    object[] attrs = prop.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        UIController fieldlar = attr as UIController;
                        if (fieldlar != null)
                        {

                            try
                            {
                                propName = prop.Name;
                                zorunlu = fieldlar.Required;
                                label = fieldlar.Label;
                                tip = fieldlar.Tip.ToString();
                                sinif = fieldlar.Class.ToString();
                                maks = fieldlar.MaxLength;
                                placeholder = fieldlar.PlaceHolder;
                                selectSQL = fieldlar.SQL;
                            }
                            catch
                            {
                                //Boş field olsa dahi çalışmaya devam edecektir.
                            }
                            string[] sections = fieldlar.Sections.Trim().Split(',');

                            for (int s = 0; s < sections.Length; s++)
                            {

                                if (sections[s].Trim() == section)
                                {
                                    if (label != "")
                                    {
                                        if (tip != "HIDDEN")
                                        {
                                            html += "<div class='control controlLabel' id='" + Utils.ClearTurkishLetter(section) + "_lbl_" + propName + "'><label for='" + propName + "'>" + label + "</label></div> ";
                                        }
                                    }
                                    if (tip == "TEXTBOX")
                                    {
                                        if (sql != "")
                                        {
                                            if (ds.Tables[0].Rows.Count > 0)
                                            {
                                                html += "<div class='control controlInput' id='" + Utils.ClearTurkishLetter(section) + "_text_" + propName + "'><input id='" + propName + "' name='" + propName + "' class='" + sinif + "' type='text' value='" + ds.Tables[0].Rows[0].Field<Object>(propName) + "'";
                                            }
                                            else
                                            {
                                                html += "<div class='control controlInput' id='" + Utils.ClearTurkishLetter(section) + "_text_" + propName + "'><input id='" + propName + "' name='" + propName + "' class='" + sinif + "' type='text' value=''";
                                            }
                                        }
                                        else
                                        {
                                            html += "<div class='control controlInput' id='" + Utils.ClearTurkishLetter(section) + "_text_" + propName + "'><input id='" + propName + "' name='" + propName + "' class='" + sinif + "' type='text'";
                                        }
                                        if (zorunlu == true)
                                        {
                                            html += " required='required' ";
                                        }
                                        if (maks > 0)
                                        {
                                            html += " maxlength='" + maks + "' ";
                                        }
                                        if (placeholder != "")
                                        {
                                            html += " placeholder='" + placeholder + "' ";
                                        }
                                        html += "/></div>";
                                    }
                                    else if (tip == "CHECKBOX")
                                    {
                                        html += "<div class='control controlInput' id='" + Utils.ClearTurkishLetter(section) + "_checkbox_" + propName + "'><input id='" + propName + "' name='" + propName + "' class='" + sinif + "' type='checkbox' value='" + fieldlar.value + "'";
                                        if (sql != "")
                                        {
                                            if (ds.Tables[0].Rows.Count > 0)
                                            {
                                                if (ds.Tables[0].Rows[0].Field<Object>(propName) != null)
                                                {
                                                    if (ds.Tables[0].Rows[0].Field<Object>(propName).ToString() == "E" ||
                                                   ds.Tables[0].Rows[0].Field<Object>(propName).ToString() == "Y" ||
                                                   ds.Tables[0].Rows[0].Field<Object>(propName).ToString() == "1" ||
                                                   ds.Tables[0].Rows[0].Field<Object>(propName).ToString() == "true" ||
                                                   ds.Tables[0].Rows[0].Field<Object>(propName).ToString() == "True")
                                                    {
                                                        html += " value='" + ds.Tables[0].Rows[0].Field<Object>(propName) + "' checked='checked' ";
                                                    }
                                                }
                                            }
                                        }
                                        if (zorunlu == true)
                                        {
                                            html += " required='required' ";
                                        }
                                        html += "/></div>";
                                    }
                                    else if (tip == "TEXTAREA")
                                    {
                                        html += "<div class='control controlTextArea' id='" + Utils.ClearTurkishLetter(section) + "_textarea_" + propName + "'><textarea id='" + propName + "' name='" + propName + "' class='" + sinif + "'";
                                        if (zorunlu == true)
                                        {
                                            html += " required='required' ";
                                        }
                                        if (maks > 0)
                                        {
                                            html += " maxlength='" + maks + "' ";
                                        }
                                        if (placeholder != "")
                                        {
                                            html += " placeholder='" + placeholder + "' ";
                                        }
                                        if (sql != "")
                                        {
                                            if (ds.Tables[0].Rows.Count > 0)
                                            {
                                                html += ">" + ds.Tables[0].Rows[0].Field<Object>(propName) + "</textarea></div>";
                                            }
                                            else
                                            {
                                                html += "></textarea></div>";
                                            }
                                        }
                                        else
                                        {
                                            html += "></textarea></div>";
                                        }
                                    }
                                    else if (tip == "SELECT")
                                    {
                                        html += "<div class='control controlSelect' id='" + Utils.ClearTurkishLetter(section) + "_select_" + propName + "'><select id='" + propName + "' name='" + propName + "' class='" + sinif + "'";
                                        if (zorunlu == true)
                                        {
                                            html += "required='required'";
                                        }
                                        if (selectSQL != "")
                                        {
                                            DataSet dsSelect = null;
                                            string prov = Config.provider.ToString();
                                            if (prov == "MSSQL")
                                            {
                                                dsSelect = WebFramework.DataSetFill(selectSQL, sqlparametreler);
                                            }
                                            else if (prov == "MYSQL")
                                            {
                                                dsSelect = MySQLDb.WebFramework.DataSetFill(selectSQL, sqlparametreler);
                                            }
                                            else if (prov == "ACCESS")
                                            {
                                                dsSelect = AccessDb.WebFramework.DataSetFill(selectSQL, sqlparametreler);
                                            }
                                            else if (prov == "ORACLE")
                                            {
                                                dsSelect = OracleDb.WebFramework.DataSetFill(selectSQL, sqlparametreler);
                                            }
                                            if (dsSelect.Tables[0].Rows.Count > 0)
                                            {
                                                html += ">";
                                                for (int i = 0; i < dsSelect.Tables[0].Rows.Count; i++)
                                                {
                                                    if (sql == "" || sql == null)
                                                    {
                                                        html += "<option value='" + dsSelect.Tables[0].Rows[i].Field<Object>(fieldlar.value) + "'>" + dsSelect.Tables[0].Rows[i].Field<Object>(fieldlar.displayValue) + "</option>";
                                                    }
                                                    else
                                                    {
                                                        if (ds.Tables[0].Rows.Count > 0)
                                                        {
                                                            if (ds.Tables[0].Rows[0].Field<Object>(propName).ToString() == dsSelect.Tables[0].Rows[i].Field<Object>(fieldlar.value).ToString())
                                                            {
                                                                html += "<option selected value='" + dsSelect.Tables[0].Rows[i].Field<Object>(fieldlar.value) + "'>" + dsSelect.Tables[0].Rows[i].Field<Object>(fieldlar.displayValue) + "</option>";
                                                            }
                                                            else
                                                            {
                                                                html += "<option value='" + dsSelect.Tables[0].Rows[i].Field<Object>(fieldlar.value) + "'>" + dsSelect.Tables[0].Rows[i].Field<Object>(fieldlar.displayValue) + "</option>";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            html += "<option value='" + dsSelect.Tables[0].Rows[i].Field<Object>(fieldlar.value) + "'>" + dsSelect.Tables[0].Rows[i].Field<Object>(fieldlar.displayValue) + "</option>";
                                                        }
                                                    }
                                                }
                                                html += "</select></div>";
                                            }
                                            else
                                            {
                                                html += "></select></div>";
                                            }
                                        }
                                        else
                                        {
                                            html += "></select></div>";
                                        }

                                    }
                                    else if (tip == "FILEUPLOAD")
                                    {
                                        html += "<div class='control controlInput' id='" + Utils.ClearTurkishLetter(section) + "_fileupload_" + propName + "'><input id='" + propName + "' name='" + propName + "' class='" + sinif + "' type='file'";
                                        if (zorunlu == true)
                                        {
                                            html += "required='required'";
                                        }
                                        html += "/></div>";
                                    }
                                    else if (tip == "PASSWORD")
                                    {
                                        html += "<div class='control controlInput' id='" + Utils.ClearTurkishLetter(section) + "_pass_" + propName + "'><input id='" + propName + "' name='" + propName + "' class='" + sinif + "' type='password'";
                                        if (zorunlu == true)
                                        {
                                            html += "required='required'";
                                        }
                                        if (maks > 0)
                                        {
                                            html += "maxlength='" + maks + "'";
                                        }
                                        if (placeholder != "")
                                        {
                                            html += "placeholder='" + placeholder + "'";
                                        }
                                        html += "/></div>";
                                    }
                                    else if (tip == "HIDDEN")
                                    {
                                        if (sql != "")
                                        {
                                            if (ds.Tables[0].Rows.Count > 0)
                                            {
                                                html += "<input id=\"" + propName + "\" name=\"" + propName + "\" value=\"" + ds.Tables[0].Rows[0].Field<Object>(propName) + "\"";
                                            }
                                            else
                                            {
                                                html += "<input id=\"" + propName + "\" name=\"" + propName + "\" value=\"\" ";
                                            }
                                        }
                                        else
                                        {
                                            html += "<input id='" + propName + "' name='" + propName + "' ";
                                        }
                                        if (zorunlu == true)
                                        {
                                            html += " required=\"required\" ";
                                        }
                                        html += " type=\"hidden\" />";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception hata)
            {
                html += "<p style='color:red;'><strong>Hata:</strong> " + hata.Message + "</p>";
            }
            return html;
        }
    }
}
