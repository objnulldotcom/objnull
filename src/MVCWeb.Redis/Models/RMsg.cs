using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Models
{
    public class RMsg
    {
        /// <summary>
        /// 对象类型：EnumObjectType
        /// </summary>
        public int ObjType { get; set; }

        public Guid ObjID { get; set; }

        public string Title { get; set; }

        public string From { get; set; }

        public int COrder { get; set; }

        public int ROrder { get; set; }

        public DateTime Date { get; set; }
    }
}
