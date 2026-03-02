export interface PersonalRecord {
  name: string;
  type: 'Strength' | 'WOD';
  weightKg?: number;
  duration?: string;
  sets?: number;
  reps?: number;
  achievedAt: string;
}
