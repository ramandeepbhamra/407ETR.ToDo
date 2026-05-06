import { Component, ElementRef, inject, input, output, signal, ViewChild, AfterViewInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { toDateOnlyString } from '../../../core/utils/date.util';
import { TodoTask } from '../models/todo-task.model';
import { TodoTaskService } from '../services/todo-task.service';
import { TrimOnBlurDirective } from '../../../core/directives/trim-on-blur.directive';
import { UpgradeDialogService } from '../../../shared/services/upgrade-dialog.service';

/**
 * Inline form for creating a new task in a specific list.
 *
 * Auto-focuses the title input on init and after each successful submission.
 * On a 400 response whose message mentions "tasks per list", opens the upgrade
 * prompt dialog instead of showing a generic error — Basic users who have hit
 * their task limit are guided to contact support.
 */
@Component({
  selector: 'app-todo-form',
  imports: [FormsModule, MatFormFieldModule, MatInput, MatButtonModule, MatIcon, TrimOnBlurDirective, MatDatepickerModule, MatNativeDateModule],
  templateUrl: './todo-form.component.html',
  styleUrl: './todo-form.component.scss',
})
export class TodoFormComponent implements AfterViewInit {
  private readonly todoTaskService = inject(TodoTaskService);
  private readonly upgradeDialog = inject(UpgradeDialogService);
  private readonly snackBar = inject(MatSnackBar);

  @ViewChild('taskInput') taskInput?: ElementRef<HTMLInputElement>;

  /** ID of the list the new task will be created in. Required. */
  readonly listId = input.required<string>();
  /** Emitted with the newly created task after a successful submission. */
  readonly taskAdded = output<TodoTask>();

  readonly title = signal('');
  readonly isSubmitting = signal(false);
  readonly titleError = signal(false);
  dueDate: Date | null = null;

  ngAfterViewInit(): void {
    setTimeout(() => this.taskInput?.nativeElement.focus(), 100);
  }

  private triggerTitleError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 3000 });
    this.titleError.set(true);
    setTimeout(() => this.titleError.set(false), 600);
  }

  /**
   * Validates the form, calls the service, and emits `taskAdded` on success.
   * Resets the form and re-focuses the input after each successful creation.
   */
  submit(): void {
    const title = this.title().trim();
    if (!title) {
      this.triggerTitleError('Task title is required');
      return;
    }
    if (title.length > 255) {
      this.triggerTitleError('Task title cannot exceed 255 characters');
      return;
    }
    if (!this.listId()) return;

    const dueDate = this.dueDate ? toDateOnlyString(this.dueDate) : null;

    this.isSubmitting.set(true);
    this.todoTaskService.create({ title, listId: this.listId(), dueDate }).subscribe({
      next: task => {
        this.taskAdded.emit(task);
        this.title.set('');
        this.dueDate = null;
        this.isSubmitting.set(false);
        setTimeout(() => this.taskInput?.nativeElement.focus(), 0);
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 400 && err.error?.message?.includes('tasks per list')) {
          this.upgradeDialog.openTaskLimitDialog();
        } else {
          this.snackBar.open(err.error?.message || 'Failed to create task', 'Close', { duration: 3000 });
        }
        this.isSubmitting.set(false);
      },
    });
  }
}
