using Mechanics.Domain.Customers;

namespace Mechanics.Tests.Unit.Mocks;

public static class CustomerMocks
{
    public static Customer CreateCustomerPf(Guid id) => new()
    {
        Id = id,
        Name = "Joao da Silva",
        Email = "joao@EXAMPLE.com",
        Document = new PersonalDocument(DocumentType.Cpf, "11144477735"),
    };

    public static Customer CreateCustomerPj(Guid id) => new()
    {
        Id = id,
        Name = "Empresa XYZ Ltda",
        Email = "contato@xyz.com",
        Document = new PersonalDocument(DocumentType.Cnpj, "11444777000161"),
    };
}
