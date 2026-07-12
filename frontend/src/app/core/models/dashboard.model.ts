export interface ExecutiveDashboard {
  totalProjects: number;
  activeProjects: number;
  completedProjects: number;
  delayedProjects: number;
  totalContractors: number;
  totalBudget: number;
  totalSpent: number;
  budgetUtilization: number;
  averageProgress: number;
  pendingApprovals: number;
  activePerformanceBonds: number;
  expiringGuarantees: number;
  projectStatusDistribution: ProjectStatusDistribution[];
  monthlyProgress: MonthlyProgress[];
  topProjectsByBudget: TopProjectByBudget[];
  recentDelays: RecentDelay[];
}

export interface ProjectStatusDistribution {
  status: string;
  count: number;
  color: string;
}

export interface MonthlyProgress {
  month: string;
  planned: number;
  actual: number;
}

export interface TopProjectByBudget {
  projectId: string;
  projectName: string;
  budget: number;
  spent: number;
  progress: number;
}

export interface RecentDelay {
  projectName: string;
  contractorName: string;
  days: number;
  reason: string;
  date: string;
}