import { Directive, ElementRef, HostListener, inject } from '@angular/core';
import { NgControl } from '@angular/forms';

/**
 * Directive that normalises a text input on blur: trims leading/trailing whitespace
 * and collapses multiple consecutive spaces between words into one.
 *
 * Works with both reactive/template-driven forms (`formControlName`, `ngModel`) and
 * signal-based inputs that use `[value]` + `(input)` event binding.
 *
 * @example
 * ```html
 * <input trimOnBlur formControlName="firstName" />
 * <input trimOnBlur [value]="title()" (input)="title.set($any($event.target).value)" />
 * ```
 */
@Directive({
  selector: 'input[trimOnBlur]',
})
export class TrimOnBlurDirective {
  private readonly control = inject(NgControl, { optional: true, self: true });
  private readonly el = inject(ElementRef<HTMLInputElement>);

  /**
   * Normalises the value on blur: trims edges and collapses internal whitespace.
   * For form-control inputs, sets the control value. For signal-based inputs,
   * updates the native element and dispatches an `input` event so the binding picks it up.
   */
  @HostListener('blur')
  onBlur(): void {
    if (this.control?.control) {
      const value = this.control.control.value;
      if (typeof value === 'string') {
        this.control.control.setValue(this.normalise(value), { emitEvent: false });
      }
    } else {
      const raw = this.el.nativeElement.value;
      const normalised = this.normalise(raw);
      if (normalised !== raw) {
        this.el.nativeElement.value = normalised;
        this.el.nativeElement.dispatchEvent(new Event('input', { bubbles: true }));
      }
    }
  }

  private normalise(value: string): string {
    return value.trim().replace(/\s{2,}/g, ' ');
  }
}
