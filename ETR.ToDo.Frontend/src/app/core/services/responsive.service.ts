import { BreakpointObserver } from '@angular/cdk/layout';
import { Injectable, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';

/**
 * Provides reactive breakpoint signals derived from CDK `BreakpointObserver`.
 *
 * Breakpoints: small ≤ 600 px | medium 600–1000 px | large > 1000 px.
 * Use these signals instead of `@media` queries in SCSS — see CLAUDE.md.
 *
 * @example
 * readonly responsive = inject(ResponsiveService);
 * // In template: @if (responsive.smallWidth()) { ... }
 */
@Injectable({
  providedIn: 'root',
})
export class ResponsiveService {
  private readonly small = '(max-width: 600px)';
  private readonly medium = '(min-width: 600.01px) and (max-width: 1000px)';
  private readonly large = '(min-width: 1000.01px)';

  private readonly screenWidth = toSignal(
    inject(BreakpointObserver).observe([this.small, this.medium, this.large])
  );

  /** `true` when the viewport width is ≤ 600 px. */
  smallWidth = computed(() => this.screenWidth()?.breakpoints[this.small]);
  /** `true` when the viewport width is between 600 px and 1000 px. */
  mediumWidth = computed(() => this.screenWidth()?.breakpoints[this.medium]);
  /** `true` when the viewport width is > 1000 px. */
  largeWidth = computed(() => this.screenWidth()?.breakpoints[this.large]);
}
