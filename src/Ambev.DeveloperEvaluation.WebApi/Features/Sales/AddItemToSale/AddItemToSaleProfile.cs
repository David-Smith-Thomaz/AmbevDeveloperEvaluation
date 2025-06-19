using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.AddItemToSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.AddItemToSale
{
    public class AddItemToSaleProfile : Profile
    {
        public AddItemToSaleProfile()
        {
            CreateMap<AddItemToSaleRequest, AddItemToSaleCommand.AddItemToSaleCommandItem>();
            CreateMap<AddItemToSaleRequest, AddItemToSaleCommand>().ForMember(dest => dest.Item, opt => opt.MapFrom(src => src));
        }
    }
}