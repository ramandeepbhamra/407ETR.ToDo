import { Component } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-progress',
  imports: [MatProgressBarModule, MatProgressSpinner],
  template: `
    <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    <br />
    <br />
    <mat-spinner></mat-spinner>
  `,
  styles: ``,
})
export class ProgressComponent {}
