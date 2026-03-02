import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';

export interface QueuedRequest {
  id?: number;
  url: string;
  method: string;
  body: string;      // JSON.stringify'd
  headers: Record<string, string>;
  timestamp: number;
}

const DB_NAME = 'replague-offline';
const STORE_NAME = 'queue';
const DB_VERSION = 1;

@Injectable({ providedIn: 'root' })
export class OfflineQueueService {
  private db: IDBDatabase | null = null;

  constructor() {
    this.openDb();
    window.addEventListener('online', () => this.flush());
  }

  // ── IndexedDB setup ──────────────────────────────────────────

  private openDb(): Promise<IDBDatabase> {
    return new Promise((resolve, reject) => {
      if (this.db) return resolve(this.db);

      const req = indexedDB.open(DB_NAME, DB_VERSION);
      req.onupgradeneeded = (evt) => {
        const db = (evt.target as IDBOpenDBRequest).result;
        if (!db.objectStoreNames.contains(STORE_NAME)) {
          db.createObjectStore(STORE_NAME, { keyPath: 'id', autoIncrement: true });
        }
      };
      req.onsuccess = (evt) => {
        this.db = (evt.target as IDBOpenDBRequest).result;
        resolve(this.db);
      };
      req.onerror = () => reject(req.error);
    });
  }

  // ── Queue operations ─────────────────────────────────────────

  async enqueue(req: Omit<QueuedRequest, 'id' | 'timestamp'>): Promise<void> {
    const db = await this.openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(STORE_NAME, 'readwrite');
      const entry: QueuedRequest = { ...req, timestamp: Date.now() };
      const request = tx.objectStore(STORE_NAME).add(entry);
      request.onsuccess = () => resolve();
      request.onerror = () => reject(request.error);
    });
  }

  private async getAll(): Promise<QueuedRequest[]> {
    const db = await this.openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(STORE_NAME, 'readonly');
      const request = tx.objectStore(STORE_NAME).getAll();
      request.onsuccess = () => resolve(request.result as QueuedRequest[]);
      request.onerror = () => reject(request.error);
    });
  }

  private async remove(id: number): Promise<void> {
    const db = await this.openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(STORE_NAME, 'readwrite');
      const request = tx.objectStore(STORE_NAME).delete(id);
      request.onsuccess = () => resolve();
      request.onerror = () => reject(request.error);
    });
  }

  // ── Flush (retry pending requests) ───────────────────────────

  async flush(): Promise<void> {
    const pending = await this.getAll();
    if (pending.length === 0) return;

    let synced = 0;
    for (const item of pending) {
      try {
        const resp = await fetch(item.url, {
          method: item.method,
          headers: { 'Content-Type': 'application/json', ...item.headers },
          body: item.body,
        });
        if (resp.ok) {
          await this.remove(item.id!);
          synced++;
        }
      } catch {
        // Keep in queue — still offline or server error
      }
    }

    if (synced > 0) {
      Swal.fire({
        toast: true,
        position: 'top-end',
        icon: 'success',
        title: `${synced} registro(s) sincronizado(s)`,
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
      });
    }
  }

  // ── Helper for POSTs that should queue when offline ──────────

  /**
   * Attempts an HTTP POST. If offline, enqueues in IndexedDB and returns null.
   * authToken should be the current JWT access token.
   */
  async postWithQueue<T>(
    url: string,
    body: any,
    authToken: string
  ): Promise<T | null> {
    if (!navigator.onLine) {
      await this.enqueue({
        url,
        method: 'POST',
        body: JSON.stringify(body),
        headers: { Authorization: `Bearer ${authToken}` },
      });
      Swal.fire({
        toast: true,
        position: 'top-end',
        icon: 'warning',
        title: 'Sin conexión — registro guardado',
        text: 'Se enviará automáticamente al recuperar la red.',
        showConfirmButton: false,
        timer: 4000,
      });
      return null;
    }

    const resp = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${authToken}`,
      },
      body: JSON.stringify(body),
    });

    if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
    return resp.json() as Promise<T>;
  }
}
