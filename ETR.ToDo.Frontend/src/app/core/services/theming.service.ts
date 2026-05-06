import { Injectable, signal } from '@angular/core';

type Theme = {
  name: string;
  background: string;
  primary: string;
  primaryLight: string;
  ripple: string;
  primaryDark: string;
  error: string;
};

/**
 * Manages the active colour theme via writable signals that are bound to CSS
 * custom properties in `styles.scss` (`--primary`, `--background`, etc.).
 *
 * Use `applyTheme` to switch to a preset from `definedThemes`, or call the
 * individual setters to apply a custom colour. Components must never hardcode
 * hex values — always read from these signals or the corresponding CSS vars.
 */
@Injectable({
  providedIn: 'root',
})
export class ThemingService {
  primary = signal('#046e8f');
  primaryLight = signal('#dce3e9');
  ripple = signal('#046e8f1e');
  primaryDark = signal('black');
  background = signal('#fbfcfe');
  error = signal('#ba1a1a');

  constructor() {}

  /**
   * Preset themes available in the theme selector.
   * Each entry maps to the full set of CSS custom properties used across the app.
   */
  definedThemes: Theme[] = [
    {
      name: 'Teal',
      background: '#fbfcfe',
      primary: '#046e8f',
      primaryLight: '#dce3e9',
      ripple: '#005cbb1e',
      primaryDark: 'black',
      error: '#ba1a1a',
    },
    {
      name: 'Blue',
      background: '#fbfcfe',
      primary: '#073763',
      primaryLight: '#E6EBEF',
      ripple: '#0737631e',
      primaryDark: 'black',
      error: '#ba1a1a',
    },
    {
      name: 'Rose',
      background: '#fbfcfe',
      primary: '#7d1135',
      primaryLight: '#f2e7ea',
      ripple: '#7d11351e',
      primaryDark: 'black',
      error: '#ba1a1a',
    },
    {
      name: 'Dark Teal',
      background: '#01212A',
      primary: '#06A7D9',
      primaryLight: '#023747',
      ripple: '#81B6C71e',
      primaryDark: '#a6cbd7',
      error: '#f09494',
    },
  ];

  /** Sets the primary brand colour. */
  setPrimary(color: string) { this.primary.set(color); }

  /** Sets the light variant of the primary colour (used for backgrounds and hover states). */
  setPrimaryLight(color: string) { this.primaryLight.set(color); }

  /** Sets the dark variant of the primary colour (used for text on light backgrounds). */
  setPrimaryDark(color: string) { this.primaryDark.set(color); }

  /** Sets the ripple overlay colour (should include alpha, e.g. `#046e8f1e`). */
  setRipple(color: string) { this.ripple.set(color); }

  /** Sets the page background colour. */
  setBackground(color: string) { this.background.set(color); }

  /** Sets the error/destructive colour (used for Material `warn` palette). */
  setError(color: string) { this.error.set(color); }

  /** Applies all colours from a preset theme in a single update. */
  applyTheme(theme: Theme) {
    const { primary, primaryLight, primaryDark, ripple, background, error } = theme;
    this.primary.set(primary);
    this.primaryLight.set(primaryLight);
    this.primaryDark.set(primaryDark);
    this.background.set(background);
    this.ripple.set(ripple);
    this.error.set(error);
  }
}
