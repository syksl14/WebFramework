using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SYuksel
{
    public static class Objects
    {
        [ComVisibleAttribute(true)]
        [Flags]
        public enum Type
        {
            ///<summary>HTML5 input olan tipi text nesnesini oluşturur.</summary>
            TEXTBOX,
            ///<summary>HTML5 select nesnesini oluşturur. İçeriğini doldurmak için SQL sorgusu, 
            ///displayValue ve Value parametrelerini doldurmanız gerekiyor.
            /// </summary>
            SELECT,
            ///<summary>HTML5 checkbox nesnesini oluşturur. İşaretin çalışması için veritabanından
            ///dönen bilgi Booelan (True, False), int(1, 0) veya string('E','Y', 'H', 'N', NULL) şeklinde olmalıdır.
            /// </summary>
            CHECKBOX,
            ///<summary>HTML5 textarea nesnesini oluşturur.</summary>
            TEXTAREA,
            ///<summary>HTML5 fileupload nesnesini oluşturur.</summary>
            FILEUPLOAD,
            ///<summary>HTML5 input olan tipi password nesnesini oluşturur.</summary>
            PASSWORD,
            ///<summary>HTML5 input olan tipi hidden nesnesini oluşturur.</summary>
            HIDDEN
        }
        [ComVisibleAttribute(true)]
        [Flags]
        public enum Provider
        {
            ///<summary>sy:XXXControl nesnelerin bağlantısı için Microsoft SQL Server bağlantısını sağlar.</summary>
            MSSQL,
            ///<summary>sy:XXXControl nesnelerin bağlantısı için Oracle MySQL bağlantısını sağlar.</summary>
            MYSQL,
            ///<summary>sy:XXXControl nesnelerin bağlantısı için Microsoft Office Access bağlantısını sağlar.</summary>
            ACCESS,
            ///<summary>sy:XXXControl nesnelerin bağlantısı için Oracle Client bağlantısını sağlar.</summary>
            ORACLE
        }
      
    }
}
