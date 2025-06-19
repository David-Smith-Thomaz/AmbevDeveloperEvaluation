using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSaleById;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSaleItemById;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales
{
    public class ListSalesProfile : Profile
    {
        public ListSalesProfile()
        {
            CreateMap<Sale, GetSaleResponse>().ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.SaleItems));
            CreateMap<SaleItem, GetSaleItemResponse>();
        }
    }
}