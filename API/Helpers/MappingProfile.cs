using API.DTOs;
using API.Entities;
using AutoMapper;

namespace API.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Transaction, TransactionDto>();
            CreateMap<AppUser, AppUser>();
        }
    }
}