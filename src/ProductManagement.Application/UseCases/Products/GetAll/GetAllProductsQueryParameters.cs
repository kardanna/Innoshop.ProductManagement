using ProductManagement.Application.Shared;

namespace ProductManagement.Application.UseCases.Products.GetAll;

public class GetAllProductsQueryParameters
{
    public ProductOrderBy? OrderBy { get; set; }
    public OrderDirection? OrderDirection { get; set; }
    public string? NameContains { get; set; }
    public string? DescriptionContains { get; set; }
    //ADD PAGINATION!!!!!!!!!!!!!!!
}