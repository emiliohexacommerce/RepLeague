import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { WodService } from './wod.service';
import {
  WodEntry,
  WodType,
  MovementType,
  CreateWodEntryRequest,
  CreateWodExerciseRequest,
} from './wod.models';

@Component({
  selector: 'app-wod',
  templateUrl: './wod.component.html',
})
export class WodComponent implements OnInit {
  entries: WodEntry[] = [];
  loading = false;
  creating = false;
  showForm = false;
  error = '';
  successMessage = '';
  selectedType: WodType | '' = '';
  currentPage = 1;

  readonly wodTypes: WodType[] = ['ForTime', 'AMRAP', 'EMOM', 'Chipper', 'Intervals'];
  readonly movementTypes: MovementType[] = ['barbell', 'kb', 'bodyweight', 'gymnastic', 'cardio', 'other'];
  readonly loadUnits = ['kg', 'lb', 'cal', 'm', 'reps'];

  wodForm: FormGroup;

  constructor(private wodService: WodService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initForm();
    this.loadHistory();
  }

  // ── Form ──────────────────────────────────────────────────────────────────

  initForm() {
    this.wodForm = this.fb.group({
      type: ['ForTime', Validators.required],
      title: [''],
      date: [new Date().toISOString().split('T')[0], Validators.required],
      timeCap: [''],
      elapsedTime: [''],
      rounds: [null],
      rxScaled: [true],
      notes: [''],
      exercises: this.fb.array([this.buildExerciseGroup()]),
      amrapRoundsCompleted: [0],
      amrapExtraReps: [0],
      emomTotalMinutes: [1],
      emomIntervalsDone: [0],
    });
  }

  buildExerciseGroup(): FormGroup {
    return this.fb.group({
      name: ['', Validators.required],
      movementType: ['barbell'],
      loadValue: [null],
      loadUnit: ['kg'],
      reps: [null],
      notes: [''],
    });
  }

  get exercises(): FormArray {
    return this.wodForm.get('exercises') as FormArray;
  }

  get currentWodType(): WodType {
    return this.wodForm.get('type')?.value as WodType;
  }

  get isAmrap(): boolean { return this.currentWodType === 'AMRAP'; }
  get isEmom(): boolean { return this.currentWodType === 'EMOM'; }
  get isTimeBased(): boolean {
    return ['ForTime', 'Chipper', 'Intervals'].includes(this.currentWodType);
  }

  addExercise() {
    this.exercises.push(this.buildExerciseGroup());
  }

  removeExercise(i: number) {
    if (this.exercises.length > 1) this.exercises.removeAt(i);
  }

  // ── Data ──────────────────────────────────────────────────────────────────

  loadHistory() {
    this.loading = true;
    this.error = '';
    this.wodService.getHistory(this.currentPage, 20, this.selectedType || undefined).subscribe({
      next: (data) => {
        this.entries = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load WOD history.';
        this.loading = false;
      },
    });
  }

  onTypeFilter(type: WodType | '') {
    this.selectedType = type;
    this.currentPage = 1;
    this.loadHistory();
  }

  toggleForm() {
    this.showForm = !this.showForm;
    if (!this.showForm) this.initForm();
  }

  submit() {
    if (this.wodForm.invalid) return;
    this.creating = true;
    this.error = '';
    this.successMessage = '';

    const val = this.wodForm.value;

    const exercisesPayload: CreateWodExerciseRequest[] = (val.exercises as any[]).map((e, i) => ({
      orderIndex: i,
      name: e.name,
      movementType: e.movementType as MovementType,
      loadValue: e.loadValue ?? undefined,
      loadUnit: e.loadUnit ?? undefined,
      reps: e.reps ?? undefined,
      notes: e.notes || undefined,
    }));

    const request: CreateWodEntryRequest = {
      type: val.type as WodType,
      title: val.title || undefined,
      date: val.date,
      timeCap: val.timeCap || undefined,
      elapsedTime: val.elapsedTime || undefined,
      rounds: val.rounds ?? undefined,
      rxScaled: val.rxScaled,
      notes: val.notes || undefined,
      exercises: exercisesPayload,
      amrapResult: this.isAmrap
        ? { roundsCompleted: +val.amrapRoundsCompleted, extraReps: +val.amrapExtraReps }
        : undefined,
      emomResult: this.isEmom
        ? { totalMinutes: +val.emomTotalMinutes, intervalsDone: +val.emomIntervalsDone }
        : undefined,
    };

    this.wodService.create(request).subscribe({
      next: () => {
        this.successMessage = 'WOD logged successfully!';
        this.creating = false;
        this.showForm = false;
        this.initForm();
        this.loadHistory();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to log WOD.';
        this.creating = false;
      },
    });
  }

  deleteEntry(id: string) {
    if (!confirm('Delete this WOD entry?')) return;
    this.wodService.delete(id).subscribe({
      next: () => this.loadHistory(),
      error: () => { this.error = 'Failed to delete WOD entry.'; },
    });
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString();
  }

  wodTypeBadgeClass(type: WodType): string {
    const map: Record<WodType, string> = {
      ForTime: 'badge-light-primary',
      AMRAP: 'badge-light-success',
      EMOM: 'badge-light-warning',
      Chipper: 'badge-light-info',
      Intervals: 'badge-light-danger',
    };
    return map[type] ?? 'badge-light-secondary';
  }
}
