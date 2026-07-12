export interface PriceAdjustment {
  id: string;
  projectId: string;
  adjustmentType: string;
  amount: number;
  reason?: string;
  approvedBy?: string;
  approvalDate?: string;
  isApproved: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface PerformanceBond {
  id: string;
  projectId: string;
  contractorId: string;
  bondAmount: number;
  bondNumber?: string;
  issuingBank?: string;
  issueDate: string;
  expiryDate: string;
  status: BondStatus;
  createdAt: string;
  updatedAt?: string;
}

export enum BondStatus {
  Active = 'Active',
  Expired = 'Expired',
  Released = 'Released',
  Forfeited = 'Forfeited',
}

export const BOND_STATUS_COLORS: Record<string, string> = {
  [BondStatus.Active]: '#388e3c',
  [BondStatus.Expired]: '#d32f2f',
  [BondStatus.Released]: '#1976d2',
  [BondStatus.Forfeited]: '#f57c00',
};

export interface AdvancePaymentGuarantee {
  id: string;
  projectId: string;
  contractorId: string;
  guaranteeAmount: number;
  guaranteeNumber?: string;
  issuingBank?: string;
  issueDate: string;
  expiryDate: string;
  status: GuaranteeStatus;
  createdAt: string;
  updatedAt?: string;
}

export enum GuaranteeStatus {
  Active = 'Active',
  Expired = 'Expired',
  Returned = 'Returned',
  Encashed = 'Encashed',
}

export const GUARANTEE_STATUS_COLORS: Record<string, string> = {
  [GuaranteeStatus.Active]: '#388e3c',
  [GuaranteeStatus.Expired]: '#d32f2f',
  [GuaranteeStatus.Returned]: '#1976d2',
  [GuaranteeStatus.Encashed]: '#f57c00',
};

export interface PhysicalProgress {
  id: string;
  projectId: string;
  contractorId: string;
  plannedProgress: number;
  actualProgress: number;
  measurementDate: string;
  remarks?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface TimeExtension {
  id: string;
  projectId: string;
  contractorId: string;
  originalCompletionDate: string;
  extendedCompletionDate: string;
  extensionDays: number;
  reason?: string;
  approvedBy?: string;
  approvalDate?: string;
  isApproved: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface DelayReason {
  id: string;
  projectId: string;
  contractorId: string;
  delayType: string;
  description: string;
  impactDays: number;
  startDate: string;
  endDate?: string;
  isResolved: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface RawMaterial {
  id: string;
  projectId: string;
  contractorId: string;
  materialName: string;
  quantity: number;
  unit: string;
  testStatus: MaterialTestStatus;
  receivedDate: string;
  supplierName?: string;
  batchNumber?: string;
  createdAt: string;
  updatedAt?: string;
}

export enum MaterialTestStatus {
  Pending = 'Pending',
  Tested = 'Tested',
  Approved = 'Approved',
  Rejected = 'Rejected',
}

export const MATERIAL_TEST_STATUS_COLORS: Record<string, string> = {
  [MaterialTestStatus.Pending]: '#f57c00',
  [MaterialTestStatus.Tested]: '#1976d2',
  [MaterialTestStatus.Approved]: '#388e3c',
  [MaterialTestStatus.Rejected]: '#d32f2f',
};

export interface LabTest {
  id: string;
  projectId: string;
  materialId?: string;
  testName: string;
  testResult: string;
  testDate: string;
  labName?: string;
  technicianName?: string;
  isCompliant: boolean;
  reportNumber?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface PhotoMonitoring {
  id: string;
  projectId: string;
  contractorId?: string;
  title: string;
  description?: string;
  photoDate: string;
  fileNames: string[];
  fileUrls: string[];
  uploadedBy: string;
  createdAt: string;
  updatedAt?: string;
}

export interface Subcontractor {
  id: string;
  projectId: string;
  contractorId: string;
  companyName: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  scopeOfWork?: string;
  contractAmount: number;
  startDate: string;
  endDate?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ResponsibleOfficial {
  id: string;
  projectId: string;
  contractorId?: string;
  name: string;
  designation: string;
  department?: string;
  email?: string;
  phone?: string;
  isActive: boolean;
  appointmentDate: string;
  createdAt: string;
  updatedAt?: string;
}

export interface ApprovalWorkflow {
  id: string;
  moduleName: string;
  recordId: string;
  recordTitle: string;
  submittedBy: string;
  submittedByName?: string;
  submittedAt: string;
  comments?: string;
  approvalLevel: number;
  status: ApprovalStatus;
  processedBy?: string;
  processedByName?: string;
  processedAt?: string;
  processedComments?: string;
  createdAt: string;
  updatedAt?: string;
}

export enum ApprovalStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected',
}

export const APPROVAL_STATUS_COLORS: Record<string, string> = {
  [ApprovalStatus.Pending]: '#f57c00',
  [ApprovalStatus.Approved]: '#388e3c',
  [ApprovalStatus.Rejected]: '#d32f2f',
};

export interface SubmitApprovalRequest {
  moduleName: string;
  recordId: string;
  recordTitle: string;
  comments: string;
  approvalLevel: number;
}

export interface ProcessApprovalRequest {
  action: 'Approved' | 'Rejected';
  comments: string;
}

export interface ReportType {
  value: string;
  label: string;
  description: string;
}

export const REPORT_TYPES: ReportType[] = [
  { value: 'contractor-wise', label: 'Contractor Performance Report', description: 'Detailed performance metrics by contractor' },
  { value: 'project-wise', label: 'Project Status Report', description: 'Comprehensive project status overview' },
  { value: 'delay-analysis', label: 'Delay Analysis Report', description: 'Analysis of project delays and their impact' },
  { value: 'pb-apg', label: 'Performance Bond & APG Report', description: 'Status of all performance bonds and APGs' },
  { value: 'time-extension', label: 'Time Extension Report', description: 'Summary of all time extensions' },
  { value: 'payment-pending', label: 'Payment Pending Report', description: 'Overview of pending payments' },
];

export interface GenerateReportRequest {
  reportType: string;
  startDate?: string;
  endDate?: string;
  projectId?: string;
  contractorId?: string;
  status?: string;
  format?: string;
}

export interface ExportRequest {
  reportType: string;
  format: 'excel' | 'pdf';
  startDate?: string;
  endDate?: string;
  projectId?: string;
}