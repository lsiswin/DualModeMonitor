using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MonitorApi.Services
{
    /// <summary>
    /// 通用CRUD服务实现
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    public class GenericService<TEntity> : IGenericService<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;

        /// <summary>
        /// 构造函数注入DbContext
        /// </summary>
        public GenericService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<TEntity>();
        }

        /// <summary>
        /// 新增实体
        /// </summary>
        public async Task AddAsync(TEntity entity, bool saveNow = true)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "新增的实体不能为null");

            await _dbSet.AddAsync(entity);
            if (saveNow)
                await SaveChangesAsync();
        }

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        public async Task DeleteAsync(int id, bool saveNow = true)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"未找到ID为{id}的实体");

            await DeleteAsync(entity, saveNow);
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        public async Task DeleteAsync(TEntity entity, bool saveNow = true)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "删除的实体不能为null");

            _dbSet.Remove(entity);
            if (saveNow)
                await SaveChangesAsync();
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        public async Task UpdateAsync(TEntity entity, bool saveNow = true)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "更新的实体不能为null");

            _dbSet.Update(entity);
            if (saveNow)
                await SaveChangesAsync();
        }

        /// <summary>
        /// 根据ID查询实体（支持导航属性）
        /// </summary>
        public async Task<TEntity> GetByIdAsync(int id, string includeProperties = "")
        {
            var query = _dbSet.AsQueryable();

            // 包含导航属性（如"ModbusConfig,Records"）
            query = IncludeProperties(query, includeProperties);

            return await query.FirstOrDefaultAsync(CreateIdPredicate(id));
        }

        /// <summary>
        /// 查询所有实体
        /// </summary>
        public async Task<List<TEntity>> GetAllAsync(string includeProperties = "")
        {
            var query = _dbSet.AsQueryable();
            query = IncludeProperties(query, includeProperties);

            return await query.ToListAsync();
        }

        /// <summary>
        /// 条件查询实体
        /// </summary>
        public async Task<List<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate, string includeProperties = "")
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate), "查询条件不能为null");

            var query = _dbSet.Where(predicate);
            query = IncludeProperties(query, includeProperties);

            return await query.ToListAsync();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public async Task<(List<TEntity> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, object>> orderBy,
            bool isAscending = true,
            string includeProperties = "")
        {
            if (pageIndex < 1)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "页码必须大于等于1");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "每页条数必须大于等于1");
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if (orderBy == null)
                throw new ArgumentNullException(nameof(orderBy));

            var query = _dbSet.Where(predicate);
            query = IncludeProperties(query, includeProperties);

            // 计算总条数
            var totalCount = await query.CountAsync();

            // 排序和分页
            if (isAscending)
                query = query.OrderBy(orderBy);
            else
                query = query.OrderByDescending(orderBy);

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// 保存变更
        /// </summary>
        public async Task SaveChangesAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // 处理数据库更新异常（如外键约束、唯一索引冲突）
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"数据保存失败：{innerMsg}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"保存变更时发生错误：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 动态包含导航属性
        /// </summary>
        private IQueryable<TEntity> IncludeProperties(IQueryable<TEntity> query, string includeProperties)
        {
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property.Trim());
                }
            }
            return query;
        }

        /// <summary>
        /// 创建ID查询条件（默认实体主键为int类型的Id）
        /// </summary>
        private Expression<Func<TEntity, bool>> CreateIdPredicate(int id)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Property(parameter, "Id");
            var constant = Expression.Constant(id);
            var equals = Expression.Equal(property, constant);
            return Expression.Lambda<Func<TEntity, bool>>(equals, parameter);
        }
    }
}
