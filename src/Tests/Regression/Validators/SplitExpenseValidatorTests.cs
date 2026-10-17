using Application.UseCases;
using Application.UseCases.FluentValidation;

namespace Tests.Regression.Validators;

public class SplitExpenseValidatorTests
{
    [Fact]
    public void Validate_WhenActorKeyIsEmpty_ReturnsActorValidationError()
    {
        var validator = new SplitExpenseRequestValidator();

        var result = validator.Validate(new SplitExpenseRequest
        {
            GroupKey = Guid.NewGuid()
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(SplitExpenseRequest.ActorKey));
    }
}
