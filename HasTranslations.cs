namespace EFTranslatable
{
    public abstract class HasTranslations<T> where T : class, new()
    {
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
