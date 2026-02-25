import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LeaguesService } from '../leagues.service';
import { League, LeagueMember, RankingEntry } from '../league.model';

@Component({
  selector: 'app-league-detail',
  templateUrl: './league-detail.component.html',
})
export class LeagueDetailComponent implements OnInit {
  league: League | null = null;
  members: LeagueMember[] = [];
  ranking: RankingEntry[] = [];
  loading = false;
  error = '';
  activeTab: 'ranking' | 'members' = 'ranking';
  inviteToken = '';
  inviteEmail = '';
  inviting = false;
  inviteError = '';
  showInviteForm = false;

  constructor(
    private route: ActivatedRoute,
    private leaguesService: LeaguesService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loading = true;

    this.leaguesService.getLeagueById(id).subscribe({
      next: (league) => {
        this.league = league;
        this.loading = false;
        this.loadRanking(id);
        this.loadMembers(id);
      },
      error: () => {
        this.error = 'League not found.';
        this.loading = false;
      },
    });
  }

  loadRanking(id: string) {
    this.leaguesService.getRanking(id).subscribe({
      next: (data) => (this.ranking = data),
    });
  }

  loadMembers(id: string) {
    this.leaguesService.getMembers(id).subscribe({
      next: (data) => (this.members = data),
    });
  }

  generateInvite() {
    if (!this.league) return;
    this.inviting = true;
    this.inviteError = '';
    this.inviteToken = '';

    this.leaguesService.inviteMember(this.league.id, this.inviteEmail || undefined).subscribe({
      next: (res) => {
        this.inviteToken = res.token;
        this.inviting = false;
      },
      error: (err) => {
        this.inviteError = err?.error?.message || 'Failed to generate invite.';
        this.inviting = false;
      },
    });
  }

  copyToken() {
    navigator.clipboard.writeText(this.inviteToken).catch(() => {});
  }
}
