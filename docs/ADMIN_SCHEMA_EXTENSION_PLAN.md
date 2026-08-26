# Admin Schema Extension Plan

Apply explicitly after `clone_ebay_sqlserver_schema.sql` and before `seed_demo_data.sql`. No EF runtime migration, startup DDL, or lecturer-schema edit.

| Table/column | Type | Null/default | FK | Business purpose | Feature | Why existing data cannot represent it |
| --- | --- | --- | --- | --- | --- | --- |
| `User.moderationStatus` | `nvarchar(20)` | NOT NULL, `Active` | None | Pending/Active/Banned state | User moderation/login | `role` is authorization, not account state |
| `User.moderationReason` | `nvarchar(500)` | NULL | None | Current ban reason | Ban | No baseline reason field exists |
| `User.moderatedBy` | `int` | NULL | `User(id)` | Responsible Admin | User auditability | Audit actor alone does not expose current decision owner |
| `User.moderatedAtUtc` | `datetime2(0)` | NULL | None | Current decision time | User moderation | No user timestamp exists |
| `Product.moderationStatus` | `nvarchar(20)` | NOT NULL, `Active` | None | Active/Hidden visibility decision | Product moderation | Auction/catalog fields do not represent Admin visibility |
| `Product.moderationReason` | `nvarchar(500)` | NULL | None | Current hide reason | Product moderation | Description is seller content |
| `Product.moderatedBy` | `int` | NULL | `User(id)` | Responsible Admin | Product auditability | No moderator metadata exists |
| `Product.moderatedAtUtc` | `datetime2(0)` | NULL | None | Current decision time | Product moderation | No moderation timestamp exists |
| `Review.moderationStatus` | `nvarchar(20)` | NOT NULL, `Visible` | None | Visible/Hidden state | Review moderation | Rating/comment cannot honestly encode visibility |
| `Review.moderationReason` | `nvarchar(500)` | NULL | None | Current hide reason | Review moderation | Comment belongs to reviewer |
| `Review.moderatedBy` | `int` | NULL | `User(id)` | Responsible Admin | Review auditability | No actor field exists |
| `Review.moderatedAtUtc` | `datetime2(0)` | NULL | None | Current decision time | Review moderation | `createdAt` is review creation time |
| `Dispute.assignedTo` | `int` | NULL | `User(id)` | Current Admin owner | Dispute assignment | `raisedBy` is claimant, not assignee |
| `Dispute.assignedAtUtc` | `datetime2(0)` | NULL | None | Assignment time | Dispute assignment | No suitable timestamp exists |
| `Dispute.reviewStartedAtUtc` | `datetime2(0)` | NULL | None | Distinguish assigned from active review | Dispute review | Status alone cannot provide timing |
| `Dispute.resolvedBy` | `int` | NULL | `User(id)` | Terminal decision actor | Dispute auditability | Resolution text is not actor identity |
| `Dispute.resolvedAtUtc` | `datetime2(0)` | NULL | None | Terminal decision time | Dispute auditability | No dispute timestamp exists |
| `AdminAuditLog` | New table | Required fields below | `adminUserId -> User(id)` | Durable mutation history | All Admin mutations | No baseline table can hold append-only cross-resource events |

`AdminAuditLog`: identity `id`; `adminUserId int`; `action nvarchar(50)`; `resourceType nvarchar(50)`; `resourceId int`; `reason nvarchar(500) NULL`; `createdAtUtc datetime2(0)` default UTC.

## Constraints And Indexes

- Check constraints restrict user/product/review moderation states and dispute workflow states.
- Index status columns for queues; index audit `(createdAtUtc DESC)` and `(resourceType, resourceId)`.
- Admin actor FKs use `NO ACTION`. Admin records referenced by decisions/audit cannot be deleted accidentally.
- Existing rows receive honest active/visible defaults. Demo seed explicitly creates alternate states.

## Deliberate Omissions

- No separate approval status: `Pending -> Active` is sufficient for selected requirements.
- No moderation-case/report table: direct reason-backed decisions cover P0/P1. Add case tables when user-submitted reports/evidence enter scope.
- No row-version column: current service style remains. Add optimistic concurrency when simultaneous moderator traffic becomes a measured concern.
- No audit JSON/export metadata: selected actions need actor/action/resource/reason only.
