export interface ContractFinancialDetail {
  id: string;
  projectId: string;
  contractorId: string;
  contractAmount: number;
  paidAmount: number;
  pendingAmount: number;
  paymentStatus: PaymentStatus;
  lastPaymentDate?: string;
  nextPaymentDue?: string;
  createdAt: string;
  updatedAt?: string;
}

export enum PaymentStatus {
  Pending = 'Pending',
  Partial = 'Partial',
  Paid = 'Paid',
  Overdue = 'Overdue',
}

export const PAYMENT_STATUS_COLORS: Record<string, string> = {
  [PaymentStatus.Pending]: '#f57c00',
  [PaymentStatus.Partial]: '#1976d2',
  [PaymentStatus.Paid]: '#388e3c',
  [PaymentStatus.Overdue]: '#d32f2f',
};