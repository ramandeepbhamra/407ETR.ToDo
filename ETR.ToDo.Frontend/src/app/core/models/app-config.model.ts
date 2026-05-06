export interface AppConfig {
  // Application Info
  applicationName: string;
  applicationVersion: string;
  supportEmail: string;

  // User Limits
  basicUserTaskLimit: number;
  basicUserTaskListLimit: number;

  // Session Management
  idleTimeoutMinutes: number;
  warningCountdownSeconds: number;
}
