import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslationModule } from '../../modules/i18n/translation.module';
import { NgApexchartsModule } from 'ng-apexcharts';
import { DashboardComponent } from './dashboard.component';

@NgModule({
  declarations: [DashboardComponent],
  imports: [
    CommonModule,
    TranslationModule,
    NgApexchartsModule,
    RouterModule.forChild([
      { path: '', component: DashboardComponent },
    ]),
  ],
})
export class DashboardModule {}
