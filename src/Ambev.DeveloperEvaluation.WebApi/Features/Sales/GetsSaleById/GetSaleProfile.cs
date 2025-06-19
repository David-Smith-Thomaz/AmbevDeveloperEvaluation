using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSaleItemById;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSaleById
{
    public class GetSaleProfile : Profile
    {
        public GetSaleProfile()
        {
            CreateMap<Sale, GetSaleResponse>().ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.SaleItems));
            CreateMap<SaleItem, GetSaleItemResponse>();
        }
    }
}