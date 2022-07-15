using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Common
{
    public static class EnumHelper
    {
        public static string Description(this Enum value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var attributes = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);


            if (attributes.Any())
            {
                return (attributes.First() as DescriptionAttribute).Description;
            }

            // If no description is found, the least we can do is replace underscores with spaces
            // You can add your own custom default formatting logic here
            var ti = CultureInfo.CurrentCulture.TextInfo;

            return ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ", StringComparison.InvariantCulture)));
        }

        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (!type.IsEnum)
            {
                throw new ArgumentException($"{nameof(type)} must be an enum type", nameof(type));
            }

            return GetAllValues(type)
                .Select(e => new ValueDescription(e, e.Description()))
                .ToList();
        }

        public static IList<Enum> GetAllValues(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (!type.IsEnum)
            {
                throw new ArgumentException($"{nameof(type)} must be an enum type", nameof(type));
            }

            return Enum.GetValues(type)
                .Cast<Enum>()
                .ToList();
        }
    }
}
