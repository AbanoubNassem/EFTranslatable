using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;

namespace EFTranslatable
{
    /// <summary>
    /// Translatable is a type which allow the Entity property Translatable 
    /// </summary>
    [JsonConverter(typeof(TranslatableJsonConverter))]
    public struct Translatable
    {
        /// <summary>
        /// A fallback locale to use if the current Locale has no translations
        /// </summary>
        public static string FallbackLocale { get; set; }

        /// <summary>
        /// The current Locales with it's Translations
        /// </summary>
        public readonly Dictionary<string, string> Translations;
        /// <summary>
        /// The current locale the Property is using
        /// </summary>
        public string CurrentLocale { get; set; }

        /// <summary>
        /// A constructor to build Translatable out of JSON string
        /// </summary>
        /// <param name="json">The giving JSON string</param>
        [JsonConstructor]
        public Translatable(string json)
        {
            CurrentLocale = null;
            Translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        /// <summary>
        /// A constructor to build/copy Translatable out of an other Translatable
        /// </summary>
        /// <param name="translations">The giving Locales with it's Translations</param>
        [JsonConstructor]
        public Translatable(Dictionary<string, string> translations)
        {
            Translations = translations;
            CurrentLocale = null;
        }

        /// <summary>
        /// Convert the current Translations to JSON
        /// </summary>
        /// <returns>JSON String</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(Translations, new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            });
        }

        /// <summary>
        /// Set/Update the giving locale with the giving translation
        /// </summary>
        /// <param name="value">the giving translation</param>
        /// <param name="locale">
        ///         the giving locale, If null the CurrentLocale will be used,
        ///         If CurrentLocale is null The current Thread locale will be used instead
        /// </param>
        /// <returns>Translatable</returns>
        public Translatable Set(string value, string locale = null)
        {
            var key = locale
                      ?? CurrentLocale
                      ?? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

            if (Translations.ContainsKey(key))
                Translations[key] = value;
            else
                Translations.Add(key, value);

            return this;
        }

        /// <summary>
        /// Returns the Value of the used locale , If null the current Thread locale will be used instead
        /// </summary>
        /// <param name="locale">
        ///     the giving locale to get it's value, If null the CurrentLocale will be used,
        ///     If CurrentLocale is null The current Thread locale will be used instead,
        ///     If no Translations found the FallbackLocale will be used
        ///     , otherwise the First Translation found, will be used instead
        /// </param>
        /// <returns></returns>
        public string Get(string locale = null)
        {
            var key = locale ?? CurrentLocale ?? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

            if (string.IsNullOrEmpty(key) || !Translations.ContainsKey(key))
            {
                key = FallbackLocale ?? Translations.Keys.FirstOrDefault() ?? "";
            }

            Translations.TryGetValue(key, out var value);

            return value ?? "";
        }

        /// <inheritdoc />
        public override string ToString() => Get();


        /// <summary>
        /// Convert the current Translatable to String
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static implicit operator string(Translatable v)
        {
            return v.Get();
        }

        /// <summary>
        /// Set the CurrentLocale and Chain 
        /// </summary>
        /// <param name="locale">The locale to be used</param>
        /// <returns>Translatable</returns>
        public Translatable WithLocale(string locale)
        {
            CurrentLocale = locale;
            return this;
        }

        private static string LocaleExtract(string property, string locale) => throw new NotSupportedException();
    }

    /// <inheritdoc />
    public class TranslatableJsonConverter : JsonConverter<Translatable>
    {
        /// <inheritdoc />
        public override Translatable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new Translatable(reader.GetString());
        }
        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Translatable value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
