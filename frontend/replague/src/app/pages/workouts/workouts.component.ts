import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { WorkoutsService } from './workouts.service';
import { Workout, CreateWorkoutRequest, WorkoutType } from './workout.model';

@Component({
  selector: 'app-workouts',
  templateUrl: './workouts.component.html',
})
export class WorkoutsComponent implements OnInit {
  workouts: Workout[] = [];
  loading = false;
  creating = false;
  showForm = false;
  error = '';
  successMessage = '';
  selectedRange: 'last7' | 'last30' | 'last90' = 'last30';

  workoutForm: FormGroup;

  constructor(private workoutsService: WorkoutsService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initForm();
    this.loadWorkouts();
  }

  initForm() {
    this.workoutForm = this.fb.group({
      date: [new Date().toISOString().split('T')[0], Validators.required],
      type: ['Strength', Validators.required],
      notes: [''],
      exercises: this.fb.array([]),
      wods: this.fb.array([]),
    });
  }

  get exercises(): FormArray {
    return this.workoutForm.get('exercises') as FormArray;
  }

  get wods(): FormArray {
    return this.workoutForm.get('wods') as FormArray;
  }

  get workoutType(): string {
    return this.workoutForm.get('type')?.value;
  }

  addExercise() {
    this.exercises.push(this.fb.group({
      exerciseName: ['', Validators.required],
      sets: [3, [Validators.required, Validators.min(1)]],
      reps: [10, [Validators.required, Validators.min(1)]],
      weightKg: [0, [Validators.required, Validators.min(0)]],
    }));
  }

  removeExercise(i: number) {
    this.exercises.removeAt(i);
  }

  addWod() {
    this.wods.push(this.fb.group({
      wodName: ['', Validators.required],
      durationSeconds: [0, [Validators.required, Validators.min(1)]],
      rounds: [1, [Validators.required, Validators.min(1)]],
    }));
  }

  removeWod(i: number) {
    this.wods.removeAt(i);
  }

  loadWorkouts() {
    this.loading = true;
    this.workoutsService.getWorkouts(this.selectedRange).subscribe({
      next: (data) => {
        this.workouts = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load workouts.';
        this.loading = false;
      },
    });
  }

  onRangeChange(range: 'last7' | 'last30' | 'last90') {
    this.selectedRange = range;
    this.loadWorkouts();
  }

  toggleForm() {
    this.showForm = !this.showForm;
    if (!this.showForm) {
      this.initForm();
    }
  }

  submit() {
    if (this.workoutForm.invalid) return;

    this.creating = true;
    this.error = '';
    this.successMessage = '';

    const val = this.workoutForm.value;
    const request: CreateWorkoutRequest = {
      date: val.date,
      type: val.type as WorkoutType,
      notes: val.notes || undefined,
      exercises: val.type === 'Strength' ? val.exercises : [],
      wods: val.type === 'WOD' ? val.wods : [],
    };

    this.workoutsService.createWorkout(request).subscribe({
      next: (workout) => {
        this.successMessage = workout.isPr ? 'Workout logged — New PR! ' : 'Workout logged successfully!';
        this.creating = false;
        this.showForm = false;
        this.initForm();
        this.loadWorkouts();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to create workout.';
        this.creating = false;
      },
    });
  }

  formatDuration(seconds: number): string {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}m ${s}s`;
  }
}
