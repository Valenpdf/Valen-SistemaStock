using AutoMapper;
using Valen_SistemaStock.Models.Entities;
using Valen_SistemaStock.Models.ViewModels.Movimiento;

namespace Valen_SistemaStock.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Movimiento, MovimientoViewModel>();

            CreateMap<MovimientoViewModel, Movimiento>();
        }
    }
}
