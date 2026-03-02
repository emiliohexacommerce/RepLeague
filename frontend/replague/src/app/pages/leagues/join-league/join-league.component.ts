import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaguesService } from '../leagues.service';
import { League } from '../league.model';

type JoinState = 'loading' | 'success' | 'already_member' | 'expired' | 'not_found' | 'error';

@Component({
  selector: 'app-join-league',
  templateUrl: './join-league.component.html',
})
export class JoinLeagueComponent implements OnInit {
  state: JoinState = 'loading';
  league: League | null = null;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private leaguesService: LeaguesService
  ) {}

  ngOnInit(): void {
    const token = this.route.snapshot.paramMap.get('token') ?? '';
    if (!token) {
      this.state = 'not_found';
      return;
    }
    this.leaguesService.joinLeague(token).subscribe({
      next: (league) => {
        this.league = league;
        this.state = 'success';
      },
      error: (err) => {
        const status: number = err?.status ?? 0;
        const msg: string = (err?.error?.message ?? '').toLowerCase();

        if (status === 404) {
          this.state = 'not_found';
        } else if (status === 409) {
          this.state = 'already_member';
          // Still try to get league id from error detail if available
          this.league = err?.error?.data ?? null;
        } else if (status === 400 && (msg.includes('expired') || msg.includes('used') || msg.includes('vencida'))) {
          this.state = 'expired';
        } else {
          this.state = 'error';
          this.errorMessage = err?.error?.message || 'Error al procesar la invitación.';
        }
      },
    });
  }

  goToLeague(): void {
    if (this.league?.id) {
      this.router.navigate(['/leagues', this.league.id]);
    } else {
      this.router.navigate(['/leagues']);
    }
  }

  goToLeagues(): void {
    this.router.navigate(['/leagues']);
  }
}
