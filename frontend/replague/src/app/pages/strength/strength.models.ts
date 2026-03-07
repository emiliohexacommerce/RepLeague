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

// ── Manual PRs ───────────────────────────────────────────────────────────────

export interface ManualLiftPrHistoryItem {
  id: string;
  weightKg: number;
  notes?: string;
  achievedAt: string; // DateOnly as ISO string YYYY-MM-DD
}

export interface ManualLiftPrGroupDto {
  exerciseName: string;
  bestWeightKg: number;
  bestAchievedAt: string;
  history: ManualLiftPrHistoryItem[];
}

export interface AddManualLiftPrRequest {
  exerciseName: string;
  weightKg: number;
  notes?: string;
  achievedAt: string; // YYYY-MM-DD
}

// ── Percentage table ─────────────────────────────────────────────────────────

export interface PercentageRow {
  percent: number;
  kg: number;
  lbs: number;
  perSideKg: number | null;
  perSideLbs: number | null;
}

export interface BarOption {
  label: string;
  kg: number;
}
