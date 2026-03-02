# Especificación Técnica — Módulo de Registro de Levantamientos y PRs
**RepLeague · v1.0 · 2026-02**

---

## Índice
1. [Modelo de datos (DDL)](#1-modelo-de-datos-ddl)
2. [API REST + CQRS](#2-api-rest--cqrs)
3. [Commands, Handlers y Validadores (C#)](#3-commands-handlers-y-validadores-c)
4. [Contratos TypeScript + Servicio Angular](#4-contratos-typescript--servicio-angular)
5. [Gráficos (ApexCharts)](#5-gráficos-apexcharts)
6. [Eventos de dominio e integración](#6-eventos-de-dominio-e-integración)
7. [Pruebas (xUnit + Gherkin)](#7-pruebas-xunit--gherkin)
8. [QA Checklist](#8-qa-checklist)

---

## 1. Modelo de datos (DDL)

### 1.1 Esquema completo

```sql
-- ─────────────────────────────────────────────────
-- Catálogo de ejercicios
-- ─────────────────────────────────────────────────
CREATE TABLE Exercise (
    Id         NVARCHAR(32)   NOT NULL,          -- slug: "SQUAT_BACK", "BENCH_PRESS"
    Code       NVARCHAR(32)   NOT NULL,
    Name       NVARCHAR(120)  NOT NULL,
    Unit       NVARCHAR(8)    NOT NULL DEFAULT 'kg',  -- 'kg' | 'lb'
    IsActive   BIT            NOT NULL DEFAULT 1,
    CONSTRAINT PK_Exercise PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX UQ_Exercise_Code ON Exercise(Code);

-- ─────────────────────────────────────────────────
-- Sesión de entrenamiento (agrupa varios sets)
-- ─────────────────────────────────────────────────
CREATE TABLE WorkoutSession (
    Id          UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    UserId      UNIQUEIDENTIFIER  NOT NULL,
    Date        DATE              NOT NULL,
    Notes       NVARCHAR(1000)    NULL,
    CreatedAt   DATETIME2(3)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2(3)      NULL,
    IsDeleted   BIT               NOT NULL DEFAULT 0,
    CONSTRAINT PK_WorkoutSession PRIMARY KEY (Id),
    CONSTRAINT FK_WorkoutSession_User FOREIGN KEY (UserId)
        REFERENCES [User](Id) ON DELETE CASCADE
);
CREATE INDEX IX_WorkoutSession_UserId_Date
    ON WorkoutSession(UserId, Date DESC) WHERE IsDeleted = 0;

-- ─────────────────────────────────────────────────
-- Set individual de fuerza
-- ─────────────────────────────────────────────────
CREATE TABLE StrengthSet (
    Id               UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    SessionId        UNIQUEIDENTIFIER  NOT NULL,
    ExerciseId       NVARCHAR(32)      NOT NULL,
    Reps             TINYINT           NOT NULL,
    Weight           DECIMAL(6,2)      NOT NULL,
    RPE              DECIMAL(4,2)      NULL,           -- Rate of Perceived Exertion 1-10
    IsPR             BIT               NOT NULL DEFAULT 0,
    Est1RM           DECIMAL(6,2)      NULL,
    ClientRequestId  UNIQUEIDENTIFIER  NULL,           -- idempotencia desde cliente
    IsDeleted        BIT               NOT NULL DEFAULT 0,
    CreatedAt        DATETIME2(3)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_StrengthSet PRIMARY KEY (Id),
    CONSTRAINT FK_StrengthSet_Session FOREIGN KEY (SessionId)
        REFERENCES WorkoutSession(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StrengthSet_Exercise FOREIGN KEY (ExerciseId)
        REFERENCES Exercise(Id),
    CONSTRAINT CK_StrengthSet_Reps   CHECK (Reps BETWEEN 1 AND 50),
    CONSTRAINT CK_StrengthSet_Weight CHECK (Weight > 0 AND Weight <= 500),
    CONSTRAINT CK_StrengthSet_RPE    CHECK (RPE IS NULL OR (RPE >= 1 AND RPE <= 10))
);
-- Impide duplicados exactos en el mismo segundo por cliente
CREATE UNIQUE INDEX UQ_StrengthSet_ClientRequestId
    ON StrengthSet(ClientRequestId) WHERE ClientRequestId IS NOT NULL AND IsDeleted = 0;
CREATE INDEX IX_StrengthSet_ExerciseId_Session
    ON StrengthSet(ExerciseId, SessionId) WHERE IsDeleted = 0;
-- Para consultar historial por usuario+ejercicio (vía JOIN con WorkoutSession)
CREATE INDEX IX_WorkoutSession_UserId_Exercise
    ON WorkoutSession(UserId) INCLUDE (Date) WHERE IsDeleted = 0;

-- ─────────────────────────────────────────────────
-- Preferencias del usuario
-- ─────────────────────────────────────────────────
CREATE TABLE UserPreference (
    UserId       UNIQUEIDENTIFIER  NOT NULL,
    OneRmMethod  NVARCHAR(16)      NOT NULL DEFAULT 'Epley',  -- 'Epley' | 'Brzycki'
    WeightUnit   NVARCHAR(4)       NOT NULL DEFAULT 'kg',
    CreatedAt    DATETIME2(3)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt    DATETIME2(3)      NULL,
    CONSTRAINT PK_UserPreference PRIMARY KEY (UserId),
    CONSTRAINT FK_UserPreference_User FOREIGN KEY (UserId)
        REFERENCES [User](Id) ON DELETE CASCADE,
    CONSTRAINT CK_UserPreference_Method CHECK (OneRmMethod IN ('Epley','Brzycki'))
);

-- ─────────────────────────────────────────────────
-- Registro histórico de PRs
-- ─────────────────────────────────────────────────
CREATE TABLE PrRecord (
    Id             UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    UserId         UNIQUEIDENTIFIER  NOT NULL,
    ExerciseId     NVARCHAR(32)      NOT NULL,
    Metric         NVARCHAR(16)      NOT NULL,  -- '1RM' | 'Weight1x1' | 'Volume'
    Value          DECIMAL(6,2)      NOT NULL,
    AchievedAt     DATETIME2(3)      NOT NULL,
    StrengthSetId  UNIQUEIDENTIFIER  NOT NULL,
    IsLatest       BIT               NOT NULL DEFAULT 1,
    CONSTRAINT PK_PrRecord PRIMARY KEY (Id),
    CONSTRAINT FK_PrRecord_User     FOREIGN KEY (UserId)     REFERENCES [User](Id),
    CONSTRAINT FK_PrRecord_Exercise FOREIGN KEY (ExerciseId) REFERENCES Exercise(Id),
    CONSTRAINT FK_PrRecord_Set      FOREIGN KEY (StrengthSetId) REFERENCES StrengthSet(Id)
);
CREATE INDEX IX_PrRecord_UserId_ExerciseId_Metric
    ON PrRecord(UserId, ExerciseId, Metric) WHERE IsLatest = 1;

-- ─────────────────────────────────────────────────
-- Auditoría
-- ─────────────────────────────────────────────────
CREATE TABLE AuditLog (
    Id          BIGINT IDENTITY(1,1) NOT NULL,
    UserId      UNIQUEIDENTIFIER     NOT NULL,
    Entity      NVARCHAR(64)         NOT NULL,
    EntityId    NVARCHAR(64)         NOT NULL,
    Action      NVARCHAR(16)         NOT NULL,  -- 'CREATE'|'UPDATE'|'DELETE'
    At          DATETIME2(3)         NOT NULL DEFAULT SYSUTCDATETIME(),
    PayloadJson NVARCHAR(MAX)        NOT NULL,
    CONSTRAINT PK_AuditLog PRIMARY KEY (Id)
);
CREATE INDEX IX_AuditLog_Entity_EntityId ON AuditLog(Entity, EntityId);
CREATE INDEX IX_AuditLog_UserId_At       ON AuditLog(UserId, At DESC);
```

### 1.2 Datos semilla (ejercicios básicos)

```sql
INSERT INTO Exercise (Id, Code, Name, Unit) VALUES
('SQUAT_BACK',   'SQUAT_BACK',   'Back Squat',        'kg'),
('SQUAT_FRONT',  'SQUAT_FRONT',  'Front Squat',        'kg'),
('BENCH_PRESS',  'BENCH_PRESS',  'Bench Press',        'kg'),
('DEADLIFT',     'DEADLIFT',     'Deadlift',           'kg'),
('OHPRESS',      'OHPRESS',      'Overhead Press',     'kg'),
('BARBELL_ROW',  'BARBELL_ROW',  'Barbell Row',        'kg'),
('PULLUP',       'PULLUP',       'Pull-up',            'kg'),
('CLEAN',        'CLEAN',        'Power Clean',        'kg');
```

---

## 2. API REST + CQRS

### Endpoints

| Método | Ruta | Command / Query | Descripción |
|--------|------|-----------------|-------------|
| `POST` | `/api/sessions` | `CreateWorkoutSessionCommand` | Crear sesión |
| `GET`  | `/api/sessions` | `GetSessionsQuery` | Listar sesiones (paginado) |
| `POST` | `/api/sessions/{sessionId}/strength-sets` | `AddStrengthSetCommand` | Agregar set + detectar PR |
| `PATCH`| `/api/strength-sets/{id}` | `UpdateStrengthSetCommand` | Editar set (recalcula métricas) |
| `DELETE`| `/api/strength-sets/{id}` | `SoftDeleteStrengthSetCommand` | Borrado lógico + auditoría |
| `GET`  | `/api/history` | `GetStrengthHistoryQuery` | Historial + agregados |
| `GET`  | `/api/prs` | `GetPrsQuery` | PRs por ejercicio y métrica |
| `POST` | `/api/preferences/onerm` | `SetOneRmPreferenceCommand` | Cambiar método 1RM |

### Contratos JSON de ejemplo

**POST /api/sessions — Request**
```json
{
  "date": "2026-02-17",
  "notes": "Día de sentadilla. Calentamiento completo."
}
```
**Response 201**
```json
{ "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

---

**POST /api/sessions/{sessionId}/strength-sets — Request**
```json
{
  "exerciseId": "SQUAT_BACK",
  "reps": 3,
  "weight": 150.0,
  "rpe": 8.5,
  "clientRequestId": "e0f1f7f6-2d7a-46d6-bf4c-7f1b1e121111"
}
```
**Response 201**
```json
{
  "id": "a1b2c3d4-0000-0000-0000-000000000001",
  "exerciseId": "SQUAT_BACK",
  "exerciseName": "Back Squat",
  "reps": 3,
  "weight": 150.0,
  "rpe": 8.5,
  "est1RM": 165.0,
  "isPR": false,
  "createdAt": "2026-02-17T10:32:00Z"
}
```

---

**GET /api/history?exerciseId=SQUAT_BACK&from=2026-01-01&to=2026-02-28 — Response 200**
```json
{
  "exerciseId": "SQUAT_BACK",
  "exerciseName": "Back Squat",
  "items": [
    {
      "date": "2026-02-10",
      "sessionId": "...",
      "sets": [
        { "id": "...", "reps": 5, "weight": 140.0, "est1RM": 156.67, "isPR": false },
        { "id": "...", "reps": 3, "weight": 145.0, "est1RM": 159.5,  "isPR": false }
      ]
    },
    {
      "date": "2026-02-17",
      "sessionId": "...",
      "sets": [
        { "id": "...", "reps": 1, "weight": 170.0, "est1RM": 170.0, "isPR": true }
      ]
    }
  ],
  "aggregates": {
    "best1RM": 170.0,
    "lastWeight": 170.0,
    "totalSets": 12,
    "volumeByWeek": [
      { "isoWeek": "2026-W06", "volume": 4500.0 },
      { "isoWeek": "2026-W07", "volume": 5200.0 }
    ]
  }
}
```

---

**GET /api/prs?exerciseId=SQUAT_BACK — Response 200**
```json
{
  "exerciseId": "SQUAT_BACK",
  "exerciseName": "Back Squat",
  "records": [
    {
      "metric": "1RM",
      "value": 170.0,
      "achievedAt": "2026-02-17T10:32:00Z",
      "strengthSetId": "...",
      "history": [
        { "value": 150.0, "achievedAt": "2026-01-15T09:00:00Z" },
        { "value": 162.5, "achievedAt": "2026-01-29T09:00:00Z" },
        { "value": 170.0, "achievedAt": "2026-02-17T10:32:00Z" }
      ]
    }
  ]
}
```

---

## 3. Commands, Handlers y Validadores (C#)

### 3.1 Entidades de dominio

```csharp
// Domain/Entities/WorkoutSession.cs
public class WorkoutSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateOnly Date { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<StrengthSet> Sets => _sets.AsReadOnly();
    private readonly List<StrengthSet> _sets = [];

    // EF constructor
    private WorkoutSession() { }

    public static WorkoutSession Create(Guid userId, DateOnly date, string? notes)
        => new() { Id = Guid.NewGuid(), UserId = userId, Date = date, Notes = notes, CreatedAt = DateTime.UtcNow };
}

// Domain/Entities/StrengthSet.cs
public class StrengthSet
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public string ExerciseId { get; private set; } = null!;
    public int Reps { get; private set; }
    public decimal Weight { get; private set; }
    public decimal? RPE { get; private set; }
    public bool IsPR { get; private set; }
    public decimal? Est1RM { get; private set; }
    public Guid? ClientRequestId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Domain events
    private readonly List<IDomainEvent> _events = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();
    public void ClearEvents() => _events.Clear();

    private StrengthSet() { }

    public static StrengthSet Create(
        Guid sessionId, string exerciseId,
        int reps, decimal weight, decimal? rpe,
        decimal? est1RM, bool isPR, Guid? clientRequestId)
    {
        var set = new StrengthSet
        {
            Id = Guid.NewGuid(), SessionId = sessionId,
            ExerciseId = exerciseId, Reps = reps, Weight = weight,
            RPE = rpe, Est1RM = est1RM, IsPR = isPR,
            ClientRequestId = clientRequestId, CreatedAt = DateTime.UtcNow
        };
        if (isPR)
            set._events.Add(new NewPrAchievedEvent(sessionId, exerciseId, est1RM ?? weight, set.Id));
        return set;
    }

    public void SoftDelete() => IsDeleted = true;
}
```

### 3.2 Lógica de negocio — cálculo 1RM

```csharp
// Application/Services/OneRmCalculator.cs
public static class OneRmCalculator
{
    /// <param name="method">Epley | Brzycki</param>
    public static decimal Calculate(decimal weight, int reps, string method)
    {
        if (reps == 1) return weight;   // 1x1 es el 1RM exacto

        return method switch
        {
            "Epley"   => Math.Round(weight * (1m + reps / 30m), 2),
            "Brzycki" => reps >= 37
                ? weight   // Brzycki diverge para reps ≥ 37
                : Math.Round(weight * (36m / (37m - reps)), 2),
            _ => throw new ArgumentException($"Método desconocido: {method}")
        };
    }
}
```

### 3.3 AddStrengthSetCommand

```csharp
// Application/Features/Lifting/Commands/AddStrengthSet/AddStrengthSetCommand.cs
public record AddStrengthSetCommand(
    Guid UserId,
    Guid SessionId,
    string ExerciseId,
    int Reps,
    decimal Weight,
    decimal? RPE,
    Guid? ClientRequestId
) : IRequest<StrengthSetDto>;
```

```csharp
// Application/Features/Lifting/Commands/AddStrengthSet/AddStrengthSetCommandHandler.cs
public class AddStrengthSetCommandHandler(
    IAppDbContext db,
    IPublisher publisher,
    IAuditService audit) : IRequestHandler<AddStrengthSetCommand, StrengthSetDto>
{
    public async Task<StrengthSetDto> Handle(AddStrengthSetCommand cmd, CancellationToken ct)
    {
        // Idempotencia: retornar existente si clientRequestId ya fue procesado
        if (cmd.ClientRequestId.HasValue)
        {
            var existing = await db.StrengthSets
                .FirstOrDefaultAsync(s => s.ClientRequestId == cmd.ClientRequestId
                                       && !s.IsDeleted, ct);
            if (existing is not null)
                return ToDto(existing);
        }

        // Verificar que la sesión pertenece al usuario
        var session = await db.WorkoutSessions
            .FirstOrDefaultAsync(s => s.Id == cmd.SessionId
                                   && s.UserId == cmd.UserId
                                   && !s.IsDeleted, ct)
            ?? throw new NotFoundException("Session not found.");

        // Preferencia del usuario para 1RM
        var pref = await db.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == cmd.UserId, ct);
        var method = pref?.OneRmMethod ?? "Epley";

        var est1RM = OneRmCalculator.Calculate(cmd.Weight, cmd.Reps, method);

        // Detectar PR: ¿es este Est1RM mayor al PR actual?
        var currentPr = await db.PrRecords
            .Where(p => p.UserId == cmd.UserId
                     && p.ExerciseId == cmd.ExerciseId
                     && p.Metric == "1RM"
                     && p.IsLatest)
            .Select(p => (decimal?)p.Value)
            .FirstOrDefaultAsync(ct);

        var isPR = currentPr == null || est1RM > currentPr;

        var set = StrengthSet.Create(
            cmd.SessionId, cmd.ExerciseId, cmd.Reps, cmd.Weight,
            cmd.RPE, est1RM, isPR, cmd.ClientRequestId);

        db.StrengthSets.Add(set);

        if (isPR)
        {
            // Marcar anterior como no-latest
            await db.PrRecords
                .Where(p => p.UserId == cmd.UserId
                         && p.ExerciseId == cmd.ExerciseId
                         && p.Metric == "1RM"
                         && p.IsLatest)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsLatest, false), ct);

            db.PrRecords.Add(new PrRecord
            {
                Id = Guid.NewGuid(), UserId = cmd.UserId,
                ExerciseId = cmd.ExerciseId, Metric = "1RM",
                Value = est1RM, AchievedAt = DateTime.UtcNow,
                StrengthSetId = set.Id, IsLatest = true
            });
        }

        await db.SaveChangesAsync(ct);

        // Auditoría y eventos de dominio
        await audit.LogAsync(cmd.UserId, "StrengthSet", set.Id.ToString(), "CREATE",
            JsonSerializer.Serialize(cmd), ct);

        foreach (var ev in set.DomainEvents)
            await publisher.Publish(ev, ct);

        return ToDto(set);
    }

    private static StrengthSetDto ToDto(StrengthSet s) => new(
        s.Id, s.ExerciseId, s.Reps, s.Weight, s.RPE, s.Est1RM, s.IsPR, s.CreatedAt);
}
```

### 3.4 FluentValidation

```csharp
// Application/Features/Lifting/Commands/AddStrengthSet/AddStrengthSetCommandValidator.cs
public class AddStrengthSetCommandValidator : AbstractValidator<AddStrengthSetCommand>
{
    public AddStrengthSetCommandValidator(IAppDbContext db)
    {
        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("exerciseId es requerido.")
            .MaximumLength(32)
            .MustAsync(async (id, ct) =>
                await db.Exercises.AnyAsync(e => e.Id == id && e.IsActive, ct))
            .WithMessage("El ejercicio no existe o está inactivo.");

        RuleFor(x => x.Reps)
            .InclusiveBetween(1, 50).WithMessage("reps debe estar entre 1 y 50.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("weight debe ser positivo.")
            .LessThanOrEqualTo(500).WithMessage("weight no puede superar 500 kg.");

        RuleFor(x => x.RPE)
            .InclusiveBetween(1, 10).When(x => x.RPE.HasValue)
            .WithMessage("RPE debe estar entre 1 y 10.");

        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("sessionId es requerido.");
    }
}
```

### 3.5 GetStrengthHistoryQueryHandler

```csharp
public record GetStrengthHistoryQuery(
    Guid UserId, string ExerciseId,
    DateOnly? From, DateOnly? To
) : IRequest<StrengthHistoryDto>;

public class GetStrengthHistoryQueryHandler(IAppDbContext db)
    : IRequestHandler<GetStrengthHistoryQuery, StrengthHistoryDto>
{
    public async Task<StrengthHistoryDto> Handle(GetStrengthHistoryQuery q, CancellationToken ct)
    {
        var query = db.StrengthSets
            .Where(s => s.Session.UserId == q.UserId
                     && s.ExerciseId == q.ExerciseId
                     && !s.IsDeleted
                     && !s.Session.IsDeleted);

        if (q.From.HasValue)
            query = query.Where(s => s.Session.Date >= q.From.Value);
        if (q.To.HasValue)
            query = query.Where(s => s.Session.Date <= q.To.Value);

        var sets = await query
            .OrderBy(s => s.Session.Date).ThenBy(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id, Date = s.Session.Date, s.SessionId,
                s.Reps, s.Weight, s.Est1RM, s.IsPR, s.RPE, s.CreatedAt
            })
            .ToListAsync(ct);

        // Agrupar por fecha de sesión
        var items = sets
            .GroupBy(s => (s.Date, s.SessionId))
            .Select(g => new SessionSetsDto(
                g.Key.Date.ToString("yyyy-MM-dd"),
                g.Key.SessionId,
                g.Select(s => new StrengthSetSummaryDto(
                    s.Id, s.Reps, s.Weight, s.Est1RM, s.IsPR)).ToList()))
            .ToList();

        // Agregados
        var allEst1RM = sets.Where(s => s.Est1RM.HasValue).Select(s => s.Est1RM!.Value).ToList();
        var best1RM = allEst1RM.Any() ? allEst1RM.Max() : 0m;
        var lastWeight = sets.Any() ? sets.Last().Weight : 0m;

        // Volumen semanal (reps × weight por semana ISO)
        var volumeByWeek = sets
            .GroupBy(s => ISOWeek.GetYear(s.Date.ToDateTime(TimeOnly.MinValue)).ToString()
                         + "-W" + ISOWeek.GetWeekOfYear(s.Date.ToDateTime(TimeOnly.MinValue)).ToString("D2"))
            .Select(g => new WeeklyVolumeDto(
                g.Key,
                g.Sum(s => s.Reps * s.Weight)))
            .OrderBy(w => w.IsoWeek)
            .ToList();

        return new StrengthHistoryDto(
            q.ExerciseId, items,
            new AggregatesDto(best1RM, lastWeight, sets.Count, volumeByWeek));
    }
}
```

### 3.6 Controller

```csharp
// API/Controllers/LiftingController.cs
[Authorize]
[Route("api")]
public class LiftingController(IMediator mediator) : BaseApiController(mediator)
{
    // POST /api/sessions
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(CreateSessionResponse), 201)]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreateSessionRequest req, CancellationToken ct)
    {
        var id = await Mediator.Send(
            new CreateWorkoutSessionCommand(CurrentUserId, req.Date, req.Notes), ct);
        return StatusCode(201, new { sessionId = id });
    }

    // POST /api/sessions/{sessionId}/strength-sets
    [HttpPost("sessions/{sessionId:guid}/strength-sets")]
    [ProducesResponseType(typeof(StrengthSetDto), 201)]
    public async Task<IActionResult> AddSet(
        Guid sessionId, [FromBody] AddStrengthSetRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new AddStrengthSetCommand(
            CurrentUserId, sessionId, req.ExerciseId,
            req.Reps, req.Weight, req.RPE, req.ClientRequestId), ct);
        return StatusCode(201, result);
    }

    // GET /api/history?exerciseId=&from=&to=
    [HttpGet("history")]
    [ProducesResponseType(typeof(StrengthHistoryDto), 200)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string exerciseId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetStrengthHistoryQuery(CurrentUserId, exerciseId, from, to), ct);
        return Ok(result);
    }

    // PATCH /api/strength-sets/{id}
    [HttpPatch("strength-sets/{id:guid}")]
    public async Task<IActionResult> UpdateSet(
        Guid id, [FromBody] UpdateStrengthSetRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateStrengthSetCommand(CurrentUserId, id, req.Reps, req.Weight, req.RPE), ct);
        return Ok(result);
    }

    // DELETE /api/strength-sets/{id}
    [HttpDelete("strength-sets/{id:guid}")]
    public async Task<IActionResult> DeleteSet(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new SoftDeleteStrengthSetCommand(CurrentUserId, id), ct);
        return NoContent();
    }

    // GET /api/prs?exerciseId=
    [HttpGet("prs")]
    [ProducesResponseType(typeof(ExercisePrsDto), 200)]
    public async Task<IActionResult> GetPrs(
        [FromQuery] string? exerciseId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPrsQuery(CurrentUserId, exerciseId), ct);
        return Ok(result);
    }

    // POST /api/preferences/onerm
    [HttpPost("preferences/onerm")]
    public async Task<IActionResult> SetOneRmPreference(
        [FromBody] SetOneRmPreferenceRequest req, CancellationToken ct)
    {
        await Mediator.Send(new SetOneRmPreferenceCommand(CurrentUserId, req.Method), ct);
        return NoContent();
    }
}

// Request models
public record CreateSessionRequest(DateOnly Date, string? Notes);
public record AddStrengthSetRequest(
    string ExerciseId, int Reps, decimal Weight,
    decimal? RPE, Guid? ClientRequestId);
public record UpdateStrengthSetRequest(int Reps, decimal Weight, decimal? RPE);
public record SetOneRmPreferenceRequest(string Method);
```

---

## 4. Contratos TypeScript + Servicio Angular

### 4.1 Interfaces

```typescript
// models/lifting.models.ts

export type OneRmMethod = 'Epley' | 'Brzycki';
export type PrMetric   = '1RM' | 'Weight1x1' | 'Volume';

export interface CreateSessionRequest {
  date: string;       // 'YYYY-MM-DD'
  notes?: string;
}
export interface CreateSessionResponse {
  sessionId: string;
}

export interface AddStrengthSetRequest {
  exerciseId: string;
  reps: number;
  weight: number;
  rpe?: number;
  clientRequestId?: string;
}

export interface StrengthSetDto {
  id: string;
  exerciseId: string;
  exerciseName?: string;
  reps: number;
  weight: number;
  rpe?: number;
  est1RM?: number;
  isPR: boolean;
  createdAt: string;
}

export interface SessionSetsDto {
  date: string;
  sessionId: string;
  sets: StrengthSetSummaryDto[];
}
export interface StrengthSetSummaryDto {
  id: string;
  reps: number;
  weight: number;
  est1RM?: number;
  isPR: boolean;
}

export interface WeeklyVolumeDto {
  isoWeek: string;    // 'YYYY-Www'
  volume: number;
}
export interface AggregatesDto {
  best1RM: number;
  lastWeight: number;
  totalSets: number;
  volumeByWeek: WeeklyVolumeDto[];
}
export interface StrengthHistoryDto {
  exerciseId: string;
  exerciseName?: string;
  items: SessionSetsDto[];
  aggregates: AggregatesDto;
}

export interface PrHistoryPoint {
  value: number;
  achievedAt: string;
}
export interface PrRecordDto {
  metric: PrMetric;
  value: number;
  achievedAt: string;
  strengthSetId: string;
  history: PrHistoryPoint[];
}
export interface ExercisePrsDto {
  exerciseId: string;
  exerciseName?: string;
  records: PrRecordDto[];
}

export interface Exercise {
  id: string;
  code: string;
  name: string;
  unit: 'kg' | 'lb';
}
```

### 4.2 Servicio Angular

```typescript
// services/lifting.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import {
  CreateSessionRequest, CreateSessionResponse,
  AddStrengthSetRequest, StrengthSetDto,
  StrengthHistoryDto, ExercisePrsDto, Exercise
} from '../models/lifting.models';

@Injectable({ providedIn: 'root' })
export class LiftingService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  // ── Sesiones ────────────────────────────────────────────────────────
  createSession(req: CreateSessionRequest): Observable<CreateSessionResponse> {
    return this.http.post<CreateSessionResponse>(`${this.base}/sessions`, req);
  }

  // ── Sets ────────────────────────────────────────────────────────────
  addSet(sessionId: string, req: AddStrengthSetRequest): Observable<StrengthSetDto> {
    return this.http.post<StrengthSetDto>(
      `${this.base}/sessions/${sessionId}/strength-sets`, req);
  }

  updateSet(id: string, reps: number, weight: number, rpe?: number): Observable<StrengthSetDto> {
    return this.http.patch<StrengthSetDto>(
      `${this.base}/strength-sets/${id}`, { reps, weight, rpe });
  }

  deleteSet(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/strength-sets/${id}`);
  }

  // ── Historial ───────────────────────────────────────────────────────
  getHistory(
    exerciseId: string, from?: string, to?: string
  ): Observable<StrengthHistoryDto> {
    let params = new HttpParams().set('exerciseId', exerciseId);
    if (from) params = params.set('from', from);
    if (to)   params = params.set('to', to);
    return this.http.get<StrengthHistoryDto>(`${this.base}/history`, { params });
  }

  // ── PRs ─────────────────────────────────────────────────────────────
  getPrs(exerciseId?: string): Observable<ExercisePrsDto[]> {
    const params = exerciseId
      ? new HttpParams().set('exerciseId', exerciseId)
      : new HttpParams();
    return this.http.get<ExercisePrsDto[]>(`${this.base}/prs`, { params });
  }

  // ── Ejercicios ──────────────────────────────────────────────────────
  getExercises(): Observable<Exercise[]> {
    return this.http.get<Exercise[]>(`${this.base}/exercises`);
  }

  // ── Preferencias ────────────────────────────────────────────────────
  setOneRmMethod(method: 'Epley' | 'Brzycki'): Observable<void> {
    return this.http.post<void>(`${this.base}/preferences/onerm`, { method });
  }
}
```

### 4.3 OfflineQueueService (PWA Background Sync)

```typescript
// services/offline-queue.service.ts
import { Injectable } from '@angular/core';
import { LiftingService } from './lifting.service';
import { AddStrengthSetRequest } from '../models/lifting.models';
import { v4 as uuidv4 } from 'uuid';

interface QueuedRequest {
  id: string;
  sessionId: string;
  payload: AddStrengthSetRequest;
  timestamp: number;
}

const QUEUE_KEY = 'lifting_offline_queue';

@Injectable({ providedIn: 'root' })
export class OfflineQueueService {
  constructor(private liftingService: LiftingService) {
    window.addEventListener('online', () => this.flush());
  }

  enqueue(sessionId: string, req: AddStrengthSetRequest): void {
    const queue = this.getQueue();
    queue.push({
      id: uuidv4(),
      sessionId,
      payload: { ...req, clientRequestId: req.clientRequestId ?? uuidv4() },
      timestamp: Date.now(),
    });
    localStorage.setItem(QUEUE_KEY, JSON.stringify(queue));
  }

  async flush(): Promise<void> {
    if (!navigator.onLine) return;
    const queue = this.getQueue();
    if (queue.length === 0) return;

    const pending = [...queue];
    localStorage.setItem(QUEUE_KEY, JSON.stringify([]));

    for (const item of pending) {
      try {
        await this.liftingService.addSet(item.sessionId, item.payload).toPromise();
      } catch {
        // Re-encolar si falla (excepto 409 Conflict = ya fue procesado)
        const remaining = this.getQueue();
        remaining.push(item);
        localStorage.setItem(QUEUE_KEY, JSON.stringify(remaining));
      }
    }
  }

  getQueueLength(): number {
    return this.getQueue().length;
  }

  private getQueue(): QueuedRequest[] {
    try {
      return JSON.parse(localStorage.getItem(QUEUE_KEY) ?? '[]');
    } catch { return []; }
  }
}
```

---

## 5. Gráficos (ApexCharts)

El proyecto ya usa **ApexCharts** (`ng-apexcharts`). Ambas configuraciones son JSON listos para pegar.

### 5.1 Línea de 1RM estimado + marcadores de PR

```typescript
// lift-progress-chart.config.ts
import { ApexOptions } from 'ng-apexcharts';
import { StrengthHistoryDto } from '../models/lifting.models';

export function buildOneRmChartOptions(history: StrengthHistoryDto): ApexOptions {
  // Aplanar todos los sets con fecha
  const allSets = history.items.flatMap(day =>
    day.sets
      .filter(s => s.est1RM != null)
      .map(s => ({ date: day.date, est1RM: s.est1RM!, isPR: s.isPR,
                   reps: s.reps, weight: s.weight }))
  );

  const dates   = allSets.map(s => s.date);
  const values  = allSets.map(s => s.est1RM);
  const prMask  = allSets.map(s => s.isPR ? s.est1RM : null);

  return {
    series: [
      {
        name: '1RM Estimado',
        type: 'line',
        data: values,
      },
      {
        name: 'PR',
        type: 'scatter',
        data: prMask,
      },
    ],
    chart: {
      height: 320,
      type: 'line',
      toolbar: { show: false },
      background: '#1E1E1E',
      foreColor: '#CCCCCC',
    },
    colors: ['#FF7A1A', '#22C55E'],
    stroke: { curve: 'smooth', width: [3, 0] },
    markers: {
      size: [0, 10],
      strokeWidth: 0,
      hover: { size: 6 },
    },
    xaxis: {
      categories: dates,
      type: 'category',
      labels: { rotate: -30, style: { fontSize: '11px' } },
    },
    yaxis: {
      title: { text: 'kg' },
      min: (min: number) => Math.floor(min * 0.9),
      labels: { formatter: (v: number) => `${v} kg` },
    },
    tooltip: {
      shared: false,
      custom: ({ seriesIndex, dataPointIndex }) => {
        const s = allSets[dataPointIndex];
        return `<div style="padding:8px;font-size:12px;">
          <b>${s.date}</b><br/>
          ${s.reps} × ${s.weight} kg<br/>
          1RM est.: <b>${s.est1RM} kg</b>
          ${s.isPR ? '<br/><span style="color:#22C55E">🏆 PR</span>' : ''}
        </div>`;
      },
    },
    annotations: {
      yaxis: [],
      points: allSets
        .filter(s => s.isPR)
        .map(s => ({
          x: s.date,
          y: s.est1RM,
          marker: { size: 10, fillColor: '#22C55E', strokeWidth: 0 },
          label: {
            text: `PR ${s.est1RM} kg`,
            style: { color: '#fff', background: '#22C55E', fontSize: '11px' },
          },
        })),
    },
    grid: { borderColor: '#2F2F2F' },
    legend: { position: 'top' },
  };
}
```

### 5.2 Barras de volumen semanal

```typescript
// lift-volume-chart.config.ts
import { ApexOptions } from 'ng-apexcharts';
import { WeeklyVolumeDto } from '../models/lifting.models';

export function buildVolumeChartOptions(weeks: WeeklyVolumeDto[]): ApexOptions {
  return {
    series: [{ name: 'Volumen (kg×reps)', data: weeks.map(w => w.volume) }],
    chart: {
      type: 'bar',
      height: 260,
      toolbar: { show: false },
      background: '#1E1E1E',
      foreColor: '#CCCCCC',
    },
    colors: ['#FF7A1A'],
    plotOptions: {
      bar: { borderRadius: 4, columnWidth: '60%' },
    },
    xaxis: {
      categories: weeks.map(w => w.isoWeek),
      labels: { style: { fontSize: '11px' } },
    },
    yaxis: {
      title: { text: 'Volumen (kg)' },
      labels: { formatter: (v: number) => `${(v / 1000).toFixed(1)}k` },
    },
    tooltip: {
      y: { formatter: (v: number) => `${v.toLocaleString()} kg` },
    },
    dataLabels: { enabled: false },
    grid: { borderColor: '#2F2F2F' },
  };
}
```

### Mock data para desarrollo

```typescript
// mocks/lifting.mock.ts
import { StrengthHistoryDto } from '../models/lifting.models';

export const MOCK_SQUAT_HISTORY: StrengthHistoryDto = {
  exerciseId: 'SQUAT_BACK',
  exerciseName: 'Back Squat',
  items: [
    { date: '2026-01-13', sessionId: 'sess-001',
      sets: [
        { id: 's1', reps: 5, weight: 130, est1RM: 143.33, isPR: true  },
        { id: 's2', reps: 5, weight: 130, est1RM: 143.33, isPR: false },
        { id: 's3', reps: 5, weight: 130, est1RM: 143.33, isPR: false },
      ]},
    { date: '2026-01-20', sessionId: 'sess-002',
      sets: [
        { id: 's4', reps: 3, weight: 140, est1RM: 154.00, isPR: true  },
        { id: 's5', reps: 3, weight: 140, est1RM: 154.00, isPR: false },
      ]},
    { date: '2026-02-03', sessionId: 'sess-003',
      sets: [
        { id: 's6', reps: 1, weight: 160, est1RM: 160.00, isPR: true  },
      ]},
    { date: '2026-02-17', sessionId: 'sess-004',
      sets: [
        { id: 's7', reps: 1, weight: 170, est1RM: 170.00, isPR: true  },
      ]},
  ],
  aggregates: {
    best1RM: 170,
    lastWeight: 170,
    totalSets: 7,
    volumeByWeek: [
      { isoWeek: '2026-W03', volume: 3900 },
      { isoWeek: '2026-W04', volume: 2800 },
      { isoWeek: '2026-W06', volume: 1600 },
      { isoWeek: '2026-W07', volume: 1700 },
    ],
  },
};
```

---

## 6. Eventos de dominio e integración

### 6.1 Evento de dominio

```csharp
// Domain/Events/NewPrAchievedEvent.cs
public record NewPrAchievedEvent(
    Guid SessionId,
    string ExerciseId,
    decimal Value,
    Guid StrengthSetId
) : IDomainEvent;
```

### 6.2 Handler del evento (outbox + Web Push)

```csharp
// Application/Features/Lifting/EventHandlers/NewPrAchievedEventHandler.cs
public class NewPrAchievedEventHandler(
    IAppDbContext db,
    IWebPushService webPush,
    IOutboxService outbox,
    ILogger<NewPrAchievedEventHandler> logger)
    : INotificationHandler<NewPrAchievedEvent>
{
    public async Task Handle(NewPrAchievedEvent ev, CancellationToken ct)
    {
        // Obtener el userId a partir de la sesión
        var userId = await db.WorkoutSessions
            .Where(s => s.Id == ev.SessionId)
            .Select(s => s.UserId)
            .FirstAsync(ct);

        var exerciseName = await db.Exercises
            .Where(e => e.Id == ev.ExerciseId)
            .Select(e => e.Name)
            .FirstOrDefaultAsync(ct) ?? ev.ExerciseId;

        // Web Push (no bloquea si falla)
        _ = Task.Run(async () =>
        {
            try
            {
                await webPush.SendAsync(userId,
                    title: "¡Nuevo PR! 🏆",
                    body: $"{exerciseName}: {ev.Value:F1} kg (1RM est.)",
                    ct: ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Web Push no enviado para userId={UserId}", userId);
            }
        });

        // Outbox para email SendGrid (envío diferido)
        await outbox.EnqueueAsync(new OutboxMessage
        {
            Type = "NewPrAchieved",
            Payload = JsonSerializer.Serialize(new
            {
                UserId = userId,
                ExerciseName = exerciseName,
                Value = ev.Value,
                AchievedAt = DateTime.UtcNow
            })
        }, ct);
    }
}
```

---

## 7. Pruebas (xUnit + Gherkin)

### 7.1 Pruebas unitarias — Cálculo 1RM

```csharp
// Tests/Unit/OneRmCalculatorTests.cs
public class OneRmCalculatorTests
{
    [Theory]
    [InlineData(100, 1,  "Epley",   100.0)]   // 1 rep = peso exacto
    [InlineData(100, 5,  "Epley",   116.67)]
    [InlineData(100, 10, "Epley",   133.33)]
    [InlineData(100, 1,  "Brzycki", 100.0)]
    [InlineData(100, 5,  "Brzycki", 112.5)]
    [InlineData(100, 10, "Brzycki", 133.33)]
    public void Calculate_ReturnsExpectedValue(
        decimal weight, int reps, string method, decimal expected)
    {
        var result = OneRmCalculator.Calculate(weight, reps, method);
        Assert.Equal(expected, result, precision: 2);
    }

    [Fact]
    public void Calculate_Brzycki_Reps37_ReturnsSafeValue()
    {
        // Brzycki diverge en reps ≥ 37 → retorna el peso
        var result = OneRmCalculator.Calculate(60, 37, "Brzycki");
        Assert.Equal(60, result);
    }

    [Fact]
    public void Calculate_UnknownMethod_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            OneRmCalculator.Calculate(100, 5, "Unknown"));
    }
}
```

### 7.2 Pruebas de integración — AddStrengthSetCommandHandler

```csharp
// Tests/Integration/AddStrengthSetTests.cs
public class AddStrengthSetTests : IClassFixture<TestDbFixture>
{
    private readonly TestDbFixture _fixture;
    public AddStrengthSetTests(TestDbFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_FirstSet_MarkedAsPR()
    {
        var (db, userId) = await _fixture.CreateUserWithPreferenceAsync("Epley");
        var session = await _fixture.CreateSessionAsync(db, userId, DateOnly.Today);

        var handler = new AddStrengthSetCommandHandler(db, NullPublisher.Instance, NullAudit.Instance);
        var result  = await handler.Handle(
            new AddStrengthSetCommand(userId, session.Id, "SQUAT_BACK", 1, 170, null, null),
            CancellationToken.None);

        Assert.True(result.IsPR);
        Assert.Equal(170m, result.Est1RM);
    }

    [Fact]
    public async Task Handle_LowerWeight_NotPR()
    {
        var (db, userId) = await _fixture.CreateUserWithPrAsync("SQUAT_BACK", 170m, "Epley");
        var session = await _fixture.CreateSessionAsync(db, userId, DateOnly.Today);

        var handler = new AddStrengthSetCommandHandler(db, NullPublisher.Instance, NullAudit.Instance);
        var result  = await handler.Handle(
            new AddStrengthSetCommand(userId, session.Id, "SQUAT_BACK", 5, 100, null, null),
            CancellationToken.None);

        Assert.False(result.IsPR);
    }

    [Fact]
    public async Task Handle_SameClientRequestId_IsIdempotent()
    {
        var (db, userId) = await _fixture.CreateUserWithPreferenceAsync("Epley");
        var session      = await _fixture.CreateSessionAsync(db, userId, DateOnly.Today);
        var requestId    = Guid.NewGuid();
        var cmd = new AddStrengthSetCommand(userId, session.Id, "BENCH_PRESS", 5, 100, null, requestId);

        var handler = new AddStrengthSetCommandHandler(db, NullPublisher.Instance, NullAudit.Instance);
        var first   = await handler.Handle(cmd, CancellationToken.None);
        var second  = await handler.Handle(cmd, CancellationToken.None);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await db.StrengthSets.CountAsync(s => s.ClientRequestId == requestId));
    }

    [Fact]
    public async Task Handle_TieBreaker_HigherEst1RmWins()
    {
        // PR con 3×150 (est1RM 165) → nuevo set 1×164 (est1RM 164) → NO es PR
        var (db, userId) = await _fixture.CreateUserWithPrAsync("DEADLIFT", 165m, "Epley");
        var session = await _fixture.CreateSessionAsync(db, userId, DateOnly.Today);

        var handler = new AddStrengthSetCommandHandler(db, NullPublisher.Instance, NullAudit.Instance);
        var result  = await handler.Handle(
            new AddStrengthSetCommand(userId, session.Id, "DEADLIFT", 1, 164, null, null),
            CancellationToken.None);

        Assert.False(result.IsPR);
    }
}
```

### 7.3 Validación — FluentValidation

```csharp
// Tests/Unit/AddStrengthSetValidatorTests.cs
public class AddStrengthSetValidatorTests
{
    private static AddStrengthSetCommandValidator CreateValidator() =>
        new(new FakeDbContextWithExercises(["SQUAT_BACK"]));

    [Theory]
    [InlineData(0,   100, false)]   // reps = 0 → inválido
    [InlineData(51,  100, false)]   // reps = 51 → inválido
    [InlineData(1,   100, true)]
    [InlineData(50,  100, true)]
    public async Task Validate_Reps(int reps, decimal weight, bool valid)
    {
        var v   = CreateValidator();
        var cmd = new AddStrengthSetCommand(Guid.NewGuid(), Guid.NewGuid(),
            "SQUAT_BACK", reps, weight, null, null);
        var res = await v.ValidateAsync(cmd);
        Assert.Equal(valid, res.IsValid);
    }

    [Theory]
    [InlineData(0,    false)]  // weight = 0 → inválido
    [InlineData(500,  true)]   // máximo permitido
    [InlineData(501,  false)]  // supera máximo
    public async Task Validate_Weight(decimal weight, bool valid)
    {
        var v   = CreateValidator();
        var cmd = new AddStrengthSetCommand(Guid.NewGuid(), Guid.NewGuid(),
            "SQUAT_BACK", 5, weight, null, null);
        var res = await v.ValidateAsync(cmd);
        Assert.Equal(valid, res.IsValid);
    }

    [Fact]
    public async Task Validate_InactiveExercise_Invalid()
    {
        var v   = new AddStrengthSetCommandValidator(new FakeDbContextWithExercises([]));
        var cmd = new AddStrengthSetCommand(Guid.NewGuid(), Guid.NewGuid(),
            "SQUAT_BACK", 5, 100, null, null);
        var res = await v.ValidateAsync(cmd);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "ExerciseId");
    }
}
```

### 7.4 Escenarios Gherkin

```gherkin
Feature: Registro de PR y progreso del atleta

  Background:
    Given un atleta autenticado con userId "user-123"
    And el atleta tiene preferencia de 1RM = Epley

  # ── Detección de PR ────────────────────────────────────────────────────

  Scenario: Primer set en un ejercicio siempre es PR
    When registra 1 rep a 100 kg en "Back Squat"
    Then el set tiene IsPR = true
    And Est1RM = 100 kg
    And existe un PrRecord con Metric="1RM", Value=100, IsLatest=true

  Scenario: Set con mayor 1RM supera el PR anterior
    Given existe un PR de 1RM = 143.33 kg en "Back Squat"
    When registra 3 reps a 140 kg en "Back Squat"   # Epley: 140*(1+3/30) = 154
    Then el set tiene IsPR = true
    And Est1RM = 154 kg
    And el PrRecord anterior tiene IsLatest = false
    And existe un nuevo PrRecord con Value = 154 y IsLatest = true

  Scenario: Set con menor 1RM no supera el PR
    Given existe un PR de 1RM = 170 kg en "Back Squat"
    When registra 1 rep a 165 kg en "Back Squat"
    Then el set tiene IsPR = false
    And Est1RM = 165 kg
    And el PrRecord de 170 kg sigue siendo IsLatest = true

  Scenario: Nuevo PR dispara notificación Web Push
    When registra 1 rep a 170 kg en "Back Squat" (nuevo PR)
    Then se envía una notificación Web Push con título "¡Nuevo PR! 🏆"
    And el mensaje incluye "Back Squat: 170.0 kg"

  # ── Idempotencia ───────────────────────────────────────────────────────

  Scenario: Envío duplicado con mismo clientRequestId es idempotente
    Given clientRequestId = "e0f1f7f6-2d7a-46d6-bf4c-7f1b1e121111"
    When registra 5 reps a 100 kg con ese clientRequestId
    And vuelve a enviar la misma request (retry)
    Then solo existe 1 StrengthSet con ese clientRequestId
    And la respuesta es idéntica en ambos envíos

  # ── Historial y gráficos ───────────────────────────────────────────────

  Scenario: Historial filtrado por rango de fechas
    Given el atleta tiene sets en enero y febrero de 2026
    When consulta GET /api/history?exerciseId=SQUAT_BACK&from=2026-02-01&to=2026-02-28
    Then la respuesta contiene solo sets de febrero
    And aggregates.best1RM refleja solo los valores de ese período

  Scenario: Volumen semanal en agregados
    Given el atleta registra: 5x130, 5x130, 5x130 en semana 2026-W06
    And registra: 3x140, 3x140 en semana 2026-W07
    When consulta el historial
    Then volumeByWeek["2026-W06"].volume = 1950   # 3 × (5 × 130)
    And  volumeByWeek["2026-W07"].volume = 840    # 2 × (3 × 140)

  # ── Borrado con auditoría ──────────────────────────────────────────────

  Scenario: Borrar un set registra auditoría
    Given el atleta tiene un set con id "set-001"
    When envía DELETE /api/strength-sets/set-001
    Then el set tiene IsDeleted = true
    And existe un registro en AuditLog con Entity="StrengthSet", Action="DELETE"
    And la respuesta es 204 No Content

  # ── Offline Sync ───────────────────────────────────────────────────────

  Scenario: Set registrado offline se sincroniza al recuperar conexión
    Given el dispositivo está offline
    When el atleta registra un set (5 reps × 100 kg)
    Then el set queda en la cola offline (localStorage)
    And el indicador de cola muestra "1 pendiente"
    When el dispositivo recupera conexión
    Then el set se envía automáticamente al servidor
    And la cola offline queda vacía
```

---

## 8. QA Checklist

### Rendimiento
- [ ] `GET /api/history` con 1 año de datos responde en < 200 ms (índice IX_WorkoutSession_UserId_Date)
- [ ] Consulta de PRs por usuario+ejercicio usa índice IX_PrRecord_UserId_ExerciseId_Metric
- [ ] Paginación implementada en historial (default page size = 30 sesiones)
- [ ] Gráfico no procesa más de 500 puntos en cliente (ventana de tiempo máxima)
- [ ] Sin N+1: historial carga sesiones + sets en 2 queries (o 1 con JOIN)

### Offline y PWA
- [ ] Cola offline persiste tras cerrar y reabrir el navegador (localStorage)
- [ ] Retry con backoff exponencial en `OfflineQueueService.flush()`
- [ ] `clientRequestId` generado en cliente evita duplicados en reconexión
- [ ] Banner visible cuando hay items en cola offline
- [ ] Service Worker registrado y cacheando `/api/exercises` (datos estáticos)

### Seguridad
- [ ] Todos los endpoints requieren JWT válido (`[Authorize]`)
- [ ] `UserId` siempre viene de claims JWT, nunca del body
- [ ] Validar que `sessionId` pertenece al `currentUserId` antes de agregar sets
- [ ] Rate limit: max 60 POST/min por JWT en `/api/sessions/*/strength-sets`
- [ ] Input validation completa via FluentValidation (reps, weight, RPE, fechas no futuras)
- [ ] No exponer `UserId` internos en respuestas públicas

### Accesibilidad (WCAG 2.1 AA)
- [ ] Contraste mínimo 4.5:1 para texto sobre fondo oscuro (naranja #FF7A1A sobre #121212)
- [ ] Gráfico ApexCharts con `aria-label` descriptivo
- [ ] Formulario de registro con `<label>` asociados a cada input
- [ ] Mensajes de error en rojo con `role="alert"` o `aria-live`
- [ ] Navegación completa por teclado (Tab, Enter, Escape en modales)
- [ ] Marcadores de PR en gráfico tienen tooltip accessible (no solo color verde)

### Funcional (regresión)
- [ ] Epley: `Calculate(100, 5)` = 116.67
- [ ] Brzycki: `Calculate(100, 5)` = 112.5
- [ ] Brzycki con reps = 1 retorna el peso exacto
- [ ] PR detectado al superar est1RM (no solo peso absoluto)
- [ ] PrRecord anterior marcado IsLatest=false cuando aparece nuevo PR
- [ ] Soft delete no elimina físicamente; IsDeleted=true excluye de queries
- [ ] AuditLog generado en CREATE, UPDATE y DELETE
- [ ] Idempotencia: segundo envío con mismo clientRequestId retorna el mismo id de set
- [ ] Historial vacío devuelve 200 con items=[] y aggregates por defecto
- [ ] Fecha futura (> hoy + 1 día) rechazada con 400

### Datos y consistencia
- [ ] PrRecord.IsLatest tiene índice parcial (solo IsLatest=1)
- [ ] Un único PrRecord con IsLatest=true por (UserId, ExerciseId, Metric)
- [ ] Migración inicial inserta ejercicios semilla (BigLifts + WODs básicos)
- [ ] UserPreference creada con defaults al primer login (Epley, kg)
