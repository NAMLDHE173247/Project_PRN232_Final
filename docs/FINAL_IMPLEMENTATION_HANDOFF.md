# Final Admin Implementation Handoff

## Architecture

```text
MVC View
-> MVC Controller
-> AdminApiClient / CoreApiClient
-> API Controller
-> Service
-> Repository
-> AppDbContext / EF Core
-> SQL Server
```

- ASP.NET Core 8 API + MVC.
- Admin MVC stores JWT in Session. API validates JWT issuer, audience, signing key, lifetime, and Admin role.
- SignalR notifications originate from successful MVC Dispute/Return mutations.
- Authorized GET responses are cached for offline fallback; writes are never queued or cached.

## Database

The lecturer schema is immutable. Reconstruct in this order only:

```text
database/clone_ebay_sqlserver_schema.sql
-> database/admin_extensions.sql
-> database/seed_demo_data.sql
```

`admin_extensions.sql` is project-owned, transactional, and reasonably idempotent. It adds:

- User/Product/Review moderation state and actor/reason/time metadata.
- Dispute assignment/review/resolution metadata.
- `AdminAuditLog`, FKs, checks, and queue indexes.

No runtime DDL, startup migration, startup seed, or `__EFMigrationsHistory` is used. Historical `Data/Migrations` and `ApplicationDbContext.cs` are excluded from compilation.

## Business Workflows

### Users

```text
Pending -> Active -> Banned -> Active
```

- Only Pending can be approved.
- Only Active can be banned; reason length is 3-500.
- Only Banned can be unbanned.
- Admin accounts cannot be moderated through User operations.
- Pending/Banned accounts cannot log in.
- Duplicate/invalid transitions return `409`; invalid reason returns `400`.

### Products

```text
Active -> Hidden -> Active
```

- Hide requires reason length 3-500.
- Duplicate/invalid transitions return `409`.
- Status, reason, actor, timestamp, and Audit persist.

### Reviews

```text
Visible -> Hidden -> Visible
```

- Hide requires reason length 3-500.
- Duplicate/invalid transitions return `409`.
- Status, reason, actor, timestamp, and Audit persist.

### Disputes

```text
Open -> Assigned -> InReview -> Resolved / Rejected
```

- Assignee must be an Active Admin.
- Only the assignee can start review or finalize an assigned case.
- Direct `Open -> Resolved/Rejected` remains supported for backward compatibility.
- Terminal cases reject further mutation with `409`.
- Assignment, review, resolver, timestamps, resolution, and Audit persist.

### Returns

```text
Pending -> Approved / Rejected
```

- Terminal second action returns `409`.
- Order, Payment, and Shipping are contextual only; return decisions do not mutate them.
- Successful decisions create Audit rows and MVC SignalR notifications.

### Audit

- Actions: User approve/ban/unban; Product hide/restore; Review hide/restore; Dispute assign/start-review/resolve/reject; Return approve/reject.
- Fields: Admin actor, action, resource type/ID, reason, UTC timestamp.
- Failed validation, conflict, or concurrency mutation creates no success Audit row.
- Audit UTC values serialize with an explicit UTC kind.

## API Surface

```text
POST /api/auth/login

GET  /api/admin/dashboard
GET  /api/admin/users
GET  /api/admin/users/{id}
PUT  /api/admin/users/{id}/approve|ban|unban

GET  /api/admin/products
GET  /api/admin/products/{id}
PUT  /api/admin/products/{id}/hide|restore

GET  /api/admin/reviews
GET  /api/admin/reviews/{id}
PUT  /api/admin/reviews/{id}/hide|restore

GET  /api/admin/disputes
GET  /api/admin/disputes/{id}
PUT  /api/admin/disputes/{id}/assign|start-review|resolve|reject

GET  /api/admin/return-requests
GET  /api/admin/return-requests/{id}
PUT  /api/admin/return-requests/{id}/approve|reject

GET  /api/admin/orders
GET  /api/admin/orders/{id}
GET  /api/admin/feedbacks
GET  /api/admin/reports/summary
GET  /api/admin/audit
```

## Security

- Every active `/api/admin/...` controller has `[Authorize(Roles = "Admin")]`; no Admin action has `[AllowAnonymous]`.
- Missing JWT returns `401`; authenticated non-Admin JWT returns `403`.
- Mutation actor is always `ClaimTypes.NameIdentifier` from the validated JWT.
- Request DTOs cannot set moderation actor, resolver, Audit actor/action/resource, or timestamps.
- `AssignDisputeRequestDto.AdminUserId` selects the assignee only; it does not select the acting Admin.

## Transactions And Concurrency

- Repositories and `IAdminAuditRepository` are scoped over the same `AppDbContext`.
- Each mutation tracks business state plus Audit, then calls one `SaveChangesAsync`.
- EF Core SQL Server wraps that save atomically; either both business state and Audit commit, or neither commits.
- Moderation/workflow status columns are EF concurrency tokens. Concurrent conflicting mutations produce one success and one `409`; the failed request's Audit insert rolls back.

## Demo Data

Password for all demo accounts: `Demo@123`.

| Purpose | Account/marker | Baseline state |
| --- | --- | --- |
| Admin | `admin.demo@gmail.com` | Active |
| Pending login/moderation | `pending.demo@gmail.com` | Pending |
| Banned login/moderation | `banned.demo@gmail.com` | Banned |
| Buyers | `buyer1.demo@gmail.com`, `buyer2.demo@gmail.com` | Active |
| Sellers | `seller1.demo@gmail.com`, `seller2.demo@gmail.com` | Active |
| Product moderation | `Demo Wireless Headphones` | Active |
| Product restore | `Demo Vintage Camera Auction` | Hidden |
| Review moderation | `DEMO REVIEW: Excellent sound quality.` | Visible |
| Review restore | `DEMO REVIEW: Product is acceptable overall.` | Hidden |
| Dispute | `DEMO DISPUTE OPEN:` | Open, unassigned |
| Return | `DEMO RETURN PENDING:` | Pending |
| Shipping | `DEMO-DELIVERED-001` | Delivered context |

Final QA cleanup is always:

```text
database/reset_demo_data.sql
-> database/seed_demo_data.sql
```

## QA Baseline

- Clean DB reconstruction: lecturer schema, extension twice, seed twice passed; 18 tables; no migration-history table.
- API matrix: 87 assertions passed across login, `401/403/404/400/409`, all moderation workflows, two assigned Dispute terminal paths, direct resolution compatibility, Returns, dashboard counters, filters/sort/page, Orders, Payment, Shipping, Feedback, Reports, and Audit coverage.
- Concurrent Return approve/reject: one `200`, one `409`, exactly one Audit row.
- Assigned Dispute ownership: wrong Admin finalize `409`; assignee review/finalize `200`.
- Playwright: Dashboard queues, Review status, Dispute filters, Orders/Payment/Shipping, Feedback, Reports, Audit, Return mutation/reload, observer-Admin SignalR toast payload, and server-offline cache/banner/write-disable passed.
- Demo baseline restored after destructive QA: no QA users/cases, no Audit rows, one Pending user, one Banned user, one Hidden product, one Hidden review, one Open dispute, one Pending return.

## Important Files

| Area | Files |
| --- | --- |
| Schema | `database/admin_extensions.sql`, `database/seed_demo_data.sql`, `database/reset_demo_data.sql` |
| EF mapping | `EbayClone.API/Data/AppDbContext.cs` |
| User/Product/Review | matching API `Controllers`, `Services`, `Repositories`, `DTOs`; MVC controllers/views |
| Dispute/Return | matching API controller-service-repository files; MVC controllers/details views |
| Audit | `Models/AdminAuditLog.cs`, `Repositories/AdminAuditRepository.cs`, `Services/AdminAuditService.cs`, `Controllers/AdminAuditController.cs`, MVC `Controllers/AuditController.cs`, `Views/Audit/Index.cshtml` |
| Dashboard | `Repositories/DashboardRepository.cs`, `Services/AdminDashboardService.cs`, DTO/MVC model/view |
| Auth/security | `Program.cs`, `Helpers/JwtHelper.cs`, `Services/AuthService.cs` |
| Offline/SignalR | MVC `Services/CoreApiClient.cs`, `ApiCacheService.cs`, `AdminNotificationService.cs`, `Hubs/AdminNotificationHub.cs`, `wwwroot/js/site.js` |

## Known Limitations

- Concurrency uses current status as the token, not a general row-version; it protects conflicting workflow transitions, not unrelated column edits.
- Offline cache is process-local and path-based. It is fallback-only; there is no cross-instance cache or queued write synchronization.
- SignalR notifications are emitted by MVC mutation actions, not direct API calls; Audit remains the durable mutation record.
- The repository has no committed standalone Playwright suite/config. Final browser regression used the installed Playwright automation skill against isolated API/MVC instances.
- Seed provides one Pending Return and one Open Dispute. QA used temporary marker-based fixtures for second terminal paths, then removed them before baseline restore.

## Deliberately Out Of Scope

No ProductReport/ReviewReport evidence workflow, seller enforcement/risk scoring, appeals, KYC/2FA, wallets/payouts/refunds, durable user notifications, AI moderation, CQRS/MediatR, runtime migrations, or policy engine.

## Fragile Areas

- API/MVC positional record contracts must change together.
- Keep mutation and Audit in one scoped `AppDbContext` and one `SaveChangesAsync`.
- Preserve concurrency tokens and controller mapping of `DbUpdateConcurrencyException` to `409`.
- Preserve direct Open Dispute resolution unless business rules explicitly remove backward compatibility.
- Never edit `clone_ebay_sqlserver_schema.sql` or re-enable historical migrations.
- Seed/reset scripts use stable natural markers, not identity IDs.
