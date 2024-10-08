using FluentValidation.Results;

namespace ProjectManager.Application.Contracts;

public sealed class ValidationException(ValidationResult validationResult)
    : Exception(string.Join(Environment.NewLine, validationResult.Errors));