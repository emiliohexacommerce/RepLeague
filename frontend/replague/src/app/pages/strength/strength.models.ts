export interface StrengthSetDto {
  id: string;
  exerciseName: string;
  setNumber: number;
  reps: number;
  weightKg: number;
  isWarmup: boolean;
  isPr: boolean;
  oneRepMaxKg?: number;
  notes?: string;
}

export interface LiftSessionDto {
  id: string;
  date: string; // YYYY-MM-DD
  title?: string;
  notes?: string;
  createdAt: string;
  sets: StrengthSetDto[];
}

export interface LiftPrDto {
  exerciseName: string;
  bestWeightKg: number;
  bestReps: number;
  best1RmKg?: number;
  achievedAt: string;
}

// ── Create request ────────────────────────────────────────────────────────────

export interface CreateStrengthSetRequest {
  exerciseName: string;
  setNumber: number;
  reps: number;
  weightKg: number;
  isWarmup: boolean;
  notes?: string;
}

export interface CreateLiftSessionRequest {
  date: string;
  title?: string;
  notes?: string;
  sets: CreateStrengthSetRequest[];
}
