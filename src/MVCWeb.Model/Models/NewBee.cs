using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVCWeb.Model.Models
{
    public class NewBee
    {
        public NewBee()
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
        /// 评论数
        /// </summary>
        public int FloorCount { get; set; }

        /// <summary>
        /// 查看数
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// 置顶
        /// </summary>
        public bool Top { get; set; }

        /// <summary>
        /// 最后盖楼日期
        /// </summary>
        public DateTime LastFloorDate { get; set; }

        /// <summary>
        /// 发布人ID
        /// </summary>
        public Guid OwnerID { get; set; }

        /// <summary>
        /// 发布人
        /// </summary>
        public virtual NullUser Owner { get; set; }

        /// <summary>
        /// 楼层集合
        /// </summary>
        public virtual ICollection<NewBeeFloor> NewBeeFloors { get; set; }
    }
}
