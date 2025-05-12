using Xunit;
using FluentValidation.TestHelper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.Validators;

public class TagRequestValidatorTests
{
    private readonly TagRequestValidator _validator = new();

    [Fact]
    public void Valid_TagRequest_Should_Pass()
    {
        var request = new TagRequest { Name = "Playful" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyOrNullName_Should_Fail(string? name)
    {
        var request = new TagRequest { Name = name ?? string.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("42.5")]
    [InlineData("0")]
    public void NumericName_Should_Fail(string numericName)
    {
        var request = new TagRequest { Name = numericName };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Name);
    }
}
