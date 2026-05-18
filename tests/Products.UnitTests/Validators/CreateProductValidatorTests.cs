using FluentAssertions;
using Products.Application.DTOs;
using Products.Application.Validators;
using Xunit;

namespace Products.UnitTests.Validators;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _sut = new();

    [Fact]
    public void Validate_ValidRequest_IsValid()
    {
        var request = new CreateProductRequest("Widget", "A widget", 9.99m, "Red");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyName_IsInvalid()
    {
        var request = new CreateProductRequest("", "desc", 9.99m, "Red");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_NegativePrice_IsInvalid()
    {
        var request = new CreateProductRequest("Widget", null, -1m, "Red");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public void Validate_ZeroPrice_IsValid()
    {
        var request = new CreateProductRequest("Widget", null, 0m, "Blue");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyColour_IsInvalid()
    {
        var request = new CreateProductRequest("Widget", null, 5m, "");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Colour");
    }
}
