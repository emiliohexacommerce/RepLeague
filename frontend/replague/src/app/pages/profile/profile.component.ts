import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProfileService, ProfileDto } from './profile.service';

@Component({
  selector: 'app-profile-page',
  templateUrl: './profile.component.html',
})
export class ProfilePageComponent implements OnInit {
  profile: ProfileDto | null = null;
  loading = false;
  saving = false;
  uploading = false;
  error = '';
  successMessage = '';

  profileForm: FormGroup;

  constructor(private profileService: ProfileService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.profileForm = this.fb.group({
      displayName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      country: ['', Validators.maxLength(100)],
      bio: ['', Validators.maxLength(500)],
    });
    this.loadProfile();
  }

  loadProfile() {
    this.loading = true;
    this.profileService.getProfile().subscribe({
      next: (data) => {
        this.profile = data;
        this.profileForm.patchValue({
          displayName: data.displayName,
          country: data.country || '',
          bio: data.bio || '',
        });
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load profile.';
        this.loading = false;
      },
    });
  }

  saveProfile() {
    if (this.profileForm.invalid) return;
    this.saving = true;
    this.error = '';
    this.successMessage = '';

    this.profileService.updateProfile({
      displayName: this.profileForm.value.displayName,
      country: this.profileForm.value.country || undefined,
      bio: this.profileForm.value.bio || undefined,
    }).subscribe({
      next: (updated) => {
        this.profile = updated;
        this.successMessage = 'Profile updated successfully!';
        this.saving = false;
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to update profile.';
        this.saving = false;
      },
    });
  }

  onAvatarChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];
    if (!file.type.startsWith('image/')) {
      this.error = 'Please select an image file.';
      return;
    }

    this.uploading = true;
    this.error = '';

    this.profileService.uploadAvatar(file).subscribe({
      next: (res) => {
        if (this.profile) {
          this.profile.avatarUrl = res.avatarUrl;
        }
        this.uploading = false;
        this.successMessage = 'Avatar updated!';
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to upload avatar.';
        this.uploading = false;
      },
    });
  }
}
