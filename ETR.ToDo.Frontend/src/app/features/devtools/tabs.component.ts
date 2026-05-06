import { Component } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';

@Component({
  selector: 'app-tabs',
  imports: [MatTabsModule],
  template: `
    <mat-tab-group>
      <mat-tab label="First"> Content 1 </mat-tab>
      <mat-tab label="Second"> Content 2 </mat-tab>
      <mat-tab label="Third"> Content 3 </mat-tab>
    </mat-tab-group>
  `,
  styles: `
  mat-tab {
    padding: 24px;
  }
  `,
})
export class TabsComponent {}
