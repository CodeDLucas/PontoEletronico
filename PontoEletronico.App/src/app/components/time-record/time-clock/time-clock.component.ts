import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { TimeRecordService, TimezoneService, NotificationService } from '../../../services';
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
    public timezoneService: TimezoneService,
    private router: Router,
    private notificationService: NotificationService
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
        this.notificationService.handleHttpError(error, 'Erro ao carregar registros do dia.');
      }
    });
  }

  private checkWorkingStatus(): void {
    if (this.todayRecords.length === 0) {
      this.isWorking = false;
      this.lastClockIn = undefined;
      return;
    }

    // Ordena os registros por timestamp (convertendo para local para comparação)
    const sortedRecords = [...this.todayRecords].sort((a, b) =>
      this.timezoneService.toLocal(a.timestamp).getTime() - this.timezoneService.toLocal(b.timestamp).getTime()
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
      timestamp: this.timezoneService.nowUtc()
    };

    this.timeRecordService.createTimeRecord(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadTodayRecord();
          this.redirectToDashboard('Entrada registrada com sucesso!');
        } else {
          this.notificationService.handleErrorResponse(response, 'Erro ao registrar entrada. Tente novamente.');
        }
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Erro ao registrar entrada:', error);
        this.notificationService.handleHttpError(error, 'Erro ao registrar entrada. Tente novamente.');
        this.isLoading = false;
      }
    });
  }

  clockOut(): void {
    if (this.isLoading || !this.isWorking) return;

    this.isLoading = true;
    const request: CreateTimeRecordRequest = {
      type: TimeRecordType.ClockOut,
      timestamp: this.timezoneService.nowUtc()
    };

    this.timeRecordService.createTimeRecord(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadTodayRecord();
          this.redirectToDashboard('Saída registrada com sucesso!');
        } else {
          this.notificationService.handleErrorResponse(response, 'Erro ao registrar saída. Tente novamente.');
        }
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Erro ao registrar saída:', error);
        this.notificationService.handleHttpError(error, 'Erro ao registrar saída. Tente novamente.');
        this.isLoading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  getWorkingTime(): string {
    if (!this.isWorking || !this.lastClockIn) return '00:00:00';

    const entryTime = this.timezoneService.toLocal(this.lastClockIn.timestamp);
    const diffMs = this.currentTime.getTime() - entryTime.getTime();

    return this.timezoneService.formatDuration(diffMs);
  }

  /**
   * Redireciona para o dashboard com delay e mensagem informativa
   */
  private redirectToDashboard(successMessage: string): void {
    this.notificationService.showSuccessWithRedirect(
      successMessage,
      'Retornando ao dashboard...',
      1500
    );

    setTimeout(() => {
      this.router.navigate(['/dashboard']);
    }, 1500);
  }

}
