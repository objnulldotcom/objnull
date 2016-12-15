using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Models
{
    public class ActionRule
    {
        public ActionRule()
        {
            ID = Guid.NewGuid();
        }

        public Guid ID { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        public int ActionType { get; set; }

        public int[] AllowRoles { get; set; }
    }
}
