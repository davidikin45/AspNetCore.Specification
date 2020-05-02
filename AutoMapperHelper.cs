using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetCore.Specification
{
    internal static class AutoMapperHelper
    {
        #region Includes Mapping
        //Expression > Func yes
        //Func > Expression no compiled
        public static Expression<Func<TDestination, Object>>[] MapIncludes<TSource, TDestination>(this IMapper mapper, params Expression<Func<TSource, Object>>[] includes)
        {
            var mappedList = MapIncludes(mapper, typeof(TSource), typeof(TDestination), includes);
            List<Expression<Func<TDestination, Object>>> returnList = new List<Expression<Func<TDestination, Object>>>();

            foreach (var item in mappedList)
            {
                returnList.Add((Expression<Func<TDestination, Object>>)item);
            }

            return returnList.ToArray();
        }

        public static LambdaExpression[] MapIncludes(this IMapper mapper, Type source, Type destination, params LambdaExpression[] includes)
        {
            if (includes == null)
                return new LambdaExpression[] { };

            Expression<Func<bool>> a = () => true;

            List<LambdaExpression> returnList = new List<LambdaExpression>();
            var sourceType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(source, typeof(object)));
            var destinationType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(destination, typeof(object)));

            foreach (var include in includes)
            {
                returnList.Add((LambdaExpression)mapper.Map(include, sourceType, destinationType));
            }

            return returnList.ToArray();
        }
        #endregion

        #region Where Clause Mapping
        public static Expression<Func<TDestination, bool>> MapWhereClause<TSource, TDestination>(this IMapper mapper, Expression<Func<TSource, bool>> whereClause)
        {
            return mapper.Map<Expression<Func<TDestination, bool>>>(whereClause);
        }

        public static LambdaExpression MapWhereClause(this IMapper mapper, Type source, Type destination, LambdaExpression whereClause)
        {
            var sourceType = typeof(Expression).MakeGenericType(typeof(Func<,>).MakeGenericType(source, typeof(bool)));
            var destinationType = typeof(Expression).MakeGenericType(typeof(Func<,>).MakeGenericType(destination, typeof(bool)));
            return (LambdaExpression)mapper.Map(whereClause, sourceType, destinationType);
        }
        #endregion

        #region Order By Mapping
        public static Expression<Func<IQueryable<TDestination>, IOrderedQueryable<TDestination>>> MapOrderBy<TSource, TDestination>(this IMapper mapper, Expression<Func<IQueryable<TSource>, IOrderedQueryable<TSource>>> orderBy)
        {
            if (orderBy == null)
                return null;

            return mapper.Map<Expression<Func<IQueryable<TDestination>, IOrderedQueryable<TDestination>>>>(orderBy);
        }
        #endregion
    }
}
