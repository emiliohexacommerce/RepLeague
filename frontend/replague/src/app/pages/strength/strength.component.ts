import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { StrengthService } from './strength.service';
import { ProfileService } from '../profile/profile.service';
import {
  LiftSessionDto,
  LiftPrDto,
  CreateLiftSessionRequest,
  CreateStrengthSetRequest,
} from './strength.models';

@Component({
  selector: 'app-strength',
  templateUrl: './strength.component.html',
})
export class StrengthComponent implements OnInit {
  sessions: LiftSessionDto[] = [];
  prs: LiftPrDto[] = [];
  loading = false;
  creating = false;
  showForm = false;
  activeTab: 'history' | 'prs' = 'history';
  error = '';
  successMessage = '';
  currentPage = 1;

  liftForm: FormGroup;
  units = 'kg';
  oneRmMethod = 'Epley';

  get unitLabel(): string { return this.units; }

  constructor(
    private strengthService: StrengthService,
    private fb: FormBuilder,
    private profileService: ProfileService,
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadHistory();
    this.loadPrs();
    this.profileService.getProfile().subscribe({
      next: (p) => {
        this.units = p.units ?? 'kg';
        this.oneRmMethod = p.oneRmMethod ?? 'Epley';
      },
    });
  }

  // ── Form ──────────────────────────────────────────────────────────────────

  initForm() {
    this.liftForm = this.fb.group({
      date: [new Date().toISOString().split('T')[0], Validators.required],
      title: [''],
      notes: [''],
      sets: this.fb.array([this.buildSetGroup(0)]),
    });
  }

  buildSetGroup(currentCount?: number): FormGroup {
    const count = currentCount ?? this.sets?.length ?? 0;
    return this.fb.group({
      exerciseName: ['', Validators.required],
      setNumber: [count + 1, [Validators.required, Validators.min(1)]],
      reps: [5, [Validators.required, Validators.min(1)]],
      weightKg: [0, [Validators.required, Validators.min(0)]],
      isWarmup: [false],
      notes: [''],
    });
  }

  get sets(): FormArray {
    return this.liftForm.get('sets') as FormArray;
  }

  addSet() {
    this.sets.push(this.buildSetGroup());
    // Auto-fill exercise name from previous set
    const len = this.sets.length;
    if (len > 1) {
      const prev = this.sets.at(len - 2).get('exerciseName')?.value;
      if (prev) this.sets.at(len - 1).get('exerciseName')?.setValue(prev);
    }
  }

  removeSet(i: number) {
    if (this.sets.length > 1) this.sets.removeAt(i);
  }

  // ── Data ──────────────────────────────────────────────────────────────────

  loadHistory() {
    this.loading = true;
    this.error = '';
    this.strengthService.getHistory(this.currentPage).subscribe({
      next: (data) => {
        this.sessions = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load lift history.';
        this.loading = false;
      },
    });
  }

  loadPrs() {
    this.strengthService.getPrs().subscribe({
      next: (data) => (this.prs = data),
      error: () => {},
    });
  }

  toggleForm() {
    this.showForm = !this.showForm;
    if (!this.showForm) this.initForm();
  }

  submit() {
    if (this.liftForm.invalid) return;
    this.creating = true;
    this.error = '';
    this.successMessage = '';

    const val = this.liftForm.value;

    const setsPayload: CreateStrengthSetRequest[] = (val.sets as any[]).map((s) => ({
      exerciseName: s.exerciseName,
      setNumber: +s.setNumber,
      reps: +s.reps,
      weightKg: this.toKg(+s.weightKg),
      isWarmup: s.isWarmup,
      notes: s.notes || undefined,
    }));

    const request: CreateLiftSessionRequest = {
      date: val.date,
      title: val.title || undefined,
      notes: val.notes || undefined,
      sets: setsPayload,
    };

    this.strengthService.create(request).subscribe({
      next: () => {
        const prCount = 0; // server computes PRs
        this.successMessage = 'Session logged successfully!';
        this.creating = false;
        this.showForm = false;
        this.initForm();
        this.loadHistory();
        this.loadPrs();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to log session.';
        this.creating = false;
      },
    });
  }

  deleteSession(id: string) {
    if (!confirm('Delete this session?')) return;
    this.strengthService.delete(id).subscribe({
      next: () => this.loadHistory(),
      error: () => { this.error = 'Failed to delete session.'; },
    });
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  uniqueExercises(session: LiftSessionDto): string[] {
    return [...new Set(session.sets.map((s) => s.exerciseName))];
  }

  getSetsForExercise(session: LiftSessionDto, exercise: string) {
    return session.sets.filter((s) => s.exerciseName === exercise);
  }

  topSetForExercise(session: LiftSessionDto, exercise: string) {
    return session.sets
      .filter((s) => s.exerciseName === exercise && !s.isWarmup)
      .sort((a, b) => b.weightKg - a.weightKg)[0];
  }

  hasPr(session: LiftSessionDto): boolean {
    return session.sets.some((s) => s.isPr);
  }

  /** Convert stored kg value to the user's display unit (rounded to 1 decimal) */
  toDisplay(kg: number | null | undefined): string {
    if (kg == null) return '—';
    const v = this.units === 'lb' ? kg * 2.20462 : kg;
    return Math.round(v * 10) / 10 + '';
  }

  /** Convert user-entered value (in their preferred unit) to kg for storage */
  toKg(value: number): number {
    if (this.units === 'lb') return Math.round((value / 2.20462) * 100) / 100;
    return value;
  }
}
