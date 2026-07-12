import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgxSpinnerModule } from 'ngx-spinner';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NgxSpinnerModule],
  template: `
    <router-outlet></router-outlet>
    <ngx-spinner
      bdColor="rgba(0, 0, 0, 0.3)"
      size="medium"
      color="#1a73e8"
      type="ball-spin-clockwise-fade"
      [fullScreen]="true"
    ></ngx-spinner>
  `,
})
export class AppComponent {}