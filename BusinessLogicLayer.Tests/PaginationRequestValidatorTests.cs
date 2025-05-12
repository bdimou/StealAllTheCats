using FluentValidation.TestHelper;
using Xunit;
using BusinessLogicLayer.Validators;
using BusinessLogicLayer.DTO;

public class PaginationRequestValidatorTests
{
    private readonly PaginationRequestValidator _validator = new();

    [Theory]
    [InlineData("1", "10")]
    
    public void Valid_PaginationRequest_Should_Pass(string page, string pageSize)
    {
        var request = new PaginationRequest { PageIndex= page, PageSize = pageSize };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "10")]
    [InlineData("1", "")]
    [InlineData("0", "5")]
    [InlineData("5", "0")]
    [InlineData("abc", "10")]
    [InlineData("-1", "10")]
    [InlineData("1", "-5")]
    public void Invalid_PaginationRequest_Should_Fail(string page, string pageSize)
    {
        var request = new PaginationRequest { PageIndex = page, PageSize = pageSize };
        var result = _validator.TestValidate(request);
        result.ShouldHaveAnyValidationError();
    }
}
