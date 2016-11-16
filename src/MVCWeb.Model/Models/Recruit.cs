using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVCWeb.Model.Models
{
    public class Recruit
    {
        public Recruit()
        {
            ID = Guid.NewGuid();
            InsertDate = DateTime.Now;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public Guid ID { get; set; }
        
        /// <summary>
        /// 添加日期
        /// </summary>
        public DateTime InsertDate { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 原始内容
        /// </summary>
        public string MDText { get; set; }

        /// <summary>
        /// html内容
        /// </summary>
        public string MDValue { get; set; }

        /// <summary>
        /// 发布人ID
        /// </summary>
        public Guid OwnerID { get; set; }

        /// <summary>
        /// 发布人ID
        /// </summary>
        public virtual NullUser Owner { get; set; }
    }
}
