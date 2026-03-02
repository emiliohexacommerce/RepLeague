import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslationModule } from '../../modules/i18n/translation.module';
import { SharedModule } from '../../_metronic/shared/shared.module';
import { PrsComponent } from './prs.component';

@NgModule({
  declarations: [PrsComponent],
  imports: [
    CommonModule,
    TranslationModule,
    SharedModule,
    RouterModule.forChild([
      { path: '', component: PrsComponent },
    ]),
  ],
})
export class PrsModule {}
