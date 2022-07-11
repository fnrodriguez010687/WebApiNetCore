using AutoMapper;
using WebApiNetCore.Dtos.Character;
using WebApiNetCore.Models;

namespace WebApiNetCore
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Character, GetCharacterDto>();
            CreateMap<AddCharacterDto, Character>();
        }
    }
}