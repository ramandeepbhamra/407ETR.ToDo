import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

/**
 * Data shape required when opening `UpgradePromptDialogComponent`.
 * Pass via `MatDialog.open(UpgradePromptDialogComponent, { data: ... })`.
 */
export interface UpgradePromptDialogData {
  /** Dialog heading text. */
  title: string;
  /** Body message explaining why the limit was reached. */
  message: string;
  /** Pre-built `mailto:` link opened when the user clicks "Contact Support". */
  mailtoLink: string;
}

/**
 * Informational dialog shown when a Basic user hits a plan limit (task or list cap).
 * Offers a "Contact Support" CTA that opens the pre-built `mailtoLink`.
 * Opened lazily via `UpgradeDialogService` — never imported eagerly.
 */
@Component({
  selector: 'app-upgrade-prompt-dialog',
  templateUrl: './upgrade-prompt-dialog.component.html',
  styleUrl: './upgrade-prompt-dialog.component.scss',
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
  ],
})
export class UpgradePromptDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<UpgradePromptDialogComponent>);
  readonly data = inject<UpgradePromptDialogData>(MAT_DIALOG_DATA);

  /** Navigates to the support mailto link and closes the dialog. */
  contactSupport(): void {
    window.location.href = this.data.mailtoLink;
    this.dialogRef.close();
  }

  close(): void {
    this.dialogRef.close();
  }
}
