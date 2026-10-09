
//using Application.Dtos.Customers;
//using AutoMapper;
//using Domain.Entities;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace Application.Mappings;

//public class CustomerProfile : Profile
//{
//    public CustomerProfile()
//    {
//        CreateMap<CreateCustomerDto, Customer>()
//            .ForMember(
//                destination => destination.CustomerId,
//                options => options.Ignore())
//            .ForMember(
//                destination => destination.CreditScore,
//                options => options.Ignore())
//            .ForMember(
//                destination => destination.Active,
//                options => options.Ignore())
//            .ForMember(
//                destination => destination.Email,
//                options => options.MapFrom(
//                    source => source.Email.Trim().ToLowerInvariant()))
//            .ForMember(
//                destination => destination.Cpf,
//                options => options.MapFrom(
//                    source => NormalizeCpf(source.Cpf)));

//        CreateMap<UpdateCustomerDto, Customer>()
//            .ForMember(
//                destination => destination.CustomerId,
//                options => options.Ignore())
//            .ForMember(
//                destination => destination.CreditScore,
//                options => options.Ignore())
//            .ForMember(
//                destination => destination.Active,
//                options => options.Ignore())
//            .ForMember(
//                destination => destination.Email,
//                options => options.MapFrom(
//                    source => source.Email.Trim().ToLowerInvariant()))
//            .ForMember(
//                destination => destination.Cpf,
//                options => options.MapFrom(
//                    source => NormalizeCpf(source.Cpf)));
//    }

//    private static string NormalizeCpf(string cpf) =>
//        new(cpf.Where(char.IsDigit).ToArray());
//}