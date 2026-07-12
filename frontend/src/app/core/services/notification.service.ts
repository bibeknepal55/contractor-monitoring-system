import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 4000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['success-snackbar'],
    });
  }

  error(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 6000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['error-snackbar'],
    });
  }

  warning(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['warning-snackbar'],
    });
  }

  info(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 4000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['info-snackbar'],
    });
  }

  async confirmDelete(itemName: string): Promise<boolean> {
    const result = await Swal.fire({
      title: 'Confirm Deletion',
      html: `Are you sure you want to delete <strong>${itemName}</strong>?<br>This action cannot be undone.`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d32f2f',
      cancelButtonColor: '#757575',
      confirmButtonText: 'Yes, delete it',
      cancelButtonText: 'Cancel',
      reverseButtons: true,
    });
    return result.isConfirmed;
  }

  async confirmAction(title: string, message: string, confirmText: string = 'Yes', icon: 'warning' | 'question' = 'warning'): Promise<boolean> {
    const result = await Swal.fire({
      title,
      html: message,
      icon,
      showCancelButton: true,
      confirmButtonColor: '#1976d2',
      cancelButtonColor: '#757575',
      confirmButtonText: confirmText,
      cancelButtonText: 'Cancel',
      reverseButtons: true,
    });
    return result.isConfirmed;
  }

  async showPrompt(title: string, inputLabel: string, inputPlaceholder: string = ''): Promise<string | null> {
    const result = await Swal.fire({
      title,
      input: 'textarea',
      inputLabel,
      inputPlaceholder,
      showCancelButton: true,
      confirmButtonColor: '#1976d2',
      cancelButtonColor: '#757575',
      confirmButtonText: 'Submit',
      cancelButtonText: 'Cancel',
      inputValidator: (value) => {
        if (!value || value.trim().length === 0) {
          return 'Please enter a value';
        }
        return null;
      },
    });
    return result.isConfirmed ? result.value : null;
  }

  showLoading(title: string = 'Please wait...'): void {
    Swal.fire({
      title,
      allowOutsideClick: false,
      allowEscapeKey: false,
      showConfirmButton: false,
      willOpen: () => {
        Swal.showLoading();
      },
    });
  }

  hideLoading(): void {
    Swal.close();
  }
}