using Xunit;
using FluentValidation.TestHelper;
using BusinessLogicLayer.Validators;

namespace BusinessLogicLayer.Tests;

public class IdValidatorTests
{
    private readonly IdValidator _validator = new();

    [Theory]
    [InlineData("1")]
    [InlineData("42")]
    public void Valid_Id_Should_Pass(string id)
    {
        var result = _validator.TestValidate(id);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyOrNull_Id_Should_Fail(string? id)
    {
        var result = _validator.TestValidate(id ?? string.Empty);
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    public void Invalid_Id_Should_Fail(string id)
    {
        var result = _validator.TestValidate(id);
        result.ShouldHaveValidationErrorFor(x => x);
    }
}
