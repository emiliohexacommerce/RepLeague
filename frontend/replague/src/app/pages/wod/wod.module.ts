import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslationModule } from '../../modules/i18n/translation.module';
import { SharedModule } from '../../_metronic/shared/shared.module';
import { WodComponent } from './wod.component';

@NgModule({
  declarations: [WodComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslationModule,
    SharedModule,
    RouterModule.forChild([
      { path: '', component: WodComponent },
    ]),
  ],
})
export class WodModule {}
