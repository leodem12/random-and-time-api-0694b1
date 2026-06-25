import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RandomAttempt {
  id: number;
  value: number;
  createdAt: string;
}

export interface TimeAttempt {
  id: number;
  serverTimeUtc: string;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  getRandom(): Observable<RandomAttempt> {
    return this.http.get<RandomAttempt>('/api/random');
  }

  getRandomHistory(): Observable<RandomAttempt[]> {
    return this.http.get<RandomAttempt[]>('/api/random/history');
  }

  getNow(): Observable<TimeAttempt> {
    return this.http.get<TimeAttempt>('/api/now');
  }

  getNowHistory(): Observable<TimeAttempt[]> {
    return this.http.get<TimeAttempt[]>('/api/now/history');
  }
}
