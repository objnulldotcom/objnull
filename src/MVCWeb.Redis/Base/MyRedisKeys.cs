using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Base
{
    public static class MyRedisKeys
    {
        public const string ActionRules = "ActionRules";
        public const string Managers = "Managers";
        public const string Pre_NewBCMsg = "NewBCMsg";
        public const string Pre_NewBCRMsg = "NewBCRMsg";
        public const string Pre_BlogDraft = "BlogDraft";
        public const string Pre_UserRecord = "UserRecord";
        public const string Pre_UserStarCache = "UserStarCache";
    }
}
