// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

#if !WINDOWS_UWP
using Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// DataGridLengthConverter - Converter class for converting instances of other types to and from DataGridLength instances.
    /// </summary>
    /// <QualityBand>Mature</QualityBand>
    public class DataGridLengthConverter : TypeConverter
    {
        /// <summary>
        /// Checks whether or not this class can convert from a given type.
        /// </summary>
        /// <param name="context">
        /// An ITypeDescriptorContext that provides a format context.
        /// </param>
        /// <param name="sourceType">The Type being queried for support.</param>
        /// <returns>
        /// <c>true</c> if this converter can convert from the provided type,
        /// <c>false</c> otherwise.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            // We can only handle strings, integral and floating types
            TypeCode tc = Type.GetTypeCode(sourceType);
            switch (tc)
            {
                case TypeCode.String:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks whether or not this class can convert to a given type.
        /// </summary>
        /// <param name="context">
        /// An ITypeDescriptorContext that provides a format context.
        /// </param>
        /// <param name="destinationType">The Type being queried for support.</param>
        /// <returns>
        /// <c>true</c> if this converter can convert to the provided type,
        /// <c>false</c> otherwise.
        /// </returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        /// <summary>
        /// Attempts to convert to a DataGridLength from the given object.
        /// </summary>
        /// <param name="context">
        /// An ITypeDescriptorContext that provides a format context.
        /// </param>
        /// <param name="culture">
        /// The CultureInfo to use for the conversion.
        /// </param>
        /// <param name="value">The object to convert to a GridLength.</param>
        /// <returns>
        /// The GridLength instance which was constructed.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// A NotSupportedException is thrown if the example object is null.
        /// </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return DataGridLength.ConvertFrom(culture, value);
        }

        /// <summary>
        /// Attempts to convert a DataGridLength instance to the given type.
        /// </summary>
        /// <param name="context">
        /// An ITypeDescriptorContext that provides a format context.
        /// </param>
        /// <param name="culture">
        /// The CultureInfo to use for the conversion.
        /// </param>
        /// <param name="value">The DataGridLength to convert.</param>
        /// <param name="destinationType">The type to which to convert the DataGridLength instance.</param>
        /// <returns>
        /// The object which was constructed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// An ArgumentNullException is thrown if the example object is null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// A NotSupportedException is thrown if the object is not null and is not a DataGridLength,
        /// or if the destinationType isn't one of the valid destination types.
        /// </exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType != typeof(string))
            {
                throw DataGridError.DataGridLengthConverter.CannotConvertTo(destinationType.ToString());
            }

            DataGridLength? dataGridLength = value as DataGridLength?;
            if (!dataGridLength.HasValue)
            {
                throw DataGridError.DataGridLengthConverter.InvalidDataGridLength("value");
            }

            return DataGridLength.ConvertToString(culture, dataGridLength.Value);
        }
    }
}

#endif
