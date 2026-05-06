import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggle } from '@angular/material/slide-toggle';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TrimOnBlurDirective } from '../../../core/directives/trim-on-blur.directive';
import { ApiLog } from '../models/api-log.model';
import { ApiLogGetRequest } from '../models/api-log-get-request.model';
import { ApiLogService } from '../services/api-log.service';

@Component({
  selector: 'app-api-log-list',
  templateUrl: './api-log-list.component.html',
  styleUrl: './api-log-list.component.scss',
  imports: [
    DatePipe,
    FormsModule,
    TrimOnBlurDirective,
    MatButtonModule,
    MatDatepickerModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatIcon,
    MatInput,
    MatPaginatorModule,
    MatProgressSpinner,
    MatSelectModule,
    MatSlideToggle,
    MatSortModule,
    MatTableModule,
    MatTooltipModule,
  ],
})
export class ApiLogListComponent implements OnInit {
  private readonly apiLogService = inject(ApiLogService);

  readonly logs = signal<ApiLog[]>([]);
  readonly totalCount = signal(0);
  readonly isLoading = signal(false);

  readonly pageSize = signal(10);
  readonly pageIndex = signal(0);
  readonly pageSizeOptions = [10, 25, 50, 100];

  readonly sortBy = signal('executionTime');
  readonly sortDirection = signal<'asc' | 'desc'>('desc');

  readonly filterServiceName = signal('');
  readonly filterStatusCode = signal('');
  readonly filterHasException = signal(false);
  readonly filterStartDate = signal<Date | null>(null);
  readonly filterEndDate = signal<Date | null>(null);

  readonly displayedColumns: string[] = [
    'executionTime', 'method', 'service', 'statusCode',
    'duration', 'user', 'ip', 'exception',
  ];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);

    const request: ApiLogGetRequest = {
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortBy(),
      sortDirection: this.sortDirection(),
      searchKeyword: null,
      startDate: this.filterStartDate()?.toISOString() ?? null,
      endDate: this.filterEndDate()?.toISOString() ?? null,
      userName: null,
      serviceName: this.filterServiceName() || null,
      methodName: null,
      statusCode: this.filterStatusCode() ? +this.filterStatusCode() : null,
      hasException: this.filterHasException() || null,
      minExecutionDuration: null,
      maxExecutionDuration: null,
    };

    this.apiLogService.getLogs(request).subscribe({
      next: result => {
        this.logs.set(result.items);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  onSortChange(sort: Sort): void {
    this.sortBy.set(sort.active);
    this.sortDirection.set((sort.direction || 'desc') as 'asc' | 'desc');
    this.pageIndex.set(0);
    this.load();
  }

  applyFilters(): void {
    this.pageIndex.set(0);
    this.load();
  }

  clearFilters(): void {
    this.filterServiceName.set('');
    this.filterStatusCode.set('');
    this.filterHasException.set(false);
    this.filterStartDate.set(null);
    this.filterEndDate.set(null);
    this.pageIndex.set(0);
    this.load();
  }
}
