import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

interface AppConfig {
  issuer: string;
}

@Injectable({ providedIn: 'root' })
export class ConfigService {
  private config: AppConfig = { issuer: '' };

  constructor(private http: HttpClient) {}

  async load(): Promise<void> {
    this.config = await firstValueFrom(this.http.get<AppConfig>('/api/config'));
  }

  get issuer(): string {
    return this.config.issuer;
  }
}
