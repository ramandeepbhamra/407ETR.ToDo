/**
 * Abstract idle/inactivity service interface.
 * 
 * Program against this interface — not against a concrete implementation.
 * To swap the underlying library, change the `useClass` binding in app.config.ts.
 * 
 * Current implementation: NgIdleService (wraps @ng-idle/core)
 * 
 * @example
 * ```typescript
 * // In app.config.ts
 * { provide: IdleService, useClass: NgIdleService }
 * 
 * // In your component
 * private readonly idleService = inject(IdleService);
 * 
 * ngOnInit() {
 *   this.idleService.start();
 * }
 * 
 * ngOnDestroy() {
 *   this.idleService.stop();
 * }
 * ```
 */
export abstract class IdleService {
  /**
   * Starts monitoring user activity for idle timeout.
   * Call this when the user authenticates.
   */
  abstract start(): void;

  /**
   * Stops monitoring user activity.
   * Call this when the user logs out or the component is destroyed.
   */
  abstract stop(): void;
}
