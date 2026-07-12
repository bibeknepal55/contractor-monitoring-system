export interface Contractor {
  id: string;
  companyName: string;
  registrationNumber?: string;
  taxId?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  phone?: string;
  email?: string;
  website?: string;
  contactPerson?: string;
  contactPersonPhone?: string;
  contactPersonEmail?: string;
  licenseNumber?: string;
  licenseExpiryDate?: string;
  insuranceDetails?: string;
  status: string;
  isActive?: boolean;
  createdAt: string;
  updatedAt?: string;
}

export enum ContractorStatus {
  Active = 'Active',
  Inactive = 'Inactive',
  Blacklisted = 'Blacklisted',
  UnderReview = 'UnderReview',
}

export const CONTRACTOR_STATUS_OPTIONS = [
  { value: ContractorStatus.Active, label: 'Active', color: '#388e3c' },
  { value: ContractorStatus.Inactive, label: 'Inactive', color: '#757575' },
  { value: ContractorStatus.Blacklisted, label: 'Blacklisted', color: '#d32f2f' },
  { value: ContractorStatus.UnderReview, label: 'Under Review', color: '#f57c00' },
];

export const CONTRACTOR_STATUS_COLORS: Record<string, string> = {
  [ContractorStatus.Active]: '#388e3c',
  [ContractorStatus.Inactive]: '#757575',
  [ContractorStatus.Blacklisted]: '#d32f2f',
  [ContractorStatus.UnderReview]: '#f57c00',
};