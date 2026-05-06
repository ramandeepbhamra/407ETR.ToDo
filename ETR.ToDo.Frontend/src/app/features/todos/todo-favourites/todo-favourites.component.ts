import { Component, inject, OnInit, signal } from '@angular/core';
import { TodoItemComponent } from '../todo-item/todo-item.component';
import { TodoTask } from '../models/todo-task.model';
import { TodoTaskService } from '../services/todo-task.service';

/**
 * Displays the cross-list favourites view — all tasks the current user has starred.
 *
 * When a task is updated and `isFavourite` is `false` (toggled off), it is
 * removed from this view immediately without a server round-trip, keeping the
 * list consistent with the user's action.
 */
@Component({
  selector: 'app-todo-favourites',
  imports: [TodoItemComponent],
  templateUrl: './todo-favourites.component.html',
  styleUrl: './todo-favourites.component.scss',
})
export class TodoFavouritesComponent implements OnInit {
  private readonly todoTaskService = inject(TodoTaskService);

  readonly tasks = signal<TodoTask[]>([]);

  ngOnInit(): void {
    this.todoTaskService.getFavourites().subscribe(tasks => this.tasks.set(tasks));
  }

  /**
   * Handles task updates from child `TodoItemComponent`.
   * If the task is still a favourite, replaces it in the list.
   * If it was un-favourited, removes it from the list entirely.
   */
  onTaskUpdated(updated: TodoTask): void {
    this.tasks.update(tasks =>
      updated.isFavourite
        ? tasks.map(t => t.id === updated.id ? updated : t)
        : tasks.filter(t => t.id !== updated.id)
    );
  }

  onTaskDeleted(taskId: string): void {
    this.tasks.update(tasks => tasks.filter(t => t.id !== taskId));
  }
}
