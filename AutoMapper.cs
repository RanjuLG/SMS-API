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
            CreateMap<GetItemDTO, Item>();




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
          

            CreateMap<TransactionDTO, Transaction>();

            CreateMap<Transaction, CreateTransactionDTO>();
            CreateMap<CreateTransactionDTO, Transaction>();

            CreateMap<Transaction, UpdateTransactionDTO>();
            CreateMap<UpdateTransactionDTO, Transaction>();

            CreateMap<Customer, CommonCustomerDTO>();
            CreateMap<Item, CommonItemDTO>();

            CreateMap<Transaction, TransactionDTO>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.TransactionItems.Select(ti => ti.Item)))
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer));
            CreateMap<Transaction, TransactionDTO>()
                        .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.TransactionItems.Select(ti => ti.Item)))
                        .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer));

            CreateMap<CreateTransactionDTO, Transaction>()
                .ForMember(dest => dest.TransactionItems, opt => opt.MapFrom((src, dest, destMember, context) =>
                    src.Items.Select(i => new TransactionItem { Item = context.Mapper.Map<Item>(i) })));

            CreateMap<UpdateTransactionDTO, Transaction>();

            CreateMap<Item, GetItemDTO>();
            CreateMap<Customer, GetCustomerDTO>();

            CreateMap<CreateItemDTO, Item>()
                .ForMember(dest => dest.ItemId, opt => opt.Ignore()) // Assuming ItemId is auto-generated
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Assuming Status is set elsewhere
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore());
        }
    }
}
