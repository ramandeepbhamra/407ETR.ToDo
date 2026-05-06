import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCheckbox } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggle } from '@angular/material/slide-toggle';
import { MatSliderModule } from '@angular/material/slider';

@Component({
  selector: 'app-inputs',
  imports: [
    MatCheckbox,
    MatRadioModule,
    MatButtonToggleModule,
    MatChipsModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatSelectModule,
    MatListModule,
    MatSlideToggle,
    MatSliderModule,
    MatIcon,
    MatInput,
    FormsModule,
    MatAutocompleteModule,
  ],
  template: `
    <div>
      <mat-checkbox>Normal</mat-checkbox>
      <mat-checkbox [indeterminate]="true">Indeterminate</mat-checkbox>
    </div>

    <mat-radio-group aria-label="Select an option">
      <mat-radio-button value="1">Option 1</mat-radio-button>
      <mat-radio-button value="2">Option 2</mat-radio-button>
    </mat-radio-group>

    <div>
      <mat-button-toggle-group name="fontstyle">
        <mat-button-toggle value="bold">Bold</mat-button-toggle>
        <mat-button-toggle value="italic">Italic</mat-button-toggle>
        <mat-button-toggle value="underline">Underline</mat-button-toggle>
      </mat-button-toggle-group>
    </div>

    <div>
      <mat-chip-set aria-label="Fish selection">
        <mat-chip>One fish</mat-chip>
        <mat-chip>Two fish</mat-chip>
        <mat-chip>Three fish</mat-chip>
        <mat-chip disabled>Four fish</mat-chip>
      </mat-chip-set>
    </div>

    <mat-form-field>
      <mat-label>Choose a date</mat-label>
      <input matInput ngModel required [matDatepicker]="picker" />
      <mat-hint>MM/DD/YYYY</mat-hint>
      <mat-datepicker-toggle matIconSuffix [for]="picker"></mat-datepicker-toggle>
      <mat-datepicker #picker></mat-datepicker>
      <mat-error>This is an error</mat-error>
    </mat-form-field>

    <mat-form-field>
      <mat-label>Enter a date range</mat-label>
      <mat-date-range-input [rangePicker]="rangePicker">
        <input matStartDate placeholder="Start date" />
        <input matEndDate placeholder="End date" />
      </mat-date-range-input>
      <mat-hint>MM/DD/YYYY – MM/DD/YYYY</mat-hint>
      <mat-datepicker-toggle matIconSuffix [for]="rangePicker"></mat-datepicker-toggle>
      <mat-date-range-picker #rangePicker></mat-date-range-picker>
    </mat-form-field>

    <mat-form-field>
      <mat-label>Input</mat-label>
      <input matInput />
    </mat-form-field>

    <mat-form-field>
      <mat-label>Select</mat-label>
      <mat-select ngModel required>
        <mat-option value="one">First option</mat-option>
        <mat-option value="two">Second option</mat-option>
      </mat-select>
    </mat-form-field>

    <mat-form-field class="example-full-width">
      <mat-label>Number</mat-label>
      <input
        type="text"
        placeholder="Pick one"
        aria-label="Number"
        matInput
        [matAutocomplete]="auto"
      />
      <mat-autocomplete autoActiveFirstOption #auto="matAutocomplete">
        @for (option of ['Option 1', 'Option 2', 'Option 3']; track option) {
        <mat-option [value]="option">{{ option }}</mat-option>
        }
      </mat-autocomplete>
    </mat-form-field>

    <mat-form-field>
      <mat-label>Textarea</mat-label>
      <textarea matInput required ngModel></textarea>
    </mat-form-field>

    <mat-form-field appearance="outline">
      <mat-label>Outline form field</mat-label>
      <input ngModel required matInput placeholder="Placeholder" />
      <mat-icon matSuffix>sentiment_very_satisfied</mat-icon>
      <mat-hint>Hint</mat-hint>
    </mat-form-field>

    <mat-selection-list #shoes>
      <mat-list-option>Option 1</mat-list-option>
      <mat-list-option>Option 2</mat-list-option>
      <mat-list-option>Option 3</mat-list-option>
    </mat-selection-list>

    <mat-selection-list [multiple]="false">
      <mat-list-option>Option 1</mat-list-option>
      <mat-list-option>Option 2</mat-list-option>
      <mat-list-option>Option 3</mat-list-option>
    </mat-selection-list>

    <p><mat-slide-toggle>Slide me!</mat-slide-toggle></p>

    <mat-slider min="0" max="100" step="1" discrete>
      <input matSliderThumb />
    </mat-slider>
  `,
  styles: `
  :host {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    align-items: center;
    gap: 24px;
  }
  `,
})
export class InputsComponent {}
