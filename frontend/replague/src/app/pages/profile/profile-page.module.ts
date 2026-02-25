import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { ProfilePageComponent } from './profile.component';

@NgModule({
  declarations: [ProfilePageComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      { path: '', component: ProfilePageComponent },
    ]),
  ],
})
export class ProfilePageModule {}
