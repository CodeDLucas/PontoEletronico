import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, TimeRecordService } from '../../../services';
import { TimeRecord, TimeRecordType } from '../../../models';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  currentUser$ = this.authService.currentUser$;
  recentRecords: TimeRecord[] = [];
  isLoading = false;
  todayStatus: 'not_started' | 'working' | 'finished' = 'not_started';

  constructor(
    private authService: AuthService,
    private timeRecordService: TimeRecordService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadRecentRecords();
    this.checkTodayStatus();
  }

  loadRecentRecords(): void {
    this.isLoading = true;
    this.timeRecordService.getTimeRecords(1, 5).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.recentRecords = response.data;
        }
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Erro ao carregar registros:', error);
        this.isLoading = false;
      }
    });
  }

  checkTodayStatus(): void {
    this.timeRecordService.getTodayTimeRecords().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const todayRecords = response.data;

          if (todayRecords.length === 0) {
            this.todayStatus = 'not_started';
          } else {
            // Ordena os registros por timestamp
            const sortedRecords = [...todayRecords].sort((a, b) =>
              new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
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
      }
    });
  }

  navigateToTimeClock(): void {
    this.router.navigate(['/time-clock']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
    this.snackBar.open('Logout realizado com sucesso!', 'Fechar', {
      duration: 3000,
      panelClass: ['success-snackbar']
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
        return 'primary';
      case TimeRecordType.ClockOut:
        return 'accent';
      case TimeRecordType.BreakStart:
        return 'warn';
      case TimeRecordType.BreakEnd:
        return 'primary';
      default:
        return 'basic';
    }
  }
}
