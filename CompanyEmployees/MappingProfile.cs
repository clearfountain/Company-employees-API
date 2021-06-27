using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDTO>()
                .ForMember(cdto => cdto.FullAddress,
                    opt => opt.MapFrom(c => string.Join(' ', c.Address, c.Country)));

            CreateMap<Employee, EmployeeDTO>();

            CreateMap<CompanyForCreationDTO, Company>();

            CreateMap<EmployeeForCreationDTO, Employee>();

            CreateMap<EmployeeForUpdateDTO, Employee>();

            CreateMap<CompanyForUpdateDTO, Company>();

            CreateMap<EmployeeForUpdateDTO, Employee>().ReverseMap();

            CreateMap<UserForRegistrationDTO, User>();
        }
    }
}
