using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EFTranslatable.Extensions
{
    public static class QueryableExtensions
    {
        private static MethodInfo _localeExtract;

        private static MethodInfo LocaleExtract => _localeExtract ??= typeof(Translatable)
            .GetMethod("LocaleExtract", BindingFlags.NonPublic | BindingFlags.Static);

        public static IQueryable<TSource> WhereLocalizedEquals<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, Translatable>> property,
            string equalsTo,
            string locale = null)
        {
            var expr = Expression.MakeBinary(ExpressionType.Equal,
                Expression.Call(LocaleExtract!, Expression.Convert(property.Body, typeof(string)),
                    Expression.Constant($"$.{locale ?? Translatable.DefaultLocale}")),
                Expression.Constant(equalsTo));


            return source.Where(Expression.Lambda<Func<TSource, bool>>(expr, false, property.Parameters));
        }


        private static MethodInfo _localeContains;
        private static MethodInfo LocaleContains => _localeContains ??= typeof(string)
            .GetMethod("Contains", new[] { typeof(string), });
        public static IQueryable<TSource> WhereLocalizedContains<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, Translatable>> property,
            string contains,
            string locale = null)
        {
            var containsExpr = Expression.Constant(contains);
            var convertedBody = Expression.Convert(property.Body, typeof(string));

            if (locale == null)
            {
                var expr = Expression.Call(convertedBody, LocaleContains!, containsExpr);

                return source.Where(Expression.Lambda<Func<TSource, bool>>(expr, false, property.Parameters));
            }

            var expr2 = Expression.Call(
                Expression.Call(LocaleExtract!, convertedBody, Expression.Constant($"$.{locale}")),
                LocaleContains!, containsExpr);

            return source.Where(Expression.Lambda<Func<TSource, bool>>(expr2, false, property.Parameters));
        }
    }
}