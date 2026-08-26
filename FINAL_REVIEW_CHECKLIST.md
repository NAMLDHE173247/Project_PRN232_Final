# Final Lecturer Demo Checklist

## Setup

- [ ] Run `database/clone_ebay_sqlserver_schema.sql` only for a new database.
- [ ] Run `database/seed_demo_data.sql`.
- [ ] Start API and MVC; verify API `/health` returns `Healthy`.
- [ ] Confirm `dotnet build EbayClone.sln --no-restore` has 0 warnings/errors.

Demo login: `admin.demo@gmail.com` / `Demo@123`.

Clean verification uses exactly lecturer schema + `seed_demo_data.sql`; no EF migration or startup seed runs.

## Recommended Demo Order

1. [ ] Admin Login: show JWT Admin authentication and MVC Session.
2. [ ] Dashboard: show User/Product/Order totals, paid revenue, Open Disputes.
3. [ ] Users: search `demo`; filter role `Seller`. Explain moderation is omitted because lecturer User table has no status.
4. [ ] Products: search `Demo`; filter Seller ID obtained from the User list. Show normal and auction products.
5. [ ] Orders: open the `Delivered` demo order. Show items, `PayPal`, paid amount, carrier, tracking, shipping status.
6. [ ] Disputes: filter `Open`; open description beginning `DEMO DISPUTE OPEN`; Resolve or Reject; reload; repeat direct action to demonstrate `409`.
7. [ ] Return Requests: filter `Pending`; open reason beginning `DEMO RETURN PENDING`; Approve or Reject; reload; repeat direct action to demonstrate `409`.
8. [ ] Feedback/Reviews: show Seller A/B aggregate ratings and positive/neutral/negative reviews.
9. [ ] Reports: use a range covering the last 30 days; show paid revenue. Demonstrate invalid reversed date range rejection if time permits.
10. [ ] Offline Mode: demonstrate cached GET/banner/write disabling only if rehearsed and stable.

IDs are intentionally not fixed. Locate mutation records by stable text prefixes:

- Dispute: `DEMO DISPUTE OPEN:`
- Return: `DEMO RETURN PENDING:`
- Delivered order tracking: `DEMO-DELIVERED-001`

## Business Checks

- [ ] Dispute `Open -> Resolved/Rejected`; blank or over-2000-character resolution rejected; terminal transition returns `409`.
- [ ] ReturnRequest `Pending -> Approved/Rejected`; terminal transition returns `409`.
- [ ] SignalR toast appears after successful MVC Dispute/Return mutation.
- [ ] Order detail shows Payment method/status and Shipping carrier/tracking/status.
- [ ] Deferred User/Product/Review/Audit/assignment actions are absent.
- [ ] Lecturer schema and `reference/` remain unchanged.
- [ ] Missing token returns `401`; Buyer token returns `403`; missing record returns `404`; reversed report dates return `400`.

## Reset

- [ ] Run `database/reset_demo_data.sql` only when a clean demo rerun is needed.
- [ ] Run `database/seed_demo_data.sql` again; IDs may change, stable names remain.
