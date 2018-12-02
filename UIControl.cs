using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
[assembly: TagPrefix("UIControl", "sy")]
namespace SYuksel
{
    [ToolboxBitmap(typeof(UIControl), "UIControl.bmp")]
    [ParseChildren(true, "Content")]
    [PersistChildren(false)]
    [DefaultProperty("Content")]
    [ToolboxData("<{0}:UIControl runat=\"server\" Section=\"\" DataSource=\"\" Action=\"\" Method=\"\" SQLParams=\"\"> </{0}:UIControl>")]
    public partial class UIControl : WebControl
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
        private String _datasource = null;
        public String DataSource
        {
            get { return _datasource; }
            set { _datasource = value; }
        }

        private string _class = null;
        public string Class
        {
            get { return "uicontrol " + _class; }
            set { _class = value; }
        }
        private string _action = null;
        public string Action
        {
            get { return _action; }
            set { _action = value; }
        }
        private string _method = null;
        public string Method
        {
            get { return _method; }
            set { _method = value; }
        }
        private Boolean _resetButton = false;
        public Boolean ResetButton
        {
            get { return _resetButton; }
            set { _resetButton = value; }
        }
        private Boolean _submitButton = false;
        public Boolean SubmitButton
        {
            get { return _submitButton; }
            set { _submitButton = value; }
        }
        private string _submitbuttonvalue = "Gönder";
        public string SubmitButtonValue
        {
            get { return _submitbuttonvalue; }
            set { _submitbuttonvalue = value; }
        }
        private string _resetbuttonvalue = "Temizle";
        public string ResetButtonValue
        {
            get { return _resetbuttonvalue; }
            set { _resetbuttonvalue = value; }
        }
        private string _submitbuttonclass = "";
        public string SubmitButtonClass
        {
            get { return _submitbuttonclass; }
            set { _submitbuttonclass = value; }
        }
        private string _resetbuttonclass = "";
        public string ResetButtonClass
        {
            get { return _resetbuttonclass; }
            set { _resetbuttonclass = value; }
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
        private string _section = "";
        public string Section
        {
            get { return _section; }
            set { _section = value; }
        }
        String html = "";
        protected override void RenderContents(HtmlTextWriter output)
        {
            output.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
            if (Class != null)
            {
                output.AddAttribute("class", this.Class);
            }

            if (Action != null)
            {
                output.AddAttribute("action", Action);
            }
            if (Method != null)
            {
                output.AddAttribute("method", Method);
            }

            output.RenderBeginTag("form");
            Type tablo = GetType(DataSource);

            html = UIController.getUI(tablo, SQL, Section, SQLParams.ToArray());

            if (ResetButton == true)
            {
                html += "<div class='control controlButton' id='reset_" + this.ID + "'><button type='reset'  class='" + ResetButtonClass + "'>" + ResetButtonValue + "</button></div>";
            }
            if (SubmitButton == true)
            {
                html += "<div class='control controlButton' id='submit_" + this.ID + "'><button type='submit'  class='" + SubmitButtonClass + "'>" + SubmitButtonValue + "</button></div>";
            }
            String content = "";
            if (this.Content != null)
            {
                Control templateContainer = new Control();
                Content.InstantiateIn(templateContainer);

                content = RenderControl(templateContainer);
            }
            output.Write(html + content);
            output.RenderEndTag();
        }
        protected override void Render(HtmlTextWriter writer)
        {
            this.RenderContents(writer);

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
