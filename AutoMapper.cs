using AutoMapper;
using SMS.Models;
using SMS.Models.DTO;
using SMS.Models.DTO.SMS.Models.DTO;

namespace SMS
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapping for Customer
            CreateMap<Customer, GetCustomerDTO>();
            CreateMap<GetCustomerDTO, Customer>();

            CreateMap<Customer, CreateCustomerDTO>();
            CreateMap<CreateCustomerDTO, Customer>();

            // Mapping for Item
            CreateMap<Item, GetItemDTO>();
            CreateMap<GetItemDTO, Item>();
            CreateMap<Item, CreateItemDTO>();
            CreateMap<CreateItemDTO, Item>();

            CreateMap<Item, UpdateItemDTO>();
            CreateMap<UpdateItemDTO, Item>();
            CreateMap<Item, GetItemDTO>()
             .ForMember(dest => dest.CustomerNIC, opt => opt.MapFrom(src => src.Customer.CustomerNIC));



            // Mapping for Invoice
            CreateMap<Invoice, CreateInvoiceDTO>();
            CreateMap<CreateInvoiceDTO, Invoice>();

            CreateMap<Invoice, GetInvoiceDTO>()
                .ForMember(dest => dest.CustomerNIC, opt => opt.MapFrom(src => src.Transaction.Customer.CustomerNIC)); ;
            CreateMap<GetInvoiceDTO, Invoice>();
            

            CreateMap<Invoice, InvoiceDTO>();
            CreateMap<InvoiceDTO, Invoice>();

            CreateMap<Invoice, UpdateInvoiceDTO>();
            CreateMap<UpdateInvoiceDTO, Invoice>();

            //Mapping for Transactions
            CreateMap<Transaction, TransactionDTO>()
                      .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                      .ForMember(dest => dest.Item, opt => opt.MapFrom(src => src.Item));

            CreateMap<TransactionDTO, Transaction>();

            CreateMap<Transaction, CreateTransactionDTO>();
            CreateMap<CreateTransactionDTO, Transaction>();

            CreateMap<Transaction, UpdateTransactionDTO>();
            CreateMap<UpdateTransactionDTO, Transaction>();

            CreateMap<Customer, CommonCustomerDTO>();
            CreateMap<Item, CommonItemDTO>();
        }
    }
}
