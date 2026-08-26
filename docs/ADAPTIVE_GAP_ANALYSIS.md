# Adaptive Gap Analysis

The immutable contract is `database/clone_ebay_sqlserver_schema.sql`. Existing migrations are historical reference only. They must not be created, applied, or required at runtime.

| Area | Existing | Problem | Decision | Priority |
| --- | --- | --- | --- | --- |
| Authentication | BCrypt, JWT, Admin role, MVC Session | Login queries absent User moderation columns | FIX mapping; KEEP 60-minute access token | P0 |
| Dashboard | Core counts and alerts | Moderation counters require absent columns | KEEP schema-safe counts; simplify unsupported counters | P0 |
| Users | Search/filter/sort/page and transitions | User has no status, reason, or approval columns | KEEP read-only monitoring; `DEFERRED_SCHEMA_CONSTRAINT` for approve/ban/unban | P0 |
| Products | Search/filter/sort/page and hide/restore | Product has no moderation status or reason | KEEP read-only monitoring; `DEFERRED_SCHEMA_CONSTRAINT` for hide/restore | P0 |
| Orders | Read-only list/detail | Mapping drift; detail can use existing payment/shipping fields | FIX and KEEP | P0 |
| Disputes | List/detail/assign/resolve/reject | Assignment fields absent; status and resolution exist | FIX; support schema-safe resolve/reject transitions; defer persisted assignment | P0 |
| Reviews | API moderation | Review has no moderation state | KEEP read-only; `DEFERRED_SCHEMA_CONSTRAINT` for hide/restore | P1 |
| Feedback | Read-only analytics | No major schema conflict | KEEP | P1 |
| Audit | Filtering/export and mutation logs | AuditLog table absent | `DEFERRED_SCHEMA_CONSTRAINT`; never create table | P0 |
| Reports | Date range, totals, breakdowns | User/Product/Dispute lack timestamps; audit absent | FIX; report only representable period metrics | P1 |
| Offline MVC | GET cache, banner, disabled writes | Login/mutations are cached; some gateway failures miss fallback | FIX | P1 |
| SignalR | MVC notifications for selected writes | No durable API event source; hub authorization weak | KEEP only safe existing behavior; do not expand | P2 |
| Error semantics | Controller exception mapping | Invalid transitions return 400 | FIX to 409 | P0 |
| Database startup | `MigrateAsync()` | Mutates immutable lecturer schema | REMOVE runtime migration | P0 |

## Reference Decisions

| Feature | Current project | Reference | Decision |
| --- | --- | --- | --- |
| User Ban | Implemented using extra columns | Strong transition validation | ADAPT validation only if schema later changes |
| Product moderation | Implemented using extra status | Reasons and history | DEFERRED_SCHEMA_CONSTRAINT |
| Dispute handling | Basic finite-state service | Rich case workflow | ADAPT strict transitions using `status` and `resolution` only |
| Order monitoring | Existing list/detail | Rich payment/shipping context | EXTEND from existing columns |
| Review moderation | API uses extra status | Flag queues and replies | KEEP read-only; SKIP extra entities |
| Dashboard | Sufficient core metrics | More charts | SKIP |
| Dynamic roles, 2FA, refunds | Not core | Comprehensive implementation | SKIP |
