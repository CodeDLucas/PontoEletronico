import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';
import { interval, Subscription } from 'rxjs';
import { AuthService, TimeRecordService, TimezoneService, NotificationService } from '../../../services';
import { TimeRecord, TimeRecordType, PagedResponse } from '../../../models';
import { MatPaginator, PageEvent } from '@angular/material/paginator';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  currentUser$ = this.authService.currentUser$;
  recentRecords: TimeRecord[] = [];
  isLoading = false;
  todayStatus: 'not_started' | 'working' | 'finished' = 'not_started';

  // Clock properties
  currentTime = new Date();
  private timeSubscription?: Subscription;

  // Pagination properties
  pageSize = 10;
  currentPage = 1;
  totalRecords = 0;
  showPagination = false;

  // Filter properties
  filterForm!: FormGroup;

  constructor(
    private authService: AuthService,
    private timeRecordService: TimeRecordService,
    public timezoneService: TimezoneService,
    private router: Router,
    private notificationService: NotificationService,
    private formBuilder: FormBuilder
  ) {}

  ngOnInit(): void {
    this.initializeFilterForm();
    this.startClock();
    this.loadRecentRecords(1, this.pageSize);
    this.checkTodayStatus();
  }

  ngOnDestroy(): void {
    if (this.timeSubscription) {
      this.timeSubscription.unsubscribe();
    }
  }

  private initializeFilterForm(): void {
    this.filterForm = this.formBuilder.group({
      startDate: [null],
      endDate: [null]
    });
  }

  private startClock(): void {
    this.timeSubscription = interval(1000).subscribe(() => {
      this.currentTime = new Date();
    });
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadRecentRecords(this.currentPage, this.pageSize);
  }

  loadRecentRecords(page: number = 1, pageSize: number = 10): void {
    this.isLoading = true;

    // Build filter from form
    const startDate = this.filterForm?.get('startDate')?.value;
    const endDate = this.filterForm?.get('endDate')?.value;

    // Use default method but add filter parameters manually
    if (startDate || endDate) {
      // Create filter parameters and call the same getTimeRecords method
      this.timeRecordService.getTimeRecordsWithDateFilter(
        page,
        pageSize,
        startDate ? this.timezoneService.utcToDateString(startDate) : undefined,
        endDate ? this.timezoneService.utcToDateString(endDate) : undefined
      ).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.recentRecords = response.data.data;
            this.totalRecords = response.data.totalCount;
            this.currentPage = response.data.page;
            this.pageSize = response.data.pageSize;
            this.showPagination = response.data.totalCount > pageSize;
          }
          this.isLoading = false;
        },
        error: (error: any) => {
          console.error('Erro ao carregar registros filtrados:', error);
          this.isLoading = false;
          this.notificationService.handleHttpError(error, 'Erro ao carregar registros.');
        }
      });
    } else {
      this.timeRecordService.getTimeRecords(page, pageSize).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.recentRecords = response.data.data;
            this.totalRecords = response.data.totalCount;
            this.currentPage = response.data.page;
            this.pageSize = response.data.pageSize;
            this.showPagination = response.data.totalCount > pageSize;
          }
          this.isLoading = false;
        },
        error: (error: any) => {
          console.error('Erro ao carregar registros:', error);
          this.isLoading = false;
          this.notificationService.handleHttpError(error, 'Erro ao carregar registros.');
        }
      });
    }
  }

  checkTodayStatus(): void {
    this.timeRecordService.getTodayTimeRecords().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const todayRecords = response.data;

          if (todayRecords.length === 0) {
            this.todayStatus = 'not_started';
          } else {
            // Ordena os registros por timestamp (convertendo para local para comparação)
            const sortedRecords = [...todayRecords].sort((a, b) =>
              this.timezoneService.toLocal(a.timestamp).getTime() - this.timezoneService.toLocal(b.timestamp).getTime()
            );

            const lastRecord = sortedRecords[sortedRecords.length - 1];

            if (lastRecord.type === TimeRecordType.ClockIn) {
              this.todayStatus = 'working';
            } else {
              this.todayStatus = 'finished';
            }
          }
        }
      },
      error: (error: any) => {
        console.error('Erro ao verificar status do dia:', error);
        this.notificationService.handleHttpError(error, 'Erro ao verificar status do dia.');
      }
    });
  }

  navigateToTimeClock(): void {
    this.router.navigate(['/time-clock']);
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: (response) => {
        this.notificationService.handleApiResult(
          response,
          'Logout realizado com sucesso!',
          'Erro ao realizar logout'
        );
        this.router.navigate(['/login']);
      },
      error: (error: any) => {
        // Mesmo em caso de erro no logout, redirecionamos para o login
        console.error('Erro no logout:', error);
        this.notificationService.showWarning('Sessão encerrada localmente.');
        this.router.navigate(['/login']);
      }
    });
  }

  getTodayStatusText(): string {
    switch (this.todayStatus) {
      case 'not_started': return 'Não iniciado';
      case 'working': return 'Trabalhando';
      case 'finished': return 'Finalizado';
      default: return 'Desconhecido';
    }
  }

  getTodayStatusColor(): string {
    switch (this.todayStatus) {
      case 'not_started': return 'warn';
      case 'working': return 'primary';
      case 'finished': return 'accent';
      default: return 'basic';
    }
  }

  getTypeColor(type: TimeRecordType): string {
    switch (type) {
      case TimeRecordType.ClockIn:
        return 'clock-in';
      case TimeRecordType.ClockOut:
        return 'clock-out';
      case TimeRecordType.BreakStart:
        return 'warn';
      case TimeRecordType.BreakEnd:
        return 'primary';
      default:
        return 'basic';
    }
  }

  formatTimestamp(timestamp: Date): string {
    return this.timezoneService.formatDateTime(timestamp);
  }

  formatTimeOnly(timestamp: Date): string {
    return this.timezoneService.formatTimeOnly(timestamp);
  }

  formatDateOnly(timestamp: Date): string {
    return this.timezoneService.formatDateOnly(timestamp);
  }


  applyFilters(): void {
    this.currentPage = 1; // Reset to first page when applying filters
    this.loadRecentRecords(1, this.pageSize);
  }

  clearFilters(): void {
    this.filterForm.patchValue({
      startDate: null,
      endDate: null
    });
    this.currentPage = 1;
    this.loadRecentRecords(1, this.pageSize);
  }
}
