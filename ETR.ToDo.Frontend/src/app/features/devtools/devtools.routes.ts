import { Routes } from '@angular/router';
import { DevtoolsComponent } from './devtools.component';
import { ButtonsComponent } from './buttons.component';
import { InputsComponent } from './inputs.component';
import { ProgressComponent } from './progress.component';
import { DialogComponent } from './dialog.component';
import { PanelsComponent } from './panels.component';
import { TableComponent } from './table.component';
import { StepperComponent } from './stepper.component';
import { TabsComponent } from './tabs.component';

export const DEVTOOLS_ROUTES: Routes = [
  {
    path: '',
    component: DevtoolsComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'buttons' },
      { path: 'buttons', component: ButtonsComponent },
      { path: 'inputs', component: InputsComponent },
      { path: 'progress', component: ProgressComponent },
      { path: 'dialog', component: DialogComponent },
      { path: 'panels', component: PanelsComponent },
      { path: 'table', component: TableComponent },
      { path: 'stepper', component: StepperComponent },
      { path: 'tabs', component: TabsComponent },
    ],
  },
];
