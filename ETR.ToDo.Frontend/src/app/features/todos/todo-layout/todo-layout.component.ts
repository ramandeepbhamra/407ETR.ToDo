import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { TodoSidebarComponent } from '../todo-sidebar/todo-sidebar.component';
import { ResponsiveService } from '../../../core/services/responsive.service';
import { SidenavService } from '../../../core/services/sidenav.service';
import { TodoTaskListService } from '../services/todo-task-list.service';
import { TodoTaskList } from '../models/todo-task-list.model';

@Component({
  selector: 'app-todo-layout',
  imports: [RouterOutlet, MatSidenavModule, TodoSidebarComponent],
  templateUrl: './todo-layout.component.html',
  styleUrl: './todo-layout.component.scss',
})
export class TodoLayoutComponent implements OnInit {
  private readonly taskListService = inject(TodoTaskListService);
  private readonly responsiveService = inject(ResponsiveService);
  readonly sidenavService = inject(SidenavService);

  readonly taskLists = signal<TodoTaskList[]>([]);

  readonly drawerMode = computed(() =>
    this.responsiveService.smallWidth() ? 'over' : 'side'
  );

  readonly contentMargin = computed(() =>
    this.responsiveService.smallWidth() ? '0' : '220px'
  );

  ngOnInit(): void {
    this.taskListService.getAll().subscribe(lists => this.taskLists.set(lists));
  }

  onListCreated(list: TodoTaskList): void {
    this.taskLists.update(lists => [...lists, list]);
  }

  onListDeleted(listId: string): void {
    this.taskLists.update(lists => lists.filter(l => l.id !== listId));
  }

  onListRenamed(updated: TodoTaskList): void {
    this.taskLists.update(lists => lists.map(l => l.id === updated.id ? updated : l));
  }
}
