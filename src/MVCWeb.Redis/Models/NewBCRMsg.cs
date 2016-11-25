using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Models
{
    public class NewBCRMsg
    {
        public Guid BlogID { get; set; }

        public string Title { get; set; }

        public string From { get; set; }

        public int COrder { get; set; }

        public int ROrder { get; set; }

        public DateTime Date { get; set; }
    }
}
