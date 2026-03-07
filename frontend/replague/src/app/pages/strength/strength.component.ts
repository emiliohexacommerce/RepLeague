import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { StrengthService } from './strength.service';
import { ProfileService } from '../profile/profile.service';
import {
  LiftSessionDto,
  LiftPrDto,
  CreateLiftSessionRequest,
  CreateStrengthSetRequest,
  ManualLiftPrGroupDto,
  PercentageRow,
  BarOption,
} from './strength.models';
import {
  CROSSFIT_BARBELL_EXERCISES,
  CrossfitExercise,
  getExercisesByCategory,
} from './crossfit-exercises';

@Component({
  selector: 'app-strength',
  templateUrl: './strength.component.html',
})
export class StrengthComponent implements OnInit {
  sessions: LiftSessionDto[] = [];
  prs: LiftPrDto[] = [];
  manualPrs: ManualLiftPrGroupDto[] = [];
  loading = false;
  creating = false;
  showForm = false;
  activeTab: 'history' | 'prs' = 'history';
  error = '';
  successMessage = '';
  currentPage = 1;

  // Manual PR form
  showPrForm = false;
  prFormExercise = '';
  prFormWeight: number | null = null;
  prFormDate = new Date().toISOString().split('T')[0];
  prFormNotes = '';
  addingPr = false;
  prFormError = '';

  // Percentage table
  selectedPrForTable: ManualLiftPrGroupDto | null = null;
  tableUnit: 'kg' | 'lb' = 'kg';
  selectedBarKg = 0;
  readonly barOptions: BarOption[] = [
    { label: 'Sin barra', kg: 0 },
    { label: 'Olímpica', kg: 20 },
    { label: 'Mujer', kg: 15 },
    { label: 'EZ', kg: 10 },
    { label: '5 kg', kg: 5 },
  ];

  // Exercises
  readonly exercises: CrossfitExercise[] = CROSSFIT_BARBELL_EXERCISES;
  readonly exerciseGroups = getExercisesByCategory();
  readonly exerciseGroupKeys = Object.keys(getExercisesByCategory());

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
    this.loadManualPrs();
    this.profileService.getProfile().subscribe({
      next: (p) => {
        this.units = p.units ?? 'kg';
        this.oneRmMethod = p.oneRmMethod ?? 'Epley';
        this.tableUnit = (p.units as 'kg' | 'lb') ?? 'kg';
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

  // ── Manual PRs ────────────────────────────────────────────────────────────

  loadManualPrs() {
    this.strengthService.getManualPrs().subscribe({
      next: (data) => (this.manualPrs = data),
      error: () => {},
    });
  }

  openPrForm(exerciseName?: string) {
    this.prFormExercise = exerciseName ?? '';
    this.prFormWeight = null;
    this.prFormDate = new Date().toISOString().split('T')[0];
    this.prFormNotes = '';
    this.prFormError = '';
    this.showPrForm = true;
  }

  closePrForm() {
    this.showPrForm = false;
    this.prFormError = '';
  }

  submitManualPr() {
    if (!this.prFormExercise || !this.prFormWeight || this.prFormWeight <= 0) {
      this.prFormError = 'Selecciona un ejercicio e ingresa un peso válido.';
      return;
    }
    this.addingPr = true;
    this.prFormError = '';

    // Convert to kg if user works in lbs
    const weightKg = this.units === 'lb'
      ? Math.round((this.prFormWeight / 2.20462) * 100) / 100
      : this.prFormWeight;

    this.strengthService.addManualPr({
      exerciseName: this.prFormExercise,
      weightKg,
      notes: this.prFormNotes || undefined,
      achievedAt: this.prFormDate,
    }).subscribe({
      next: () => {
        this.addingPr = false;
        this.closePrForm();
        this.loadManualPrs();
      },
      error: (err) => {
        this.prFormError = err?.error?.message ?? 'Error al guardar el PR.';
        this.addingPr = false;
      },
    });
  }

  openPrTable(pr: ManualLiftPrGroupDto) {
    this.selectedPrForTable = pr;
    this.tableUnit = this.units as 'kg' | 'lb';
    this.selectedBarKg = 0;
  }

  closePrTable() {
    this.selectedPrForTable = null;
  }

  get percentageRows(): PercentageRow[] {
    if (!this.selectedPrForTable) return [];
    const baseKg = this.selectedPrForTable.bestWeightKg;
    const barLbs = Math.round(this.selectedBarKg * 2.20462 * 10) / 10;
    const rows: PercentageRow[] = [];
    for (let pct = 100; pct >= 40; pct -= 5) {
      const kg = Math.round((baseKg * pct / 100) * 10) / 10;
      const lbs = Math.round(kg * 2.20462 * 10) / 10;
      const perSideKg = this.selectedBarKg > 0
        ? Math.max(0, Math.round((kg - this.selectedBarKg) / 2 * 10) / 10)
        : null;
      const perSideLbs = this.selectedBarKg > 0
        ? Math.max(0, Math.round((lbs - barLbs) / 2 * 10) / 10)
        : null;
      rows.push({ percent: pct, kg, lbs, perSideKg, perSideLbs });
    }
    return rows;
  }

  prHistoryDisplay(weightKg: number): string {
    return this.toDisplay(weightKg) + ' ' + this.unitLabel;
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
