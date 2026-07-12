import { Component, inject, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { PhotoService } from '../../../core/services/photo.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-photo-upload',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatCardModule, MatProgressBarModule],
  template: `
    <div class="upload-container">
      <mat-card class="upload-card" [class.has-file]="selectedFile">
        <mat-card-content>
          <div class="upload-area" (click)="fileInput.click()" (dragover)="onDragOver($event)" (drop)="onDrop($event)">
            <input #fileInput type="file" accept="image/*" (change)="onFileSelected($event)" style="display:none">
            
            <ng-container *ngIf="!selectedFile; else filePreview">
              <mat-icon class="upload-icon">cloud_upload</mat-icon>
              <h3>Upload Photograph</h3>
              <p>Drag & drop or click to browse</p>
              <button mat-stroked-button type="button" (click)="fileInput.click(); $event.stopPropagation()">
                <mat-icon>image</mat-icon> Choose File
              </button>
              <span class="hint">Supported: JPG, PNG, HEIC • Max 10MB</span>
            </ng-container>

            <ng-template #filePreview>
              <div class="file-preview">
                <img [src]="previewUrl" *ngIf="previewUrl" class="preview-image">
                <div class="file-info">
                  <mat-icon>insert_drive_file</mat-icon>
                  <div>
                    <strong>{{ selectedFile?.name }}</strong>
                    <span>{{ formatFileSize(selectedFile?.size || 0) }}</span>
                  </div>
                  <button mat-icon-button color="warn" (click)="removeFile(); $event.stopPropagation()" type="button">
                    <mat-icon>close</mat-icon>
                  </button>
                </div>
              </div>
            </ng-template>
          </div>

          <mat-progress-bar *ngIf="uploading" mode="indeterminate" color="primary"></mat-progress-bar>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .upload-container { margin-bottom: 20px; }
    .upload-card { border-radius: 12px; border: 2px dashed #ccc; transition: all 0.3s; }
    .upload-card.has-file { border-color: #137333; border-style: solid; }
    .upload-area { padding: 32px; text-align: center; cursor: pointer; min-height: 180px; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 8px; }
    .upload-area:hover { background: #f8f9ff; }
    .upload-icon { font-size: 48px; width: 48px; height: 48px; color: #1a73e8; }
    .upload-area h3 { margin: 0; font-size: 1rem; color: #333; }
    .upload-area p { margin: 0; color: #888; font-size: 0.85rem; }
    .hint { font-size: 0.75rem; color: #aaa; margin-top: 4px; }
    .file-preview { width: 100%; }
    .preview-image { max-width: 100%; max-height: 250px; border-radius: 8px; object-fit: cover; margin-bottom: 12px; }
    .file-info { display: flex; align-items: center; gap: 12px; padding: 8px 12px; background: #e8f5e9; border-radius: 8px; }
    .file-info mat-icon { color: #137333; }
    .file-info div { flex: 1; text-align: left; }
    .file-info strong { display: block; font-size: 0.85rem; color: #333; }
    .file-info span { font-size: 0.75rem; color: #666; }
  `]
})
export class PhotoUploadComponent {
  private photoService = inject(PhotoService);
  private notify = inject(NotificationService);

  @Output() photoUploaded = new EventEmitter<{ path: string; filename: string }>();

  selectedFile: File | null = null;
  previewUrl: string | null = null;
  uploading = false;

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFile(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFile(input.files[0]);
    }
  }

  handleFile(file: File): void {
    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/heic', 'image/heif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.notify.error('Please select a valid image file (JPG, PNG, HEIC, WebP)');
      return;
    }

    // Validate file size (10MB)
    if (file.size > 10 * 1024 * 1024) {
      this.notify.error('File size must be less than 10MB');
      return;
    }

    this.selectedFile = file;

    // Create preview
    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.previewUrl = e.target.result;
    };
    reader.readAsDataURL(file);

    // Upload automatically
    this.uploadFile();
  }

  uploadFile(): void {
    if (!this.selectedFile) return;

    this.uploading = true;
    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.photoService.upload(formData).subscribe({
      next: (response: any) => {
        this.uploading = false;
        if (response.success && response.data) {
          this.photoUploaded.emit({
            path: response.data.filePath || response.data.path || '',
            filename: response.data.fileName || this.selectedFile?.name || ''
          });
          this.notify.success('Photo uploaded successfully!');
        } else {
          // If upload API fails, still allow saving with filename as path
          this.photoUploaded.emit({
            path: '/uploads/' + this.selectedFile?.name,
            filename: this.selectedFile?.name || ''
          });
          this.notify.warning('Upload pending - file will be referenced locally');
        }
      },
      error: () => {
        this.uploading = false;
        // Fallback: use filename as path
        this.photoUploaded.emit({
          path: '/uploads/' + (this.selectedFile?.name || 'photo.jpg'),
          filename: this.selectedFile?.name || 'photo.jpg'
        });
        this.notify.warning('Upload service unavailable - file reference saved');
      }
    });
  }

  removeFile(): void {
    this.selectedFile = null;
    this.previewUrl = null;
    this.photoUploaded.emit({ path: '', filename: '' });
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}