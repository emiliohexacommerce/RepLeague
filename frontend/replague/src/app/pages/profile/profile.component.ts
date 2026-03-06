import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  ProfileService,
  ProfileDto,
  ProfileSummaryDto,
} from './profile.service';
import { AuthService } from '../../modules/auth';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexYAxis,
  ApexStroke,
  ApexMarkers,
  ApexTooltip,
  ApexGrid,
} from 'ng-apexcharts';

@Component({
  selector: 'app-profile-page',
  templateUrl: './profile.component.html',
})
export class ProfilePageComponent implements OnInit {
  profile: ProfileDto | null = null;
  summary: ProfileSummaryDto | null = null;
  loading = false;
  loadingSummary = false;
  saving = false;
  uploading = false;
  error = '';
  successMessage = '';

  activeTab: 'overview' | 'personal' | 'preferences' = 'overview';

  // Chart
  chartSeries: ApexAxisChartSeries = [];
  selectedExercise = '';
  loadingChart = false;

  chartChart: ApexChart = { type: 'line', height: 220, toolbar: { show: false } };
  chartStroke: ApexStroke = { curve: 'smooth', width: 2 };
  chartXaxis: ApexXAxis = { type: 'datetime' };
  chartYaxis: ApexYAxis = { title: { text: 'Peso (kg)' }, labels: { formatter: (v: number) => v + ' kg' } };
  chartColors: string[] = ['#FF7A1A'];
  chartMarkers: ApexMarkers = { size: 4, colors: ['#FF7A1A'] };
  chartTooltip: ApexTooltip = { x: { format: 'dd MMM yyyy' } };
  chartGrid: ApexGrid = { borderColor: '#2F2F2F22' };

  profileForm: FormGroup;
  prefsForm: FormGroup;

  constructor(
    private profileService: ProfileService,
    private fb: FormBuilder,
    private auth: AuthService,
  ) {}

  ngOnInit(): void {
    this.profileForm = this.fb.group({
      displayName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      country: ['', Validators.maxLength(3)],
      bio: ['', Validators.maxLength(500)],
      phone: [''],
      birthDate: [''],
      city: ['', Validators.maxLength(100)],
      gymName: ['', Validators.maxLength(80)],
    });
    this.prefsForm = this.fb.group({
      units: ['kg'],
      oneRmMethod: ['Epley'],
      visibility: ['leagues'],
      marketingConsent: [false],
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
          phone: data.phone || '',
          birthDate: data.birthDate || '',
          city: data.city || '',
          gymName: data.gymName || '',
        });
        this.prefsForm.patchValue({
          units: data.units || 'kg',
          oneRmMethod: data.oneRmMethod || 'Epley',
          visibility: data.visibility || 'leagues',
          marketingConsent: data.marketingConsent || false,
        });
        this.loading = false;
        this.loadSummary();
      },
      error: () => {
        this.error = 'Failed to load profile.';
        this.loading = false;
      },
    });
  }

  loadSummary() {
    if (this.summary) return;
    this.loadingSummary = true;
    this.profileService.getProfileSummary().subscribe({
      next: (data) => {
        this.summary = data;
        this.loadingSummary = false;
      },
      error: () => {
        this.loadingSummary = false;
      },
    });
  }

  setTab(tab: 'overview' | 'personal' | 'preferences') {
    this.activeTab = tab;
    this.error = '';
    this.successMessage = '';
  }

  saveProfile() {
    if (this.profileForm.invalid) return;
    this.saving = true;
    this.error = '';
    this.successMessage = '';

    const val = this.profileForm.value;
    this.profileService.updateProfile({
      displayName: val.displayName,
      country: val.country || undefined,
      bio: val.bio || undefined,
      phone: val.phone || undefined,
      birthDate: val.birthDate || undefined,
      city: val.city || undefined,
      gymName: val.gymName || undefined,
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

  savePrefs() {
    this.saving = true;
    this.error = '';
    this.successMessage = '';

    const val = this.prefsForm.value;
    this.profileService.updateProfile({
      units: val.units,
      oneRmMethod: val.oneRmMethod,
      visibility: val.visibility,
      marketingConsent: val.marketingConsent,
    }).subscribe({
      next: (updated) => {
        this.profile = updated;
        this.successMessage = 'Preferences saved!';
        this.saving = false;
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to save preferences.';
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
        if (this.profile) this.profile.avatarUrl = res.avatarUrl;
        // Propagate new avatarUrl to the header dropdown immediately
        const currentUser = this.auth.currentUserValue;
        if (currentUser) {
          currentUser.avatarUrl = res.avatarUrl;
          this.auth.currentUserSubject.next(currentUser);
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

  loadChart(exercise: string) {
    this.selectedExercise = exercise;
    this.loadingChart = true;
    this.chartSeries = [];
    this.profileService.getStrengthChart(exercise).subscribe({
      next: (points) => {
        this.chartSeries = [{
          name: exercise,
          data: points.map(p => ({
            x: new Date(p.date).getTime(),
            y: p.weightKg,
          })),
        }];
        this.loadingChart = false;
      },
      error: () => {
        this.loadingChart = false;
      },
    });
  }

  calculateAge(birthDate: string): number {
    const birth = new Date(birthDate);
    const today = new Date();
    let age = today.getFullYear() - birth.getFullYear();
    if (today < new Date(today.getFullYear(), birth.getMonth(), birth.getDate())) age--;
    return age;
  }

  get availableExercises(): string[] {
    return this.summary?.topPrs.map(p => p.exerciseName) ?? [];
  }
}
