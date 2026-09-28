namespace KiteKey.Core.Attributes;

using System.Reflection;
using System.Text;

/// <summary>Perform default (convention-based) validation across all properties.</summary>
[AttributeUsage(AttributeTargets.Class)]
public class ValidatedAttribute : ValidationAttribute
{
    private readonly NullabilityInfoContext nullabilityContext = new();

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value is null
            ? new ValidationResult($"{validationContext.DisplayName} is required")
            : this.ValidateProperties(value, validationContext);
    }

    private static void AppendMessages(HashSet<string> invalidProperties, StringBuilder messageBuilder, string propertyName, IEnumerable<string> messages)
    {
        bool first = true;
        foreach (string message in messages)
        {
            if (invalidProperties.Add(propertyName))
            {
                if (messageBuilder.Length > 0)
                {
                    messageBuilder.AppendLine();
                }

                messageBuilder.Append(propertyName);
                messageBuilder.Append(": ");
            }

            if (first)
            {
                first = false;
            }
            else
            {
                messageBuilder.Append(", ");
            }

            messageBuilder.Append(message);
        }
    }

    private static ValidationContext CreateContext(object? value, ValidationContext parentContext, PropertyInfo? property)
    {
        return value is null
            ? parentContext
            : new ValidationContext(value, parentContext, parentContext.Items)
            {
                DisplayName = property?.Name ?? parentContext.DisplayName,
                MemberName = property?.Name ?? parentContext.MemberName,
            };
    }

    private static IEnumerable<string> ValidateDataAnnotations(object? value, ValidationContext context)
    {
        if (value is null)
        {
            yield break;
        }

        List<ValidationResult> results = new(1);
        if (Validator.TryValidateObject(value, context, results))
        {
            yield break;
        }

        foreach (ValidationResult result in results)
        {
            string members = string.Join(", ", result.MemberNames);
            string message = result.ErrorMessage ?? "is not valid";
            yield return $"{members}: {message}";
        }
    }

    private static IEnumerable<string> ValidateEnumerable(object? value, NullabilityInfo listNullability, ValidationContext listContext)
    {
        if (value is not IEnumerable<object?> enumerable)
        {
            yield break;
        }

        NullabilityInfo elementNullability = listNullability.ElementType ?? listNullability.GenericTypeArguments[0];
        bool found = false;

        foreach (object? item in enumerable)
        {
            found = true;

            ValidationContext itemContext = CreateContext(item, listContext, null);
            IEnumerable<string> valueMessages = ValidateValue(item, elementNullability, itemContext);

            foreach (string valueMessage in valueMessages)
            {
                yield return valueMessage;
            }
        }

        if (!found)
        {
            yield return "should have items";
        }
    }

    private static IEnumerable<string> ValidateNullability(object? value, NullabilityInfo nullability)
    {
        if (nullability.ReadState == NullabilityState.Nullable)
        {
            yield break;
        }

        if (value is null)
        {
            yield return "should not be null";
        }
    }

    private static IEnumerable<string> ValidateString(object? value)
    {
        if (value is not string str)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(str))
        {
            yield return "should not be empty";
        }
    }

    private static IEnumerable<string> ValidateValue(object? value, NullabilityInfo nullability, ValidationContext context)
    {
        return ValidateNullability(value, nullability)
            .Concat(ValidateEnumerable(value, nullability, context))
            .Concat(ValidateString(value))
            .Concat(ValidateDataAnnotations(value, context));
    }

    private ValidationResult? ValidateProperties(object target, ValidationContext context)
    {
        Type targetType = target.GetType();
        PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        HashSet<string> invalidProperties = new(properties.Length);
        StringBuilder messageBuilder = new();

        foreach (PropertyInfo property in properties)
        {
            IEnumerable<string> messages = this.ValidateProperty(target, property, context);
            AppendMessages(invalidProperties, messageBuilder, property.Name, messages);
        }

        if (invalidProperties.Count == 0)
        {
            return ValidationResult.Success;
        }

        string names = string.Join(", ", invalidProperties);
        return new ValidationResult(messageBuilder.ToString(), invalidProperties);
    }

    private IEnumerable<string> ValidateProperty(object target, PropertyInfo property, ValidationContext parentContext)
    {
        object? value = property.GetValue(target);
        NullabilityInfo nullability = this.nullabilityContext.Create(property);
        ValidationContext validationContext = CreateContext(value, parentContext, property);
        return ValidateValue(value, nullability, validationContext);
    }
}
