using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Gaussians.DataConverters
{
    internal class ValidationRuleWithConverter : ValidationRule
    {
        public ValidationRuleWithConverter() { }
        public ValidationRuleWithConverter(IValueConverter converter)
        {
            Converter = converter;
        }
        public IValueConverter Converter { get; set; }
        public object Parameter { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                Converter.ConvertBack(value, value.GetType(), Parameter, cultureInfo);
                return new ValidationResult(true, null);
            }
            catch (Exception)
            {

                return new ValidationResult(false, null);
            }
        }
    }
}
