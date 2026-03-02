import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TranslationModule } from '../../modules/i18n/translation.module';
import { LeaguesComponent } from './leagues.component';
import { LeagueDetailComponent } from './league-detail/league-detail.component';
import { JoinLeagueComponent } from './join-league/join-league.component';

@NgModule({
  declarations: [LeaguesComponent, LeagueDetailComponent, JoinLeagueComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslationModule,
    RouterModule.forChild([
      { path: '', component: LeaguesComponent },
      { path: 'join/:token', component: JoinLeagueComponent },
      { path: ':id', component: LeagueDetailComponent },
    ]),
  ],
})
export class LeaguesModule {}
