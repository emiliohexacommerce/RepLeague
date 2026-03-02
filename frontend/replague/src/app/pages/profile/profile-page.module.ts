import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslationModule } from '../../modules/i18n/translation.module';
import { ProfilePageComponent } from './profile.component';
import { NgApexchartsModule } from 'ng-apexcharts';

@NgModule({
  declarations: [ProfilePageComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslationModule,
    NgApexchartsModule,
    RouterModule.forChild([
      { path: '', component: ProfilePageComponent },
    ]),
  ],
})
export class ProfilePageModule {}
