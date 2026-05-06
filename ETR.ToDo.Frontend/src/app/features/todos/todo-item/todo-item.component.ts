import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckbox } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { toDateOnlyString } from '../../../core/utils/date.util';
import { TrimOnBlurDirective } from '../../../core/directives/trim-on-blur.directive';
import { TodoTask } from '../models/todo-task.model';
import { TodoTaskService } from '../services/todo-task.service';
import { ResponsiveService } from '../../../core/services/responsive.service';

/**
 * Renders a single task row with inline editing, subtask management,
 * completion/favourite toggles, and soft-delete with confirmation.
 *
 * Communicates changes upward via `taskUpdated` and `taskDeleted` outputs —
 * it never mutates the parent's list directly.
 * Subtask and task delete flows use lazy-loaded `ConfirmDialogComponent`.
 */
@Component({
  selector: 'app-todo-item',
  imports: [
    FormsModule,
    MatCheckbox,
    MatButtonModule,
    MatFormFieldModule,
    MatInput,
    MatIcon,
    MatTooltipModule,
    MatDatepickerModule,
    MatNativeDateModule,
    TrimOnBlurDirective,
  ],
  templateUrl: './todo-item.component.html',
  styleUrl: './todo-item.component.scss',
})
export class TodoItemComponent {
  private readonly todoTaskService = inject(TodoTaskService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  readonly responsiveService = inject(ResponsiveService);

  /** The task to display. Required — provided by the parent list component. */
  readonly task = input.required<TodoTask>();
  /** Emitted when any field on the task (or one of its subtasks) changes. */
  readonly taskUpdated = output<TodoTask>();
  /** Emitted with the task ID after a successful soft-delete. */
  readonly taskDeleted = output<string>();

  readonly isEditing = signal(false);
  readonly editTitle = signal('');
  editDueDate: Date | null = null;

  readonly editingSubTaskId = signal<string | null>(null);
  readonly editSubTaskTitle = signal('');

  readonly newSubTaskTitle = signal('');
  readonly showSubTasks = signal(false);

  readonly titleError = signal(false);
  readonly subtaskEditError = signal(false);
  readonly newSubtaskError = signal(false);

  private triggerError(errorSignal: ReturnType<typeof signal<boolean>>, message: string): void {
    this.snackBar.open(message, 'Close', { duration: 3000 });
    errorSignal.set(true);
    setTimeout(() => errorSignal.set(false), 600);
  }

  /** `true` when the task has a due date in the past and is not yet completed. */
  get isDueDateOverdue(): boolean {
    if (!this.task().dueDate || this.task().isCompleted) return false;
    const dueDateStr = this.task().dueDate;
    if (!dueDateStr) return false;
    const dueDate = new Date(dueDateStr);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    dueDate.setHours(0, 0, 0, 0);
    return dueDate < today;
  }

  /** Formats an ISO date string to `MM/DD/YYYY` for display. Returns `''` for null. */
  formatDueDate(dateString: string | null): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${month}/${day}/${year}`;
  }

  toggleComplete(): void {
    this.todoTaskService.toggleComplete(this.task().id).subscribe(updated => this.taskUpdated.emit(updated));
  }

  toggleFavourite(): void {
    this.todoTaskService.toggleFavourite(this.task().id).subscribe(updated => this.taskUpdated.emit(updated));
  }

  /** Populates edit signals from the current task and enters edit mode. */
  startEdit(): void {
    this.editTitle.set(this.task().title);
    const dueDateStr = this.task().dueDate;
    this.editDueDate = dueDateStr ? new Date(dueDateStr) : null;
    this.isEditing.set(true);
  }

  /** Validates and persists the edited title and due date, then exits edit mode. */
  saveEdit(): void {
    const title = this.editTitle().trim();
    if (!title) {
      this.triggerError(this.titleError, 'Task title is required');
      return;
    }
    if (title.length > 255) {
      this.triggerError(this.titleError, 'Task title cannot exceed 255 characters');
      return;
    }

    const dueDate = this.editDueDate ? toDateOnlyString(this.editDueDate) : null;

    this.todoTaskService
      .update(this.task().id, { title, listId: this.task().listId, dueDate })
      .subscribe(updated => {
        this.taskUpdated.emit(updated);
        this.isEditing.set(false);
      });
  }

  cancelEdit(): void {
    this.isEditing.set(false);
  }

  startEditSubTask(subTaskId: string, title: string): void {
    this.editingSubTaskId.set(subTaskId);
    this.editSubTaskTitle.set(title);
  }

  /** Validates and persists an edited subtask title. Merges the updated subtask into the emitted task. */
  saveSubTaskEdit(subTaskId: string): void {
    const title = this.editSubTaskTitle().trim();
    if (!title) {
      this.triggerError(this.subtaskEditError, 'Subtask title is required');
      return;
    }
    if (title.length > 255) {
      this.triggerError(this.subtaskEditError, 'Subtask title cannot exceed 255 characters');
      return;
    }

    this.todoTaskService.updateSubTask(this.task().id, subTaskId, { title }).subscribe(updated => {
      const subTasks = this.task().subTasks.map(s => s.id === updated.id ? updated : s);
      this.taskUpdated.emit({ ...this.task(), subTasks });
      this.cancelSubTaskEdit();
    });
  }

  cancelSubTaskEdit(): void {
    this.editingSubTaskId.set(null);
    this.editSubTaskTitle.set('');
  }

  /**
   * Opens a confirmation dialog before deleting the task.
   * The message includes the subtask count when subtasks are present.
   * `ConfirmDialogComponent` is lazy-loaded to keep the initial bundle lean.
   */
  async confirmDelete(): Promise<void> {
    const { ConfirmDialogComponent } = await import(
      '../../../shared/components/confirm-dialog/confirm-dialog.component'
    );

    const subtaskCount = this.task().subTasks.length;
    const message = subtaskCount > 0
      ? `Are you sure you want to delete "${this.task().title}"? This will also delete ${subtaskCount} subtask${subtaskCount > 1 ? 's' : ''}.`
      : `Are you sure you want to delete "${this.task().title}"?`;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Task',
        message,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.delete();
      }
    });
  }

  private delete(): void {
    this.todoTaskService.delete(this.task().id).subscribe({
      next: () => {
        this.taskDeleted.emit(this.task().id);
        this.snackBar.open('Task deleted successfully', 'Close', { duration: 3000 });
      },
      error: (err: HttpErrorResponse) => {
        this.snackBar.open(err.error?.message || 'Failed to delete task', 'Close', { duration: 3000 });
      }
    });
  }

  /** Toggles completion on a subtask and merges the result into the emitted task. */
  toggleSubTaskComplete(subTaskId: string): void {
    this.todoTaskService.toggleSubTaskComplete(this.task().id, subTaskId).subscribe(updated => {
      const subTasks = this.task().subTasks.map(s => s.id === updated.id ? updated : s);
      this.taskUpdated.emit({ ...this.task(), subTasks });
    });
  }

  /**
   * Opens a confirmation dialog before deleting a subtask.
   * `ConfirmDialogComponent` is lazy-loaded to keep the initial bundle lean.
   */
  async confirmDeleteSubTask(subTaskId: string, title: string): Promise<void> {
    const { ConfirmDialogComponent } = await import(
      '../../../shared/components/confirm-dialog/confirm-dialog.component'
    );

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Subtask',
        message: `Are you sure you want to delete "${title}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.deleteSubTask(subTaskId);
      }
    });
  }

  private deleteSubTask(subTaskId: string): void {
    this.todoTaskService.deleteSubTask(this.task().id, subTaskId).subscribe(() => {
      const subTasks = this.task().subTasks.filter(s => s.id !== subTaskId);
      this.taskUpdated.emit({ ...this.task(), subTasks });
    });
  }

  /** Validates and adds a new subtask, then appends it to the emitted task's subtask list. */
  addSubTask(): void {
    const title = this.newSubTaskTitle().trim();
    if (!title) {
      this.triggerError(this.newSubtaskError, 'Subtask title is required');
      return;
    }
    if (title.length > 255) {
      this.triggerError(this.newSubtaskError, 'Subtask title cannot exceed 255 characters');
      return;
    }

    this.todoTaskService.addSubTask(this.task().id, { title }).subscribe({
      next: (subTask) => {
        this.taskUpdated.emit({ ...this.task(), subTasks: [...this.task().subTasks, subTask] });
        this.newSubTaskTitle.set('');
      },
      error: (err: HttpErrorResponse) => {
        this.snackBar.open(err.error?.message || 'Failed to add subtask', 'Close', { duration: 3000 });
      }
    });
  }
}
