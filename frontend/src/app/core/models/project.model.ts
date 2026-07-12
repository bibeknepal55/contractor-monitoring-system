export interface Project {
  id: string;
  projectCode: string;
  projectName: string;
  description?: string;
  startDate: string;
  endDate?: string;
  status: string;
  budget: number;
  location?: string;
  projectManager?: string;
  contactNumber?: string;
  contractNumber?: string;
  priority: string;
  contractorId?: string;
  contractorName?: string;
  progress?: number;
  createdAt: string;
  updatedAt?: string;
}

export enum ProjectStatus {
  Planned = 'Planned',
  Active = 'Active',
  OnHold = 'OnHold',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  Delayed = 'Delayed',
}

export enum ProjectPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical',
}

export const PROJECT_STATUS_OPTIONS = [
  { value: 'Planned', label: 'Planned' },
  { value: 'Active', label: 'Active' },
  { value: 'OnHold', label: 'On Hold' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Cancelled', label: 'Cancelled' },
  { value: 'Delayed', label: 'Delayed' },
];

export const PROJECT_PRIORITY_OPTIONS = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' },
];

export const PROJECT_STATUS_COLORS: Record<string, string> = {
  'Planned': '#1976d2',
  'Active': '#388e3c',
  'OnHold': '#f57c00',
  'Completed': '#689f38',
  'Cancelled': '#d32f2f',
  'Delayed': '#fbc02d',
};