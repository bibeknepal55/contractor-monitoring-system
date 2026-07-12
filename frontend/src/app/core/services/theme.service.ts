import { Injectable, inject, Renderer2, RendererFactory2 } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private renderer: Renderer2;
  private darkMode = new BehaviorSubject<boolean>(false);
  darkMode$ = this.darkMode.asObservable();

  constructor() {
    const factory = inject(RendererFactory2);
    this.renderer = factory.createRenderer(null, null);
    // Check saved preference
    const saved = localStorage.getItem('theme');
    if (saved === 'dark') this.enableDark();
  }

  enableDark(): void {
    this.renderer.addClass(document.body, 'dark-theme');
    this.darkMode.next(true);
    localStorage.setItem('theme', 'dark');
  }

  enableLight(): void {
    this.renderer.removeClass(document.body, 'dark-theme');
    this.darkMode.next(false);
    localStorage.setItem('theme', 'light');
  }

  toggle(): void {
    this.darkMode.value ? this.enableLight() : this.enableDark();
  }
}