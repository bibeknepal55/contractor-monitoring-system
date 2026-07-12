using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractorMonitoring.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalLevel = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NextApprover = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalWorkflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractorOfficeDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContactPersonPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContactPersonEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InsuranceDetails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorOfficeDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProjectName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Budget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActualCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProjectManager = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ContactNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContractNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProgressPercentage = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    ContractorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_ContractorOfficeDetails_ContractorId",
                        column: x => x.ContractorId,
                        principalTable: "ContractorOfficeDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdvancePaymentGuarantees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuaranteeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GuaranteeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IssuingBank = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdvanceAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountRecovered = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LastRecoveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancePaymentGuarantees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancePaymentGuarantees_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractFinancialDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AdvancePayment = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AdvancePaymentRecovered = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TotalPaidAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PendingPayment = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PaymentTerms = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PaymentMilestones = table.Column<int>(type: "integer", nullable: true),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankBranch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SwiftCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContractSigningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastPaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractFinancialDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractFinancialDetails_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DelayReasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelayCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DelayStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DelayEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DelayDays = table.Column<int>(type: "integer", nullable: false),
                    ImpactLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResponsibleParty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MitigationAction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelayReasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DelayReasons_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TestName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TestCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LabName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TechnicianName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    TestStandard = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TestResult = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TestReportPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NextTestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ParameterTested = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SpecificationLimit = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ActualValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabTests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceBonds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    BondNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BondAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BondType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IssuingBank = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BondDocument = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceBonds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceBonds_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhotoMonitorings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PhotoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PhotoDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PhotoType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoMonitorings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhotoMonitorings_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhysicalProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgressDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedProgress = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ActualProgress = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ActivityDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Bottlenecks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MitigationPlan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SupportingDocument = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReportedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    VerifiedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhysicalProgresses_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RawMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MaterialCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QuantityOrdered = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    QuantityUsed = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualityCertificate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawMaterials_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResponsibleOfficials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    Department = table.Column<string>(type: "text", nullable: false),
                    Organization = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Mobile = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RelievingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Qualifications = table.Column<string>(type: "text", nullable: true),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibleOfficials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponsibleOfficials_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subcontractors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ScopeOfWork = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ContractAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PerformanceRating = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InsuranceDetails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcontractors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subcontractors_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeExtensions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtensionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaysRequested = table.Column<int>(type: "integer", nullable: false),
                    DaysGranted = table.Column<int>(type: "integer", nullable: true),
                    OriginalCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevisedCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SupportingDocument = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeExtensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeExtensions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreviousAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    NewAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PercentageChange = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ReferenceDocument = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AdjustmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RequestedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ContractFinancialDetailId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceAdjustments_ContractFinancialDetails_ContractFinancial~",
                        column: x => x.ContractFinancialDetailId,
                        principalTable: "ContractFinancialDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PriceAdjustments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvancePaymentGuarantees_ExpiryDate",
                table: "AdvancePaymentGuarantees",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancePaymentGuarantees_GuaranteeNumber",
                table: "AdvancePaymentGuarantees",
                column: "GuaranteeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdvancePaymentGuarantees_ProjectId",
                table: "AdvancePaymentGuarantees",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancePaymentGuarantees_Status",
                table: "AdvancePaymentGuarantees",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancePaymentGuarantees_TenantId",
                table: "AdvancePaymentGuarantees",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_ModuleName",
                table: "ApprovalWorkflows",
                column: "ModuleName");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_RecordId",
                table: "ApprovalWorkflows",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_Status",
                table: "ApprovalWorkflows",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_TenantId",
                table: "ApprovalWorkflows",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractFinancialDetails_PaymentStatus",
                table: "ContractFinancialDetails",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ContractFinancialDetails_ProjectId",
                table: "ContractFinancialDetails",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractFinancialDetails_TenantId",
                table: "ContractFinancialDetails",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorOfficeDetails_Email",
                table: "ContractorOfficeDetails",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorOfficeDetails_RegistrationNumber",
                table: "ContractorOfficeDetails",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractorOfficeDetails_Status",
                table: "ContractorOfficeDetails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorOfficeDetails_TaxId",
                table: "ContractorOfficeDetails",
                column: "TaxId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractorOfficeDetails_TenantId",
                table: "ContractorOfficeDetails",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DelayReasons_DelayCategory",
                table: "DelayReasons",
                column: "DelayCategory");

            migrationBuilder.CreateIndex(
                name: "IX_DelayReasons_DelayStartDate",
                table: "DelayReasons",
                column: "DelayStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_DelayReasons_ImpactLevel",
                table: "DelayReasons",
                column: "ImpactLevel");

            migrationBuilder.CreateIndex(
                name: "IX_DelayReasons_ProjectId",
                table: "DelayReasons",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DelayReasons_TenantId",
                table: "DelayReasons",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_Category",
                table: "LabTests",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_ProjectId",
                table: "LabTests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_TenantId",
                table: "LabTests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_TestCode",
                table: "LabTests",
                column: "TestCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_TestDate",
                table: "LabTests",
                column: "TestDate");

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_TestResult",
                table: "LabTests",
                column: "TestResult");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceBonds_BondNumber",
                table: "PerformanceBonds",
                column: "BondNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceBonds_ExpiryDate",
                table: "PerformanceBonds",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceBonds_ProjectId",
                table: "PerformanceBonds",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceBonds_Status",
                table: "PerformanceBonds",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceBonds_TenantId",
                table: "PerformanceBonds",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Group",
                table: "Permissions",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhotoMonitorings_PhotoDate",
                table: "PhotoMonitorings",
                column: "PhotoDate");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoMonitorings_PhotoType",
                table: "PhotoMonitorings",
                column: "PhotoType");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoMonitorings_ProjectId",
                table: "PhotoMonitorings",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoMonitorings_TenantId",
                table: "PhotoMonitorings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalProgresses_ProgressDate",
                table: "PhysicalProgresses",
                column: "ProgressDate");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalProgresses_ProjectId",
                table: "PhysicalProgresses",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalProgresses_Status",
                table: "PhysicalProgresses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalProgresses_TenantId",
                table: "PhysicalProgresses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAdjustments_AdjustmentDate",
                table: "PriceAdjustments",
                column: "AdjustmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAdjustments_ContractFinancialDetailId",
                table: "PriceAdjustments",
                column: "ContractFinancialDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAdjustments_ProjectId",
                table: "PriceAdjustments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAdjustments_Status",
                table: "PriceAdjustments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAdjustments_TenantId",
                table: "PriceAdjustments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ContractorId",
                table: "Projects",
                column: "ContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectCode",
                table: "Projects",
                column: "ProjectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectCode_TenantId",
                table: "Projects",
                columns: new[] { "ProjectCode", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId",
                table: "Projects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_Category",
                table: "RawMaterials",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_MaterialCode",
                table: "RawMaterials",
                column: "MaterialCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_ProjectId",
                table: "RawMaterials",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_Status",
                table: "RawMaterials",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_TenantId",
                table: "RawMaterials",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibleOfficials_ProjectId",
                table: "ResponsibleOfficials",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subcontractors_CompanyName",
                table: "Subcontractors",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_Subcontractors_ProjectId",
                table: "Subcontractors",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcontractors_Status",
                table: "Subcontractors",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Subcontractors_TenantId",
                table: "Subcontractors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeExtensions_ExtensionNumber",
                table: "TimeExtensions",
                column: "ExtensionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeExtensions_ProjectId",
                table: "TimeExtensions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeExtensions_RequestDate",
                table: "TimeExtensions",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_TimeExtensions_Status",
                table: "TimeExtensions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TimeExtensions_TenantId",
                table: "TimeExtensions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvancePaymentGuarantees");

            migrationBuilder.DropTable(
                name: "ApprovalWorkflows");

            migrationBuilder.DropTable(
                name: "DelayReasons");

            migrationBuilder.DropTable(
                name: "LabTests");

            migrationBuilder.DropTable(
                name: "PerformanceBonds");

            migrationBuilder.DropTable(
                name: "PhotoMonitorings");

            migrationBuilder.DropTable(
                name: "PhysicalProgresses");

            migrationBuilder.DropTable(
                name: "PriceAdjustments");

            migrationBuilder.DropTable(
                name: "RawMaterials");

            migrationBuilder.DropTable(
                name: "ResponsibleOfficials");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Subcontractors");

            migrationBuilder.DropTable(
                name: "TimeExtensions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "ContractFinancialDetails");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "ContractorOfficeDetails");
        }
    }
}
