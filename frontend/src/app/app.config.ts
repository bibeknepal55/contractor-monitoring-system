import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter, withViewTransitions } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { JwtModule } from '@auth0/angular-jwt';
import { NgxSpinnerModule } from 'ngx-spinner';
import { MatNativeDateModule } from '@angular/material/core';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { routes } from './app.routes';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { environment } from '../environments/environment';

// Derive the domain from the API URL so it works in all environments
function getApiDomain(): string {
  try {
    return new URL(environment.apiUrl).host;
  } catch {
    return 'localhost:5185';
  }
}

// Used only by JwtHelperService for token decoding — NOT for attaching headers
// Header attachment is handled exclusively by jwtInterceptor
export function tokenGetter(): string | null {
  return null; // tokens are in memory, not localStorage
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withViewTransitions()),
    // Only our custom interceptors handle auth headers — JwtModule does NOT add its own interceptor here
    provideHttpClient(withInterceptors([jwtInterceptor, errorInterceptor])),
    provideAnimations(),
    importProvidersFrom(
      // JwtModule registered only for JwtHelperService (token decoding/expiry checks)
      // allowedDomains is set but header injection is disabled via tokenGetter returning null
      JwtModule.forRoot({
        config: {
          tokenGetter,
          allowedDomains: [getApiDomain()],
          disallowedRoutes: [],
        },
      }),
      NgxSpinnerModule.forRoot({ type: 'ball-spin-clockwise-fade' }),
      MatNativeDateModule,
      NgxChartsModule,
    ),
  ],
};
