using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
[assembly: TagPrefix("SelectControl", "sy")]
namespace SYuksel
{
    [Localizable(true)]
    [Bindable(true)]
    [Browsable(true)]
    [DefaultProperty("")]
    [ThemeableAttribute(true)]
    [ToolboxData("<{0}:SelectControl runat=\"server\" Class=\"\" ID=\"\" DisplayValue=\"\" Value=\"\" SelectedValue=\"\" SQL=\"\"  SQLParams=\"\"></{0}:SelectControl>")]
    public class SelectControl : WebControl , IAttributeAccessor
    {
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        private string _class = null;
        public string Class
        {
            get { return "selectcontrol " + _class; }
            set { _class = value; }
        }
        private string _sql = "";
        public string SQL
        {
            get { return _sql; }
            set { _sql = value; }
        }

        private object[] _sqlparams = new object[Config.sqlparamsSize];
        public  object[] SQLParams
        {
            get { return _sqlparams; }
            set { _sqlparams = value; }
        }

        private string _value = "";
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        private string _displayvalue = "";
        public string DisplayValue
        {
            get { return _displayvalue; }
            set { _displayvalue = value; }
        }
        private string _selectedvalue = "";
        public string SelectedValue
        {
            get { return _selectedvalue; }
            set { _selectedvalue = value; }
        }
        private Boolean _required = false;
        public Boolean Required
        {
            get { return _required; }
            set { _required = value; }
        }


        protected override void RenderContents(HtmlTextWriter output)
        {
            String html = "";
           // output.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
            if (Class != null)
            {
                output.AddAttribute("class", this.Class);
            }
            if (Required == true)
            {
                output.AddAttribute("required", "required");
            }
            try
            {
                DataSet ds = null;
                if (SQL != "")
                {
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
                            if(SelectedValue == ds.Tables[0].Rows[i].Field<Object>(Value).ToString())
                            {
                                html += "<option selected value='" + ds.Tables[0].Rows[i].Field<Object>(Value) + "'>" + ds.Tables[0].Rows[i].Field<Object>(DisplayValue) + "</option>";
                            }
                            else
                            {
                                html += "<option value='" + ds.Tables[0].Rows[i].Field<Object>(Value) + "'>" + ds.Tables[0].Rows[i].Field<Object>(DisplayValue) + "</option>";
                            }
                        }
                    }
                }
                AddAttributesToRender(output);
                output.RenderBeginTag(HtmlTextWriterTag.Select);
                
            }
            catch(Exception hata)
            {
                output.RenderBeginTag("div");
                html = "<p style='color:red;'><strong>Hata:</strong> " + hata.Message + "</p>";
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
