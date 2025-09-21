import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { interval, Subscription } from 'rxjs';
import { TimeRecordService } from '../../../services';
import { CreateTimeRecordRequest, TimeRecord, TimeRecordType } from '../../../models';

@Component({
  selector: 'app-time-clock',
  templateUrl: './time-clock.component.html',
  styleUrls: ['./time-clock.component.scss']
})
export class TimeClockComponent implements OnInit, OnDestroy {
  currentTime = new Date();
  isLoading = false;
  todayRecords: TimeRecord[] = [];
  isWorking = false;
  lastClockIn?: TimeRecord;
  private timeSubscription?: Subscription;

  constructor(
    private timeRecordService: TimeRecordService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.startClock();
    this.loadTodayRecord();
  }

  ngOnDestroy(): void {
    if (this.timeSubscription) {
      this.timeSubscription.unsubscribe();
    }
  }

  private startClock(): void {
    this.timeSubscription = interval(1000).subscribe(() => {
      this.currentTime = new Date();
    });
  }

  private loadTodayRecord(): void {
    this.timeRecordService.getTodayTimeRecords().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.todayRecords = response.data;
          this.checkWorkingStatus();
        }
      },
      error: (error: any) => {
        console.error('Erro ao carregar registros do dia:', error);
      }
    });
  }

  private checkWorkingStatus(): void {
    if (this.todayRecords.length === 0) {
      this.isWorking = false;
      this.lastClockIn = undefined;
      return;
    }

    // Ordena os registros por timestamp
    const sortedRecords = [...this.todayRecords].sort((a, b) =>
      new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
    );

    const lastRecord = sortedRecords[sortedRecords.length - 1];

    if (lastRecord.type === TimeRecordType.ClockIn) {
      this.isWorking = true;
      this.lastClockIn = lastRecord;
    } else {
      this.isWorking = false;
      this.lastClockIn = undefined;
    }
  }

  clockIn(): void {
    if (this.isLoading || this.isWorking) return;

    this.isLoading = true;
    const request: CreateTimeRecordRequest = {
      type: TimeRecordType.ClockIn,
      timestamp: new Date()
    };

    this.timeRecordService.createTimeRecord(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.showSuccessMessage('Entrada registrada com sucesso!');
          this.loadTodayRecord();
        } else {
          this.showErrorMessage(response.message);
        }
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Erro ao registrar entrada:', error);
        this.showErrorMessage('Erro ao registrar entrada. Tente novamente.');
        this.isLoading = false;
      }
    });
  }

  clockOut(): void {
    if (this.isLoading || !this.isWorking) return;

    this.isLoading = true;
    const request: CreateTimeRecordRequest = {
      type: TimeRecordType.ClockOut,
      timestamp: new Date()
    };

    this.timeRecordService.createTimeRecord(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.showSuccessMessage('Saída registrada com sucesso!');
          this.loadTodayRecord();
        } else {
          this.showErrorMessage(response.message);
        }
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Erro ao registrar saída:', error);
        this.showErrorMessage('Erro ao registrar saída. Tente novamente.');
        this.isLoading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  getWorkingTime(): string {
    if (!this.isWorking || !this.lastClockIn) return '00:00:00';

    const entryTime = new Date(this.lastClockIn.timestamp);
    const diffMs = this.currentTime.getTime() - entryTime.getTime();

    const hours = Math.floor(diffMs / (1000 * 60 * 60));
    const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((diffMs % (1000 * 60)) / 1000);

    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  private showSuccessMessage(message: string): void {
    this.snackBar.open(message, 'Fechar', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  private showErrorMessage(message: string): void {
    this.snackBar.open(message, 'Fechar', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}
