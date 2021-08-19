using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace EFTranslatable.Extensions
{
    /// <summary>
    /// Add Translatable/Localizations Methods to IQueryable
    /// </summary>
    public static class QueryableExtensions
    {
        private static MethodInfo _localeExtract;

        private static MethodInfo LocaleExtract => _localeExtract ??= typeof(Translatable)
            .GetMethod("LocaleExtract", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Filter the current EntitySet where the giving property-predicate Equals to the giving String
        /// </summary>
        /// <param name="source">DbSet</param>
        /// <param name="property">The predicate to select a Translatable property</param>
        /// <param name="equalsTo">The giving String to compare property to</param>
        /// <param name="locale">The locale where the predicate will run on, If null the current Thread locale will be used instead</param>
        /// <typeparam name="TSource">EntityFrameWork Model</typeparam>
        /// <returns>IQueryable</returns>
        public static IQueryable<TSource> WhereLocalizedEquals<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, Translatable>> property,
            string equalsTo,
            string locale = null)
        {
            var l = locale
                    ?? Translatable.FallbackLocale
                    ?? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

            var expr = Expression.MakeBinary(ExpressionType.Equal,
                Expression.Call(LocaleExtract!, Expression.Convert(property.Body, typeof(string)),
                    Expression.Constant($"$.{l}")),
                Expression.Constant(equalsTo));


            return source.Where(Expression.Lambda<Func<TSource, bool>>(expr, false, property.Parameters));
        }


        private static MethodInfo _localeContains;
        private static MethodInfo LocaleContains => _localeContains ??= typeof(string)
            .GetMethod("Contains", new[] { typeof(string), });
        /// <summary>
        /// Filter the current EntitySet where the giving property-predicate Contains to the giving String
        /// </summary>
        /// <param name="source">DbSet</param>
        /// <param name="property">The predicate to select a Translatable property</param>
        /// <param name="contains">The giving String to check if property contains it</param>
        /// <param name="locale">The locale where the predicate will run on, If null the current Thread locale will be used instead</param>
        /// <typeparam name="TSource">EntityFrameWork Model</typeparam>
        /// <returns>IQueryable</returns>
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