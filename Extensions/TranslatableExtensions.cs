using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;


namespace EFTranslatable.Extensions
{
    /// <summary>
    /// Add the required Database functions to the current ModelBuilder
    /// </summary>
    public static class TranslatableExtensions
    {
        /// <summary>
        /// Extend the ModelBuilder to allow the Translatable functionalists
        /// </summary>
        /// <param name="modelBuilder">The current EntityFrameWork ModelBuilder being used</param>
        /// <param name="context">The current EntityFrameWork DbContext being used</param>
        /// <param name="fallbackLocale">A fallback locale to use if the current Locale has no translations</param>
        /// <returns>ModelBuilder</returns>
        public static ModelBuilder WithTranslatable(this ModelBuilder modelBuilder, DbContext context, string fallbackLocale = null)
        {
            if (fallbackLocale != null)
            {
                Translatable.FallbackLocale = fallbackLocale;
            }

            modelBuilder.HasDbFunction(typeof(Translatable).GetMethod("LocaleExtract", BindingFlags.NonPublic | BindingFlags.Static)!)
                .HasTranslation((args) =>
                {
                    return new SqlFunctionExpression(
                      context.Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer" ? "JSON_Value" : "JSON_Extract",
                        args, nullable: true,
                        argumentsPropagateNullability: new[] { false, false }, typeof(string), null
                        );
                });


            var converter = new ValueConverter<Translatable, string>(
               v => v.ToJson(),
               v => new Translatable(v)
           );

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var mBuilder = modelBuilder.Entity(entityType.Name);

                var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(Translatable));
                foreach (var property in properties)
                {
                    mBuilder.Property(property.Name).HasConversion
                    (
                        converter
                        , new ValueComparer<Translatable>(
                            (first, second) => first.Translations.SequenceEqual(second.Translations),
                            c => c.Translations.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => new Translatable(c.Translations.ToDictionary(_ => _.Key, _ => _.Value))
                        )
                    );
                }
            }

            return modelBuilder;
        }
    }
}
