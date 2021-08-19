namespace EFTranslatable
{
    /// <summary>
    /// Give the ability to driven Entity to translate it's self
    /// </summary>
    /// <typeparam name="T">EntityFrameWork Model</typeparam>
    public abstract class HasTranslations<T> where T : class, new()
    {
        /// <summary>
        /// Translate current Entity to the specified locale 
        /// </summary>
        /// <param name="locale">The locale to translate to</param>
        /// <returns>New translated Entity with the giving locale</returns>
        public T Translate(string locale)
        {
            var result = new T();
            var currentType = GetType();

            foreach (var property in currentType.GetProperties())
            {
                if (property.PropertyType == typeof(Translatable))
                {
                    var value = (Translatable)property.GetValue(this)!;
                    var tempLocale = value.CurrentLocale;
                    value.WithLocale(locale);
                    property.SetValue(result, value);
                    value.WithLocale(tempLocale);
                }
                else
                {
                    property.SetValue(result, property.GetValue(this));
                }
            }

            return result;
        }
    }
}
