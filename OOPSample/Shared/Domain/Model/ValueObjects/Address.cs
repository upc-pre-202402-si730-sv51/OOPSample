namespace OOPSample.Shared.Domain.Model.ValueObjects;

public record Address(string Street, string Number, string City, string State, string ZipCode, string Country)
{
    public string AddressAsString => $"{Street} {Number}, {City}, {State}, {ZipCode}, {Country}";
}