namespace DynamicPropertyValueReader
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    #endregion

    /// <summary>
    ///     The dynamic property value reader.
    /// </summary>
    public static class DynamicPropertyValueReader
    {
        #region Constants

        /// <summary>
        ///     The closing square brace.
        /// </summary>
        private const string ClosingSquareBrace = @"]";

        /// <summary>
        ///     The equal comparison operator.
        /// </summary>
        private const string EqualComparisonOperator = @"==";

        /// <summary>
        ///     The match dictionary.
        /// </summary>
        private const string MatchDictionary = @"\[([^]]*)\]";

        /// <summary>
        ///     The opening square brace.
        /// </summary>
        private const string OpeningSquareBrace = @"[";

        /// <summary>
        ///     The value of operator.
        /// </summary>
        private const string ValueOfOperator = @".";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The get property value.
        /// </summary>
        /// <param name="parsedValue">
        ///     The from object.
        /// </param>
        /// <param name="propertyName">
        ///     The property name.
        /// </param>
        /// <returns>
        ///     The <see cref="object" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Invalid argument
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        ///     Object key not found
        /// </exception>
        public static dynamic GetPropertyValue(object parsedValue, string propertyName)
        {
            if (null == parsedValue)
            {
                return null;
            }

            if (propertyName == null)
            {
                return null;
            }

            string formattedPropertyName = propertyName,
                   dictionaryValue = string.Empty,
                   listPropertyName = string.Empty,
                   listValueToCompare = string.Empty,
                   listPropertyValueField = string.Empty;

            if (Regex.Match(formattedPropertyName, MatchDictionary).Success)
            {
                dictionaryValue = PreparePropertyFormatString(
                    ref formattedPropertyName,
                    ref listPropertyName,
                    ref listValueToCompare,
                    ref listPropertyValueField);
            }

            var objectType = parsedValue.GetType();
            var propInfo = objectType.GetProperty(formattedPropertyName);
            if ((propInfo == null) && propertyName.Contains(ValueOfOperator))
            {
                var firstProp = propertyName.Substring(
                    0,
                    propertyName.IndexOf(ValueOfOperator, StringComparison.OrdinalIgnoreCase));
                propInfo = objectType.GetProperty(firstProp);
                if (propInfo == null)
                {
                    throw new ArgumentException("propInfo");
                }

                return GetPropertyValue(
                    propInfo.GetValue(parsedValue, null),
                    propertyName.Substring(
                        propertyName.IndexOf(ValueOfOperator, StringComparison.OrdinalIgnoreCase) + 1));
            }

            if (propInfo != null)
            {
                return ScanPropertyValue(
                    parsedValue,
                    propInfo,
                    listPropertyName,
                    listValueToCompare,
                    listPropertyValueField,
                    dictionaryValue);
            }

            throw new KeyNotFoundException("property");
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The prepare property format string.
        /// </summary>
        /// <param name="formattedPropertyName">
        ///     The formatted property name.
        /// </param>
        /// <param name="listPropertyName">
        ///     The list property name.
        /// </param>
        /// <param name="listValueToCompare">
        ///     The list value to compare.
        /// </param>
        /// <param name="listPropertyValueField">
        ///     The list property value field.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        private static string PreparePropertyFormatString(
            ref string formattedPropertyName,
            ref string listPropertyName,
            ref string listValueToCompare,
            ref string listPropertyValueField)
        {
            var dictionaryValue = Regex.Match(formattedPropertyName, MatchDictionary).Groups[1].Value;
            if (dictionaryValue.Contains(EqualComparisonOperator))
            {
                listPropertyName = dictionaryValue.Substring(
                    0,
                    dictionaryValue.IndexOf(EqualComparisonOperator, StringComparison.OrdinalIgnoreCase));
                listValueToCompare =
                    dictionaryValue.Substring(
                        dictionaryValue.IndexOf(EqualComparisonOperator, StringComparison.OrdinalIgnoreCase) + 2);
                listPropertyValueField =
                    formattedPropertyName.Substring(
                        formattedPropertyName.IndexOf(ClosingSquareBrace, StringComparison.OrdinalIgnoreCase) + 2);
                formattedPropertyName =
                    formattedPropertyName.Replace(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}{1}{2}{3}{4}",
                            OpeningSquareBrace,
                            dictionaryValue,
                            ClosingSquareBrace,
                            ValueOfOperator,
                            listPropertyValueField),
                        string.Empty);
            }
            else
            {
                formattedPropertyName =
                    formattedPropertyName.Replace(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}{1}{2}",
                            OpeningSquareBrace,
                            dictionaryValue,
                            ClosingSquareBrace),
                        string.Empty);
            }

            return dictionaryValue;
        }

        /// <summary>
        ///     The scan property value.
        /// </summary>
        /// <param name="fromObject">
        ///     The from object.
        /// </param>
        /// <param name="propInfo">
        ///     The prop info.
        /// </param>
        /// <param name="listPropertyName">
        ///     The list property name.
        /// </param>
        /// <param name="listValueToCompare">
        ///     The list value to compare.
        /// </param>
        /// <param name="listPropertyValueField">
        ///     The list property value field.
        /// </param>
        /// <param name="dictionaryValue">
        ///     The dictionary value.
        /// </param>
        /// <returns>
        ///     The <see cref="object" />.
        /// </returns>
        private static object ScanPropertyValue(
            object fromObject,
            PropertyInfo propInfo,
            string listPropertyName,
            string listValueToCompare,
            string listPropertyValueField,
            string dictionaryValue)
        {
            var value = propInfo.GetValue(fromObject, null);
            if (!string.IsNullOrWhiteSpace(listPropertyName) && !string.IsNullOrWhiteSpace(listValueToCompare)
                && !string.IsNullOrWhiteSpace(listPropertyValueField))
            {
                var dictionaryToScan = value as IEnumerable<dynamic>;
                if (dictionaryToScan != null)
                {
                    var matchProperties =
                        dictionaryToScan.Where(
                            element =>
                                element.GetType().GetProperty(listPropertyName).GetValue(element, null)
                                == listValueToCompare);
                    var matchValue =
                        matchProperties.Select(
                            matchProperty =>
                                matchProperty.GetType()
                                    .GetProperty(listPropertyValueField)
                                    .GetValue(matchProperty, null)).ToList();
                    return matchValue.Count() == 1 ? matchValue.FirstOrDefault() : matchValue;
                }

                return null;
            }

            return !string.IsNullOrWhiteSpace(dictionaryValue) ? (value as dynamic)[dictionaryValue] : value;
        }

        #endregion
    }
}