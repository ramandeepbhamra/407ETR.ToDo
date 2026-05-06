import { inject, Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ConfigService } from '../../core/services/config.service';

@Injectable({ providedIn: 'root' })
export class UpgradeDialogService {
  private readonly dialog = inject(MatDialog);
  private readonly configService = inject(ConfigService);

  /**
   * Opens the upgrade prompt dialog
   * @param title Dialog title
   * @param message Dialog message
   */
  async openUpgradeDialog(title: string, message: string): Promise<void> {
    const { UpgradePromptDialogComponent } = await import(
      '../components/upgrade-prompt-dialog/upgrade-prompt-dialog.component'
    );

    const supportEmail = this.configService.config().supportEmail;
    const mailtoLink = `mailto:${supportEmail}`;

    this.dialog.open(UpgradePromptDialogComponent, {
      width: '500px',
      disableClose: false,
      data: { 
        title, 
        message, 
        mailtoLink 
      },
    });
  }

  /**
   * Opens upgrade dialog for task limit exceeded
   */
  openTaskLimitDialog(): void {
    this.openUpgradeDialog(
      'Task Limit Reached',
      'You have reached the limit of 10 tasks per list. Upgrade to Premium for unlimited tasks!'
    );
  }

  /**
   * Opens upgrade dialog for task list limit exceeded
   */
  openTaskListLimitDialog(): void {
    this.openUpgradeDialog(
      'Task List Limit Reached',
      'You have reached the limit of 10 task lists. Upgrade to Premium for unlimited task lists!'
    );
  }
}
