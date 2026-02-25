export interface WorkoutExercise {
  exerciseName: string;
  sets: number;
  reps: number;
  weightKg: number;
}

export interface WorkoutWod {
  wodName: string;
  durationSeconds: number;
  rounds: number;
}

export type WorkoutType = 'Strength' | 'WOD' | 'Cardio';

export interface Workout {
  id: string;
  date: string;
  type: WorkoutType;
  notes?: string;
  isPr: boolean;
  exercises: WorkoutExercise[];
  wods: WorkoutWod[];
  createdAt: string;
}

export interface CreateWorkoutRequest {
  date: string;
  type: WorkoutType;
  notes?: string;
  exercises: WorkoutExercise[];
  wods: WorkoutWod[];
}
