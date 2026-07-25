namespace ContractorMonitoring.Domain.Constants;

public static class Permissions
{
    public static class Project
    {
        public const string Create = "Project.Create";
        public const string View   = "Project.View";
        public const string Update = "Project.Update";
        public const string Delete = "Project.Delete";
    }

    public static class ContractorOfficeDetail
    {
        public const string Create = "ContractorOfficeDetail.Create";
        public const string View   = "ContractorOfficeDetail.View";
        public const string Update = "ContractorOfficeDetail.Update";
        public const string Delete = "ContractorOfficeDetail.Delete";
    }

    public static class ContractFinancialDetail
    {
        public const string Create = "ContractFinancialDetail.Create";
        public const string View   = "ContractFinancialDetail.View";
        public const string Update = "ContractFinancialDetail.Update";
        public const string Delete = "ContractFinancialDetail.Delete";
    }

    public static class PriceAdjustment
    {
        public const string Create = "PriceAdjustment.Create";
        public const string View   = "PriceAdjustment.View";
        public const string Update = "PriceAdjustment.Update";
        public const string Delete = "PriceAdjustment.Delete";
    }

    public static class PerformanceBond
    {
        public const string Create = "PerformanceBond.Create";
        public const string View   = "PerformanceBond.View";
        public const string Update = "PerformanceBond.Update";
        public const string Delete = "PerformanceBond.Delete";
    }

    public static class AdvancePaymentGuarantee
    {
        public const string Create = "AdvancePaymentGuarantee.Create";
        public const string View   = "AdvancePaymentGuarantee.View";
        public const string Update = "AdvancePaymentGuarantee.Update";
        public const string Delete = "AdvancePaymentGuarantee.Delete";
    }

    public static class PhysicalProgress
    {
        public const string Create = "PhysicalProgress.Create";
        public const string View   = "PhysicalProgress.View";
        public const string Update = "PhysicalProgress.Update";
        public const string Delete = "PhysicalProgress.Delete";
    }

    public static class TimeExtension
    {
        public const string Create = "TimeExtension.Create";
        public const string View   = "TimeExtension.View";
        public const string Update = "TimeExtension.Update";
        public const string Delete = "TimeExtension.Delete";
    }

    public static class DelayReason
    {
        public const string Create = "DelayReason.Create";
        public const string View   = "DelayReason.View";
        public const string Update = "DelayReason.Update";
        public const string Delete = "DelayReason.Delete";
    }

    public static class RawMaterial
    {
        public const string Create = "RawMaterial.Create";
        public const string View   = "RawMaterial.View";
        public const string Update = "RawMaterial.Update";
        public const string Delete = "RawMaterial.Delete";
    }

    public static class LabTest
    {
        public const string Create = "LabTest.Create";
        public const string View   = "LabTest.View";
        public const string Update = "LabTest.Update";
        public const string Delete = "LabTest.Delete";
    }

    public static class PhotoMonitoring
    {
        public const string Create = "PhotoMonitoring.Create";
        public const string View   = "PhotoMonitoring.View";
        public const string Update = "PhotoMonitoring.Update";
        public const string Delete = "PhotoMonitoring.Delete";
    }

    public static class Subcontractor
    {
        public const string Create = "Subcontractor.Create";
        public const string View   = "Subcontractor.View";
        public const string Update = "Subcontractor.Update";
        public const string Delete = "Subcontractor.Delete";
    }

    public static class ResponsibleOfficial
    {
        public const string Create = "ResponsibleOfficial.Create";
        public const string View   = "ResponsibleOfficial.View";
        public const string Update = "ResponsibleOfficial.Update";
        public const string Delete = "ResponsibleOfficial.Delete";
    }

    public static class Dashboard
    {
        public const string View = "Dashboard.View";
    }

    public static class Reports
    {
        public const string View   = "Reports.View";
        public const string Export = "Reports.Export";
    }

    public static class ApprovalWorkflow
    {
        public const string Create  = "ApprovalWorkflow.Create";
        public const string View    = "ApprovalWorkflow.View";
        public const string Update  = "ApprovalWorkflow.Update";
        public const string Approve = "ApprovalWorkflow.Approve";
        public const string Reject  = "ApprovalWorkflow.Reject";
    }

    public static class UserManagement
    {
        public const string Create     = "UserManagement.Create";
        public const string View       = "UserManagement.View";
        public const string Update     = "UserManagement.Update";
        public const string Delete     = "UserManagement.Delete";
        public const string AssignRole = "UserManagement.AssignRole";
    }

    // Fix M-18: RoleManagement permission — nav item was using this but it didn't exist
    public static class RoleManagement
    {
        public const string View   = "RoleManagement.View";
        public const string Create = "RoleManagement.Create";
        public const string Update = "RoleManagement.Update";
        public const string Delete = "RoleManagement.Delete";
    }

    public static IEnumerable<string> GetAllPermissions() => new[]
    {
        Project.Create, Project.View, Project.Update, Project.Delete,
        ContractorOfficeDetail.Create, ContractorOfficeDetail.View, ContractorOfficeDetail.Update, ContractorOfficeDetail.Delete,
        ContractFinancialDetail.Create, ContractFinancialDetail.View, ContractFinancialDetail.Update, ContractFinancialDetail.Delete,
        PriceAdjustment.Create, PriceAdjustment.View, PriceAdjustment.Update, PriceAdjustment.Delete,
        PerformanceBond.Create, PerformanceBond.View, PerformanceBond.Update, PerformanceBond.Delete,
        AdvancePaymentGuarantee.Create, AdvancePaymentGuarantee.View, AdvancePaymentGuarantee.Update, AdvancePaymentGuarantee.Delete,
        PhysicalProgress.Create, PhysicalProgress.View, PhysicalProgress.Update, PhysicalProgress.Delete,
        TimeExtension.Create, TimeExtension.View, TimeExtension.Update, TimeExtension.Delete,
        DelayReason.Create, DelayReason.View, DelayReason.Update, DelayReason.Delete,
        RawMaterial.Create, RawMaterial.View, RawMaterial.Update, RawMaterial.Delete,
        LabTest.Create, LabTest.View, LabTest.Update, LabTest.Delete,
        PhotoMonitoring.Create, PhotoMonitoring.View, PhotoMonitoring.Update, PhotoMonitoring.Delete,
        Subcontractor.Create, Subcontractor.View, Subcontractor.Update, Subcontractor.Delete,
        ResponsibleOfficial.Create, ResponsibleOfficial.View, ResponsibleOfficial.Update, ResponsibleOfficial.Delete,
        Dashboard.View,
        Reports.View, Reports.Export,
        ApprovalWorkflow.Create, ApprovalWorkflow.View, ApprovalWorkflow.Update, ApprovalWorkflow.Approve, ApprovalWorkflow.Reject,
        UserManagement.Create, UserManagement.View, UserManagement.Update, UserManagement.Delete, UserManagement.AssignRole,
        RoleManagement.View, RoleManagement.Create, RoleManagement.Update, RoleManagement.Delete,
    };
}
