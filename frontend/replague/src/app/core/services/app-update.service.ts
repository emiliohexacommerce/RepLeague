import { Injectable } from '@angular/core';
import { SwUpdate, VersionReadyEvent } from '@angular/service-worker';
import { filter, interval } from 'rxjs';
import Swal from 'sweetalert2';

@Injectable({ providedIn: 'root' })
export class AppUpdateService {
  constructor(private swUpdate: SwUpdate) {}

  /** Call once from AppComponent.ngOnInit() */
  init(): void {
    if (!this.swUpdate.isEnabled) return;

    // Listen for new version ready
    this.swUpdate.versionUpdates
      .pipe(filter((evt): evt is VersionReadyEvent => evt.type === 'VERSION_READY'))
      .subscribe(() => this.promptUpdate());

    // Periodically check for updates (every 6 hours)
    interval(6 * 60 * 60 * 1000).subscribe(() => {
      this.swUpdate.checkForUpdate().catch(() => {});
    });
  }

  private promptUpdate(): void {
    Swal.fire({
      title: 'Nueva versión disponible',
      text: 'RepLeague se ha actualizado. ¿Deseas recargar ahora?',
      icon: 'info',
      confirmButtonText: 'Actualizar',
      confirmButtonColor: '#FF7A1A',
      cancelButtonText: 'Más tarde',
      showCancelButton: true,
      allowOutsideClick: false,
    }).then((result) => {
      if (result.isConfirmed) {
        this.swUpdate.activateUpdate().then(() => document.location.reload());
      }
    });
  }
}
