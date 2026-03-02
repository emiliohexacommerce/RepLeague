export type WodType = 'ForTime' | 'AMRAP' | 'EMOM' | 'Chipper' | 'Intervals';
export type MovementType = 'barbell' | 'kb' | 'bodyweight' | 'gymnastic' | 'cardio' | 'other';

export interface WodExercise {
  id: string;
  orderIndex: number;
  name: string;
  movementType: MovementType;
  loadValue?: number;
  loadUnit?: string;
  reps?: number;
  notes?: string;
}

export interface WodResultAmrap {
  roundsCompleted: number;
  extraReps: number;
}

export interface WodResultEmom {
  totalMinutes: number;
  intervalsDone: number;
}

export interface WodEntry {
  id: string;
  type: WodType;
  title?: string;
  date: string; // YYYY-MM-DD
  timeCap?: string;
  elapsedTime?: string;
  rounds?: number;
  rxScaled: boolean;
  notes?: string;
  createdAt: string;
  exercises: WodExercise[];
  amrapResult?: WodResultAmrap;
  emomResult?: WodResultEmom;
}

// ── Create request ────────────────────────────────────────────────────────────

export interface CreateWodExerciseRequest {
  orderIndex: number;
  name: string;
  movementType: MovementType;
  loadValue?: number;
  loadUnit?: string;
  reps?: number;
  notes?: string;
}

export interface CreateWodEntryRequest {
  type: WodType;
  title?: string;
  date: string;
  timeCap?: string;
  elapsedTime?: string;
  rounds?: number;
  rxScaled: boolean;
  notes?: string;
  exercises: CreateWodExerciseRequest[];
  amrapResult?: WodResultAmrap;
  emomResult?: WodResultEmom;
}
