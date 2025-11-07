using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MonitorApi.Services
{
    /// <summary>
    /// 通用CRUD服务接口
    /// </summary>
    /// <typeparam name="TEntity">实体类型（需继承自BaseEntity）</typeparam>
    public interface IGenericService<TEntity> where TEntity : class
    {
        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="saveNow">是否立即保存（默认true）</param>
        Task AddAsync(TEntity entity, bool saveNow = true);

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <param name="saveNow">是否立即保存</param>
        Task DeleteAsync(int id, bool saveNow = true);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="saveNow">是否立即保存</param>
        Task DeleteAsync(TEntity entity, bool saveNow = true);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="saveNow">是否立即保存</param>
        Task UpdateAsync(TEntity entity, bool saveNow = true);

        /// <summary>
        /// 根据ID查询实体
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <param name="includeProperties">需要包含的导航属性（如"ModbusConfig,Records"）</param>
        Task<TEntity> GetByIdAsync(int id, string includeProperties = "");

        /// <summary>
        /// 查询所有实体
        /// </summary>
        /// <param name="includeProperties">需要包含的导航属性</param>
        Task<List<TEntity>> GetAllAsync(string includeProperties = "");

        /// <summary>
        /// 条件查询实体
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <param name="includeProperties">需要包含的导航属性</param>
        Task<List<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate, string includeProperties = "");

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="includeProperties">需要包含的导航属性</param>
        Task<(List<TEntity> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, object>> orderBy,
            bool isAscending = true,
            string includeProperties = "");

        /// <summary>
        /// 保存变更（用于saveNow=false的场景）
        /// </summary>
        Task SaveChangesAsync();
    }
}
