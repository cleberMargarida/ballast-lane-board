import { Injectable } from '@angular/core';
import { AuthConfig, OAuthService } from 'angular-oauth2-oidc';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ConfigService } from './config.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    private oauthService: OAuthService,
    private http: HttpClient,
    private configService: ConfigService,
  ) {}

  async init(): Promise<void> {
    await this.configService.load();
    const authConfig: AuthConfig = {
      issuer: this.configService.issuer,
      redirectUri: window.location.origin + '/callback',
      postLogoutRedirectUri: window.location.origin,
      clientId: 'ballast-lane-board-spa',
      scope: 'openid profile email',
      responseType: 'code',
      useSilentRefresh: true,
    };
    this.oauthService.configure(authConfig);
    this.oauthService.setupAutomaticSilentRefresh();
    await this.oauthService.loadDiscoveryDocumentAndTryLogin();
    if (this.isLoggedIn) {
      this.syncUser();
    }
  }

  login(): void {
    this.oauthService.initCodeFlow();
  }

  logout(): void {
    this.oauthService.logOut();
  }

  get isLoggedIn(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  get accessToken(): string {
    return this.oauthService.getAccessToken();
  }

  get username(): string | null {
    const claims = this.oauthService.getIdentityClaims();
    return claims ? (claims as Record<string, string>)['preferred_username'] : null;
  }

  get roles(): string[] {
    const token = this.oauthService.getAccessToken();
    if (!token) return [];
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload?.realm_access?.roles ?? [];
  }

  get isAdmin(): boolean {
    return this.roles.includes('admin');
  }

  private syncUser(): void {
    firstValueFrom(this.http.post('/api/auth/sync', {})).catch(() => {});
  }
}
