import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SwPush } from '@angular/service-worker';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class PushService {
  private readonly base = environment.apiUrl;

  constructor(
    private swPush: SwPush,
    private http: HttpClient,
    private router: Router
  ) {}

  get isSupported(): boolean {
    return 'Notification' in window && 'serviceWorker' in navigator && this.swPush.isEnabled;
  }

  /** Call once from AppComponent.ngOnInit() to handle push clicks */
  init(): void {
    if (!this.swPush.isEnabled) return;

    // Show toast when a push message arrives while app is open
    this.swPush.messages.subscribe((msg: any) => {
      const notification = msg?.notification ?? msg;
      Swal.fire({
        toast: true,
        position: 'top-end',
        icon: 'info',
        title: notification?.title ?? 'RepLeague',
        text: notification?.body ?? '',
        showConfirmButton: false,
        timer: 5000,
        timerProgressBar: true,
      });
    });

    // Navigate when user clicks a notification
    this.swPush.notificationClicks.subscribe(({ notification }) => {
      const url: string = (notification as any)?.data?.url ?? '/dashboard';
      this.router.navigateByUrl(url);
    });
  }

  /** Request push permission and send subscription to backend */
  async subscribe(): Promise<void> {
    if (!this.isSupported || !environment.vapidPublicKey) return;

    try {
      const sub = await this.swPush.requestSubscription({
        serverPublicKey: environment.vapidPublicKey,
      });
      await this.http
        .post(`${this.base}/push/subscribe`, sub.toJSON())
        .toPromise();
    } catch (err) {
      console.warn('[PushService] Subscription failed:', err);
    }
  }
}
