import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslationModule } from '../../modules/i18n/translation.module';
import { SharedModule } from '../../_metronic/shared/shared.module';
import { StrengthComponent } from './strength.component';

@NgModule({
  declarations: [StrengthComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslationModule,
    SharedModule,
    RouterModule.forChild([
      { path: '', component: StrengthComponent },
    ]),
  ],
})
export class StrengthModule {}
