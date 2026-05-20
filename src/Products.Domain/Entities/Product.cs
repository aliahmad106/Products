namespace Products.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Colour { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Product() { }

    public Product(string name, string? description, decimal price, string colour)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Colour = colour;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description, decimal price, string colour)
    {
        Name = name;
        Description = description;
        Price = price;
        Colour = colour;
    }
}
