using FsCheck;
using Products.Application.DTOs;

namespace Products.PropertyTests.Generators;

public static class ProductGenerators
{
    public static Arbitrary<CreateProductRequest> ValidProduct()
    {
        var validNameGen = from prefix in Gen.Elements("Product", "Item", "Widget", "Gadget", "Tool")
                           from suffix in Arb.Generate<NonEmptyString>().Select(s => s.Get.Trim())
                           select string.IsNullOrWhiteSpace(suffix) ? prefix : $"{prefix} {suffix}";

        return (from name in validNameGen
                from desc in Gen.OneOf(
                    Gen.Constant<string?>(null),
                    Arb.Generate<NonEmptyString>().Select(s => (string?)s.Get))
                from price in Gen.Choose(0, 100000).Select(p => (decimal)p / 100m)
                from colour in Gen.Elements("Red", "Blue", "Green", "Yellow", "Black", "White", "Purple")
                select new CreateProductRequest(name, desc, price, colour))
            .ToArbitrary();
    }

    public static Arbitrary<CreateProductRequest> InvalidProduct()
    {
        var emptyName = from price in Gen.Choose(0, 10000).Select(p => (decimal)p / 100m)
                        from colour in Gen.Elements("Red", "Blue", "Green")
                        select new CreateProductRequest("", null, price, colour);

        var negativePrice = from name in Arb.Generate<NonEmptyString>()
                            from price in Gen.Choose(1, 10000).Select(p => -(decimal)p / 100m)
                            from colour in Gen.Elements("Red", "Blue", "Green")
                            select new CreateProductRequest(name.Get, null, price, colour);

        var emptyColour = from name in Arb.Generate<NonEmptyString>()
                          from price in Gen.Choose(0, 10000).Select(p => (decimal)p / 100m)
                          select new CreateProductRequest(name.Get, null, price, "");

        return Gen.OneOf(emptyName, negativePrice, emptyColour).ToArbitrary();
    }

    public static Arbitrary<string> WhitespaceString()
    {
        return Gen.Elements("", " ", "  ", "\t", "  \t  ").ToArbitrary();
    }
}
