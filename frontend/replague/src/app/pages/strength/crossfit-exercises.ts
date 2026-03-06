/**
 * Lista estandarizada de movimientos con barra de CrossFit.
 * Se usa como fuente única en el dropdown de registro de sesiones
 * y en el módulo de PRs manuales.
 */

export interface CrossfitExercise {
  /** Clave única usada para almacenamiento (no cambia al traducir). */
  key: string;
  /** Nombre en español. */
  nameEs: string;
  /** Nombre en inglés (nombre oficial CrossFit). */
  nameEn: string;
  /** Categoría para agrupar opciones en el dropdown. */
  category: 'squat' | 'hinge' | 'press' | 'olympic' | 'carry';
}

export const CROSSFIT_BARBELL_EXERCISES: CrossfitExercise[] = [
  // ── Sentadillas ──────────────────────────────────────────────────────────
  { key: 'back_squat',       nameEs: 'Sentadilla con barra',       nameEn: 'Back Squat',          category: 'squat' },
  { key: 'front_squat',      nameEs: 'Sentadilla frontal',         nameEn: 'Front Squat',          category: 'squat' },
  { key: 'overhead_squat',   nameEs: 'Sentadilla sobre cabeza',    nameEn: 'Overhead Squat',       category: 'squat' },
  { key: 'zercher_squat',    nameEs: 'Sentadilla Zercher',         nameEn: 'Zercher Squat',        category: 'squat' },

  // ── Jalones / Bisagra ─────────────────────────────────────────────────────
  { key: 'deadlift',         nameEs: 'Peso muerto',                nameEn: 'Deadlift',             category: 'hinge' },
  { key: 'sumo_deadlift',    nameEs: 'Peso muerto sumo',           nameEn: 'Sumo Deadlift',        category: 'hinge' },
  { key: 'romanian_dl',      nameEs: 'Peso muerto rumano',         nameEn: 'Romanian Deadlift',    category: 'hinge' },
  { key: 'good_morning',     nameEs: 'Buenos días',                nameEn: 'Good Morning',         category: 'hinge' },

  // ── Press ────────────────────────────────────────────────────────────────
  { key: 'strict_press',     nameEs: 'Press estricto',             nameEn: 'Strict Press',         category: 'press' },
  { key: 'push_press',       nameEs: 'Push press',                 nameEn: 'Push Press',           category: 'press' },
  { key: 'push_jerk',        nameEs: 'Push jerk',                  nameEn: 'Push Jerk',            category: 'press' },
  { key: 'split_jerk',       nameEs: 'Split jerk',                 nameEn: 'Split Jerk',           category: 'press' },
  { key: 'bench_press',      nameEs: 'Press de banca',             nameEn: 'Bench Press',          category: 'press' },
  { key: 'barbell_row',      nameEs: 'Remo con barra',             nameEn: 'Barbell Row',          category: 'press' },

  // ── Olímpicos ─────────────────────────────────────────────────────────────
  { key: 'clean',            nameEs: 'Clean',                      nameEn: 'Clean',                category: 'olympic' },
  { key: 'power_clean',      nameEs: 'Power clean',                nameEn: 'Power Clean',          category: 'olympic' },
  { key: 'hang_clean',       nameEs: 'Hang clean',                 nameEn: 'Hang Clean',           category: 'olympic' },
  { key: 'hang_power_clean', nameEs: 'Hang power clean',           nameEn: 'Hang Power Clean',     category: 'olympic' },
  { key: 'clean_and_jerk',   nameEs: 'Clean & Jerk',               nameEn: 'Clean & Jerk',         category: 'olympic' },
  { key: 'snatch',           nameEs: 'Snatch',                     nameEn: 'Snatch',               category: 'olympic' },
  { key: 'power_snatch',     nameEs: 'Power snatch',               nameEn: 'Power Snatch',         category: 'olympic' },
  { key: 'hang_snatch',      nameEs: 'Hang snatch',                nameEn: 'Hang Snatch',          category: 'olympic' },
  { key: 'hang_power_snatch',nameEs: 'Hang power snatch',          nameEn: 'Hang Power Snatch',    category: 'olympic' },
  { key: 'thruster',         nameEs: 'Thruster',                   nameEn: 'Thruster',             category: 'olympic' },
];

/** Nombres de categorías para mostrar en el dropdown agrupado. */
export const EXERCISE_CATEGORY_LABELS: Record<CrossfitExercise['category'], string> = {
  squat:   'Sentadillas',
  hinge:   'Jalones / Peso muerto',
  press:   'Press',
  olympic: 'Olímpicos',
  carry:   'Cargadas',
};

/** Obtiene los ejercicios agrupados por categoría. */
export function getExercisesByCategory(): Record<string, CrossfitExercise[]> {
  return CROSSFIT_BARBELL_EXERCISES.reduce((acc, ex) => {
    const label = EXERCISE_CATEGORY_LABELS[ex.category];
    if (!acc[label]) acc[label] = [];
    acc[label].push(ex);
    return acc;
  }, {} as Record<string, CrossfitExercise[]>);
}

/** Devuelve el nombre para mostrar de un exercise key. */
export function getExerciseName(key: string): string {
  return CROSSFIT_BARBELL_EXERCISES.find(e => e.key === key)?.nameEs ?? key;
}
