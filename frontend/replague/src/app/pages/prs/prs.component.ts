import { Component, OnInit } from '@angular/core';
import { PrsService } from './prs.service';
import { PersonalRecord } from './pr.model';

@Component({
  selector: 'app-prs',
  templateUrl: './prs.component.html',
})
export class PrsComponent implements OnInit {
  allPrs: PersonalRecord[] = [];
  loading = false;
  error = '';
  activeTab: 'all' | 'Strength' | 'WOD' = 'all';

  constructor(private prsService: PrsService) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.error = '';
    this.prsService.getMyPrs().subscribe({
      next: (data) => {
        this.allPrs = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load personal records.';
        this.loading = false;
      },
    });
  }

  get filtered(): PersonalRecord[] {
    if (this.activeTab === 'all') return this.allPrs;
    return this.allPrs.filter((p) => p.type === this.activeTab);
  }

  get strengthCount(): number {
    return this.allPrs.filter((p) => p.type === 'Strength').length;
  }

  get wodCount(): number {
    return this.allPrs.filter((p) => p.type === 'WOD').length;
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }
}
