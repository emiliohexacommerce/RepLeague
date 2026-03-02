import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class InstallPromptService {
  private deferredPrompt: any = null;

  /** Emits true when the browser's install prompt is available */
  readonly isInstallable$ = new BehaviorSubject<boolean>(false);

  constructor() {
    window.addEventListener('beforeinstallprompt', (e) => {
      e.preventDefault();
      this.deferredPrompt = e;
      this.isInstallable$.next(true);
    });

    window.addEventListener('appinstalled', () => {
      this.deferredPrompt = null;
      this.isInstallable$.next(false);
    });
  }

  /** Shows the native install prompt. Returns 'accepted' or 'dismissed'. */
  async promptInstall(): Promise<string> {
    if (!this.deferredPrompt) return 'unavailable';

    this.deferredPrompt.prompt();
    const { outcome } = await this.deferredPrompt.userChoice;
    this.deferredPrompt = null;
    this.isInstallable$.next(false);
    return outcome;
  }
}
