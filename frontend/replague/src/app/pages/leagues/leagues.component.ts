import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LeaguesService } from './leagues.service';
import { League, CreateLeagueRequest } from './league.model';

@Component({
  selector: 'app-leagues',
  templateUrl: './leagues.component.html',
})
export class LeaguesComponent implements OnInit {
  leagues: League[] = [];
  loading = false;
  creating = false;
  showForm = false;
  error = '';
  successMessage = '';
  joinToken = '';
  joining = false;

  createForm: FormGroup;

  constructor(private leaguesService: LeaguesService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.createForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
    });
    this.loadLeagues();
  }

  loadLeagues() {
    this.loading = true;
    this.leaguesService.getMyLeagues().subscribe({
      next: (data) => {
        this.leagues = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load leagues.';
        this.loading = false;
      },
    });
  }

  toggleForm() {
    this.showForm = !this.showForm;
    if (!this.showForm) this.createForm.reset();
  }

  submitCreate() {
    if (this.createForm.invalid) return;
    this.creating = true;
    this.error = '';

    const req: CreateLeagueRequest = {
      name: this.createForm.value.name,
      description: this.createForm.value.description || undefined,
    };

    this.leaguesService.createLeague(req).subscribe({
      next: () => {
        this.creating = false;
        this.showForm = false;
        this.createForm.reset();
        this.loadLeagues();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to create league.';
        this.creating = false;
      },
    });
  }

  joinByToken() {
    if (!this.joinToken.trim()) return;
    this.joining = true;
    this.error = '';

    this.leaguesService.joinLeague(this.joinToken.trim()).subscribe({
      next: () => {
        this.successMessage = 'Joined league successfully!';
        this.joinToken = '';
        this.joining = false;
        this.loadLeagues();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Invalid or expired invite token.';
        this.joining = false;
      },
    });
  }
}
