# Admin Expansion Handoff

## Strategy Change

`database/clone_ebay_sqlserver_schema.sql` remains the untouched lecturer baseline, not the permanent schema ceiling. Future work may add explicit, traceable schema extensions for realistic Admin workflows. Never edit the baseline SQL file. Preserve all currently passing behavior and clean-database coverage.

## 1. Current Architecture

```text
MVC View
  -> MVC Controller
  -> AdminApiClient/CoreApiClient
  -> API Controller
  -> Service
  -> Repository
  -> AppDbContext / EF Core
  -> SQL Server
```

- ASP.NET Core 8 API + MVC.
- BCrypt password verification; JWT Bearer issuer/audience/key/lifetime validation.
- Admin role authorization on Admin APIs; MVC stores JWT in Session.
- SignalR Admin toast infrastructure in MVC.
- In-memory GET cache, Offline Mode banner, disabled offline writes, recovery worker.
- Date-filtered reports and paid-payment revenue.
- Explicit SQL demo seed/reset scripts; no startup seed or runtime migration.
- Playwright browser/API QA used as the current regression oracle.

## 2. Current Working Features

| Area | Working behavior |
| --- | --- |
| Authentication | Admin/Buyer/Seller login, BCrypt, JWT, Session routing, logout; missing token `401`, wrong role `403`. |
| Dashboard | Total Users, Products, Orders, paid revenue, Open Dispute count/action alert. |
| Users | Read-only search, role filter, sort, pagination, API detail. |
| Products | Read-only search, Seller filter, sort, pagination, API detail. |
| Orders | Monitoring list/filter/sort/page; detail with buyer, items, Payment method/status/amount, Shipping carrier/tracking/status. |
| Disputes | List/filter/detail; `Open -> Resolved` or `Open -> Rejected`; resolution 10-2000 chars; terminal conflicts `409`; persistence/reload and SignalR toast verified. |
| Return Requests | List by status/user/order/date; contextual detail; `Pending -> Approved` or `Pending -> Rejected`; terminal conflicts `409`; persistence/reload and SignalR toast verified. |
| Reviews | Read-only MVC list/detail; positive/mixed/negative demo records. |
| Feedback | Read-only Seller aggregate filters/detail. |
| Reports | Date-filtered Orders and Paid revenue; Order/Dispute breakdowns; reversed dates `400`. |
| Offline Mode | Cached authenticated GETs, banner, mutation disabling, recovery infrastructure. Login and mutation responses are not cached. |
| SignalR | MVC broadcasts successful Dispute and Return terminal actions. |

Navigation intentionally exposes only Dashboard, Users, Products, Orders, Disputes, Return Requests, Reviews, Feedback, Reports.

## 3. Current Database Baseline

The lecturer SQL creates 17 tables: User, Address, Category, Product, OrderTable, OrderItem, Payment, ShippingInfo, ReturnRequest, Bid, Review, Message, Coupon, Inventory, Feedback, Dispute, Store. Existing PK/FK relationships connect marketplace users, products, orders, payments, shipping, returns, reviews, disputes, and stores.

Baseline gaps for richer Admin operations:

- User has no persisted moderation/approval status, reason, actor, or timestamps.
- Product has no moderation visibility/status, violation reason, or moderator metadata.
- Review has no moderation status/report/response metadata.
- No persistent AuditLog table.
- Dispute has status/resolution, but no assignee, priority, assignment/resolution actor/timestamps, evidence, or timeline.
- No dedicated ProductReport/ReviewReport/SellerEnforcement tables.
- User/Product/Dispute lack creation timestamps needed for accurate period growth reports.

## 4. Previous Deferred Features

These were formerly `DEFERRED_SCHEMA_CONSTRAINT`. Under the new strategy they are implementation candidates:

- User approve/reject/ban/unban and transition history.
- Product hide/restore and violation reasons.
- Review hide/restore and report moderation.
- Persistent Audit Log, filtering, metadata, Excel export.
- Dispute assignment and assignment/resolution metadata.
- Accurate Active/Banned User and Hidden Product dashboard counters.

Do not restore old fake/in-memory behavior. Add explicit persistence and migrate data safely.

## 5. Reference Project Findings

References are read-only. Adapt business rules and UI concepts; do not copy architecture/schema wholesale.

| Feature | Purpose and useful reference files | Adapt | Do not copy blindly |
| --- | --- | --- | --- |
| User moderation | `reference/ebay_clone_adminRole/src/Domain/Entities/User.cs`; `src/Web/Endpoints/Users.cs`; `src/Application/Users/Commands/{ApproveUser,BanUser,UnbanUser,RejectUser,UpdateUserStatus}/` | Persist status, approval state, reason, actor/time; enforce legal transitions; generic API errors. | CQRS/MediatR stack, 2FA/KYC fields, complex permission model unless required. |
| Product moderation | `.../Domain/Entities/{Product,ProductReport}.cs`; `.../Application/Products/Commands/{CreateProductReport,ResolveProductViolation}/`; `.../Queries/GetManagedProducts/` | Moderation queue, report reason, hide/restore decision, before/after audit metadata. | Seller product lifecycle, automated AI moderation, extra catalog model unrelated to Admin scope. |
| Disputes | `.../Domain/Entities/{Dispute,DisputeMessage}.cs`; `.../Application/Admin/Disputes/Commands/{AssignDispute,ResolveDispute}/`; `.../Queries/{GetDisputeDocket,GetDisputeDetail,GetDisputeStatistics}/`; `.../Web/Hubs/DisputeHub.cs` | Assignment, priority, clear finite transitions, contextual docket/timeline, notifications. | Refund/payment mutation until financial semantics are explicitly designed; evidence/chat complexity before core assignment is stable. |
| Review moderation | `.../Domain/Entities/{Review,ReviewReport}.cs`; `.../Application/Reviews/{GetFlaggedReviewsQuery,ReviewModerationDto}.cs`; commands `ReportReview`, `UpdateReviewStatus`, `ReplyToReview` | Flagged queue, moderation status/reason, hide/restore, audit. | Seller replies/report entities if lecturer demo does not need them. |
| Audit | `.../Web/Endpoints/AuditLogs.cs`; `.../Application/AuditLogs/`; `reference/EbayCloneAll/.../Model/{AuditLog,AuditLogSensitiveData}.cs`; `.../Services/Imp/{AuditLogService,AuditLogBuilder}.cs` | Actor/action/resource/time, sanitized JSON metadata, filters/export, transactionally consistent mutation records. | Queue/worker/sensitive-data encryption infrastructure for a course-sized synchronous audit table. |
| Dashboard/action queues | `.../GetDisputeStatistics/`, `GetManagedProducts/`, `GetFlaggedReviewsQuery.cs`; `reference/EbayCloneAll/.../SellerRiskScannerHostedService.cs` | Counts linking directly to pending moderation queues. | Large chart suite, scanners, ML/risk automation before core queues work. |
| Seller operations | `.../Application/Sellers/Queries/GetSellerPerformanceMetrics/`; `.../Domain/Entities/{SellerWallet,SellerLevelCriteria}.cs`; `reference/EbayCloneAll/.../SellerTrustSafetyService.cs` | Read-only Store/Product/Feedback operational overview; warning/penalty only after persistence design. | Wallet/payout, ranking engine, scheduled seller evaluation, KYC. |
| Alternative implementation patterns | `reference/EbayCloneAll/.../Controllers/Dispute/AdminDisputeController.cs`; `.../Services/Imp/DisputeService.cs`; `.../Services/ProductModerationService.cs`; `.../Controllers/Audit/` | API naming, validation ideas, operational screens. | Its broad schema, queue/background services, React/mock UI, or inconsistent patterns. Keep this repo's Controller-Service-Repository flow. |

## 6. Current Demo Data

Scripts:

- `database/seed_demo_data.sql`: transactional, idempotent, natural-key lookup, BCrypt hashes.
- `database/reset_demo_data.sql`: deletes only known demo data in FK-safe order.

Local demo password: `Demo@123`.

| Role | Email |
| --- | --- |
| Admin | `admin.demo@gmail.com` |
| Buyer A/B | `buyer1.demo@gmail.com`, `buyer2.demo@gmail.com` |
| Seller A/B | `seller1.demo@gmail.com`, `seller2.demo@gmail.com` |

Stable markers:

- Open Dispute: `DEMO DISPUTE OPEN:`
- Pending Return: `DEMO RETURN PENDING:`
- Delivered-order tracking: `DEMO-DELIVERED-001`

Expand seed data by stable names/emails/markers, `IF NOT EXISTS`, ID lookup after insert, transaction protection. Extend reset in reverse FK order. Never assume identity IDs.

## 7. Current QA Regression Baseline

- `dotnet build EbayClone.sln --no-restore`: PASS, 0 warnings, 0 errors.
- Clean isolated DB built from lecturer schema + seed only: PASS; 17 tables; no `__EFMigrationsHistory`; no invalid table/column SQL errors.
- Playwright PASS: invalid/valid login, Dashboard, User Seller filter, Product search, Order items/PayPal/tracking, Dispute terminal transition + reload + controls disappear, Return terminal transition + reload + context + controls disappear, Reviews, Feedback, Reports.
- Negative PASS: no token `401`; Buyer token `403`; missing Order `404`; short Dispute resolution `400`; reversed report dates `400`; second Dispute/Return terminal action `409`; logout clears Session.

Regression oracles: `docs/ADMIN_QA_TEST_PLAN.md`, `FINAL_REVIEW_CHECKLIST.md`.

## 8. Important Files

| Area | Files |
| --- | --- |
| Startup/mapping | `EbayClone.API/Program.cs`, `EbayClone.API/Data/AppDbContext.cs`, `EbayClone.API/EbayClone.API.csproj` |
| Authentication | `Controllers/AuthController.cs`, `Services/AuthService.cs`, `Repositories/UserRepository.cs`, `Helpers/JwtHelper.cs`; MVC `Controllers/AccountController.cs`, `Filters/AdminSessionAttribute.cs` |
| Users | `Controllers/AdminUserController.cs`, `Services/AdminUserService.cs`, `Repositories/UserRepository.cs`; MVC `Controllers/UsersController.cs`, `Views/Users/Index.cshtml` |
| Products | `Controllers/AdminProductController.cs`, `Services/AdminProductService.cs`, `Repositories/ProductRepository.cs`; MVC `Controllers/ProductsController.cs`, `Views/Products/Index.cshtml` |
| Orders | `Controllers/AdminOrderController.cs`, `Services/AdminOrderService.cs`, `Repositories/OrderRepository.cs`; MVC `Controllers/OrdersController.cs`, `Views/Orders/` |
| Disputes | `Controllers/AdminDisputeController.cs`, `Services/AdminDisputeService.cs`, `Repositories/DisputeRepository.cs`; MVC `Controllers/DisputesController.cs`, `Views/Disputes/` |
| Returns | `Controllers/AdminReturnRequestController.cs`, `Services/AdminReturnRequestService.cs`, `Repositories/ReturnRequestRepository.cs`; MVC `Controllers/ReturnRequestsController.cs`, `Views/ReturnRequests/` |
| Reviews/Feedback | API AdminReview/AdminFeedback controller-service-repository files; MVC `Controllers/{Reviews,Feedbacks}Controller.cs`, matching Views |
| Reports/Dashboard | `Services/AdminReportService.cs`, `Repositories/DashboardRepository.cs`, corresponding controllers/DTOs; MVC `Controllers/{Reports,Dashboard}Controller.cs` |
| SignalR | `EbayClone.MVC/Hubs/AdminNotificationHub.cs`, `Services/AdminNotificationService.cs`, `wwwroot/js/site.js` |
| Cache/offline | `Services/{CoreApiClient,ApiCacheService,CacheRefreshApiClient,CacheRefreshBackgroundService}.cs`, `Views/Dashboard/Offline.cshtml` |
| Demo DB | `database/clone_ebay_sqlserver_schema.sql`, `database/seed_demo_data.sql`, `database/reset_demo_data.sql` |

## 9. Fragile Areas / Warnings

- Never edit `database/clone_ebay_sqlserver_schema.sql`; add separate extension SQL and/or new explicit migrations.
- `EbayClone.API/Data/Migrations/` contains historical extension attempts. They are excluded from compilation via the API `.csproj`, were not used by the clean verification, and may conflict with the current trimmed model. Audit/reconcile before re-enabling anything; do not apply them wholesale.
- API/MVC JSON positional record contracts must change together. Run browser regression after DTO changes.
- `ResolveDisputeRequestDto` is intentionally a class: record validation metadata previously caused runtime `500`.
- No startup DB mutation currently exists. Keep seed execution explicit.
- SignalR broadcasts originate from MVC mutation controllers, not direct API calls.
- Cache keys are path-based; mutation responses/login are intentionally uncached. Invalidate related GET keys if expansion causes stale reads.
- Playwright locates seeded records by text markers, not IDs. Preserve or deliberately update markers/checklist.
- Worktree is not clean. Do not reset/revert current changes; they are the tested baseline.
- Tracked common config currently contains local development/demo values. Preserve local operability, but move real deployment secrets to environment variables and never commit real credentials.

## 10. Recommended Expansion Candidates

### P0

1. Schema extension foundation: choose one traceable extension mechanism, reconcile historical migrations, add clean upgrade/test procedure while retaining baseline SQL.
2. Persistent AuditLog: synchronous audit in the same transaction as Admin state changes; filters and Excel export.
3. User moderation: Pending/Active/Banned, approval state, required ban/reject reason, actor/time, legal transitions, login denial for Banned accounts.
4. Product moderation: Active/Hidden, required reason, restore, report/moderation queue if kept minimal.

### P1

1. Dispute assignment: `Open -> Assigned -> InReview/Resolved`, assignee, priority, actor/timestamps; retain existing terminal validation.
2. Review moderation: Visible/Hidden, reason, flagged queue, audit.
3. Dashboard action queues and real Active/Banned/Hidden counters linked to list filters.
4. Extend deterministic seed/reset and Playwright for every new transition and negative duplicate action.

### P2

1. Seller/Store operations dashboard using Store, Products, Feedback, moderation history.
2. Admin role granularity only if lecturer review benefits clearly.
3. Dispute evidence/timeline or product/review reports after P0/P1 are stable.

Skip for now: wallets/payouts, KYC/2FA, AI moderation, ML risk scoring, microservices, CQRS/MediatR rewrite, complex background queues.

## 11. Current Git State

```text
Branch: main
HEAD: 44833de58ef19bafb83e4af4060f83df5da45758
Working tree: DIRTY
Status at handoff: 79 modified/deleted files, 17 untracked files
```

Current uncommitted work is the clean-DB-tested stable checkpoint. First next-session action: inspect `git status`/`git diff`, rerun build, then create a deliberate checkpoint commit only if requested. Never discard this work.

## Suggested Skills

- `codebase-design`: define a minimal deep schema-extension/audit seam.
- `domain-modeling`: lock moderation states and legal transitions before schema work.
- `tdd`: add transition and integration tests for each expansion.
- `playwright`: preserve deterministic lecturer workflows.
- `code-review`: review the large current diff before checkpointing.
