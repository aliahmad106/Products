using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Application.Services;
using Products.Domain.Entities;
using Xunit;

namespace Products.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateProductRequest>> _validatorMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _validatorMock = new Mock<IValidator<CreateProductRequest>>();
        _sut = new ProductService(_repositoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsProductResponse()
    {
        // Arrange
        var request = new CreateProductRequest("Widget", "A widget", 9.99m, "Red");
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Name.Should().Be("Widget");
        result.Description.Should().Be("A widget");
        result.Price.Should().Be(9.99m);
        result.Colour.Should().Be("Red");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateProductRequest("", null, -1m, "");
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required."),
            new("Price", "Price must be greater than or equal to 0.")
        };
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var act = () => _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetAllAsync_NullColour_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new("A", null, 1m, "Red"),
            new("B", null, 2m, "Blue")
        };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.AsReadOnly());

        // Act
        var result = await _sut.GetAllAsync(null);

        // Assert
        result.Should().HaveCount(2);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.GetByColourAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WhitespaceColour_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product> { new("A", null, 1m, "Red") };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.AsReadOnly());

        // Act
        var result = await _sut.GetAllAsync("   ");

        // Assert
        result.Should().HaveCount(1);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithColour_FiltersProducts()
    {
        // Arrange
        var products = new List<Product> { new("A", null, 1m, "Red") };
        _repositoryMock.Setup(r => r.GetByColourAsync("Red", It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.AsReadOnly());

        // Act
        var result = await _sut.GetAllAsync("Red");

        // Assert
        result.Should().HaveCount(1);
        result[0].Colour.Should().Be("Red");
        _repositoryMock.Verify(r => r.GetByColourAsync("Red", It.IsAny<CancellationToken>()), Times.Once);
    }
}
