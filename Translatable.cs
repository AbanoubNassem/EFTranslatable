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
    [JsonConverter(typeof(TranslatableJsonConverter))]
    public struct Translatable
    {
        public static string DefaultLocale { get; set; } = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

        public readonly Dictionary<string, string> Translations;
        public string CurrentLocale { get; set; }

        [JsonConstructor]
        public Translatable(string json)
        {
            CurrentLocale = null;
            Translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        [JsonConstructor]
        public Translatable(Dictionary<string, string> translations)
        {
            Translations = translations;
            CurrentLocale = null;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(Translations, new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            });
        }

        public Translatable Set(string value, string locale = null)
        {
            var key = locale ?? CurrentLocale ?? DefaultLocale;

            if (Translations.ContainsKey(key))
                Translations[key] = value;
            else
                Translations.Add(key, value);

            return this;
        }

        public string Get(string locale = null)
        {
            var key = locale ?? CurrentLocale;

            if (string.IsNullOrEmpty(key) || !Translations.ContainsKey(key))
            {
                key = Translations.Keys.FirstOrDefault() ?? DefaultLocale;
            }

            Translations.TryGetValue(key, out var value);

            return value ?? "";
        }

        public override string ToString()
        {
            return Get();
        }


        public static implicit operator string(Translatable v)
        {
            return v.Get();
        }

        public Translatable WithLocale(string locale)
        {
            CurrentLocale = locale;
            return this;
        }

        private static string LocaleExtract(string property, string locale) => throw new NotSupportedException();
    }

    public class TranslatableJsonConverter : JsonConverter<Translatable>
    {
        public override Translatable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new Translatable(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Translatable value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
