import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TrimOnBlurDirective } from '../../../core/directives/trim-on-blur.directive';
import { UserService } from '../services/user.service';
import { UserDialogComponent } from '../user-dialog/user-dialog.component';
import { UserDialogData } from '../models/user-dialog-data.model';
import { User } from '../models/user.model';
import { UserRole } from '../../../core/enums/user-role.enum';

/**
 * Admin-only paginated user management table.
 *
 * Supports server-side pagination, sorting, and multi-field filtering (name,
 * email, role, active status, system user flag). Filters reset the page to 0
 * before reloading. All filter state is held in signals — no route params or
 * query string synchronisation.
 *
 * Opens `UserDialogComponent` for both create (null userId) and edit (string userId).
 * Reloads the list after the dialog closes with a saved result.
 */
@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss',
  imports: [
    FormsModule,
    TrimOnBlurDirective,
    MatButtonModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatIcon,
    MatInput,
    MatPaginatorModule,
    MatProgressSpinner,
    MatSelectModule,
    MatSlideToggleModule,
    MatSortModule,
    MatTableModule,
    MatTooltipModule,
  ],
})
export class UserListComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly dialog = inject(MatDialog);

  readonly UserRole = UserRole;
  readonly roles = Object.values(UserRole);

  readonly users = signal<User[]>([]);
  readonly totalCount = signal(0);
  readonly isLoading = signal(false);

  readonly pageSize = signal(10);
  readonly pageIndex = signal(0);
  readonly pageSizeOptions = [10, 25, 50];

  readonly sortBy = signal('firstName');
  readonly sortDirection = signal<'asc' | 'desc'>('asc');

  readonly filterName = signal('');
  readonly filterEmail = signal('');
  readonly filterRole = signal<UserRole | null>(null);
  readonly filterIsActive = signal<boolean | null>(null);
  readonly filterIsSystemUser = signal<boolean | null>(null);

  readonly displayedColumns = ['firstName', 'lastName', 'email', 'phone', 'role', 'isActive', 'actions'];

  ngOnInit(): void {
    this.load();
  }

  /** Fetches the current page of users with all active filter and sort state applied. */
  load(): void {
    this.isLoading.set(true);
    this.userService.getUsers({
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortBy(),
      sortDirection: this.sortDirection(),
      searchKeyword: null,
      name: this.filterName() || null,
      email: this.filterEmail() || null,
      role: this.filterRole(),
      isActive: this.filterIsActive(),
      isSystemUser: this.filterIsSystemUser(),
    }).subscribe({
      next: result => {
        this.users.set(result.items);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  /** Resets to page 0 and reloads when any filter value changes. */
  applyFilters(): void {
    this.pageIndex.set(0);
    this.load();
  }

  /** Clears all filter signals, resets to page 0, and reloads. */
  clearFilters(): void {
    this.filterName.set('');
    this.filterEmail.set('');
    this.filterRole.set(null);
    this.filterIsActive.set(null);
    this.filterIsSystemUser.set(null);
    this.pageIndex.set(0);
    this.load();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  onSortChange(sort: Sort): void {
    this.sortBy.set(sort.active);
    this.sortDirection.set((sort.direction || 'asc') as 'asc' | 'desc');
    this.pageIndex.set(0);
    this.load();
  }

  /**
   * Opens the user dialog in create or edit mode.
   * @param userId Pass `null` to open in create mode, or a user ID string for edit mode.
   */
  openDialog(userId: string | null): void {
    const data: UserDialogData = { userId };
    this.dialog.open(UserDialogComponent, { data, width: '600px', disableClose: true })
      .afterClosed()
      .subscribe(saved => { if (saved) this.load(); });
  }
}
