# Admin Reference Parity Matrix

Static tracing verified routes, handlers/services, persistence, and UI callers. A class name alone does not count as support.

| Capability | Current | Ref A: `ebay_clone_adminRole` | Ref B: `EbayCloneAll` | Business value | Complexity | Decision |
| --- | --- | --- | --- | --- | --- | --- |
| User monitoring | Wired list/detail API; MVC list | Wired list/detail UI and API | Wired seller/user lists | High | Low | Keep; add status filter/detail actions |
| User approval | Absent | Wired, but approval guards are incomplete | Wired seller approval queue | High | Medium | P0: `Pending -> Active` |
| Ban/unban | Absent | Wired with open-order guard | Wired; login blocks Banned | High | Medium | P0: `Active -> Banned -> Active`; reason required; block login |
| Product monitoring | Wired list/detail API; MVC list | Wired managed-product queue | Wired Admin product list | High | Low | Keep; add status filter/actions |
| Product reporting | Absent | Wired report creation and queue; status inconsistencies remain | Wired reports/evidence; authorization gaps | Medium | High | Defer; direct moderation reason supplies current need |
| Product moderation | Absent | Wired hide/ban/restore decisions | Wired automated/manual approval; weak transitions | High | Medium | P0: `Active -> Hidden -> Active` |
| Review monitoring | Wired read-only list/detail | Wired flagged queue and hide/keep | No verified review moderation | Medium | Low | Keep |
| Review moderation | Absent | Wired `Visible/PendingReview/Hidden`; report closure/audit missing | Absent | Medium | Medium | P1: `Visible -> Hidden -> Visible` |
| Persistent audit | Absent | Wired read model; writes cover only some actions | Wired asynchronous interceptor/Redis worker; weak Admin console | High | Medium | P0: synchronous `AdminAuditLog`; audit every selected mutation |
| Dispute list/resolution | Wired `Open -> Resolved/Rejected` | Wired docket/routes; broad model | Wired broad case decisions and financial actions | High | Medium | Preserve current terminal behavior |
| Dispute assignment | Absent | Mapped assign route and assignee model | `take` action exists but no durable owner field | High | Medium | P1: `Open -> Assigned -> InReview`; Admin assignee only |
| Return Request | Wired standalone `Pending -> Approved/Rejected` | Wired broad return routes | Return handling folded into disputes | High | Low | Preserve; add audit only |
| Seller monitoring | Feedback aggregates/product seller filter | Metrics and background evaluation wired | Risk snapshots/enforcement API wired; UI partial | Medium | High | Defer P2; current data remains read-only |
| Dashboard action queues | Open-dispute count only | Product/review/dispute/return queue routes | Separate queues; no unified queue UI | High | Medium | Add moderation counts after core transitions |
| Notifications | MVC SignalR toast for dispute/return | Durable rows, broadcasts, SignalR; usage mixed | Queue, persistence, SignalR; email simulated | Medium | High | Preserve current SignalR; no durable notification subsystem |
| Reporting | Wired date-filtered orders/paid revenue | Stats/financial routes; export not verified | Broad statistics wired | Medium | Medium | Preserve; no speculative growth metrics |
| Auction controls | Absent | Seller product lifecycle only | Wired suspend/unsuspend/cancel | Low | High | Defer; outside Admin expansion priority |
| Email templates | Absent | Broadcast management exists | Wired template/manual-send flow | Low | High | Defer |
| Appeals/evidence | Absent | Rich dispute/review/product models; partial wiring | Wired dispute evidence/appeal paths | Low | High | Defer P2 |

## Verified Reference Caveats

- Ref A uses endpoint-to-handler-to-EF, not this project's repository architecture.
- Ref B uses controller-to-service-to-EF; its repository directory is empty.
- Ref A product/review/return routes have authorization gaps. Ref B product-report routes expose unsafe access. Neither pattern should be copied.
- Ref A audit coverage is partial. Ref B asynchronous audit can be lost after a committed business mutation.
- Ref B dispute handling consists of two overlapping systems. It is not one coherent state machine.
- Reference background workers, KYC, wallets, payouts, AI moderation, seller risk, and appeals exceed the selected course-project scope.

## Selected Scope

P0: user moderation, product moderation, persistent synchronous audit.

P1: dispute assignment/review, review moderation, dashboard action counts.

Deferred: report/evidence entities, seller enforcement, appeals, durable notifications, financial dispute effects, policy engines.
