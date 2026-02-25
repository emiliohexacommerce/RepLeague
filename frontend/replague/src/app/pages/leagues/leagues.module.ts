import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { LeaguesComponent } from './leagues.component';
import { LeagueDetailComponent } from './league-detail/league-detail.component';

@NgModule({
  declarations: [LeaguesComponent, LeagueDetailComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      { path: '', component: LeaguesComponent },
      { path: ':id', component: LeagueDetailComponent },
    ]),
  ],
})
export class LeaguesModule {}
