using FluentValidation;

namespace PocketStorage.Application.Extensions;

public static class FluentValidationRuleBuilderExtensions
{
    public const string SpecialCharacters = @"!@#$%^&*()_+-=~`[]{};:',<.>/?\|";

    public static IRuleBuilderOptions<T, string?> Url<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.Must(url =>
        {
            bool hasValidUri = !string.IsNullOrWhiteSpace(url);

            if (hasValidUri)
            {
                hasValidUri = Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            }

            return hasValidUri;
        });

    public static IRuleBuilderOptions<T, string?> HasNumericCharacter<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.Must(value =>
        {
            ReadOnlySpan<char> valueSpan = value.AsSpan();

            if (valueSpan.IsEmpty)
            {
                return false;
            }

            foreach (char c in valueSpan)
            {
                if (char.IsNumber(c))
                {
                    return true;
                }
            }

            return false;
        });

    public static IRuleBuilderOptions<T, string?> HasSpecialCharacter<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.Must(value =>
        {
            ReadOnlySpan<char> valueSpan = value.AsSpan();

            if (valueSpan.IsEmpty)
            {
                return false;
            }

            foreach (char c in valueSpan)
            {
                if (SpecialCharacters.Contains(c))
                {
                    return true;
                }
            }

            return false;
        });

    public static IRuleBuilderOptions<T, string?> HasLowerCaseCharacter<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.Must(value =>
        {
            ReadOnlySpan<char> valueSpan = value.AsSpan();

            if (valueSpan.IsEmpty)
            {
                return false;
            }

            foreach (char c in valueSpan)
            {
                if (char.IsLower(c))
                {
                    return true;
                }
            }

            return false;
        });

    public static IRuleBuilderOptions<T, string?> HasUpperCaseCharacter<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.Must(value =>
        {
            ReadOnlySpan<char> valueSpan = value.AsSpan();

            if (valueSpan.IsEmpty)
            {
                return false;
            }

            foreach (char c in valueSpan)
            {
                if (char.IsUpper(c))
                {
                    return true;
                }
            }

            return false;
        });
}
