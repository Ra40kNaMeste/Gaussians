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
    internal interface IValidationConverter : IValueConverter
    {
        public bool CanValidation(object value, CultureInfo cultureInfo);
    }
    internal class ValidationRuleWithConverter : ValidationRule
    {
        public ValidationRuleWithConverter() { }
        public ValidationRuleWithConverter(IValidationConverter converter)
        {
            Converter = converter;
        }
        public IValidationConverter Converter { get; set; }
        public object Parameter { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
                return new ValidationResult(Converter.CanValidation(value, cultureInfo), null);
        }
    }
}
