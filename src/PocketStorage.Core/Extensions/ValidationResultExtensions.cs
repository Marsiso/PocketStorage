﻿using FluentValidation.Results;

namespace PocketStorage.Core.Extensions;

public static class ValidationResultExtensions
{
    public static Dictionary<string, string[]> DistinctErrorsByProperty(this ValidationResult validationResult) =>
        validationResult.Errors
            .GroupBy(validationFailure =>
                    validationFailure.PropertyName,
                validationFailure => validationFailure.ErrorMessage,
                (propertyName, validationFailuresByProperty) => new { Key = propertyName, Values = validationFailuresByProperty.Distinct().ToArray() })
            .ToDictionary(
                group => group.Key,
                group => group.Values);

    public static Dictionary<string, string[]> DistinctErrorsByProperty(this IEnumerable<ValidationResult> validationResults) =>
        validationResults
            .Where(validationResult => validationResult is { IsValid: false, Errors: not null, Errors.Count: > 0 })
            .SelectMany(validationResult => validationResult.Errors, (_, vf) => vf)
            .GroupBy(
                validationFailure => validationFailure.PropertyName,
                validationFailure => validationFailure.ErrorMessage,
                (propertyName, validationFailures) => new { Key = propertyName, Values = validationFailures.Distinct().ToArray() })
            .ToDictionary(
                group => group.Key,
                group => group.Values);
}
