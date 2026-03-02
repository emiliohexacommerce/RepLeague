import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslationModule } from '../../modules/i18n/translation.module';
import { WorkoutsComponent } from './workouts.component';

@NgModule({
  declarations: [WorkoutsComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslationModule,
    RouterModule.forChild([
      { path: '', component: WorkoutsComponent },
    ]),
  ],
})
export class WorkoutsModule {}
