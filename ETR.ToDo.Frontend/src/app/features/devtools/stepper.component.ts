import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatStepperModule } from '@angular/material/stepper';

@Component({
  selector: 'app-stepper',
  imports: [MatStepperModule, MatFormFieldModule, MatInput, MatButtonModule, FormsModule],
  template: `
    <mat-stepper #stepper>
      <mat-step>
        <form>
          <ng-template matStepLabel>Fill out your name</ng-template>
          <mat-form-field>
            <mat-label>Name</mat-label>
            <input matInput placeholder="Last name, First name" required />
          </mat-form-field>
          <div>
            <button type="button" mat-button matStepperNext>Next</button>
          </div>
        </form>
      </mat-step>
      <mat-step label="Fill out your address">
        <form>
          <mat-form-field>
            <mat-label>Address</mat-label>
            <input matInput placeholder="Ex. 1 Main St, New York, NY" />
          </mat-form-field>
          <div>
            <button mat-button type="button" matStepperPrevious>Back</button>
            <button mat-button type="button" matStepperNext>Next</button>
          </div>
        </form>
      </mat-step>
      <mat-step>
        <ng-template matStepLabel>Done</ng-template>
        <p>You are now done.</p>
        <div>
          <button mat-button type="button" matStepperPrevious>Back</button>
          <button mat-button type="button" (click)="stepper.reset()">Reset</button>
        </div>
      </mat-step>
    </mat-stepper>
  `,
  styles: ``,
})
export class StepperComponent {}
