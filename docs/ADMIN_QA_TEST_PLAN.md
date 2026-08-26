# Admin Business QA Test Plan

## Authentication

### TC-AUTH-ADMIN-01

- Preconditions: active database account with role `Admin`; valid BCrypt hash.
- Steps: submit valid email/password at MVC Login.
- Expected UI: Dashboard opens; Admin navigation visible.
- Expected HTTP: `POST /api/auth/login` returns `200` and JWT; protected API returns `200`.
- Expected state/side effects: JWT stored only in MVC Session; no password returned.
- Negative: wrong password returns `401`; no Session token.

### TC-AUTH-AUTHZ-01

- Preconditions: valid non-Admin JWT, then no JWT.
- Steps: call a protected Admin endpoint with each credential state.
- Expected UI: non-Admin cannot enter Admin panel; expired/missing Session redirects to Login.
- Expected HTTP: non-Admin `403`; missing/invalid token `401`.
- Expected state/side effects: no mutation.

### TC-AUTH-LOGOUT-01

- Preconditions: logged-in Admin.
- Steps: logout; revisit Dashboard.
- Expected UI: redirected to Login.
- Expected HTTP: subsequent API client has no Bearer token.
- Expected state/side effects: Session token removed.

## Users

### TC-USER-LIST-01

- Preconditions: Admin authenticated; multiple users.
- Steps: search, filter role, sort, change page.
- Expected UI: matching rows and correct pager.
- Expected HTTP: `200`; bounded page/pageSize.
- Expected state/side effects: read-only; no DB mutation.
- Negative: invalid query values return `400` or a safely normalized page.

### TC-USER-MODERATION-01

- Preconditions: Pending, Active, and Banned demo users; Admin authenticated.
- Steps: approve Pending; ban Active with a valid reason; unban Banned; reload list/detail after each action.
- Expected UI: persisted status and legal next actions appear.
- Expected HTTP: each legal transition returns `200`.
- Expected state/side effects: state, actor/time, and one audit row update atomically.
- Negative: duplicate/illegal transition returns `409`; missing/blank ban reason returns `400`; no duplicate audit.

### TC-USER-LOGIN-STATUS-01

- Preconditions: valid credentials for Banned and Pending users.
- Steps: attempt login.
- Expected HTTP: `401` with the generic login failure contract.
- Expected state/side effects: no JWT issued.

## Products

### TC-PRODUCT-LIST-01

- Preconditions: Admin authenticated; products from multiple sellers.
- Steps: search, seller filter, sort, paginate; open detail if supported.
- Expected UI: matching products and stable paging.
- Expected HTTP: `200`.
- Expected state/side effects: read-only.

### TC-PRODUCT-MODERATION-01

- Preconditions: Active and Hidden products.
- Steps: hide Active with a valid reason; reload; restore Hidden; reload.
- Expected UI: persisted status and legal next action appear.
- Expected HTTP: legal transitions return `200`.
- Expected state/side effects: moderation metadata and one audit row per transition commit atomically.
- Negative: duplicate transition `409`; blank hide reason `400`; missing product `404`; no audit on failure.

## Orders

### TC-ORDER-LIST-01

- Preconditions: orders across statuses, buyers, and dates.
- Steps: filter status/buyer/date; paginate.
- Expected UI: correct rows, totals, page state.
- Expected HTTP: `200`.
- Expected state/side effects: read-only.
- Negative: `from > to` returns `400`.

### TC-ORDER-DETAIL-01

- Preconditions: order with items, payment, and shipping information.
- Steps: open detail.
- Expected UI: buyer, items, payment status/method/amount, shipping carrier/tracking/status shown.
- Expected HTTP: existing ID `200`; missing ID `404`.
- Expected state/side effects: none.

## Disputes

### TC-DISPUTE-LIST-01

- Preconditions: disputes in multiple statuses.
- Steps: filter status; paginate; open detail.
- Expected UI: matching cases and order/raiser context.
- Expected HTTP: `200`; missing detail `404`.
- Expected state/side effects: none.

### TC-DISPUTE-RESOLVE-01

- Preconditions: Dispute status is `Open`.
- Steps: submit a nonblank business resolution.
- Expected UI: success message; reopened detail shows `Resolved` and resolution.
- Expected HTTP: `200`.
- Expected state/side effects: `status` and `resolution` update atomically.
- Negative: blank resolution returns `400`; state unchanged.

### TC-DISPUTE-REJECT-01

- Preconditions: Dispute is `Open`.
- Steps: submit a nonblank rejection reason.
- Expected UI: reopened detail shows `Rejected` and reason.
- Expected HTTP: `200`.
- Expected state/side effects: `Open -> Rejected`; resolution stores reason.
- Negative: second rejection returns `409`; no mutation.

### TC-DISPUTE-INVALID-01

- Preconditions: Dispute is terminal (`Resolved` or `Rejected`).
- Steps: attempt another resolution/rejection.
- Expected UI: conflict message; original detail remains.
- Expected HTTP: `409 Conflict`.
- Expected state/side effects: no mutation; no false success.

### TC-DISPUTE-ASSIGN-01

- Preconditions: Open dispute and active Admin assignee.
- Steps: assign; start review; reload after each transition.
- Expected UI: assignee and `Assigned`, then `InReview`, persist.
- Expected HTTP: legal transitions return `200`.
- Expected state/side effects: assignment/review timestamps and audit rows commit atomically.
- Negative: non-Admin/missing assignee `400`; terminal dispute or repeated/conflicting transition `409`; missing dispute `404`.

## Return Requests

### TC-RETURN-001

- Preconditions: Admin authenticated; ReturnRequest status is `Pending`.
- Steps: open detail; approve; reload detail.
- Expected UI: status badge shows `Approved`; terminal-action controls disappear; success notification appears.
- Expected HTTP: approve returns `200`.
- Expected state/side effects: database status becomes `Approved`; SignalR Admin toast is broadcast by MVC.
- Negative: no unrelated Order, Payment, or Shipping state changes.

### TC-RETURN-002

- Preconditions: Admin authenticated; a second ReturnRequest status is `Pending`.
- Steps: open detail; reject; reload detail.
- Expected UI: status badge shows `Rejected`; terminal-action controls disappear.
- Expected HTTP: reject returns `200`.
- Expected state/side effects: database status becomes `Rejected`.

### TC-RETURN-003

- Preconditions: ReturnRequest status is `Approved` or `Rejected`.
- Steps: directly submit approve or reject again.
- Expected UI: conflict message; original terminal status remains after reload.
- Expected HTTP: `409 Conflict`.
- Expected state/side effects: no state mutation; no success notification.

### TC-RETURN-LIST-001

- Preconditions: seeded Pending, Approved, and Rejected requests.
- Steps: filter by status, user, order, and valid date range; paginate.
- Expected UI: newest first; matching request, user, order, and status shown.
- Expected HTTP: `200`.
- Negative: `from > to` returns `400`.

### TC-RETURN-DETAIL-001

- Preconditions: seeded request linked to an order, payment, and shipping row.
- Steps: open detail.
- Expected UI: reason, user, order status/total/date, payment method/status, carrier/tracking/status shown.
- Expected HTTP: existing ID `200`; missing ID `404`.
- Expected state/side effects: read-only.

## Reviews And Feedback

### TC-REVIEW-LIST-01

- Preconditions: reviews exist.
- Steps: list/filter/page and inspect review details.
- Expected UI: rating/comment/product/reviewer and moderation status visible.
- Expected HTTP: `200`.
- Expected state/side effects: read-only.

### TC-REVIEW-MODERATION-01

- Preconditions: Visible and Hidden reviews.
- Steps: hide Visible with reason; reload; restore Hidden; reload.
- Expected HTTP: legal transitions `200`.
- Expected state/side effects: moderation metadata and audit row commit atomically.
- Negative: duplicate transition `409`; blank hide reason `400`; missing review `404`; no audit on failure.

### TC-FEEDBACK-LIST-01

- Preconditions: seller feedback aggregates exist.
- Steps: filter seller/min/max rating; paginate; open detail.
- Expected UI: average rating, review count, positive rate.
- Expected HTTP: `200`; invalid rating range `400`.
- Expected state/side effects: read-only.

## Reports And Audit

### TC-REPORT-RANGE-01

- Preconditions: paid and unpaid payments; dated orders.
- Steps: select valid date range.
- Expected UI: order totals and paid revenue reflect range; all-time metrics labeled.
- Expected HTTP: `200`.
- Expected state/side effects: none.
- Negative: `from > to` returns `400`.

### TC-AUDIT-SCHEMA-01

- Preconditions: extension script applied; selected Admin mutation completed.
- Steps: filter Audit by action/resource; open the latest rows.
- Expected UI: correct Admin, action, resource, reason, and UTC time appear.
- Expected HTTP: `GET /api/admin/audit` returns `200` with bounded pagination.
- Expected state/side effects: read-only; failed/conflicting mutations produce no row.

### TC-AUDIT-COVERAGE-01

- Preconditions: fresh demo workflow records.
- Steps: perform user approve/ban/unban, product hide/restore, dispute assign/review/resolve or reject, Return approve/reject, review hide/restore.
- Expected state/side effects: exactly one matching audit row for every successful mutation.

## Offline Mode

### TC-OFFLINE-GET-01

- Preconditions: successful cached Admin GET; API then unavailable.
- Steps: revisit cached page.
- Expected UI: cached data remains; Offline Mode banner appears.
- Expected HTTP: transport/`502`/`503`/`504` handled as offline fallback.
- Expected state/side effects: cache read only; no token cached.

### TC-OFFLINE-WRITE-01

- Preconditions: MVC in Offline Mode.
- Steps: inspect and attempt mutation controls.
- Expected UI: mutation controls disabled or hidden.
- Expected HTTP: no write sent.
- Expected state/side effects: no queued synchronization; DB unchanged.

### TC-OFFLINE-RECOVERY-01

- Preconditions: MVC offline; API restored.
- Steps: wait for health refresh or reload.
- Expected UI: banner disappears; fresh GET replaces stale cache; valid writes re-enable.
- Expected HTTP: health and GET return `200`.
- Expected state/side effects: online state restored.
