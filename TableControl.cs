using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
[assembly: TagPrefix("SelectControl", "sy")]
namespace SYuksel
{
    [BindableAttribute(true)]
    [BrowsableAttribute(true)]
    [ParseChildren(true, "Content")]
    [PersistChildren(true)]
    [DefaultProperty("Content")]
    [ToolboxData("<{0}:TableControl runat=\"server\" Class=\"\" Columns=\"\" DisplayFields=\"\" SQL=\"\" SQLParams=\"\"></{0}:TableControl>")]
    public class TableControl : WebControl
    {
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
        private string _class = null;
        public string Class
        {
            get { return "tablecontrol " + _class; }
            set { _class = value; }
        }
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
        private string _columns;
        public string Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }
      
        private string _displayfields;
        public string DisplayFields
        {
            get { return _displayfields; }
            set { _displayfields = value; }
        }
        private string _primarykey = "";
        public string PrimaryKey
        {
            get { return _primarykey; }
            set { _primarykey = value; }
        }
        private string _contentheadertitle;
        public string ContentHeaderTitle
        {
            get { return _contentheadertitle; }
            set { _contentheadertitle = value; }
        }
        protected override void RenderContents(HtmlTextWriter output)
        {
            String html = "";
            output.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
            if (Class != null)
            {
                output.AddAttribute("class", this.Class);
            }

            try
            {
                DataSet ds = null;
                if (SQL != "")
                {
                    string[] kolonlar = Columns.Split(',');
                    string[] fieldlar = DisplayFields.Split(',');
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
                        html += "<thead><tr>";
                        for (int i = 0; i < fieldlar.Length; i++)
                        {
                            html += "<td>" + fieldlar[i].Trim() + "</td>";
                        }
                        String content = "";
                        if (this.Content != null)
                        {
                            Control templateContainer = new Control();
                            Content.InstantiateIn(templateContainer);

                            content = RenderControl(templateContainer);
                        }
                        if (content != "")
                        {
                            html += "<td class='tablecontrol_content'>" + ContentHeaderTitle + "</td>";
                        }
                        html += "</tr></thead>";
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (PrimaryKey != "")
                            {
                              
                                html += "<tr data-id='" + ds.Tables[0].Rows[i].Field<Object>(PrimaryKey) + "'>";
                            }
                            else
                            {
                                html += "<tr>";
                            }
                            for (int j = 0; j < kolonlar.Length; j++)
                            {
                                html += "<td>" + ds.Tables[0].Rows[i].Field<Object>(kolonlar[j].Trim()) + "</td>";
                            }
                            if (content != "")
                            {
                                html += "<td>" + content + "</td>";
                            }
                            html += "</tr>";
                        }
                    }
                }
                output.RenderBeginTag("table");
            }
            catch (Exception hata)
            {
                output.RenderBeginTag("div");
                html += "<p style='color:red;'><strong>Hata:</strong> " + hata.Message + "</p>";
                
            }
            output.Write(html);
            output.RenderEndTag();
        }
        protected override void Render(HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }

    }
}
