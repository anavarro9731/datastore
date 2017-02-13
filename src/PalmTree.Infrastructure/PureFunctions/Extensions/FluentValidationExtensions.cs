using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;

namespace PalmTree.Infrastructure.PureFunctions.Extensions
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> MaxLength<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, int maxLength)
        {
            return ruleBuilder.SetValidator(new MaxLengthValidator(maxLength));
        }

        public static IRuleBuilderOptions<T, TProperty> In<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder,
            IEnumerable<TProperty> enumerable)
        {
            return ruleBuilder.SetValidator(new InValidator<TProperty>(enumerable));
        }
    }

    public class MaxLengthValidator : PropertyValidator
    {
        public MaxLengthValidator(int maxLength) : base("{PropertyName} must contain less than " + maxLength + " characters")
        {
            MaxLength = maxLength;
        }

        public int MaxLength { get; }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var propValue = context.PropertyValue as string;
            if (string.IsNullOrEmpty(propValue) || propValue.Length > MaxLength) return false;
            return true;
        }
    }

    public class InValidator<T> : PropertyValidator
    {
        public InValidator(IEnumerable<T> enumerable)
            : base("Property {PropertyName} not in the specified enumerable.")
        {
            Enumerable = enumerable;

            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable", "Enumerable should not be null.");
            }
        }

        public IEnumerable<T> Enumerable { get; }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var item = (T) context.PropertyValue;
            return Enumerable.Contains(item);
        }
    }
}