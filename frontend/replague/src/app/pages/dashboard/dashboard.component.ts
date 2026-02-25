import { Component, OnInit } from '@angular/core';
import { ProfileService, ProfileDto } from '../profile/profile.service';
import { WorkoutsService } from '../workouts/workouts.service';
import { LeaguesService } from '../leagues/leagues.service';
import { Workout } from '../workouts/workout.model';
import { League } from '../leagues/league.model';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  profile: ProfileDto | null = null;
  recentWorkouts: Workout[] = [];
  leagues: League[] = [];
  loading = true;

  constructor(
    private profileService: ProfileService,
    private workoutsService: WorkoutsService,
    private leaguesService: LeaguesService
  ) {}

  ngOnInit(): void {
    forkJoin({
      profile: this.profileService.getProfile().pipe(catchError(() => of(null))),
      workouts: this.workoutsService.getWorkouts('last7').pipe(catchError(() => of([]))),
      leagues: this.leaguesService.getMyLeagues().pipe(catchError(() => of([]))),
    }).subscribe(({ profile, workouts, leagues }) => {
      this.profile = profile;
      this.recentWorkouts = (workouts as Workout[]).slice(0, 5);
      this.leagues = leagues as League[];
      this.loading = false;
    });
  }

  get prCount(): number {
    return this.recentWorkouts.filter(w => w.isPr).length;
  }
}
