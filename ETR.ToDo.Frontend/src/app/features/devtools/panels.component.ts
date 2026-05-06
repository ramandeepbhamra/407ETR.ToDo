import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatExpansionModule } from '@angular/material/expansion';

@Component({
  selector: 'app-panels',
  imports: [MatExpansionModule, MatCardModule, MatButtonModule],
  template: `
    <mat-accordion>
      <mat-expansion-panel hideToggle>
        <mat-expansion-panel-header>
          <mat-panel-title> This is the expansion title </mat-panel-title>
          <mat-panel-description>
            This is a summary of the content
          </mat-panel-description>
        </mat-expansion-panel-header>
        <p>This is the primary content of the panel.</p>
      </mat-expansion-panel>
      <mat-expansion-panel
        (opened)="panelOpenState = true"
        (closed)="panelOpenState = false"
      >
        <mat-expansion-panel-header>
          <mat-panel-title> Self aware panel </mat-panel-title>
          <mat-panel-description>
            Currently I am {{ panelOpenState ? 'open' : 'closed' }}
          </mat-panel-description>
        </mat-expansion-panel-header>
        <p>I'm visible because I am open</p>
      </mat-expansion-panel>
    </mat-accordion>

    <br />

    <mat-card class="example-card">
      <mat-card-header>
        <div mat-card-avatar class="example-header-image"></div>
        <mat-card-title>Shiba Inu</mat-card-title>
        <mat-card-subtitle>Dog Breed</mat-card-subtitle>
      </mat-card-header>
      <img
        mat-card-image
        src="https://material.angular.io/assets/img/examples/shiba2.jpg"
        alt="Photo of a Shiba Inu"
      />
      <mat-card-content>
        <p>
          The Shiba Inu is the smallest of the six original and distinct spitz
          breeds of dog from Japan. A small, agile dog that copes very well with
          mountainous terrain, the Shiba Inu was originally bred for hunting.
        </p>
      </mat-card-content>
      <mat-card-actions>
        <button mat-button>LIKE</button>
        <button mat-button>SHARE</button>
      </mat-card-actions>
    </mat-card>
  `,
  styles: ``,
})
export class PanelsComponent {
  panelOpenState = false;
}
