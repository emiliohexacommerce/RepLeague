# Especificación Técnica — Módulo de Registro de WODs
**RepLeague · v1.0 · 2026-02**

---

## Índice
1. [Modelo de datos (DDL)](#1-modelo-de-datos-ddl)
2. [API REST + CQRS](#2-api-rest--cqrs)
3. [Commands, Handlers y Validadores (C#)](#3-commands-handlers-y-validadores-c)
4. [Contratos TypeScript + Servicio Angular](#4-contratos-typescript--servicio-angular)
5. [Componentes Angular](#5-componentes-angular)
6. [Gráficos (ApexCharts)](#6-gráficos-apexcharts)
7. [Eventos de dominio e integración](#7-eventos-de-dominio-e-integración)
8. [Pruebas (xUnit + Gherkin)](#8-pruebas-xunit--gherkin)
9. [QA Checklist](#9-qa-checklist)

---

## 1. Modelo de datos (DDL)

### 1.1 Esquema completo

```sql
-- ─────────────────────────────────────────────────
-- Registro principal del WOD
-- ─────────────────────────────────────────────────
CREATE TABLE WodEntry (
    Id              UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    UserId          UNIQUEIDENTIFIER  NOT NULL,
    Type            VARCHAR(16)       NOT NULL,          -- ForTime|AMRAP|EMOM|Chipper|Intervals
    Title           NVARCHAR(100)     NULL,              -- nombre benchmark (ej. "Fran")
    Date            DATE              NOT NULL,
    TimeCapSeconds  INT               NULL,              -- cap en AMRAP/EMOM/Intervals
    ElapsedSeconds  INT               NULL,              -- resultado ForTime/Chipper
    Rounds          INT               NULL,              -- rondas programadas
    RxScaled        BIT               NOT NULL DEFAULT 1, -- 1=Rx, 0=Scaled
    Notes           NVARCHAR(1000)    NULL,
    IsDeleted       BIT               NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2(3)      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)      NULL,
    DeletedAt       DATETIME2(3)      NULL,
    CONSTRAINT PK_WodEntry PRIMARY KEY (Id),
    CONSTRAINT FK_WodEntry_User FOREIGN KEY (UserId)
        REFERENCES [User](Id) ON DELETE CASCADE,
    CONSTRAINT CK_WodEntry_Type CHECK (
        Type IN ('ForTime','AMRAP','EMOM','Chipper','Intervals')),
    CONSTRAINT CK_WodEntry_TimeCapSeconds  CHECK (TimeCapSeconds  > 0),
    CONSTRAINT CK_WodEntry_ElapsedSeconds  CHECK (ElapsedSeconds  > 0),
    CONSTRAINT CK_WodEntry_Rounds          CHECK (Rounds          >= 1)
);
CREATE INDEX IX_WodEntry_UserId_Date
    ON WodEntry(UserId, Date DESC) WHERE IsDeleted = 0;
CREATE INDEX IX_WodEntry_Type_Date
    ON WodEntry(Type, Date DESC)   WHERE IsDeleted = 0;
-- Índice para búsquedas por benchmark (Title normalizado)
CREATE INDEX IX_WodEntry_UserId_Title
    ON WodEntry(UserId, Title)     WHERE IsDeleted = 0 AND Title IS NOT NULL;

-- ─────────────────────────────────────────────────
-- Detalle de ejercicios del WOD
-- ─────────────────────────────────────────────────
CREATE TABLE WodExercise (
    Id            UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    WodEntryId    UNIQUEIDENTIFIER  NOT NULL,
    OrderIndex    INT               NOT NULL,
    Name          NVARCHAR(80)      NOT NULL,
    MovementType  VARCHAR(24)       NOT NULL,  -- barbell|kb|bodyweight|gymnastic|cardio|other
    LoadValue     DECIMAL(6,2)      NULL,
    LoadUnit      VARCHAR(8)        NULL,      -- kg|lb|cal|m|reps
    Reps          INT               NULL,
    Notes         NVARCHAR(400)     NULL,
    CONSTRAINT PK_WodExercise PRIMARY KEY (Id),
    CONSTRAINT FK_WodExercise_Entry FOREIGN KEY (WodEntryId)
        REFERENCES WodEntry(Id) ON DELETE CASCADE,
    CONSTRAINT CK_WodExercise_LoadValue CHECK (LoadValue IS NULL OR LoadValue >= 0),
    CONSTRAINT CK_WodExercise_Reps      CHECK (Reps      IS NULL OR Reps >= 0),
    CONSTRAINT CK_WodExercise_OrderIndex CHECK (OrderIndex >= 0)
);
CREATE UNIQUE INDEX UQ_WodExercise_EntryOrder
    ON WodExercise(WodEntryId, OrderIndex);
CREATE INDEX IX_WodExercise_WodEntryId
    ON WodExercise(WodEntryId);

-- ─────────────────────────────────────────────────
-- Resultado AMRAP
-- ─────────────────────────────────────────────────
CREATE TABLE WodResultAmrap (
    WodEntryId      UNIQUEIDENTIFIER  NOT NULL,
    RoundsCompleted INT               NOT NULL DEFAULT 0,
    ExtraReps       INT               NOT NULL DEFAULT 0,
    CONSTRAINT PK_WodResultAmrap PRIMARY KEY (WodEntryId),
    CONSTRAINT FK_WodResultAmrap_Entry FOREIGN KEY (WodEntryId)
        REFERENCES WodEntry(Id) ON DELETE CASCADE,
    CONSTRAINT CK_WodResultAmrap_RoundsCompleted CHECK (RoundsCompleted >= 0),
    CONSTRAINT CK_WodResultAmrap_ExtraReps       CHECK (ExtraReps       >= 0)
);

-- ─────────────────────────────────────────────────
-- Resultado EMOM
-- ─────────────────────────────────────────────────
CREATE TABLE WodResultEmom (
    WodEntryId     UNIQUEIDENTIFIER  NOT NULL,
    TotalMinutes   INT               NOT NULL,
    IntervalsDone  INT               NOT NULL,
    CONSTRAINT PK_WodResultEmom PRIMARY KEY (WodEntryId),
    CONSTRAINT FK_WodResultEmom_Entry FOREIGN KEY (WodEntryId)
        REFERENCES WodEntry(Id) ON DELETE CASCADE,
    CONSTRAINT CK_WodResultEmom_TotalMinutes  CHECK (TotalMinutes  >= 1),
    CONSTRAINT CK_WodResultEmom_IntervalsDone CHECK (IntervalsDone >= 0)
);

-- ─────────────────────────────────────────────────
-- Detalle por intervalo (Intervals/EMOM opcional)
-- ─────────────────────────────────────────────────
CREATE TABLE WodIntervalDetail (
    Id            UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    WodEntryId    UNIQUEIDENTIFIER  NOT NULL,
    IndexNo       INT               NOT NULL,
    WorkSeconds   INT               NULL,
    RestSeconds   INT               NULL,
    Reps          INT               NULL,
    Notes         NVARCHAR(200)     NULL,
    CONSTRAINT PK_WodIntervalDetail PRIMARY KEY (Id),
    CONSTRAINT FK_WodIntervalDetail_Entry FOREIGN KEY (WodEntryId)
        REFERENCES WodEntry(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX UQ_WodIntervalDetail_EntryIndex
    ON WodIntervalDetail(WodEntryId, IndexNo);
CREATE INDEX IX_WodIntervalDetail_WodEntryId
    ON WodIntervalDetail(WodEntryId);
```

### 1.2 Reglas de coherencia por tipo (resumen)

| Tipo | TimeCapSeconds | ElapsedSeconds | Rounds | AmrapResult | EmomResult |
|------|:--------------:|:--------------:|:------:|:-----------:|:----------:|
| ForTime | — | **requerido** | opcional | — | — |
| AMRAP | **requerido** | — | — | **requerido** | — |
| EMOM | **requerido** | — | — | — | **requerido** |
| Chipper | opcional | **requerido** | — | — | — |
| Intervals | **requerido** | — | opcional | — | — |

---

## 2. API REST + CQRS

### Endpoints

| Método | Ruta | Command / Query |
|--------|------|-----------------|
| `POST` | `/api/wod` | `CreateWodEntryCommand` |
| `PATCH` | `/api/wod/{id}` | `UpdateWodEntryCommand` |
| `DELETE` | `/api/wod/{id}` | `DeleteWodEntryCommand` |
| `GET` | `/api/wod/{id}` | `GetWodByIdQuery` |
| `GET` | `/api/wod/history` | `GetWodHistoryQuery` |
| `GET` | `/api/wod/best` | `GetBestMarksQuery` |

---

## 3. Commands, Handlers y Validadores (C#)

### 3.1 Utilidad de parseo de tiempo

```csharp
// Application/Common/Utils/TimeParser.cs
public static class TimeParser
{
    /// <summary>
    /// Convierte "mm:ss" o "hh:mm:ss" a segundos totales.
    /// Retorna null si el formato es inválido.
    /// </summary>
    public static int? ParseToSeconds(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        if (TimeSpan.TryParseExact(input, @"m\:ss",   null, out var ts1)) return (int)ts1.TotalSeconds;
        if (TimeSpan.TryParseExact(input, @"mm\:ss",  null, out var ts2)) return (int)ts2.TotalSeconds;
        if (TimeSpan.TryParseExact(input, @"h\:mm\:ss",  null, out var ts3)) return (int)ts3.TotalSeconds;
        if (TimeSpan.TryParseExact(input, @"hh\:mm\:ss", null, out var ts4)) return (int)ts4.TotalSeconds;

        return null;
    }

    /// <summary>Formatea segundos a mm:ss (o hh:mm:ss si ≥ 3600).</summary>
    public static string FormatSeconds(int seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return seconds >= 3600
            ? ts.ToString(@"h\:mm\:ss")
            : ts.ToString(@"m\:ss");
    }
}
```

### 3.2 DTOs de entrada

```csharp
// Application/Features/Wod/DTOs/CreateWodEntryDto.cs
public record CreateWodEntryDto(
    string Type,
    string? Title,
    DateOnly Date,
    string? TimeCap,        // mm:ss
    string? Elapsed,        // mm:ss — ForTime/Chipper
    int? Rounds,
    bool RxScaled,
    AmrapResultDto? AmrapResult,
    EmomResultDto? EmomResult,
    List<WodExerciseDto> Exercises,
    List<IntervalDetailDto>? Intervals,
    string? Notes
);

public record AmrapResultDto(int RoundsCompleted, int ExtraReps);
public record EmomResultDto(int TotalMinutes, int IntervalsDone);

public record WodExerciseDto(
    int Order,
    string Name,
    string MovementType,
    decimal? LoadValue,
    string? LoadUnit,
    int? Reps,
    string? Notes
);

public record IntervalDetailDto(
    int Index,
    string? Work,    // mm:ss
    string? Rest,    // mm:ss
    int? Reps,
    string? Notes
);
```

### 3.3 CreateWodEntryCommand

```csharp
// Application/Features/Wod/Commands/Create/CreateWodEntryCommand.cs
public record CreateWodEntryCommand(
    Guid UserId,
    CreateWodEntryDto Dto,
    Guid? ClientRequestId
) : IRequest<WodEntryResponseDto>;
```

```csharp
// Application/Features/Wod/Commands/Create/CreateWodEntryCommandHandler.cs
public class CreateWodEntryCommandHandler(
    IAppDbContext db,
    IAuditService audit,
    IPublisher publisher)
    : IRequestHandler<CreateWodEntryCommand, WodEntryResponseDto>
{
    public async Task<WodEntryResponseDto> Handle(
        CreateWodEntryCommand cmd, CancellationToken ct)
    {
        var dto = cmd.Dto;

        // Idempotencia
        if (cmd.ClientRequestId.HasValue)
        {
            var dup = await db.WodEntries
                .FirstOrDefaultAsync(w => w.ClientRequestId == cmd.ClientRequestId
                                       && w.UserId == cmd.UserId
                                       && !w.IsDeleted, ct);
            if (dup is not null)
                return await BuildResponseAsync(dup, ct);
        }

        var entry = new WodEntry
        {
            Id              = Guid.NewGuid(),
            UserId          = cmd.UserId,
            Type            = dto.Type,
            Title           = dto.Title?.Trim(),
            Date            = dto.Date,
            TimeCapSeconds  = TimeParser.ParseToSeconds(dto.TimeCap),
            ElapsedSeconds  = TimeParser.ParseToSeconds(dto.Elapsed),
            Rounds          = dto.Rounds,
            RxScaled        = dto.RxScaled,
            Notes           = dto.Notes,
            CreatedAt       = DateTime.UtcNow,
            ClientRequestId = cmd.ClientRequestId
        };

        // Ejercicios
        entry.Exercises = dto.Exercises
            .OrderBy(e => e.Order)
            .Select(e => new WodExercise
            {
                Id           = Guid.NewGuid(),
                WodEntryId   = entry.Id,
                OrderIndex   = e.Order,
                Name         = e.Name.Trim(),
                MovementType = e.MovementType,
                LoadValue    = e.LoadValue,
                LoadUnit     = e.LoadUnit,
                Reps         = e.Reps,
                Notes        = e.Notes
            }).ToList();

        db.WodEntries.Add(entry);

        // Resultado AMRAP
        if (dto.Type == "AMRAP" && dto.AmrapResult is not null)
        {
            db.WodResultAmraps.Add(new WodResultAmrap
            {
                WodEntryId      = entry.Id,
                RoundsCompleted = dto.AmrapResult.RoundsCompleted,
                ExtraReps       = dto.AmrapResult.ExtraReps
            });
        }

        // Resultado EMOM
        if (dto.Type == "EMOM" && dto.EmomResult is not null)
        {
            db.WodResultEmoms.Add(new WodResultEmom
            {
                WodEntryId    = entry.Id,
                TotalMinutes  = dto.EmomResult.TotalMinutes,
                IntervalsDone = dto.EmomResult.IntervalsDone
            });
        }

        // Intervalos opcionales
        if (dto.Intervals?.Count > 0)
        {
            db.WodIntervalDetails.AddRange(dto.Intervals.Select(i => new WodIntervalDetail
            {
                Id          = Guid.NewGuid(),
                WodEntryId  = entry.Id,
                IndexNo     = i.Index,
                WorkSeconds = TimeParser.ParseToSeconds(i.Work),
                RestSeconds = TimeParser.ParseToSeconds(i.Rest),
                Reps        = i.Reps,
                Notes       = i.Notes
            }));
        }

        // Detectar mejor marca (ForTime / AMRAP) y publicar evento si aplica
        await CheckAndPublishPrAsync(entry, cmd.UserId, ct);

        await db.SaveChangesAsync(ct);

        await audit.LogAsync(cmd.UserId, "WodEntry", entry.Id.ToString(),
            "CREATE", JsonSerializer.Serialize(dto), ct);

        return await BuildResponseAsync(entry, ct);
    }

    // ── Mejor marca ───────────────────────────────────────────────────────────

    private async Task CheckAndPublishPrAsync(
        WodEntry entry, Guid userId, CancellationToken ct)
    {
        bool isBest = false;

        if (entry.Type == "ForTime" && entry.ElapsedSeconds.HasValue && entry.Title != null)
        {
            var previousBest = await db.WodEntries
                .Where(w => w.UserId == userId
                         && w.Type  == "ForTime"
                         && w.Title == entry.Title
                         && w.ElapsedSeconds.HasValue
                         && !w.IsDeleted)
                .MinAsync(w => (int?)w.ElapsedSeconds, ct);

            isBest = previousBest == null || entry.ElapsedSeconds < previousBest;
        }

        if (entry.Type == "AMRAP" && entry.Title != null)
        {
            // La mejor marca es la mayor combinación rondas*ejercicios + extraReps
            // Simplificado: más rondas completadas
            var prevBestRounds = await db.WodResultAmraps
                .Where(a => a.WodEntry.UserId  == userId
                         && a.WodEntry.Title   == entry.Title
                         && !a.WodEntry.IsDeleted)
                .MaxAsync(a => (int?)a.RoundsCompleted, ct);

            var newRounds = await db.WodResultAmraps
                .Where(a => a.WodEntryId == entry.Id)
                .Select(a => (int?)a.RoundsCompleted)
                .FirstOrDefaultAsync(ct);

            isBest = prevBestRounds == null || (newRounds ?? 0) > prevBestRounds;
        }

        if (isBest)
            await publisher.Publish(new WodBestMarkAchievedEvent(
                userId, entry.Id, entry.Type, entry.Title ?? entry.Type,
                entry.ElapsedSeconds), ct);
    }

    private async Task<WodEntryResponseDto> BuildResponseAsync(
        WodEntry entry, CancellationToken ct)
    {
        // Cargar relaciones si no están cargadas
        var exercises = await db.WodExercises
            .Where(e => e.WodEntryId == entry.Id)
            .OrderBy(e => e.OrderIndex)
            .ToListAsync(ct);

        return new WodEntryResponseDto(
            entry.Id, entry.Type, entry.Title, entry.Date.ToString("yyyy-MM-dd"),
            entry.TimeCapSeconds.HasValue
                ? TimeParser.FormatSeconds(entry.TimeCapSeconds.Value) : null,
            entry.ElapsedSeconds.HasValue
                ? TimeParser.FormatSeconds(entry.ElapsedSeconds.Value) : null,
            entry.ElapsedSeconds,
            entry.Rounds, entry.RxScaled, entry.Notes, entry.CreatedAt,
            exercises.Select(e => new WodExerciseResponseDto(
                e.OrderIndex, e.Name, e.MovementType,
                e.LoadValue, e.LoadUnit, e.Reps, e.Notes)).ToList()
        );
    }
}
```

### 3.4 FluentValidation

```csharp
// Application/Features/Wod/Commands/Create/CreateWodEntryCommandValidator.cs
public class CreateWodEntryCommandValidator : AbstractValidator<CreateWodEntryCommand>
{
    private static readonly string[] ValidTypes =
        ["ForTime", "AMRAP", "EMOM", "Chipper", "Intervals"];

    private static readonly string[] ValidMovementTypes =
        ["barbell","kb","bodyweight","gymnastic","cardio","other"];

    private static readonly string[] ValidLoadUnits =
        ["kg","lb","cal","m","reps"];

    public CreateWodEntryCommandValidator()
    {
        RuleFor(x => x.Dto.Type)
            .NotEmpty()
            .Must(t => ValidTypes.Contains(t))
            .WithMessage($"type debe ser uno de: {string.Join(", ", ValidTypes)}.");

        RuleFor(x => x.Dto.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("date no puede ser futura (máximo mañana).");

        // ── Validación cruzada por tipo ────────────────────────────────────────

        When(x => x.Dto.Type == "ForTime" || x.Dto.Type == "Chipper", () =>
        {
            RuleFor(x => x.Dto.Elapsed)
                .NotEmpty().WithMessage("elapsed es requerido para ForTime/Chipper.")
                .Must(BeValidTimeFormat!)
                .WithMessage("elapsed debe tener formato mm:ss o hh:mm:ss.");
        });

        When(x => x.Dto.Type == "AMRAP", () =>
        {
            RuleFor(x => x.Dto.TimeCap)
                .NotEmpty().WithMessage("timeCap es requerido para AMRAP.")
                .Must(BeValidTimeFormat!)
                .WithMessage("timeCap debe tener formato mm:ss o hh:mm:ss.");

            RuleFor(x => x.Dto.AmrapResult)
                .NotNull().WithMessage("result.roundsCompleted/extraReps son requeridos para AMRAP.");

            When(x => x.Dto.AmrapResult != null, () =>
            {
                RuleFor(x => x.Dto.AmrapResult!.RoundsCompleted)
                    .GreaterThanOrEqualTo(0).WithMessage("roundsCompleted debe ser ≥ 0.");
                RuleFor(x => x.Dto.AmrapResult!.ExtraReps)
                    .GreaterThanOrEqualTo(0).WithMessage("extraReps debe ser ≥ 0.");
            });
        });

        When(x => x.Dto.Type == "EMOM", () =>
        {
            RuleFor(x => x.Dto.TimeCap)
                .NotEmpty().WithMessage("timeCap (duración total) es requerido para EMOM.")
                .Must(BeValidTimeFormat!)
                .WithMessage("timeCap debe tener formato mm:ss o hh:mm:ss.");

            RuleFor(x => x.Dto.EmomResult)
                .NotNull().WithMessage("emomResult es requerido para EMOM.");

            When(x => x.Dto.EmomResult != null, () =>
            {
                RuleFor(x => x.Dto.EmomResult!.TotalMinutes)
                    .GreaterThanOrEqualTo(1).WithMessage("totalMinutes debe ser ≥ 1.");
                RuleFor(x => x.Dto.EmomResult!.IntervalsDone)
                    .GreaterThanOrEqualTo(0).WithMessage("intervalsDone debe ser ≥ 0.");
            });
        });

        When(x => x.Dto.Type == "Intervals", () =>
        {
            RuleFor(x => x.Dto.TimeCap)
                .NotEmpty().WithMessage("timeCap es requerido para Intervals.")
                .Must(BeValidTimeFormat!)
                .WithMessage("timeCap debe tener formato mm:ss o hh:mm:ss.");
        });

        // ── Ejercicios ─────────────────────────────────────────────────────────

        RuleFor(x => x.Dto.Exercises)
            .NotEmpty().WithMessage("El WOD debe tener al menos 1 ejercicio.");

        RuleForEach(x => x.Dto.Exercises).ChildRules(e =>
        {
            e.RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
            e.RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
            e.RuleFor(x => x.MovementType)
                .Must(m => ValidMovementTypes.Contains(m))
                .WithMessage($"movementType debe ser uno de: {string.Join(", ", ValidMovementTypes)}.");
            e.RuleFor(x => x.LoadValue).GreaterThanOrEqualTo(0)
                .When(x => x.LoadValue.HasValue).WithMessage("loadValue debe ser ≥ 0.");
            e.RuleFor(x => x.LoadUnit)
                .Must(u => u == null || ValidLoadUnits.Contains(u))
                .WithMessage($"loadUnit debe ser uno de: {string.Join(", ", ValidLoadUnits)}.");
            e.RuleFor(x => x.Reps).GreaterThanOrEqualTo(0)
                .When(x => x.Reps.HasValue).WithMessage("reps debe ser ≥ 0.");
        });

        // Verificar OrderIndex únicos
        RuleFor(x => x.Dto.Exercises)
            .Must(exs => exs.Select(e => e.Order).Distinct().Count() == exs.Count)
            .WithMessage("Los valores de order deben ser únicos dentro del WOD.");

        // ── Intervalos opcionales ──────────────────────────────────────────────

        When(x => x.Dto.Intervals?.Count > 0, () =>
        {
            RuleForEach(x => x.Dto.Intervals!).ChildRules(i =>
            {
                i.RuleFor(x => x.Work).Must(BeValidTimeFormatOrNull!)
                    .WithMessage("work debe tener formato mm:ss.");
                i.RuleFor(x => x.Rest).Must(BeValidTimeFormatOrNull!)
                    .WithMessage("rest debe tener formato mm:ss.");
            });
        });

        RuleFor(x => x.Dto.Notes).MaximumLength(1000).When(x => x.Dto.Notes != null);
    }

    private static bool BeValidTimeFormat(string? value)
        => TimeParser.ParseToSeconds(value).HasValue;

    private static bool BeValidTimeFormatOrNull(string? value)
        => value == null || TimeParser.ParseToSeconds(value).HasValue;
}
```

### 3.5 GetWodHistoryQueryHandler

```csharp
public record GetWodHistoryQuery(
    Guid UserId,
    string? Type,
    string? Benchmark,       // filtrar por Title
    DateOnly? From,
    DateOnly? To,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<WodEntryResponseDto>>;

public class GetWodHistoryQueryHandler(IAppDbContext db)
    : IRequestHandler<GetWodHistoryQuery, PagedResult<WodEntryResponseDto>>
{
    public async Task<PagedResult<WodEntryResponseDto>> Handle(
        GetWodHistoryQuery q, CancellationToken ct)
    {
        var query = db.WodEntries
            .Where(w => w.UserId == q.UserId && !w.IsDeleted);

        if (!string.IsNullOrEmpty(q.Type))
            query = query.Where(w => w.Type == q.Type);

        if (!string.IsNullOrEmpty(q.Benchmark))
            query = query.Where(w => w.Title == q.Benchmark);

        if (q.From.HasValue) query = query.Where(w => w.Date >= q.From.Value);
        if (q.To.HasValue)   query = query.Where(w => w.Date <= q.To.Value);

        var total = await query.CountAsync(ct);

        var entries = await query
            .OrderByDescending(w => w.Date)
            .ThenByDescending(w => w.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Include(w => w.Exercises.OrderBy(e => e.OrderIndex))
            .Include(w => w.AmrapResult)
            .Include(w => w.EmomResult)
            .ToListAsync(ct);

        var items = entries.Select(w => MapToDto(w)).ToList();

        return new PagedResult<WodEntryResponseDto>(items, total, q.Page, q.PageSize);
    }

    private static WodEntryResponseDto MapToDto(WodEntry w) => new(
        w.Id, w.Type, w.Title, w.Date.ToString("yyyy-MM-dd"),
        w.TimeCapSeconds.HasValue ? TimeParser.FormatSeconds(w.TimeCapSeconds.Value) : null,
        w.ElapsedSeconds.HasValue ? TimeParser.FormatSeconds(w.ElapsedSeconds.Value) : null,
        w.ElapsedSeconds,
        w.Rounds, w.RxScaled, w.Notes, w.CreatedAt,
        w.Exercises.Select(e => new WodExerciseResponseDto(
            e.OrderIndex, e.Name, e.MovementType,
            e.LoadValue, e.LoadUnit, e.Reps, e.Notes)).ToList(),
        w.AmrapResult is null ? null
            : new AmrapResultDto(w.AmrapResult.RoundsCompleted, w.AmrapResult.ExtraReps),
        w.EmomResult is null ? null
            : new EmomResultDto(w.EmomResult.TotalMinutes, w.EmomResult.IntervalsDone)
    );
}
```

### 3.6 GetBestMarksQueryHandler

```csharp
public record GetBestMarksQuery(
    Guid UserId,
    string? Type,
    string? Benchmark
) : IRequest<List<BestMarkDto>>;

public class GetBestMarksQueryHandler(IAppDbContext db)
    : IRequestHandler<GetBestMarksQuery, List<BestMarkDto>>
{
    public async Task<List<BestMarkDto>> Handle(
        GetBestMarksQuery q, CancellationToken ct)
    {
        var results = new List<BestMarkDto>();

        // ── ForTime/Chipper: menor ElapsedSeconds ────────────────────────────
        var forTimeQuery = db.WodEntries
            .Where(w => w.UserId == q.UserId && !w.IsDeleted
                     && (w.Type == "ForTime" || w.Type == "Chipper")
                     && w.ElapsedSeconds.HasValue);

        if (!string.IsNullOrEmpty(q.Type))      forTimeQuery = forTimeQuery.Where(w => w.Type == q.Type);
        if (!string.IsNullOrEmpty(q.Benchmark)) forTimeQuery = forTimeQuery.Where(w => w.Title == q.Benchmark);

        var forTimeBests = await forTimeQuery
            .GroupBy(w => w.Title ?? w.Type)
            .Select(g => new
            {
                Benchmark    = g.Key,
                BestSeconds  = g.Min(w => w.ElapsedSeconds!.Value),
                AchievedAt   = g.OrderBy(w => w.ElapsedSeconds).First().Date
            })
            .ToListAsync(ct);

        results.AddRange(forTimeBests.Select(b => new BestMarkDto(
            b.Benchmark, "ForTime", "BestTime",
            TimeParser.FormatSeconds(b.BestSeconds),
            b.BestSeconds, b.AchievedAt.ToString("yyyy-MM-dd"))));

        // ── AMRAP: mayor RoundsCompleted + ExtraReps ─────────────────────────
        var amrapQuery = db.WodResultAmraps
            .Where(a => a.WodEntry.UserId == q.UserId && !a.WodEntry.IsDeleted);

        if (!string.IsNullOrEmpty(q.Benchmark))
            amrapQuery = amrapQuery.Where(a => a.WodEntry.Title == q.Benchmark);

        var amrapBests = await amrapQuery
            .GroupBy(a => a.WodEntry.Title ?? "AMRAP")
            .Select(g => new
            {
                Benchmark   = g.Key,
                BestRounds  = g.Max(a => a.RoundsCompleted),
                BestExtra   = g.OrderByDescending(a => a.RoundsCompleted)
                               .ThenByDescending(a => a.ExtraReps)
                               .First().ExtraReps,
                AchievedAt  = g.OrderByDescending(a => a.RoundsCompleted)
                               .First().WodEntry.Date
            })
            .ToListAsync(ct);

        results.AddRange(amrapBests.Select(b => new BestMarkDto(
            b.Benchmark, "AMRAP", "BestRounds",
            $"{b.BestRounds} rds + {b.BestExtra} reps",
            b.BestRounds, b.AchievedAt.ToString("yyyy-MM-dd"))));

        return results;
    }
}
```

### 3.7 Controller

```csharp
// API/Controllers/WodController.cs
[Authorize]
[Route("api/wod")]
public class WodController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost]
    [ProducesResponseType(typeof(WodEntryResponseDto), 201)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWodRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateWodEntryCommand(CurrentUserId, req.ToDto(), req.ClientRequestId), ct);
        return StatusCode(201, result);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(WodEntryResponseDto), 200)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateWodRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateWodEntryCommand(CurrentUserId, id, req.ToDto()), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWodEntryCommand(CurrentUserId, id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WodEntryResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetWodByIdQuery(CurrentUserId, id), ct);
        return Ok(result);
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(PagedResult<WodEntryResponseDto>), 200)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? type,
        [FromQuery] string? benchmark,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetWodHistoryQuery(CurrentUserId, type, benchmark, from, to, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("best")]
    [ProducesResponseType(typeof(List<BestMarkDto>), 200)]
    public async Task<IActionResult> GetBest(
        [FromQuery] string? type,
        [FromQuery] string? benchmark,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetBestMarksQuery(CurrentUserId, type, benchmark), ct);
        return Ok(result);
    }
}

// Request models
public record CreateWodRequest(
    string Type, string? Title, DateOnly Date,
    string? TimeCap, string? Elapsed, int? Rounds,
    bool RxScaled, AmrapResultDto? AmrapResult,
    EmomResultDto? EmomResult,
    List<WodExerciseDto> Exercises,
    List<IntervalDetailDto>? Intervals,
    string? Notes, Guid? ClientRequestId)
{
    public CreateWodEntryDto ToDto() => new(
        Type, Title, Date, TimeCap, Elapsed, Rounds, RxScaled,
        AmrapResult, EmomResult, Exercises, Intervals, Notes);
}
```

---

## 4. Contratos TypeScript + Servicio Angular

### 4.1 Interfaces

```typescript
// models/wod.models.ts

export type WodType = 'ForTime' | 'AMRAP' | 'EMOM' | 'Chipper' | 'Intervals';
export type MovementType = 'barbell' | 'kb' | 'bodyweight' | 'gymnastic' | 'cardio' | 'other';
export type LoadUnit = 'kg' | 'lb' | 'cal' | 'm' | 'reps';

export interface WodExerciseDto {
  order: number;
  name: string;
  movementType: MovementType;
  loadValue?: number;
  loadUnit?: LoadUnit;
  reps?: number;
  notes?: string;
}

export interface AmrapResult {
  roundsCompleted: number;
  extraReps: number;
}

export interface EmomResult {
  totalMinutes: number;
  intervalsDone: number;
}

export interface IntervalDetail {
  index: number;
  work?: string;   // mm:ss
  rest?: string;   // mm:ss
  reps?: number;
  notes?: string;
}

export interface CreateWodRequest {
  type: WodType;
  title?: string;
  date: string;           // YYYY-MM-DD
  timeCap?: string;       // mm:ss
  elapsed?: string;       // mm:ss
  rounds?: number;
  rxScaled: boolean;
  amrapResult?: AmrapResult;
  emomResult?: EmomResult;
  exercises: WodExerciseDto[];
  intervals?: IntervalDetail[];
  notes?: string;
  clientRequestId?: string;
}

export interface WodEntryResponse {
  id: string;
  type: WodType;
  title?: string;
  date: string;
  timeCap?: string;
  elapsed?: string;
  elapsedSeconds?: number;
  rounds?: number;
  rxScaled: boolean;
  notes?: string;
  createdAt: string;
  exercises: WodExerciseDto[];
  amrapResult?: AmrapResult;
  emomResult?: EmomResult;
}

export interface BestMarkDto {
  benchmark: string;
  type: WodType;
  metric: 'BestTime' | 'BestRounds';
  displayValue: string;
  numericValue: number;
  achievedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface WodHistoryFilters {
  type?: WodType;
  benchmark?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}
```

### 4.2 WodService

```typescript
// services/wod.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import {
  CreateWodRequest, WodEntryResponse,
  BestMarkDto, PagedResult, WodHistoryFilters
} from '../models/wod.models';

@Injectable({ providedIn: 'root' })
export class WodService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/wod`;

  create(req: CreateWodRequest): Observable<WodEntryResponse> {
    return this.http.post<WodEntryResponse>(this.base, req);
  }

  update(id: string, req: Partial<CreateWodRequest>): Observable<WodEntryResponse> {
    return this.http.patch<WodEntryResponse>(`${this.base}/${id}`, req);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  getById(id: string): Observable<WodEntryResponse> {
    return this.http.get<WodEntryResponse>(`${this.base}/${id}`);
  }

  getHistory(filters: WodHistoryFilters = {}): Observable<PagedResult<WodEntryResponse>> {
    let params = new HttpParams();
    if (filters.type)      params = params.set('type', filters.type);
    if (filters.benchmark) params = params.set('benchmark', filters.benchmark);
    if (filters.from)      params = params.set('from', filters.from);
    if (filters.to)        params = params.set('to', filters.to);
    if (filters.page)      params = params.set('page', filters.page.toString());
    if (filters.pageSize)  params = params.set('pageSize', filters.pageSize.toString());
    return this.http.get<PagedResult<WodEntryResponse>>(`${this.base}/history`, { params });
  }

  getBestMarks(type?: string, benchmark?: string): Observable<BestMarkDto[]> {
    let params = new HttpParams();
    if (type)      params = params.set('type', type);
    if (benchmark) params = params.set('benchmark', benchmark);
    return this.http.get<BestMarkDto[]>(`${this.base}/best`, { params });
  }
}
```

### 4.3 Utilidad de tiempo en cliente

```typescript
// utils/time.utils.ts

/** Convierte "mm:ss" o "hh:mm:ss" a segundos totales. Retorna null si inválido. */
export function parseTimeToSeconds(value: string): number | null {
  const mmss    = /^(\d{1,2}):(\d{2})$/;
  const hhmmss  = /^(\d{1,2}):(\d{2}):(\d{2})$/;

  let m = mmss.exec(value);
  if (m) return parseInt(m[1], 10) * 60 + parseInt(m[2], 10);

  m = hhmmss.exec(value);
  if (m) return parseInt(m[1], 10) * 3600 + parseInt(m[2], 10) * 60 + parseInt(m[3], 10);

  return null;
}

/** Formatea segundos a "mm:ss" o "hh:mm:ss" si ≥ 3600. */
export function formatSeconds(total: number): string {
  const h = Math.floor(total / 3600);
  const m = Math.floor((total % 3600) / 60);
  const s = total % 60;
  if (h > 0)
    return `${h}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
}

/** Valida el formato mm:ss o hh:mm:ss. */
export function isValidTimeFormat(value: string): boolean {
  return parseTimeToSeconds(value) !== null;
}
```

---

## 5. Componentes Angular

### 5.1 WodFormComponent (standalone)

```typescript
// components/wod-form/wod-form.component.ts
import { Component, EventEmitter, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormArray, FormBuilder, FormGroup,
  ReactiveFormsModule, Validators
} from '@angular/forms';
import { v4 as uuidv4 } from 'uuid';
import { WodService } from '../../services/wod.service';
import { OfflineQueueService } from '../../services/offline-queue.service';
import { isValidTimeFormat, parseTimeToSeconds } from '../../utils/time.utils';
import { CreateWodRequest, WodType } from '../../models/wod.models';

@Component({
  selector: 'app-wod-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './wod-form.component.html',
})
export class WodFormComponent {
  @Output() saved = new EventEmitter<void>();

  fb   = inject(FormBuilder);
  wod  = inject(WodService);
  offline = inject(OfflineQueueService);

  saving = false;
  error  = '';
  successMsg = '';

  readonly wodTypes: WodType[] = ['ForTime', 'AMRAP', 'EMOM', 'Chipper', 'Intervals'];
  readonly movementTypes = ['barbell','kb','bodyweight','gymnastic','cardio','other'];

  form: FormGroup = this.fb.group({
    type:     ['ForTime', Validators.required],
    title:    [''],
    date:     [new Date().toISOString().split('T')[0], Validators.required],
    timeCap:  [''],
    elapsed:  [''],
    rounds:   [null],
    rxScaled: [true],
    notes:    [''],
    // Resultado AMRAP
    amrapRoundsCompleted: [0],
    amrapExtraReps:       [0],
    // Resultado EMOM
    emomTotalMinutes:  [null],
    emomIntervalsDone: [null],
    exercises: this.fb.array([]),
  });

  get exercises(): FormArray {
    return this.form.get('exercises') as FormArray;
  }

  get selectedType(): WodType {
    return this.form.get('type')?.value as WodType;
  }

  get requiresElapsed(): boolean {
    return ['ForTime', 'Chipper'].includes(this.selectedType);
  }

  get requiresTimeCap(): boolean {
    return ['AMRAP', 'EMOM', 'Intervals'].includes(this.selectedType);
  }

  get isAmrap():    boolean { return this.selectedType === 'AMRAP'; }
  get isEmom():     boolean { return this.selectedType === 'EMOM'; }
  get isIntervals():boolean { return this.selectedType === 'Intervals'; }

  addExercise(): void {
    this.exercises.push(this.fb.group({
      order:        [this.exercises.length, Validators.required],
      name:         ['', [Validators.required, Validators.maxLength(80)]],
      movementType: ['bodyweight', Validators.required],
      loadValue:    [null],
      loadUnit:     ['kg'],
      reps:         [null],
      notes:        [''],
    }));
  }

  removeExercise(i: number): void {
    this.exercises.removeAt(i);
    // Reindexar
    this.exercises.controls.forEach((c, idx) => c.get('order')?.setValue(idx));
  }

  validateTime(controlName: string): boolean {
    const val = this.form.get(controlName)?.value;
    return !val || isValidTimeFormat(val);
  }

  getTimePreview(controlName: string): string {
    const val = this.form.get(controlName)?.value;
    const secs = val ? parseTimeToSeconds(val) : null;
    return secs != null ? `${secs} segundos` : '';
  }

  submit(): void {
    if (this.form.invalid) return;
    if (this.requiresElapsed  && !this.validateTime('elapsed'))  return;
    if (this.requiresTimeCap  && !this.validateTime('timeCap'))  return;

    this.saving = true;
    this.error  = '';

    const v = this.form.value;
    const request: CreateWodRequest = {
      type:     v.type,
      title:    v.title || undefined,
      date:     v.date,
      elapsed:  v.elapsed  || undefined,
      timeCap:  v.timeCap  || undefined,
      rounds:   v.rounds   || undefined,
      rxScaled: v.rxScaled,
      notes:    v.notes    || undefined,
      clientRequestId: uuidv4(),
      exercises: v.exercises,
      amrapResult: this.isAmrap
        ? { roundsCompleted: v.amrapRoundsCompleted, extraReps: v.amrapExtraReps }
        : undefined,
      emomResult: this.isEmom
        ? { totalMinutes: v.emomTotalMinutes, intervalsDone: v.emomIntervalsDone }
        : undefined,
    };

    if (!navigator.onLine) {
      this.offline.enqueueWod(request);
      this.successMsg = 'WOD en cola — se sincronizará cuando haya conexión.';
      this.saving = false;
      this.saved.emit();
      return;
    }

    this.wod.create(request).subscribe({
      next: () => {
        this.successMsg = '¡WOD guardado exitosamente!';
        this.saving = false;
        this.saved.emit();
      },
      error: (err) => {
        this.error  = err?.error?.detail || 'No se pudo guardar el WOD.';
        this.saving = false;
      },
    });
  }
}
```

### 5.2 Template clave (wod-form.component.html — fragmento)

```html
<!-- Selector de tipo -->
<div class="mb-4">
  <label class="block text-sm font-semibold text-white mb-1">Tipo de WOD</label>
  <div class="flex flex-wrap gap-2">
    <button *ngFor="let t of wodTypes"
      type="button"
      [class]="selectedType === t
        ? 'px-4 py-2 rounded-lg bg-[#FF7A1A] text-white font-bold min-h-[44px]'
        : 'px-4 py-2 rounded-lg bg-[#2F2F2F] text-gray-300 font-semibold min-h-[44px] hover:bg-[#3a3a3a]'"
      (click)="form.get('type')?.setValue(t)">
      {{ t }}
    </button>
  </div>
</div>

<!-- Elapsed (ForTime/Chipper) -->
<div *ngIf="requiresElapsed" class="mb-4">
  <label class="block text-sm font-semibold text-white mb-1">
    Tiempo final <span class="text-gray-400 font-normal">(mm:ss)</span>
  </label>
  <input formControlName="elapsed" type="text" placeholder="05:08"
    class="w-full bg-[#2F2F2F] text-white rounded-lg px-4 py-3 min-h-[44px]
           border border-transparent focus:border-[#FF7A1A] outline-none"
    maxlength="8" />
  <p *ngIf="form.get('elapsed')?.value && !validateTime('elapsed')"
     class="text-red-400 text-xs mt-1">
    Formato inválido. Usa mm:ss (ej: 05:08) o hh:mm:ss.
  </p>
  <p *ngIf="validateTime('elapsed')" class="text-gray-400 text-xs mt-1">
    {{ getTimePreview('elapsed') }}
  </p>
</div>

<!-- Resultado AMRAP -->
<ng-container *ngIf="isAmrap">
  <div class="grid grid-cols-2 gap-4 mb-4">
    <div>
      <label class="block text-sm font-semibold text-white mb-1">Rondas completas</label>
      <input formControlName="amrapRoundsCompleted" type="number" min="0"
        class="w-full bg-[#2F2F2F] text-white rounded-lg px-4 py-3 min-h-[44px]
               border border-transparent focus:border-[#FF7A1A] outline-none" />
    </div>
    <div>
      <label class="block text-sm font-semibold text-white mb-1">Reps extra</label>
      <input formControlName="amrapExtraReps" type="number" min="0"
        class="w-full bg-[#2F2F2F] text-white rounded-lg px-4 py-3 min-h-[44px]
               border border-transparent focus:border-[#FF7A1A] outline-none" />
    </div>
  </div>
</ng-container>

<!-- Lista de ejercicios -->
<div class="mb-4">
  <div class="flex justify-between items-center mb-2">
    <h4 class="text-sm font-semibold text-white">Ejercicios</h4>
    <button type="button" (click)="addExercise()"
      class="px-3 py-1 bg-[#FF7A1A] text-white text-sm rounded-lg min-h-[44px]
             font-semibold hover:bg-orange-600 transition">
      + Agregar
    </button>
  </div>

  <div formArrayName="exercises" class="space-y-3">
    <div *ngFor="let ex of exercises.controls; let i = index"
      [formGroupName]="i"
      class="bg-[#2F2F2F] rounded-xl p-4 border border-[#3a3a3a]">

      <div class="grid grid-cols-12 gap-2">
        <div class="col-span-5">
          <input formControlName="name" type="text" placeholder="Nombre del ejercicio"
            class="w-full bg-[#1E1E1E] text-white rounded-lg px-3 py-2 min-h-[44px]
                   text-sm border border-transparent focus:border-[#FF7A1A] outline-none" />
        </div>
        <div class="col-span-3">
          <select formControlName="movementType"
            class="w-full bg-[#1E1E1E] text-white rounded-lg px-3 py-2 min-h-[44px]
                   text-sm border border-transparent focus:border-[#FF7A1A] outline-none">
            <option *ngFor="let mt of movementTypes" [value]="mt">{{ mt }}</option>
          </select>
        </div>
        <div class="col-span-2">
          <input formControlName="reps" type="number" placeholder="Reps" min="0"
            class="w-full bg-[#1E1E1E] text-white rounded-lg px-3 py-2 min-h-[44px]
                   text-sm border border-transparent focus:border-[#FF7A1A] outline-none" />
        </div>
        <div class="col-span-2 flex items-center">
          <button type="button" (click)="removeExercise(i)"
            class="text-red-400 hover:text-red-300 text-sm px-2 py-2 min-h-[44px]">
            ✕
          </button>
        </div>
      </div>

      <!-- Carga (opcional) -->
      <div class="grid grid-cols-2 gap-2 mt-2">
        <input formControlName="loadValue" type="number" placeholder="Carga" min="0"
          class="bg-[#1E1E1E] text-white rounded-lg px-3 py-2 text-sm min-h-[44px]
                 border border-transparent focus:border-[#FF7A1A] outline-none" />
        <select formControlName="loadUnit"
          class="bg-[#1E1E1E] text-white rounded-lg px-3 py-2 text-sm min-h-[44px]
                 border border-transparent focus:border-[#FF7A1A] outline-none">
          <option value="kg">kg</option>
          <option value="lb">lb</option>
          <option value="cal">cal</option>
          <option value="m">m</option>
        </select>
      </div>
    </div>
  </div>
</div>

<!-- Toggle RX / Scaled -->
<div class="flex items-center gap-3 mb-6">
  <button type="button"
    [class]="form.get('rxScaled')?.value
      ? 'px-4 py-2 rounded-lg bg-[#FF7A1A] text-white font-bold min-h-[44px]'
      : 'px-4 py-2 rounded-lg bg-[#2F2F2F] text-gray-400 min-h-[44px]'"
    (click)="form.get('rxScaled')?.setValue(true)">Rx</button>
  <button type="button"
    [class]="!form.get('rxScaled')?.value
      ? 'px-4 py-2 rounded-lg bg-[#FF7A1A] text-white font-bold min-h-[44px]'
      : 'px-4 py-2 rounded-lg bg-[#2F2F2F] text-gray-400 min-h-[44px]'"
    (click)="form.get('rxScaled')?.setValue(false)">Scaled</button>
</div>

<!-- Botón submit -->
<button type="submit" (click)="submit()"
  [disabled]="saving"
  class="w-full py-4 bg-[#FF7A1A] hover:bg-orange-600 disabled:opacity-50
         text-white font-bold rounded-xl min-h-[44px] transition text-base">
  {{ saving ? 'Guardando...' : 'Guardar WOD' }}
</button>

<!-- Mensajes -->
<p *ngIf="successMsg" class="mt-3 text-green-400 text-sm font-semibold">✓ {{ successMsg }}</p>
<p *ngIf="error"      class="mt-3 text-red-400  text-sm" role="alert">{{ error }}</p>
```

---

## 6. Gráficos (ApexCharts)

### 6.1 ForTime — Línea de mejor tiempo (eje Y invertido)

```typescript
// charts/wod-fortime-chart.config.ts
import { ApexOptions } from 'ng-apexcharts';
import { WodEntryResponse } from '../models/wod.models';
import { formatSeconds } from '../utils/time.utils';

export function buildForTimeChartOptions(entries: WodEntryResponse[]): ApexOptions {
  const sorted = [...entries]
    .filter(e => e.elapsedSeconds != null)
    .sort((a, b) => a.date.localeCompare(b.date));

  const dates    = sorted.map(e => e.date);
  const seconds  = sorted.map(e => e.elapsedSeconds!);

  // Detectar mejor marca en cada punto
  let best = Infinity;
  const isBest = seconds.map(s => {
    if (s < best) { best = s; return true; }
    return false;
  });

  return {
    series: [
      { name: 'Tiempo (seg)', data: seconds },
    ],
    chart: {
      type: 'line',
      height: 300,
      toolbar: { show: false },
      background: '#1E1E1E',
      foreColor: '#CCCCCC',
    },
    colors: ['#FF7A1A'],
    stroke: { curve: 'smooth', width: 3 },
    markers: {
      size: 5,
      colors: seconds.map((_, i) => isBest[i] ? '#22C55E' : '#FF7A1A'),
      strokeWidth: 0,
    },
    xaxis: {
      categories: dates,
      labels: { style: { fontSize: '11px' } },
    },
    yaxis: {
      reversed: true,    // Menos segundos = mejor = arriba
      title: { text: 'Tiempo' },
      labels: {
        formatter: (v: number) => formatSeconds(v),
      },
    },
    annotations: {
      points: sorted
        .filter((_, i) => isBest[i])
        .map((e, i) => ({
          x: e.date,
          y: e.elapsedSeconds!,
          marker: { size: 10, fillColor: '#22C55E', strokeWidth: 0 },
          label: {
            text: `🏆 ${formatSeconds(e.elapsedSeconds!)}`,
            style: { color: '#fff', background: '#22C55E', fontSize: '11px' },
          },
        })),
    },
    tooltip: {
      custom: ({ dataPointIndex }) => {
        const e = sorted[dataPointIndex];
        const label = isBest[dataPointIndex] ? '🏆 Mejor tiempo<br/>' : '';
        return `<div style="padding:8px;font-size:12px;background:#1E1E1E;color:#fff">
          <b>${e.date}</b><br/>
          ${label}
          Tiempo: <b>${formatSeconds(e.elapsedSeconds!)}</b>
          ${e.rxScaled ? ' · Rx' : ' · Scaled'}
        </div>`;
      },
    },
    grid: { borderColor: '#2F2F2F' },
  };
}
```

### 6.2 AMRAP — Barras de rondas + extraReps

```typescript
// charts/wod-amrap-chart.config.ts
import { ApexOptions } from 'ng-apexcharts';
import { WodEntryResponse } from '../models/wod.models';

export function buildAmrapChartOptions(entries: WodEntryResponse[]): ApexOptions {
  const sorted = [...entries]
    .filter(e => e.amrapResult != null)
    .sort((a, b) => a.date.localeCompare(b.date));

  const dates      = sorted.map(e => e.date);
  const rounds     = sorted.map(e => e.amrapResult!.roundsCompleted);
  const extraReps  = sorted.map(e => e.amrapResult!.extraReps);

  return {
    series: [
      { name: 'Rondas',     data: rounds },
      { name: 'Reps extra', data: extraReps },
    ],
    chart: {
      type: 'bar',
      height: 300,
      stacked: true,
      toolbar: { show: false },
      background: '#1E1E1E',
      foreColor: '#CCCCCC',
    },
    colors: ['#FF7A1A', '#F97316'],
    plotOptions: { bar: { borderRadius: 4, columnWidth: '55%' } },
    xaxis: {
      categories: dates,
      labels: { style: { fontSize: '11px' } },
    },
    yaxis: {
      title: { text: 'Rondas' },
      labels: { formatter: (v: number) => Math.floor(v).toString() },
    },
    tooltip: {
      custom: ({ dataPointIndex }) => {
        const e = sorted[dataPointIndex];
        return `<div style="padding:8px;font-size:12px;background:#1E1E1E;color:#fff">
          <b>${e.date}</b><br/>
          Rondas: <b>${e.amrapResult!.roundsCompleted}</b><br/>
          Reps extra: <b>${e.amrapResult!.extraReps}</b>
          ${e.rxScaled ? ' · Rx' : ' · Scaled'}
        </div>`;
      },
    },
    dataLabels: { enabled: false },
    legend: { position: 'top' },
    grid: { borderColor: '#2F2F2F' },
  };
}
```

### Mock data

```typescript
// mocks/wod.mock.ts
import { WodEntryResponse } from '../models/wod.models';

export const MOCK_FRAN_HISTORY: WodEntryResponse[] = [
  { id: '1', type: 'ForTime', title: 'Fran', date: '2026-01-10',
    elapsedSeconds: 390, elapsed: '6:30', rxScaled: false, createdAt: '',
    exercises: [
      { order: 1, name: 'Thruster', movementType: 'barbell', loadValue: 43, loadUnit: 'kg', reps: 21 },
      { order: 2, name: 'Pull-up',  movementType: 'gymnastic', reps: 21 },
    ]},
  { id: '2', type: 'ForTime', title: 'Fran', date: '2026-01-24',
    elapsedSeconds: 342, elapsed: '5:42', rxScaled: false, createdAt: '',
    exercises: [] },
  { id: '3', type: 'ForTime', title: 'Fran', date: '2026-02-07',
    elapsedSeconds: 308, elapsed: '5:08', rxScaled: true,  createdAt: '',
    exercises: [] },
  { id: '4', type: 'ForTime', title: 'Fran', date: '2026-02-21',
    elapsedSeconds: 295, elapsed: '4:55', rxScaled: true,  createdAt: '',
    exercises: [] },
];

export const MOCK_AMRAP_HISTORY: WodEntryResponse[] = [
  { id: '5', type: 'AMRAP', title: 'AMRAP 12', date: '2026-01-15',
    timeCap: '12:00', rxScaled: true, createdAt: '',
    amrapResult: { roundsCompleted: 5, extraReps: 8 }, exercises: [] },
  { id: '6', type: 'AMRAP', title: 'AMRAP 12', date: '2026-01-29',
    timeCap: '12:00', rxScaled: true, createdAt: '',
    amrapResult: { roundsCompleted: 6, extraReps: 3 }, exercises: [] },
  { id: '7', type: 'AMRAP', title: 'AMRAP 12', date: '2026-02-12',
    timeCap: '12:00', rxScaled: true, createdAt: '',
    amrapResult: { roundsCompleted: 7, extraReps: 12 }, exercises: [] },
];
```

---

## 7. Eventos de dominio e integración

```csharp
// Domain/Events/WodBestMarkAchievedEvent.cs
public record WodBestMarkAchievedEvent(
    Guid UserId,
    Guid WodEntryId,
    string WodType,
    string Benchmark,
    int? ElapsedSeconds
) : IDomainEvent;

// Application/Features/Wod/EventHandlers/WodBestMarkAchievedEventHandler.cs
public class WodBestMarkAchievedEventHandler(
    IWebPushService webPush,
    IOutboxService outbox,
    ILogger<WodBestMarkAchievedEventHandler> logger)
    : INotificationHandler<WodBestMarkAchievedEvent>
{
    public async Task Handle(WodBestMarkAchievedEvent ev, CancellationToken ct)
    {
        var body = ev.WodType == "ForTime" && ev.ElapsedSeconds.HasValue
            ? $"{ev.Benchmark}: {TimeParser.FormatSeconds(ev.ElapsedSeconds.Value)} 🏅"
            : $"{ev.Benchmark}: nueva mejor marca 🏅";

        _ = Task.Run(async () =>
        {
            try { await webPush.SendAsync(ev.UserId, "¡Mejor marca WOD! 🏆", body, ct); }
            catch (Exception ex) { logger.LogWarning(ex, "Web Push no enviado"); }
        });

        await outbox.EnqueueAsync(new OutboxMessage
        {
            Type    = "WodBestMarkAchieved",
            Payload = JsonSerializer.Serialize(ev)
        }, ct);
    }
}
```

---

## 8. Pruebas (xUnit + Gherkin)

### 8.1 TimeParser — tests unitarios

```csharp
// Tests/Unit/TimeParserTests.cs
public class TimeParserTests
{
    [Theory]
    [InlineData("00:59", 59)]
    [InlineData("05:08", 308)]
    [InlineData("59:59", 3599)]
    [InlineData("1:02:03", 3723)]
    [InlineData("01:02:03", 3723)]
    public void ParseToSeconds_ValidFormats_ReturnsCorrectSeconds(
        string input, int expected)
    {
        var result = TimeParser.ParseToSeconds(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("5:8")]      // segundos sin 2 dígitos
    [InlineData("60:00")]    // minutos=60 es válido pero raro; TimeSpan lo acepta
    [InlineData("abc")]
    [InlineData("1:2:3")]    // hh:mm:ss incompleto
    public void ParseToSeconds_InvalidFormats_ReturnsNull(string? input)
    {
        var result = TimeParser.ParseToSeconds(input);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(308,  "5:08")]
    [InlineData(59,   "0:59")]
    [InlineData(3600, "1:00:00")]
    [InlineData(3723, "1:02:03")]
    public void FormatSeconds_ReturnsExpectedString(int seconds, string expected)
    {
        Assert.Equal(expected, TimeParser.FormatSeconds(seconds));
    }
}
```

### 8.2 Validación por tipo — tests

```csharp
// Tests/Unit/CreateWodEntryValidatorTests.cs
public class CreateWodEntryValidatorTests
{
    private static CreateWodEntryCommandValidator V() => new();
    private static CreateWodEntryDto BaseDto(
        string type = "ForTime",
        string? elapsed = "05:08",
        string? timeCap = null)
        => new(type, null, DateOnly.Today, timeCap, elapsed, null, true,
               null, null,
               [new WodExerciseDto(0, "Pull-up", "gymnastic", null, null, 21, null)],
               null, null);

    [Fact]
    public async Task ForTime_WithElapsed_IsValid()
    {
        var result = await V().ValidateAsync(new(Guid.NewGuid(), BaseDto("ForTime","05:08"), null));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ForTime_WithoutElapsed_IsInvalid()
    {
        var result = await V().ValidateAsync(new(Guid.NewGuid(), BaseDto("ForTime", null), null));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Elapsed"));
    }

    [Fact]
    public async Task ForTime_InvalidTimeFormat_IsInvalid()
    {
        var result = await V().ValidateAsync(new(Guid.NewGuid(), BaseDto("ForTime","5:8"), null));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("mm:ss"));
    }

    [Fact]
    public async Task AMRAP_WithoutAmrapResult_IsInvalid()
    {
        var dto = BaseDto("AMRAP", null, "12:00") with { AmrapResult = null };
        var result = await V().ValidateAsync(new(Guid.NewGuid(), dto, null));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("AmrapResult"));
    }

    [Fact]
    public async Task AMRAP_WithValidResult_IsValid()
    {
        var dto = BaseDto("AMRAP", null, "12:00") with
        {
            AmrapResult = new AmrapResultDto(7, 12)
        };
        var result = await V().ValidateAsync(new(Guid.NewGuid(), dto, null));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Exercises_DuplicateOrder_IsInvalid()
    {
        var dto = new CreateWodEntryDto("ForTime", null, DateOnly.Today,
            null, "05:08", null, true, null, null,
            [
                new WodExerciseDto(0, "Thruster", "barbell", 43, "kg", 21, null),
                new WodExerciseDto(0, "Pull-up",  "gymnastic", null, null, 21, null), // mismo order
            ], null, null);
        var result = await V().ValidateAsync(new(Guid.NewGuid(), dto, null));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("únicos"));
    }

    [Fact]
    public async Task FutureDate_IsInvalid()
    {
        var dto = BaseDto("ForTime", "05:08") with { Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)) };
        var result = await V().ValidateAsync(new(Guid.NewGuid(), dto, null));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Date"));
    }
}
```

### 8.3 Escenarios Gherkin

```gherkin
Feature: Registro de WOD y progreso del atleta

  Background:
    Given un atleta autenticado con userId "user-123"

  # ── Parseo de tiempo ──────────────────────────────────────────────────

  Scenario Outline: Guardar tiempo en formato mm:ss
    Given el atleta registra un WOD tipo ForTime con elapsed "<input>"
    Then el backend persiste <seconds> segundos en ElapsedSeconds
    And la respuesta muestra elapsed = "<output>"
    Examples:
      | input   | seconds | output |
      | 05:08   | 308     | 5:08   |
      | 59:59   | 3599    | 59:59  |
      | 1:02:03 | 3723    | 1:02:03|

  Scenario: Formato de tiempo inválido retorna 400
    When registra un WOD ForTime con elapsed "5:8"
    Then la respuesta es 400 Bad Request
    And el mensaje de error menciona "mm:ss"

  # ── Coherencia por tipo ───────────────────────────────────────────────

  Scenario: ForTime sin elapsed retorna 400
    When registra un WOD tipo ForTime sin campo elapsed
    Then la respuesta es 400 Bad Request
    And el error menciona "elapsed es requerido"

  Scenario: AMRAP sin timeCap retorna 400
    When registra un WOD tipo AMRAP sin timeCap
    Then la respuesta es 400 Bad Request
    And el error menciona "timeCap es requerido"

  Scenario: AMRAP con resultado válido se guarda
    When registra un WOD tipo AMRAP con timeCap "12:00"
    And result.roundsCompleted = 7 y extraReps = 12
    Then la respuesta es 201 Created
    And WodResultAmrap tiene RoundsCompleted = 7 y ExtraReps = 12

  # ── Historial y filtros ───────────────────────────────────────────────

  Scenario: Historial ordenado por fecha DESC
    Given el atleta tiene WODs en enero y febrero de 2026
    When consulta GET /api/wod/history
    Then los WODs vienen ordenados de más reciente a más antiguo

  Scenario: Historial filtrado por tipo
    When consulta GET /api/wod/history?type=ForTime
    Then todos los items tienen type = "ForTime"

  Scenario: Historial paginado
    Given el atleta tiene 25 WODs registrados
    When consulta GET /api/wod/history?page=1&pageSize=20
    Then la respuesta contiene 20 items y total = 25
    When consulta page=2
    Then la respuesta contiene 5 items

  # ── Mejor marca ───────────────────────────────────────────────────────

  Scenario: Nuevo mejor tiempo en ForTime dispara notificación
    Given el atleta tiene un Fran previo con elapsed = 330 segundos
    When registra un nuevo Fran con elapsed "05:08" (308 segundos)
    Then la respuesta es 201 Created
    And se publica un WodBestMarkAchievedEvent
    And el evento contiene ElapsedSeconds = 308 y Benchmark = "Fran"
    And se envía Web Push con "¡Mejor marca WOD! 🏆"

  # ── Idempotencia ──────────────────────────────────────────────────────

  Scenario: Crear WOD con mismo clientRequestId es idempotente
    Given clientRequestId = "uuid-fijo-1234"
    When registra un WOD ForTime "05:08" con ese clientRequestId
    And vuelve a enviar la misma request
    Then solo existe 1 WodEntry con ese clientRequestId

  # ── Soft delete con auditoría ─────────────────────────────────────────

  Scenario: Eliminar WOD registra auditoría y no aparece en historial
    Given el atleta tiene un WOD con id "wod-001"
    When envía DELETE /api/wod/wod-001
    Then la respuesta es 204 No Content
    And WodEntry tiene IsDeleted = true
    And existe AuditLog con Entity="WodEntry", Action="DELETE"
    And el WOD no aparece en GET /api/wod/history

  # ── Offline ───────────────────────────────────────────────────────────

  Scenario: WOD registrado offline se sincroniza al reconectar
    Given el dispositivo está sin conexión
    When registra un WOD ForTime "05:08"
    Then el WOD queda en la cola offline
    And el indicador muestra "1 WOD pendiente de sincronización"
    When el dispositivo recupera conexión
    Then el WOD se envía automáticamente con el clientRequestId original
    And la cola queda vacía

  # ── Seguridad ────────────────────────────────────────────────────────

  Scenario: Usuario no puede editar WOD de otro usuario
    Given el WOD "wod-999" pertenece a "user-456"
    When user-123 envía PATCH /api/wod/wod-999
    Then la respuesta es 404 Not Found
```

---

## 9. QA Checklist

### Rendimiento
- [ ] `GET /api/wod/history` con 500 WODs responde en < 200 ms (índice IX_WodEntry_UserId_Date)
- [ ] `GET /api/wod/best` usa índice IX_WodEntry_UserId_Title con predicado IsDeleted=0
- [ ] Paginación implementada (pageSize máximo: 50)
- [ ] `Include(Exercises)` no genera N+1 (un único LEFT JOIN)
- [ ] No cargar WodIntervalDetail en el listado de historial (solo en detalle)

### Offline y PWA
- [ ] `clientRequestId` generado en cliente (UUID v4) antes de encolar
- [ ] Cola en localStorage sobrevive a cierre del navegador
- [ ] Reintentos con backoff (1s, 2s, 4s) máximo 3 intentos por item
- [ ] Respuesta 409 (duplicate clientRequestId) no re-encola el item
- [ ] Banner visible cuando `offlineQueue.length > 0`
- [ ] Service Worker cachea `/api/wod/best` (TTL 5 min) para mostrar datos offline

### Seguridad
- [ ] `UserId` siempre de JWT claims, nunca del body
- [ ] `GetWodByIdQuery` verifica `w.UserId == currentUserId` → 404 si no coincide
- [ ] `UpdateWodEntryCommand` verifica propiedad antes de modificar
- [ ] Rate limit: máximo 30 POST/min por JWT en `/api/wod`
- [ ] Soft delete no expone datos de otros usuarios en `history`

### Accesibilidad (WCAG 2.1 AA)
- [ ] Todos los `<input>` y `<button>` tienen min-height 44px
- [ ] Contraste naranja #FF7A1A sobre negro #121212: ratio 3.5:1 ✓ (pasa AA para UI components)
- [ ] `<label>` con `for` asociado a cada `<input>`
- [ ] Errores de validación con `role="alert"` o `aria-live="polite"`
- [ ] Drag-and-drop de ejercicios tiene alternativa de teclado (botones Subir/Bajar)
- [ ] Gráfico ApexCharts con `aria-label` descriptivo y tabla de datos alternativa
- [ ] Selector de tipo WOD navigable con teclado (Tab + Space)

### Funcional (regresión)
- [ ] `"05:08"` → 308 segundos → response muestra `"5:08"` (sin cero inicial en minutos < 10)
- [ ] `"59:59"` → 3599 segundos (borde límite mm:ss)
- [ ] `"1:02:03"` → 3723 segundos (formato hh:mm:ss)
- [ ] Formato inválido `"5:8"` → 400 con mensaje descriptivo
- [ ] ForTime sin elapsed → 400 "elapsed es requerido"
- [ ] AMRAP sin timeCap → 400 "timeCap es requerido"
- [ ] AMRAP sin amrapResult → 400 descriptivo
- [ ] OrderIndex duplicado en exercises[] → 400 "únicos"
- [ ] Fecha futura (> hoy+1) → 400
- [ ] DELETE propio → 204 + IsDeleted=true + AuditLog
- [ ] DELETE de otro usuario → 404
- [ ] Segundo POST con mismo clientRequestId → 201 con mismo id (no duplicado)
- [ ] Historial excluye IsDeleted=true
- [ ] Best marks excluye IsDeleted=true

### Datos y consistencia
- [ ] WodEntry.Type CHECK constraint cubre exactamente los 5 tipos
- [ ] WodResultAmrap.WodEntryId es PK (garantiza 1:1 con WodEntry)
- [ ] WodResultEmom.WodEntryId es PK (ídem)
- [ ] UQ_WodExercise_EntryOrder garantiza OrderIndex único por WodEntry
- [ ] ON DELETE CASCADE en WodExercise, WodResultAmrap, WodResultEmom, WodIntervalDetail
