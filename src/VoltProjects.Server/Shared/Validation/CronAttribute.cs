using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Cronos;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VoltProjects.Server.Shared.Validation;

public sealed class CronAttribute : ValidationAttribute, IClientModelValidator
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        string cronValue = (string)value;
        
        bool expression = CronExpression.TryParse(cronValue, CronFormat.IncludeSeconds, out _);
        return !expression ? new ValidationResult("Must be a valid cron format!") : ValidationResult.Success;
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-cron", "Must be a valid cron format!");
    }
    
    private void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (attributes.ContainsKey(key))
        {
            return;
        }

        attributes.Add(key, value);
    }
}