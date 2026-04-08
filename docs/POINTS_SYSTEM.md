# RepLeague — Sistema de Puntos y WOD del Día

## Descripción general

RepLeague incluye un sistema de puntos diarios que permite a los miembros de una liga competir de forma amistosa, independientemente de su nivel físico. El sistema recompensa tanto la consistencia como el rendimiento.

## Sistema de Puntos

### Puntos por día (máximo 10 puntos diarios)

| Criterio | Condición | Puntos |
|---|---|---|
| 🟢 Asistencia | Registró al menos una sesión (LiftSession o WodEntry) | +1 |
| 💪 Volumen | Volumen del día (sum reps×kg) supera su promedio histórico | +1 |
| 🏅 PR del día | Superó su 1RM estimado en cualquier ejercicio | +2 |
| ⏱️ WOD del día | Completó el WOD del día de la liga | +2 |
| 🥇 Mejor del WOD | Obtuvo el mejor resultado en el WOD del día | +2 |
| 🔥 Racha | Lleva 3 o más días consecutivos entrenando | +2 |

### Rankings disponibles

- **Diario**: puntos del día actual
- **Semanal**: suma de puntos de lunes a domingo de la semana actual
- **Mensual**: suma de puntos del mes actual

---

## WOD del Día

### ¿Cómo funciona?

1. El **primer atleta** que registra una sesión en la liga durante el día puede **proponer el WOD del día**.
2. El WOD es específico de cada liga (no es global).
3. Los demás miembros de la liga pueden sumarse al WOD del día y registrar su resultado.
4. No es obligatorio hacer el WOD del día — los atletas que no lo hacen simplemente no reciben los puntos de WOD (+2 y +2).

### Tipos de WOD soportados

- **ForTime**: se rankea por menor tiempo (`ElapsedSeconds`)
- **AMRAP**: se rankea por mayor número de rondas (`RoundsCompleted`)
- **EMOM**, **Chipper**, **Intervals**: se rankea por mayor número de reps totales (`TotalReps`)

### Resultado del WOD

Al registrar un resultado, el atleta puede especificar:
- Score general (tiempo / rondas / reps)
- **Desglose por ejercicio** (reps completadas, peso usado, duración)
- Si completó en Rx o Scaled
- Notas libres

---

## Activación del sistema en ligas existentes

El sistema de puntos **no se activa automáticamente** en ligas existentes. El administrador (owner) de la liga debe activarlo explícitamente.

### Opciones de activación

**Opción A — Arranque limpio (recomendado)**
- Los puntos comienzan desde la fecha de activación.
- Todos los miembros parten desde 0. Justo y competitivo.

**Opción B — Backfill histórico**
- Al activar, el sistema recorre el historial de sesiones existentes y genera puntos retroactivos.
- Los puntos retroactivos son **parciales** (máximo 6 de 10 por día):
  - ✅ Asistencia (+1)
  - ✅ Volumen (+1)
  - ✅ PR del día (+2)
  - ✅ Racha (+2)
  - ❌ WOD del día (no aplica — no existía el sistema)
  - ❌ Mejor del WOD (no aplica)
- Los puntos históricos se muestran con la nota "Puntos históricos parciales".

### Endpoint de activación

```
POST /api/leagues/{leagueId}/daily-wod/activate
Body: { "runBackfill": true|false }
```

---

## Visibilidad de perfiles en la liga

Cada atleta controla su visibilidad con el campo `Visibility` de su perfil:

| Nivel | Dentro de su liga | Fuera de su liga |
|---|---|---|
| `private` | Aparece en ranking como "Atleta Anónimo 🥷" — solo posición y puntos visibles | Invisible |
| `leagues` | Perfil completo visible: nombre, avatar, gym, bio, sesiones, resultados WOD | Invisible para no miembros |
| `public` | Perfil completo visible para todos | Visible para cualquier usuario |

### Nota sobre atletas `private`

Un atleta con visibilidad `private` **sí compite y acumula puntos** en la liga, pero sus compañeros no pueden ver su perfil ni sus sesiones. Solo se muestra su posición en el ranking y el total de puntos.

---

## Cálculo de puntos — Detalle técnico

Los puntos se calculan **en el momento** en que el atleta registra una sesión o resultado. El cálculo es idempotente (upsert): si ya existe un registro de `DailyPoints` para (UserId, LeagueId, Date), se actualiza.

### Fórmula de volumen
```
VolumenDiario = SUM(StrengthSet.Reps × StrengthSet.WeightKg)
PromHistórico = AVG(VolumenDiario) de todos los días anteriores del atleta en esa liga
VolumePoints = VolumenDiario > PromHistórico ? 1 : 0
```

### Fórmula de racha
```
StreakPoints = 2 si el atleta tiene AttendancePoints > 0 en los 2 días calendario anteriores
```

### Ranking del WOD del día
- **ForTime**: gana quien tenga menor `ElapsedSeconds` (excluyendo DNF)
- **AMRAP**: gana quien tenga mayor `RoundsCompleted`
- **Otros**: gana quien tenga mayor `TotalReps`
- En caso de empate, gana quien registró primero (`CreatedAt`)
