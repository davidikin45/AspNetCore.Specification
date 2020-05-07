using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Specification.Data
{
    public class SpecificationDbQuery<TEntity> where TEntity : class
    {
        private readonly DbContext _context;

        private IncludeSpecification<TEntity> _includeSpecification = IncludeSpecification<TEntity>.Nothing;
        private FilterSpecification<TEntity> _filterSpecification = FilterSpecification<TEntity>.All;
        private OrderBySpecification<TEntity> _orderBySpecification = OrderBySpecification<TEntity>.Nothing;

        private int? _skip;
        private int? _take;
        private bool _fullgraph;
        private bool? _tracking;

        public SpecificationDbQuery(DbContext context)
        {
            _context = context;
        }

        public static SpecificationDbQuery<TEntity> Create(DbContext context)
        {
            return new SpecificationDbQuery<TEntity>(context);
        }

        public SpecificationDbQuery<TEntity> Include(IncludeSpecification<TEntity> includeSpecification)
        {
            _includeSpecification = _includeSpecification.And(includeSpecification);
            return this;
        }

        public SpecificationDbQuery<TEntity> Where(FilterSpecification<TEntity> whereSpecification)
        {
            _filterSpecification = _filterSpecification.And(whereSpecification);
            return this;
        }

        public SpecificationDbQuery<TEntity> OrderBy(OrderBySpecification<TEntity> orderBySpecification)
        {
            _orderBySpecification = _orderBySpecification.ThenBy(orderBySpecification);
            return this;
        }

        public SpecificationDbQuery<TEntity> Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public SpecificationDbQuery<TEntity> Take(int take)
        {
            _take = take;
            return this;
        }

        public SpecificationDbQuery<TEntity> Page(int page, int pageSize)
        {
            _skip = (page - 1) * pageSize;
            _take = pageSize;
            return this;
        }

        public SpecificationDbQuery<TEntity> FullGraph()
        {
            _fullgraph = true;
            return this;
        }

        public SpecificationDbQuery<TEntity> AsTracking()
        {
            _tracking = true;
            return this;
        }

        public SpecificationDbQuery<TEntity> AsNoTracking()
        {
            _tracking = false;
            return this;
        }

        public override string ToString()
        {
            var query = GetQueryable(_filterSpecification.ToExpression(), _orderBySpecification.ToExpression().Compile(), null, null, _fullgraph, _includeSpecification.ToExpression());
            return query.ToString();
        }

        public string ToQueryString() => ToString();

        public List<TEntity> ToList()
        {
            return GetQueryable(_filterSpecification.ToExpression(), _orderBySpecification.ToExpression().Compile(), null, null, _fullgraph, _includeSpecification.ToExpression()).ToList();
        }

        public Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken)
        {
            return GetQueryable(_filterSpecification.ToExpression(), _orderBySpecification.ToExpression().Compile(), null, null, _fullgraph, _includeSpecification.ToExpression()).ToListAsync(cancellationToken);
        }

        public CountList<TEntity> ToCountList()
        {
            var query = GetQueryable(_filterSpecification.ToExpression(), _orderBySpecification.ToExpression().Compile(), null, null, _fullgraph, _includeSpecification.ToExpression());
            return CountList<TEntity>.Create(query, _skip, _take);
        }

        public Task<CountList<TEntity>> ToCountListAsync(CancellationToken cancellationToken)
        {
            var query = GetQueryable(_filterSpecification.ToExpression(), _orderBySpecification.ToExpression().Compile(), null, null, _fullgraph, _includeSpecification.ToExpression());
            return CountList<TEntity>.CreateAsync(query, _skip, _take, cancellationToken);
        }

        public PagedList<TEntity> ToPagedList()
        {
            var query = GetQueryable(_filterSpecification.ToExpression(), _orderBySpecification.ToExpression().Compile(), null, null, _fullgraph, _includeSpecification.ToExpression());
            return PagedList<TEntity>.Create(query, _skip, _take);
        }

        public Task<PagedList<TEntity>> ToPagedListAsync(CancellationToken cancellationToken)
        {
            var query = GetQueryable(_filterSpecification.ToExpression(), _orderBySpecification.ToExpression().Compile(), null, null, _fullgraph, _includeSpecification.ToExpression());
            return PagedList<TEntity>.CreateAsync(query, _skip, _take, cancellationToken);
        }

        private IQueryable<TEntity> GetQueryable(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           int? skip = null,
           int? take = null,
           bool getFullGraph = false,
           params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            //includeProperties = includeProperties ?? string.Empty;
            IQueryable<TEntity> query = _context.Set<TEntity>();

            //By default use DbContext tracking
            if(_tracking.HasValue)
            {
                if (!_tracking.Value)
                {
                    query = query.AsNoTracking();
                }
                else
                {
                    //By default tracking is QueryTrackingBehavior.TrackAll. If the DbContext is set to QueryTrackingBehavior.NoTracking we don't want to allow a user to override this behaviour.
                    query = query.AsTracking();
                }
            }

            //where clause
            if (filter != null)
                query = query.Where(filter);

            //include
            if (getFullGraph)
            {
                var includesList = GetFullGraphIncludes(typeof(TEntity));

                foreach (var include in includesList)
                {
                    query = query.Include(include);
                }
            }
            else
            {
                if (includeProperties != null && includeProperties.Count() > 0)
                {
                    foreach (var includeExpression in includeProperties)
                    {
                        query = query.Include(includeExpression);
                    }
                }
            }

            //order by
            if (orderBy != null)
                query = orderBy(query);

            //skip
            if (skip.HasValue)
                query = query.Skip(skip.Value);

            //take
            if (take.HasValue)
                query = query.Take(take.Value);

            //.ToQueryString() or .ToString()
            DebugSQL(query);

            return query;
        }

        private void DebugSQL(IQueryable<TEntity> query)
        {
            var sql = query.ToString();
        }

        private List<string> GetFullGraphIncludes(Type type, int maxDepth = 10)
        {
            return GetAllCompositionAndAggregationRelationshipPropertyIncludes(false, type, null, 0, maxDepth);
        }

        // Association = Composition (Doesn't exist without parent) or Aggregation (Exists without parent)
        private List<string> GetAllCompositionAndAggregationRelationshipPropertyIncludes(bool compositionRelationshipsOnly, Type type, string path = null, int depth = 0, int maxDepth = 5)
        {
            List<string> includesList = new List<string>();
            if (depth > maxDepth)
            {
                return includesList;
            }

            List<Type> excludeTypes = new List<Type>()
            {
                typeof(DateTime),
                typeof(String),
                typeof(byte[])
           };

            IEnumerable<PropertyInfo> properties = type.GetProperties().Where(p =>
            //Ignore value types
            p.CanWrite && !p.PropertyType.IsValueType && !excludeTypes.Contains(p.PropertyType)
            &&
            (
                //One way traversal.
                (p.PropertyType.IsCollection() && type != p.PropertyType.GetGenericArguments().First())
                ||
                //Link to another Aggregate.
                (!p.PropertyType.IsCollection() && type != p.PropertyType && !compositionRelationshipsOnly)
                )
            ).ToList();

            foreach (var p in properties)
            {
                var includePath = !string.IsNullOrWhiteSpace(path) ? path + "." + p.Name : p.Name;

                includesList.Add(includePath);

                Type propType = null;
                if (p.PropertyType.IsCollection())
                {
                    propType = type.GetGenericArguments(p.Name).First();
                }
                else
                {
                    propType = p.PropertyType;
                }

                includesList.AddRange(GetAllCompositionAndAggregationRelationshipPropertyIncludes(compositionRelationshipsOnly, propType, includePath, depth + 1, maxDepth));
            }

            return includesList;
        }
    }
}
