using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.AddItemToSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale
{
    public class CreateSaleProfile : Profile
    {
        public CreateSaleProfile()
        {
            CreateMap<CreateSaleRequest, CreateSaleCommand>();
            CreateMap<AddItemToSaleRequest, CreateSaleCommand.CreateSaleCommandItem>();
            CreateMap<Guid, CreateSaleResponse>().ForMember(dest => dest.SaleId, opt => opt.MapFrom(src => src));
        }
    }
}