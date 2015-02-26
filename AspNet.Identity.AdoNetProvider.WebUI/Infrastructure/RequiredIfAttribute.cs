using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic;
using System.Web.Mvc;

namespace AspNet.Identity.AdoNetProvider.WebUI.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RequiredIfAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly string _condition;
        private readonly string _propertyName;

        public RequiredIfAttribute(string condition, string propertyName)
        {
            _condition = condition;
            _propertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var conditionFunction = CreateExpression(validationContext.ObjectType, _condition);
            var conditionMet = (bool)conditionFunction.DynamicInvoke(validationContext.ObjectInstance);

            if (!conditionMet)
            {
                return ValidationResult.Success;
            }

            return value == null ? new ValidationResult(FormatErrorMessage(null)) : ValidationResult.Success;
        }

        private static Delegate CreateExpression(Type objectType, string expression)
        {
            var lambdaExpression = DynamicExpression.ParseLambda(objectType, typeof(bool), expression);
            var function = lambdaExpression.Compile();

            return function;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var modelClientValidationRule = new ModelClientValidationRule
            {
                ValidationType = "requiredif",
                ErrorMessage = ErrorMessage
            };

            modelClientValidationRule.ValidationParameters.Add("param", _propertyName);

            return new List<ModelClientValidationRule>
            {
                modelClientValidationRule
            };
        }
    }
}