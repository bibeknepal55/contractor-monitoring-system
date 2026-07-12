using AutoMapper;
using ContractorMonitoring.Application.DTOs.AdvancePaymentGuarantee;
using ContractorMonitoring.Application.DTOs.ApprovalWorkflow;
using ContractorMonitoring.Application.DTOs.Auth;
using ContractorMonitoring.Application.DTOs.ContractFinancialDetail;
using ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;
using ContractorMonitoring.Application.DTOs.DelayReason;
using ContractorMonitoring.Application.DTOs.LabTest;
using ContractorMonitoring.Application.DTOs.PerformanceBond;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;
using ContractorMonitoring.Application.DTOs.PhysicalProgress;
using ContractorMonitoring.Application.DTOs.PriceAdjustment;
using ContractorMonitoring.Application.DTOs.Project;
using ContractorMonitoring.Application.DTOs.RawMaterial;
using ContractorMonitoring.Application.DTOs.ResponsibleOfficial;
using ContractorMonitoring.Application.DTOs.Subcontractor;
using ContractorMonitoring.Application.DTOs.TimeExtension;
using DomainEntities = ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Mappings;

// AutoMapper profile for all mappings
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Auth mappings
        CreateMap<DomainEntities.User, UserDto>();

        // Project mappings
        CreateMap<DomainEntities.Project, ProjectDto>()
            .ForMember(dest => dest.ContractorName, opt => opt.MapFrom(src => src.Contractor != null ? src.Contractor.CompanyName : string.Empty));
        CreateMap<CreateProjectDto, DomainEntities.Project>().IgnoreAuditFields();
        CreateMap<UpdateProjectDto, DomainEntities.Project>().IgnoreAuditFields();

        //AutoMapper for ApprovalWorkflow
        CreateMap<Domain.Entities.ApprovalWorkflow, ApprovalWorkflowDto>();

        // Contractor Office Detail mappings
        CreateMap<DomainEntities.ContractorOfficeDetail, ContractorOfficeDetailDto>()
            .ForMember(dest => dest.ProjectCount, opt => opt.MapFrom(src => src.Projects != null ? src.Projects.Count : 0));
        CreateMap<CreateContractorOfficeDetailDto, DomainEntities.ContractorOfficeDetail>().IgnoreAuditFields();
        CreateMap<UpdateContractorOfficeDetailDto, DomainEntities.ContractorOfficeDetail>().IgnoreAuditFields();

        // Contract Financial Detail mappings
        CreateMap<DomainEntities.ContractFinancialDetail, ContractFinancialDetailDto>();
        CreateMap<CreateContractFinancialDetailDto, DomainEntities.ContractFinancialDetail>().IgnoreAuditFields();
        CreateMap<UpdateContractFinancialDetailDto, DomainEntities.ContractFinancialDetail>().IgnoreAuditFields();

        // Price Adjustment mappings
        CreateMap<DomainEntities.PriceAdjustment, PriceAdjustmentDto>();
        CreateMap<CreatePriceAdjustmentDto, DomainEntities.PriceAdjustment>().IgnoreAuditFields();
        CreateMap<UpdatePriceAdjustmentDto, DomainEntities.PriceAdjustment>().IgnoreAuditFields();

        // Performance Bond mappings
        CreateMap<DomainEntities.PerformanceBond, PerformanceBondDto>();
        CreateMap<CreatePerformanceBondDto, DomainEntities.PerformanceBond>().IgnoreAuditFields();
        CreateMap<UpdatePerformanceBondDto, DomainEntities.PerformanceBond>().IgnoreAuditFields();

        // Advance Payment Guarantee mappings
        CreateMap<DomainEntities.AdvancePaymentGuarantee, AdvancePaymentGuaranteeDto>();
        CreateMap<CreateAdvancePaymentGuaranteeDto, DomainEntities.AdvancePaymentGuarantee>().IgnoreAuditFields();
        CreateMap<UpdateAdvancePaymentGuaranteeDto, DomainEntities.AdvancePaymentGuarantee>().IgnoreAuditFields();

        // Physical Progress mappings
        CreateMap<DomainEntities.PhysicalProgress, PhysicalProgressDto>();
        CreateMap<CreatePhysicalProgressDto, DomainEntities.PhysicalProgress>().IgnoreAuditFields();
        CreateMap<UpdatePhysicalProgressDto, DomainEntities.PhysicalProgress>().IgnoreAuditFields();

        // Time Extension mappings
        CreateMap<DomainEntities.TimeExtension, TimeExtensionDto>();
        CreateMap<CreateTimeExtensionDto, DomainEntities.TimeExtension>().IgnoreAuditFields();
        CreateMap<UpdateTimeExtensionDto, DomainEntities.TimeExtension>().IgnoreAuditFields();

        // Delay Reason mappings
        CreateMap<DomainEntities.DelayReason, DelayReasonDto>();
        CreateMap<CreateDelayReasonDto, DomainEntities.DelayReason>().IgnoreAuditFields();
        CreateMap<UpdateDelayReasonDto, DomainEntities.DelayReason>().IgnoreAuditFields();

        // Raw Material mappings
        CreateMap<DomainEntities.RawMaterial, RawMaterialDto>();
        CreateMap<CreateRawMaterialDto, DomainEntities.RawMaterial>().IgnoreAuditFields();
        CreateMap<UpdateRawMaterialDto, DomainEntities.RawMaterial>().IgnoreAuditFields();

        // Lab Test mappings
        CreateMap<DomainEntities.LabTest, LabTestDto>();
        CreateMap<CreateLabTestDto, DomainEntities.LabTest>().IgnoreAuditFields();
        CreateMap<UpdateLabTestDto, DomainEntities.LabTest>().IgnoreAuditFields();

        // Photo Monitoring mappings
        CreateMap<DomainEntities.PhotoMonitoring, PhotoMonitoringDto>();
        CreateMap<CreatePhotoMonitoringDto, DomainEntities.PhotoMonitoring>().IgnoreAuditFields();
        CreateMap<UpdatePhotoMonitoringDto, DomainEntities.PhotoMonitoring>().IgnoreAuditFields();

        // Subcontractor mappings
        CreateMap<DomainEntities.Subcontractor, SubcontractorDto>();
        CreateMap<CreateSubcontractorDto, DomainEntities.Subcontractor>().IgnoreAuditFields();
        CreateMap<UpdateSubcontractorDto, DomainEntities.Subcontractor>().IgnoreAuditFields();

        // Responsible Official mappings
        CreateMap<DomainEntities.ResponsibleOfficial, ResponsibleOfficialDto>();
        CreateMap<CreateResponsibleOfficialDto, DomainEntities.ResponsibleOfficial>().IgnoreAuditFields();
        CreateMap<UpdateResponsibleOfficialDto, DomainEntities.ResponsibleOfficial>().IgnoreAuditFields();
    }
}

// Extension method to ignore audit fields
public static class AutoMapperExtensions
{
    public static IMappingExpression<TSource, TDestination> IgnoreAuditFields<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> mapping)
        where TDestination : ContractorMonitoring.Domain.Entities.Base.AuditableEntity
    {
        return mapping
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore());
    }
}