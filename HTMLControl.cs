using HtmlAgilityPack;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

[assembly: TagPrefix("HTMLControl", "sy")]
namespace SYuksel
{
    [ParseChildren(true, "Content")]
    [PersistChildren(false)]
    [DefaultProperty("")]
    [ToolboxData("<{0}:HTMLControl runat=\"server\" SQL=\"\" SQLParams=\"\" DataSource=\"\" Class=\"\"></{0}:HTMLControl>")]
    public class HTMLControl : WebControl
    {
        private string _sql = "";
        public string SQL
        {
            get { return _sql; }
            set { _sql = value; }
        }
        private object[] _sqlparams = new object[Config.sqlparamsSize];
        public object[] SQLParams
        {
            get { return _sqlparams; }
            set { _sqlparams = value; }
        }
        private String _datasource = null;
        public String DataSource
        {
            get { return _datasource; }
            set { _datasource = value; }
        }
        private string _class = null;
        public string Class
        {
            get { return "htmlcontrol " + _class; }
            set { _class = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        private ITemplate content = null;
        [
        TemplateContainer(typeof(TemplateControl)),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateInstance(TemplateInstance.Single),
        ]
        public ITemplate Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
            }
        }
        private string RenderControl(Control control)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(sb))
            {
                using (HtmlTextWriter textWriter = new HtmlTextWriter(stringWriter))
                {
                    control.RenderControl(textWriter);
                }
            }

            return sb.ToString();
        }
        String html = "";
        String tempHTML = "";
        protected override void RenderContents(HtmlTextWriter output)
        {
            output.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
            if (Class != null)
            {
                output.AddAttribute("class", this.Class);
            }
            output.RenderBeginTag("div");
            output.BeginRender();
            try
            {

                Type tablo = null;
                if (this.Content != null)
                {
                    Control templateContainer = new Control();
                    Content.InstantiateIn(templateContainer);

                    var s = RenderControl(templateContainer);
                    html = s;
                   // HttpContext.Current.Response.Write("<textarea>"+s+"</textarea>");
                }
                try
                {
                    tablo = GetType(DataSource);
                }
                catch
                {
                    //DataSource Attributesi boş ise
                }
                if (SQL != string.Empty)
                {
                    HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                    dokuman.LoadHtml(html);

                    //html = dokuman.DocumentNode.OuterHtml; //Template üzerinde manipule edilen html manipule edilmiş hali ile Content değişkenine atılıyor.
                    //dokuman.LoadHtml(html);
                    PropertyInfo[] props = tablo.GetProperties();
                    DataSet ds = new DataSet();
                    string prov = Config.provider.ToString();
                    if (prov == "MSSQL")
                    {
                        ds = WebFramework.DataSetFill(SQL, SQLParams.ToArray());
                    }
                    else if (prov == "MYSQL")
                    {
                        ds = MySQLDb.WebFramework.DataSetFill(SQL, SQLParams.ToArray());
                    }
                    else if (prov == "ACCESS")
                    {
                        ds = AccessDb.WebFramework.DataSetFill(SQL, SQLParams.ToArray());
                    }
                    else if (prov == "ORACLE")
                    {
                        ds = OracleDb.WebFramework.DataSetFill(SQL, SQLParams.ToArray());
                    }
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            var paragraflar = dokuman.DocumentNode.SelectNodes("//p[@data-name]"); //Paragraf tagları değişkene atılıyor.
                            if (paragraflar != null)
                            {
                                foreach (HtmlNode node in paragraflar) //düğüm null'dan farklı ise döngüye sokuluyor 
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value; //düğümdeki data-name attribute değerini okur. //[@data-name='ADSOYAD']
                                    if (nodeDataName != null || nodeDataName.ToString() != String.Empty) //Eğer data-name null'dan veya boş'dan farklı ise
                                    {
                                        //Model sınıfındaki propertiler döngüye sokulur.
                                        foreach (PropertyInfo prop in props)
                                        {
                                            //Stabil ve doğruluk olması amacıyla model dosyasındaki properti isim ile düğümdeki data-name eşit ise veritabanından veri okulur. 
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name); //tablo örneğinden sütün adının propertisi okunuyor
                                                if (veri != null) //Kayıt null'dan farklı ise
                                                {
                                                    node.InnerHtml += veri.ToString(); //InnerHtml ile tagların arasına ekliyor
                                                    node.Attributes.Remove("data-name"); //Güvenlik amaçlı data-name attirubetesi siliniyor.
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            var inputlar = dokuman.DocumentNode.SelectNodes("//input[@data-name]");
                            if (inputlar != null)
                            {
                                foreach (HtmlNode node in inputlar)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null || nodeDataName.ToString() != String.Empty)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    string nodeType = node.Attributes["type"].Value;
                                                    if (nodeType == "text" || nodeType == "email" || nodeType == "password" || nodeType == "hidden")
                                                    {
                                                        node.SetAttributeValue("value", veri.ToString());
                                                    }
                                                    else if (nodeType == "radio" || nodeType == "checkbox")
                                                    {
                                                        if (node.Attributes["value"].Value == veri.ToString())
                                                        {
                                                            node.SetAttributeValue("checked", "checked");
                                                        }
                                                    }
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            var divler = dokuman.DocumentNode.SelectNodes("//div[@data-name]");
                            if (divler != null)
                            {
                                foreach (HtmlNode node in divler)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null || nodeDataName.ToString() != String.Empty)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.InnerHtml += veri.ToString();
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            var imgs = dokuman.DocumentNode.SelectNodes("//img[@data-name]");
                            if (imgs != null)
                            {
                                foreach (HtmlNode node in imgs)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.SetAttributeValue("src", veri.ToString());
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            var textarealar = dokuman.DocumentNode.SelectNodes("//textarea[@data-name]");
                            if (textarealar != null)
                            {
                                foreach (HtmlNode node in textarealar)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null || nodeDataName.ToString() != String.Empty)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.InnerHtml = veri.ToString();
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }

                                    }
                                }
                            }


                            var lis = dokuman.DocumentNode.SelectNodes("//li[@data-name]");
                            if (lis != null)
                            {
                                foreach (HtmlNode node in lis)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null || nodeDataName.ToString() != String.Empty)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.InnerHtml += veri.ToString();
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            var tds = dokuman.DocumentNode.SelectNodes("//td[@data-name]");
                            if (tds != null)
                            {
                                foreach (HtmlNode node in tds)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null || nodeDataName.ToString() != String.Empty)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.InnerHtml += veri.ToString();
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            var spanlar = dokuman.DocumentNode.SelectNodes("//span[@data-name]");
                            if (spanlar != null)
                            {
                                foreach (HtmlNode node in spanlar)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null || nodeDataName.ToString() != String.Empty)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.InnerHtml += veri.ToString();
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            var sources = dokuman.DocumentNode.SelectNodes("//source[@data-name]");
                            if (sources != null)
                            {
                                foreach (HtmlNode node in sources)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.SetAttributeValue("src", veri.ToString());
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            var iframes = dokuman.DocumentNode.SelectNodes("//iframe[@data-name]");
                            if (iframes != null)
                            {
                                foreach (HtmlNode node in iframes)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.SetAttributeValue("src", veri.ToString());
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            var labels = dokuman.DocumentNode.SelectNodes("//label[@data-name]");
                            if (labels != null)
                            {
                                foreach (HtmlNode node in labels)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.InnerHtml += veri.ToString();
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                           var selects = dokuman.DocumentNode.SelectNodes("//select[@data-name]/option");
                            if (selects != null)
                            {
                                foreach (HtmlNode node in selects)
                                {
                                    object nodeDataName = node.ParentNode.Attributes["data-name"].Value;
                                    if (nodeDataName != null)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    if (node.Attributes["value"].Value == veri.ToString())
                                                    {
                                                        node.SetAttributeValue("selected", "");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            } 

                            //Select optionları seçildikten sonra kalan ve seçilen selectlerin data-name attributeler siliniyor.
                           var selects2 = dokuman.DocumentNode.SelectNodes("//select[@data-name]");
                            if (selects2 != null)
                            {
                                foreach (HtmlNode node in selects2)
                                {
                                    node.Attributes.Remove("data-name");
                                }
                            } 

                            var a = dokuman.DocumentNode.SelectNodes("//a[@data-name]");
                            if (a != null)
                            {
                                foreach (HtmlNode node in a)
                                {
                                    //a tagının hem href hemde yazılacak bir text alanı olduğu için bu iki parametreli alıyor. ÖRNEK: KULLANICI_ID, KULLANICI_ADI
                                    //0.indis = href, 1.indis = text alanı
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    string[] nodeDataNames = nodeDataName.ToString().Trim().Split(','); //Boşluklar temizlenip virgül ile ayrılıyor değerler.

                                    if (nodeDataNames != null)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            try
                                            {
                                                if (nodeDataName.ToString().IndexOf(",") == -1)
                                                {
                                                    if (prop.Name == nodeDataName.ToString())
                                                    {
                                                        Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                        string href = null;
                                                        try
                                                        {
                                                            href = node.Attributes["href"].Value;
                                                        }
                                                        catch
                                                        {
                                                            href = "";
                                                        }
                                                        node.SetAttributeValue("href", href + veri.ToString());
                                                    }
                                                }
                                                else
                                                {
                                                    if (prop.Name == nodeDataNames[1].Trim())
                                                    {
                                                        Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);

                                                        if (veri != null)
                                                        {
                                                            node.InnerHtml += veri.ToString();
                                                        }
                                                    }
                                                    else if (prop.Name == nodeDataNames[0].Trim())
                                                    {
                                                        Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                        string href = null;
                                                        try
                                                        {
                                                            href = node.Attributes["href"].Value;
                                                        }
                                                        catch
                                                        {
                                                            href = "";
                                                        }
                                                        node.SetAttributeValue("href", href + veri.ToString());
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                            }

                                        }
                                    }
                                    node.Attributes.Remove("data-name");
                                }
                            }

                            var headers = dokuman.DocumentNode.SelectNodes("//*[self::h1[@data-name] or self::h2[@data-name] or self::h3[@data-name] or self::h4[@data-name] or self::h5[@data-name] or self::h6[@data-name]]");
                            if (headers != null)
                            {
                                foreach (HtmlNode node in headers)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.InnerHtml += veri.ToString();
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            var progress = dokuman.DocumentNode.SelectNodes("//progress[@data-name]");
                            if (progress != null)
                            {
                                foreach (HtmlNode node in progress)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.SetAttributeValue("value", veri.ToString());
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            var meters = dokuman.DocumentNode.SelectNodes("//meter[@data-name]");
                            if (meters != null)
                            {
                                foreach (HtmlNode node in meters)
                                {
                                    object nodeDataName = node.Attributes["data-name"].Value;
                                    if (nodeDataName != null)
                                    {
                                        foreach (PropertyInfo prop in props)
                                        {
                                            if (prop.Name == nodeDataName.ToString())
                                            {
                                                Object veri = ds.Tables[0].Rows[i].Field<Object>(prop.Name);
                                                if (veri != null)
                                                {
                                                    node.SetAttributeValue("value", veri.ToString());
                                                    node.Attributes.Remove("data-name");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            tempHTML += dokuman.DocumentNode.OuterHtml;
                            dokuman.LoadHtml(html);
                        }
                        ds = new DataSet();
                        ds.Dispose();
                        output.Write(tempHTML);
                        output.RenderEndTag();
                        output.EndRender();
                    }
                    else
                    {
                        //ds kaynağında hiç kayıt yok ise.
                        output.Write(html);
                        output.RenderEndTag();
                        output.EndRender();
                    }

                }
                else
                {
                    //SQL Attributesi boş ise
                    output.Write(html);
                    output.RenderEndTag();
                    output.EndRender();
                }

            }
            catch (Exception hata)
            {
                output.RenderBeginTag("div");
                html = "<p style='color:red;'><strong>Hata:</strong> " + hata + "</p>";
                output.Write(html);
                output.RenderEndTag();
                output.EndRender();
            }
            finally
            {
                tempHTML = "";
                html  = "";
            }

        }
        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

    }
}
