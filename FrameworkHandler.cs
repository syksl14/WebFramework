using System;
using System.Web;
using System.IO;
using System.Web.SessionState;
using System.Text.RegularExpressions;

namespace SYuksel
{
    public class FrameworkHandler : IHttpHandler, IRequiresSessionState, IHttpModule
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members
        /*
         $.ajax({
        type: "POST",
        url: "FrameworkHandler.ashx",
        //data ile parametre belirleyebiliyoruz.
        //querystring mantığı diyebiliriz.
        data: "",
        success: function(msg){

        //dönen sonuç json tipinde olmadığı için
        //direk olarak alıyoruz ve kullanıyoruz.
        $("body").html(msg);
        }
        });*/
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            System.Threading.Thread.Sleep(1000);
            try
            {
                if (File.Exists(HttpContext.Current.Server.MapPath("~/Layouts/" + context.Request["control"] + ".json")))
                {
                    File.Delete(HttpContext.Current.Server.MapPath("~/Layouts/" + context.Request["control"] + ".json"));
                }
                StreamWriter sw = File.AppendText(HttpContext.Current.Server.MapPath("~/Layouts/" + context.Request["control"] + ".json"));
                sw.WriteLine(context.Request["layout"]);
                sw.Flush();
                sw.Close();
                context.Response.Write("Değişiklikler başarıyla kaydedildi!");
            }
            catch (Exception hata)
            {
                context.Response.Write("Hata: " + hata.Message);
            }
        }
        public static void DebugLogger(string mesaj)
        {
            try
            {
                if (File.Exists("C:/Logger/" + DateTime.Now.ToString("dd/MM/yyyy") + ".txt"))
                {
                    Logger(mesaj);
                }
                else
                {
                    if (!Directory.Exists("C:/Logger"))
                    {
                        Directory.CreateDirectory("C:/Logger");
                    }
                    StreamWriter fs = new StreamWriter(@"C:/Logger/" + DateTime.Now.ToString("dd/MM/yyyy") + ".txt");
                    fs.Write("");
                    fs.Close();
                    Logger(mesaj);
                }
            }
            catch
            {
            }
        }
        private static void Logger(string mesaj)
        {
            using (StreamWriter w = File.AppendText("C:/Logger/" + DateTime.Now.ToString("dd/MM/yyyy") + ".txt"))
            {
                w.WriteLine(DateTime.Now + " " + HttpContext.Current.Request.Url + " >> " + mesaj);
                w.Close();
            }
        }

        #endregion

        void IHttpModule.Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        void IHttpModule.Dispose()
        {
            // Nothing to dispose; 
        }
        private static string getResponseWrite()
        {
            return "<script>setTimeout(function(){ "
      + "try{document.getElementsByName('WebFramework').item(0).remove();}catch(msg){}var headID = document.getElementsByTagName('head')[0];   "
     + "var tag = document.createElement('meta');"
     + "tag.name = 'WebFramework';"
     + "tag.content = 'http://framework.selahattinyuksel.net';"
     + "headID.appendChild(tag);"
       + "}, 2000);</script>";
        }
        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            foreach (string url in Config.minifyPagesUrl)
            {
                if (app.Request.RawUrl.Contains(url) || app.Request.RawUrl.Contains(".html") || app.Request.RawUrl.Contains(".htm") || app.Request.RawUrl.Contains(".aspx"))
                {
                    if (app.Request.RawUrl.Contains(".jpg") || app.Request.RawUrl.Contains(".png") ||
                        app.Request.RawUrl.Contains(".jpeg") || app.Request.RawUrl.Contains(".gif") ||
                        app.Request.RawUrl.Contains(".css") || app.Request.RawUrl.Contains("scripts") 
                        || app.Request.RawUrl.Contains(".js"))
                    {
                      
                    }
                    else
                    {
                        app.Response.Filter = new WhitespaceFilter(app.Response.Filter);
                    }
                }
            }
        }


        private class WhitespaceFilter : Stream
        {

            public WhitespaceFilter(Stream sink)
            {
                _sink = sink;
            }

            private Stream _sink;
            private static Regex reg = new Regex(@"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n\t])\s{2,}");

            #region Properites

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {
                _sink.Flush();
            }

            public override long Length
            {
                get { return 0; }
            }

            private long _position;
            public override long Position
            {
                get { return _position; }
                set { _position = value; }
            }

            #endregion

            #region Methods

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _sink.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _sink.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _sink.SetLength(value);
            }

            public override void Close()
            {
                _sink.Close();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                byte[] data = new byte[count];
                Buffer.BlockCopy(buffer, offset, data, 0, count);
                string html = System.Text.Encoding.Default.GetString(buffer);
                html = reg.Replace(html, String.Empty);
                byte[] outdata = System.Text.Encoding.Default.GetBytes(html);
                _sink.Write(outdata, 0, outdata.GetLength(0));
            }

            #endregion

        }
    }
}