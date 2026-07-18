import { Injectable, inject } from '@angular/core';
import { AuthService } from './auth.service';

export interface TrainingProgress {
  completed: boolean;
  completedModules: string[];
  lastOpened: string;
}

@Injectable({ providedIn: 'root' })
export class TrainingService {
  private auth = inject(AuthService);

  private getStorageKey(): string {
    const user = this.auth.getCurrentUser();
    const userId = user?.id || 'anonymous';
    return `training_progress_${userId}`;
  }

  getProgress(): TrainingProgress {
    const key = this.getStorageKey();
    const saved = localStorage.getItem(key);
    if (saved) {
      try { return JSON.parse(saved); } catch { return this.getDefault(); }
    }
    return this.getDefault();
  }

  private getDefault(): TrainingProgress {
    return { completed: false, completedModules: [], lastOpened: '' };
  }

  markModuleComplete(moduleName: string): void {
    const progress = this.getProgress();
    if (!progress.completedModules.includes(moduleName)) {
      progress.completedModules.push(moduleName);
    }
    progress.lastOpened = new Date().toISOString();
    this.saveProgress(progress);
  }

  markAllComplete(): void {
    const progress = this.getProgress();
    progress.completed = true;
    progress.lastOpened = new Date().toISOString();
    this.saveProgress(progress);
  }

  isNewUser(): boolean {
    const progress = this.getProgress();
    return !progress.completed && progress.completedModules.length === 0;
  }

  shouldShowPopup(): boolean {
    return this.isNewUser();
  }

  saveProgress(progress: TrainingProgress): void {
    localStorage.setItem(this.getStorageKey(), JSON.stringify(progress));
  }

  getCompletionPercentage(totalModules: number): number {
    const progress = this.getProgress();
    if (progress.completed) return 100;
    return Math.round((progress.completedModules.length / totalModules) * 100);
  }
}