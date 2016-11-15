using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Data.Entity;
using MVCWeb.Model.DBContext;

namespace MVCWeb.DataSvc.Base
{

    /// <summary>
    /// 实体基础操作接口（依赖注入时提供调用接口）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseDataSvc<T> where T : class
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="entity"></param>
        void Add(T entity);

        /// <summary>
        /// 添加实体集合
        /// </summary>
        /// <param name="entityList"></param>
        void AddList(IEnumerable<T> entityList);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity"></param>
        void Update(T entity);

        /// <summary>
        /// 更新实体集合
        /// </summary>
        /// <param name="entityList"></param>
        void UpdateList(IEnumerable<T> entityList);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity"></param>
        void DeleteByID(params object[] entityID);

        /// <summary>
        /// 删除符合条件的实体
        /// </summary>
        /// <param name="where"></param>
        void DeleteByCondition(Expression<Func<T, bool>> where);

        /// <summary>
        /// 查询指定ID实体
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        T GetByID(params object[] entityID);

        /// <summary>
        /// 查询符合条件的实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IEnumerable<T> GetByCondition(Expression<Func<T, bool>> where);

        /// <summary>
        /// 查询所有实体
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// 分页查询实体
        /// </summary>
        /// <typeparam name="TOrder">定义泛型：排序表达式返回类型</typeparam>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="where">查询条件</param>
        /// <param name="order">排序表达式</param>
        /// <param name="orderDesc">是否降序</param>
        /// <param name="totalCount">总数</param>
        /// <returns></returns>
        IEnumerable<T> GetPagedEntitys<TOrder>(ref int pageNumber, int pageSize, Expression<Func<T, bool>> where, Expression<Func<T, TOrder>> order, bool orderDesc, out int totalCount);
    }

    /// <summary>
    /// 实体基础操作实现，抽象类可定义抽象方法，虚方法实现接口可重写，被继承后BLL层直接获得基础操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseDataSvc<T> : IBaseDataSvc<T> where T : class
    {
        /*
        属性注入在构造函数执行之后
        切记属性注入时构造函数不要调用被注入属性
        */
        public MyDBContext MyDBContext { get; set; }

        //实体T的实体集
        private IDbSet<T> EntitySet
        {
            get
            {
                return MyDBContext.Set<T>();
            }
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Add(T entity)
        {
            EntitySet.Add(entity);
            MyDBContext.SaveChanges();
        }

        /// <summary>
        /// 添加实体集合
        /// </summary>
        /// <param name="entityList"></param>
        public virtual void AddList(IEnumerable<T> entityList)
        {
            foreach (T entity in entityList)
            {
                EntitySet.Add(entity);
            }
            MyDBContext.SaveChanges();
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Update(T entity)
        {
            EntitySet.Attach(entity);
            MyDBContext.Entry(entity).State = EntityState.Modified;
            MyDBContext.SaveChanges();
        }

        /// <summary>
        /// 更新实体集合
        /// </summary>
        /// <param name="entityList"></param>
        public virtual void UpdateList(IEnumerable<T> entityList)
        {
            foreach (T entity in entityList)
            {
                EntitySet.Attach(entity);
                MyDBContext.Entry(entity).State = EntityState.Modified;
            }
            MyDBContext.SaveChanges();
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity"></param>
        public virtual void DeleteByID(params object[] entityID)
        {
            EntitySet.Remove(EntitySet.Find(entityID));
            MyDBContext.SaveChanges();
        }

        /// <summary>
        /// 删除符合条件的实体
        /// </summary>
        /// <param name="where"></param>
        public virtual void DeleteByCondition(Expression<Func<T, bool>> where)
        {
            IEnumerable<T> entityList = EntitySet.Where(where);
            foreach (T entity in entityList)
            {
                EntitySet.Remove(entity);
            }
            MyDBContext.SaveChanges();
        }

        /// <summary>
        /// 查询指定ID实体
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        public virtual T GetByID(params object[] entityID)
        {
            return EntitySet.Find(entityID);
        }

        /// <summary>
        /// 查询符合条件的实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetByCondition(Expression<Func<T, bool>> where)
        {
            return EntitySet.Where(where).ToList();
        }

        /// <summary>
        /// 查询所有实体
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAll()
        {
            return EntitySet.ToList();
        }

        /// <summary>
        /// 分页查询实体
        /// </summary>
        /// <typeparam name="TOrder">定义泛型：排序表达式返回类型</typeparam>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="where">查询条件</param>
        /// <param name="order">排序表达式</param>
        /// <param name="orderDesc">是否降序</param>
        /// <param name="totalCount">总数</param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetPagedEntitys<TOrder>(ref int pageNumber, int pageSize, Expression<Func<T, bool>> where, Expression<Func<T, TOrder>> order, bool orderDesc, out int totalCount)
        {
            totalCount = EntitySet.Where(where).Count();
            if (totalCount > 0 && (pageNumber - 1) * pageSize >= totalCount)
            {
                pageNumber = totalCount % pageSize == 0 ? totalCount / pageSize : totalCount / pageSize + 1;
            }
            if (orderDesc)//是否降序
            {
                return EntitySet.Where(where).OrderByDescending(order).Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            else
            {
                return EntitySet.Where(where).OrderBy(order).Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
        }
    }
}
