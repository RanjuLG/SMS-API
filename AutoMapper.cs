using AutoMapper;
using SMS.Models;
using SMS.Models.DTO;

namespace SMS
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Customer mappings
            CreateMap<Customer, GetCustomerDTO>();
            CreateMap<CreateCustomerDTO, Customer>();
            CreateMap<UpdateCustomerDTO, Customer>();
            CreateMap<Customer, CreateCustomerDTO>();

            // Item mappings
            CreateMap<Item, GetItemDTO>()
                .ForMember(dest => dest.CustomerNIC, opt => opt.MapFrom(src => src.Customer.CustomerNIC))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CustomerName));
            CreateMap<CreateItemDTO, Item>()
                .ForMember(dest => dest.ItemId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<UpdateItemDTO, Item>();

            // Transaction mappings
            CreateMap<Transaction, GetTransactionDTO>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CustomerName))
                .ForMember(dest => dest.CustomerNIC, opt => opt.MapFrom(src => src.Customer.CustomerNIC))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.TransactionItems));
            CreateMap<CreateTransactionDTO, Transaction>();

            // TransactionItem mappings
            CreateMap<TransactionItem, TransactionItemDTO>()
                .ForMember(dest => dest.ItemDescription, opt => opt.MapFrom(src => src.Item.ItemDescription));

            // Invoice mappings
            CreateMap<Invoice, GetInvoiceDTO>()
                .ForMember(dest => dest.CustomerNIC, opt => opt.MapFrom(src => src.Transaction.Customer.CustomerNIC))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Transaction.Customer.CustomerName))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Transaction.TotalAmount));
            CreateMap<CreateInvoiceDTO, Invoice>();
            CreateMap<UpdateInvoiceDTO, Invoice>();

            // Karatage mappings
            CreateMap<Karat, GetKaratDTO>();
            CreateMap<CreateKaratDTO, Karat>();
            CreateMap<UpdateKaratDTO, Karat>();

            // Loan Period mappings
            CreateMap<LoanPeriod, GetLoanPeriodDTO>();
            CreateMap<CreateLoanPeriodDTO, LoanPeriod>();
            CreateMap<UpdateLoanPeriodDTO, LoanPeriod>();

            // Pricing mappings
            CreateMap<Pricing, GetPricingDTO>();
            CreateMap<CreatePricingDTO, Pricing>();
            CreateMap<UpdatePricingDTO, Pricing>();

            // Installment mappings
            CreateMap<Installment, GetInstallmentDTO>();
            CreateMap<CreateInstallmentDTO, Installment>();
        }
    }
}
