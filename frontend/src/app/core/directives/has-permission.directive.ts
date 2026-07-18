import { Directive, Input, TemplateRef, ViewContainerRef, inject, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Subscription } from 'rxjs';

@Directive({
  selector: '[appHasPermission]',
  standalone: true,
})
export class HasPermissionDirective implements OnInit, OnDestroy {
  private auth = inject(AuthService);
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private userSub!: Subscription;

  private requiredPermission: string = '';

  @Input('appHasPermission')
  set permission(perm: string) {
    this.requiredPermission = perm;
    this.updateView();
  }

  ngOnInit(): void {
    this.userSub = this.auth.currentUser$.subscribe(() => {
      this.updateView();
    });
  }

  ngOnDestroy(): void {
    if (this.userSub) this.userSub.unsubscribe();
  }

  private updateView(): void {
    if (this.auth.hasPermission(this.requiredPermission)) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
}

@Directive({
  selector: '[appHasRole]',
  standalone: true,
})
export class HasRoleDirective implements OnInit, OnDestroy {
  private auth = inject(AuthService);
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private userSub!: Subscription;

  @Input('appHasRole')
  set role(role: string) {
    this.updateView(role);
  }

  ngOnInit(): void {
    this.userSub = this.auth.currentUser$.subscribe(() => {
      // Re-evaluate with the last set role
    });
  }

  ngOnDestroy(): void {
    if (this.userSub) this.userSub.unsubscribe();
  }

  private updateView(role: string): void {
    if (this.auth.hasRole(role)) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
}

@Directive({
  selector: '[appHasAnyRole]',
  standalone: true,
})
export class HasAnyRoleDirective implements OnInit, OnDestroy {
  private auth = inject(AuthService);
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private userSub!: Subscription;

  @Input('appHasAnyRole')
  set roles(roles: string[]) {
    this.updateView(roles);
  }

  ngOnInit(): void {
    this.userSub = this.auth.currentUser$.subscribe(() => {});
  }

  ngOnDestroy(): void {
    if (this.userSub) this.userSub.unsubscribe();
  }

  private updateView(roles: string[]): void {
    if (this.auth.hasAnyRole(roles)) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
}