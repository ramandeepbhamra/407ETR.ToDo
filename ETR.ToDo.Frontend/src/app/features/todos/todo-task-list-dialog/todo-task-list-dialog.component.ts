import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TrimOnBlurDirective } from '../../../core/directives/trim-on-blur.directive';
import { TodoTaskList } from '../models/todo-task-list.model';
import { TodoTaskListService } from '../services/todo-task-list.service';
import { UpgradeDialogService } from '../../../shared/services/upgrade-dialog.service';

export interface TodoTaskListDialogData {
  list?: TodoTaskList;
}

@Component({
  selector: 'app-todo-task-list-dialog',
  templateUrl: './todo-task-list-dialog.component.html',
  styleUrl: './todo-task-list-dialog.component.scss',
  imports: [
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInput,
    MatIcon,
    MatProgressSpinnerModule,
    TrimOnBlurDirective,
  ],
})
export class TodoTaskListDialogComponent {
  private readonly taskListService = inject(TodoTaskListService);
  private readonly upgradeDialog = inject(UpgradeDialogService);
  private readonly dialogRef = inject(MatDialogRef<TodoTaskListDialogComponent>);
  readonly data = inject<TodoTaskListDialogData>(MAT_DIALOG_DATA);

  readonly isEditMode = !!this.data?.list;
  readonly title = signal(this.data?.list?.title ?? '');
  readonly isSubmitting = signal(false);
  readonly titleError = signal(false);

  private triggerError(message: string): void {
    this.titleError.set(true);
    setTimeout(() => this.titleError.set(false), 600);
  }

  submit(): void {
    const title = this.title().trim();
    if (!title) {
      this.triggerError('List name is required');
      return;
    }
    if (title.length > 100) {
      this.triggerError('List name cannot exceed 100 characters');
      return;
    }

    this.isSubmitting.set(true);

    const request$ = this.isEditMode
      ? this.taskListService.update(this.data.list!.id, { title })
      : this.taskListService.create({ title });

    request$.subscribe({
      next: (list) => this.dialogRef.close(list),
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        if (err.status === 400 && err.error?.message?.includes('task list')) {
          this.dialogRef.close();
          this.upgradeDialog.openTaskListLimitDialog();
        } else {
          this.triggerError(err.error?.message || 'Failed to save list');
        }
      },
    });
  }
}
