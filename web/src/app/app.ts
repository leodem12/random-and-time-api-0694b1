import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { ApiService, RandomAttempt, TimeAttempt } from './api';

type ResultState =
  | { kind: 'none' }
  | { kind: 'random'; attempt: RandomAttempt }
  | { kind: 'randomList'; attempts: RandomAttempt[] }
  | { kind: 'time'; attempt: TimeAttempt }
  | { kind: 'timeList'; attempts: TimeAttempt[] }
  | { kind: 'error'; message: string };

@Component({
  selector: 'app-root',
  imports: [MatToolbarModule, MatButtonModule, MatCardModule, MatProgressBarModule, MatTableModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  private api = inject(ApiService);

  loading = signal(false);
  state = signal<ResultState>({ kind: 'none' });

  readonly randomHistoryColumns = ['id', 'value', 'createdAt'];
  readonly timeHistoryColumns = ['id', 'serverTimeUtc'];

  getRandom(): void {
    this.loading.set(true);
    this.state.set({ kind: 'none' });
    this.api.getRandom().subscribe({
      next: (r) => {
        this.state.set({ kind: 'random', attempt: r });
        this.loading.set(false);
      },
      error: () => {
        this.state.set({ kind: 'error', message: 'Failed to get random number. Is the API running?' });
        this.loading.set(false);
      }
    });
  }

  getRandomHistory(): void {
    this.loading.set(true);
    this.state.set({ kind: 'none' });
    this.api.getRandomHistory().subscribe({
      next: (list) => {
        this.state.set({ kind: 'randomList', attempts: list });
        this.loading.set(false);
      },
      error: () => {
        this.state.set({ kind: 'error', message: 'Failed to get random history. Is the API running?' });
        this.loading.set(false);
      }
    });
  }

  getNow(): void {
    this.loading.set(true);
    this.state.set({ kind: 'none' });
    this.api.getNow().subscribe({
      next: (r) => {
        this.state.set({ kind: 'time', attempt: r });
        this.loading.set(false);
      },
      error: () => {
        this.state.set({ kind: 'error', message: 'Failed to get server time. Is the API running?' });
        this.loading.set(false);
      }
    });
  }

  getNowHistory(): void {
    this.loading.set(true);
    this.state.set({ kind: 'none' });
    this.api.getNowHistory().subscribe({
      next: (list) => {
        this.state.set({ kind: 'timeList', attempts: list });
        this.loading.set(false);
      },
      error: () => {
        this.state.set({ kind: 'error', message: 'Failed to get time history. Is the API running?' });
        this.loading.set(false);
      }
    });
  }
}
