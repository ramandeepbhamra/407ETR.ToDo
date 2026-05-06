import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import { TrimOnBlurDirective } from '../../../core/directives/trim-on-blur.directive';
import { TodoFormComponent } from '../todo-form/todo-form.component';
import { TodoItemComponent } from '../todo-item/todo-item.component';
import { TodoTask } from '../models/todo-task.model';
import { TodoTaskService } from '../services/todo-task.service';

/**
 * Displays all tasks for the active list (resolved from the route `listId` param).
 *
 * Maintains the full task array in a signal and derives filtered/sorted computed
 * signals for the active and completed tabs. Drag-drop reordering optimistically
 * updates the signal then persists the new order via `TodoTaskService.updateOrder`.
 *
 * Search and favourites filters are applied client-side against the already-loaded
 * task array — no additional HTTP calls are made when filters change.
 */
@Component({
  selector: 'app-todo-list',
  imports: [
    FormsModule,
    TrimOnBlurDirective,
    TodoFormComponent,
    TodoItemComponent,
    DragDropModule,
    MatIcon,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSlideToggleModule,
    MatTabsModule,
  ],
  templateUrl: './todo-list.component.html',
  styleUrl: './todo-list.component.scss',
})
export class TodoListComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly todoTaskService = inject(TodoTaskService);

  readonly listId = signal<string>('');
  readonly tasks = signal<TodoTask[]>([]);

  readonly searchText = signal<string>('');
  readonly showFavouritesOnly = signal<boolean>(false);
  readonly selectedTabIndex = signal<number>(0);

  readonly activeTasks = computed(() => this.tasks().filter(t => !t.isCompleted).sort((a, b) => a.order - b.order));
  readonly completedTasks = computed(() => this.tasks().filter(t => t.isCompleted).sort((a, b) => a.order - b.order));

  readonly filteredActiveTasks = computed(() => this.applyFilters(this.activeTasks()));
  readonly filteredCompletedTasks = computed(() => this.applyFilters(this.completedTasks()));

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('listId') ?? '';
      this.listId.set(id);
      this.loadTasks(id);
    });
  }

  /** Fetches all tasks for the given list and replaces the local signal. */
  loadTasks(listId: string): void {
    this.todoTaskService.getByList(listId).subscribe(tasks => this.tasks.set(tasks));
  }

  /**
   * Applies text search and favourites filter to a task array.
   * Search matches against task title and any subtask title.
   */
  applyFilters(tasks: TodoTask[]): TodoTask[] {
    let filtered = tasks;

    if (this.searchText()) {
      const search = this.searchText().toLowerCase();
      filtered = filtered.filter(t =>
        t.title.toLowerCase().includes(search) ||
        t.subTasks.some(st => st.title.toLowerCase().includes(search))
      );
    }

    if (this.showFavouritesOnly()) {
      filtered = filtered.filter(t => t.isFavourite);
    }

    return filtered;
  }

  clearFilters(): void {
    this.searchText.set('');
    this.showFavouritesOnly.set(false);
  }

  onTaskAdded(task: TodoTask): void {
    this.tasks.update(tasks => [...tasks, task]);
  }

  onTaskUpdated(updated: TodoTask): void {
    this.tasks.update(tasks => tasks.map(t => t.id === updated.id ? updated : t));
  }

  onTaskDeleted(taskId: string): void {
    this.tasks.update(tasks => tasks.filter(t => t.id !== taskId));
  }

  /**
   * Handles CDK drag-drop reorder events on the active tasks list.
   * Optimistically updates `order` values in the signal, then persists to the backend.
   * Only active (non-completed) tasks are reorderable.
   */
  onTaskDrop(event: CdkDragDrop<TodoTask[]>): void {
    const tasks = this.activeTasks();
    moveItemInArray(tasks, event.previousIndex, event.currentIndex);

    tasks.forEach((task, index) => {
      task.order = index + 1;
    });

    this.tasks.update(allTasks => {
      const updatedTasks = [...allTasks];
      tasks.forEach(task => {
        const idx = updatedTasks.findIndex(t => t.id === task.id);
        if (idx !== -1) updatedTasks[idx] = task;
      });
      return updatedTasks;
    });

    this.todoTaskService.updateOrder(this.listId(), tasks.map(t => ({ id: t.id, order: t.order }))).subscribe();
  }
}
