using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVCWeb.Model.Models
{
    public class NewBeeFloor
    {
        public NewBeeFloor()
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
        /// 原始内容
        /// </summary>
        public string MDText { get; set; }

        /// <summary>
        /// MD html
        /// </summary>
        public string MDValue { get; set; }
        
        /// <summary>
        /// 回复数
        /// </summary>
        public int ReplyCount { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 垃圾评论
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// 发布人ID
        /// </summary>
        public Guid OwnerID { get; set; }

        /// <summary>
        /// 所属NewBeeID
        /// </summary>
        public Guid NewBeeID { get; set; }

        /// <summary>
        /// 发布人
        /// </summary>
        public virtual NullUser Owner { get; set; }

        /// <summary>
        /// 所属NewBee
        /// </summary>
        public virtual NewBee NewBee { get; set; }

        /// <summary>
        /// 楼层回复集合
        /// </summary>
        public virtual ICollection<NewBeeFloorReply> NewBeeFloorReplys { get; set; }
    }
}
