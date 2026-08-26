# Adaptive Implementation Plan

## Contract

- `database/clone_ebay_sqlserver_schema.sql` is immutable and authoritative.
- Do not create/apply migrations or call `Database.Migrate`.
- Keep MVC -> API -> Service -> Repository -> DbContext.
- Unsupported persistence is marked `DEFERRED_SCHEMA_CONSTRAINT`, never simulated as success.

## P0 - Must Work

1. Build both projects without requiring schema changes.
2. Map active EF entities exactly to lecturer tables, columns, lengths, precision, and SQL date types.
3. Keep BCrypt login, JWT validation, Admin role authorization, MVC Session, and logout.
4. Keep schema-safe Dashboard counts.
5. Keep User and Product list/detail monitoring. Disable unsupported mutations clearly.
6. Keep Order filtering and detail with payment/shipping information.
7. Keep Dispute filtering/detail plus schema-safe resolve/reject transitions. Return `409 Conflict` for invalid transitions.
8. Keep Feedback and Review read-only monitoring.
9. Remove Audit runtime dependency because no AuditLog table exists.
10. Keep navigation operational; never show false mutation success.

## P1 - High Value

1. Validate date ranges and pagination at API boundaries.
2. Make report semantics explicit: dated Order/Payment metrics only; all-time counts where timestamps do not exist.
3. Cache GET responses only; support offline fallback for `502`, `503`, and `504`; invalidate affected cache after writes.
4. Keep SignalR behavior only where authorization and actual mutation behavior remain valid.
5. Add low-risk ProblemDetails/error consistency.
6. Improve schema-safe Order and Dispute detail presentation.

## P2 - Optional

1. Additional realtime notifications.
2. Extra charts and UI polish.
3. Rate limiting using native ASP.NET Core middleware.

## Deferred Schema Constraints

- User approve/ban/unban: no User status/reason/approval fields.
- Product hide/restore: no Product moderation field.
- Review hide/restore: no Review moderation field.
- Audit logs/export: no AuditLog table.
- Persisted Dispute assignment: no assignee/assignment timestamp fields.
- Period User/Product/Dispute creation metrics: no creation timestamps.

These features require a lecturer-approved schema revision. No migration will be generated locally.

## Execution Gates

1. Schema gate: no runtime DDL; mapping matches SQL file.
2. Build gate: `dotnet build EbayClone.sln` passes.
3. API gate: authentication and protected reads return correct HTTP statuses.
4. Business gate: valid Dispute transitions persist; invalid transitions return `409` without mutation.
5. MVC gate: online and offline states are truthful; unsupported writes are unavailable.
6. QA gate: planned business cases pass or carry an explicit environment blocker.
