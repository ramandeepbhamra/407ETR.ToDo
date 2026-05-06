import { Routes } from '@angular/router';
import { TodoLayoutComponent } from './todo-layout/todo-layout.component';
import { TodoListComponent } from './todo-list/todo-list.component';
import { TodoFavouritesComponent } from './todo-favourites/todo-favourites.component';

export const TODO_ROUTES: Routes = [
  {
    path: '',
    component: TodoLayoutComponent,
    children: [
      { path: '', redirectTo: 'favourites', pathMatch: 'full' },
      { path: 'favourites', component: TodoFavouritesComponent },
      { path: 'list/:listId', component: TodoListComponent },
    ],
  },
];

