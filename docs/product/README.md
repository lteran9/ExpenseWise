# Product scope and definition of done

Status: Phase 1 boundaries were confirmed on 2026-10-16. Account, identity, group, and permission rules for package 1.2 were confirmed on 2026-10-17. Expense and share rules for package 1.3 were confirmed on 2026-10-19; repayment rules for package 1.4 remain proposed.

- [Current-state assessment](current-state.md): what exists and the evidence behind the gaps.
- [Delivery roadmap](roadmap.md): ordered work packages and release gates.

## Purpose

ExpenseWise is a small-group expense ledger. It answers:

1. Who paid for a shared bill, check, or expense?
2. Which people share that expense, and how much is each person's share?
3. Who owes whom after accounting for all expenses and recorded repayments?
4. What remains to be repaid?

It records money spent and repaid elsewhere. It is not a payment processor or a banking/accounting system.

## Confirmed Phase 1 boundaries

| Decision | Scope |
| --- | --- |
| Deployment | Local Docker Compose for personal/trusted use. |
| Participants | Every participant has an account. One organizer records expenses and settlements. |
| Splitting | Equal splits among selected people, not necessarily every group member. |
| Settlement | Show who owes whom and record manual repayments. ExpenseWise does not move money. |
| Currency | USD only. No currency conversion. |

The organizer is the group owner. Recording an expense for somebody else must distinguish the person entering the record from the person who actually paid.

### The Phase 1 user journey

1. Start the UI, API, and MySQL stack using Docker Compose with local configuration.
2. Register accounts, sign in as the organizer, and create a group.
3. Add existing account holders to the group; the organizer is also a member.
4. Enter an expense with a description, USD amount, one payer, and selected participants.
5. Review the expense and each person's allocated share.
6. Add more expenses paid by different people, including expenses shared by only a subset of the group.
7. See each member's balance and explicit repayment suggestions.
8. Record full or partial repayments that happened outside ExpenseWise.
9. See balances update and reach zero when everyone has settled.
10. Restart the stack without losing the ledger.

These steps must be possible in the browser, without Swagger, manual SQL, or editing application code.

## Confirmed detailed rules for Phase 1.2

- Registration, sign-in, and sign-out work with friendly errors. Existing phone/country-code lookup can be reused for adding registered members; this is not an email/SMS invitation service.
- An authenticated identity is established and checked on the server. A user key in a form or URL is not proof of identity.
- Only the organizer can change group metadata/membership, enter or void expenses, and record repayments.
- Other members can view their groups and the shared ledger. Nonmembers cannot read or modify a group's data, including through direct API requests.
- Empty group/expense lists are normal states with a clear next action, not application failures.
- Duplicate membership is rejected without a server error. Adding a member does not change old expense allocations.
- Removing members with financial history and deleting accounts with financial history are not supported in Phase 1; reject these actions explicitly. Group archival must retain the ledger.

Local/trusted deployment limits the deployment target; it does not remove the need for per-user access checks.

## Confirmed detailed rules for package 1.3

### Expenses and shares

- One expense has one payer who is an active group member, at least one distinct participating member, a description, and a positive USD amount with at most two decimal places and no more than 16 integer digits.
- The payer may be included in or excluded from the participants. The person entering the record is not implicitly the payer.
- An organizer may record an expense paid by another member. Multiple payers for one expense are deferred.
- Store exact decimal/minor-unit amounts; do not use floating-point money arithmetic.
- Divide the total in cents. Allocate remainder cents in ascending stable participant-ID order, and persist the resulting shares. For USD 10.00 among three people, the shares are 3.34, 3.33, and 3.33.
- Shares always sum exactly to the expense amount. An empty participant list, duplicate participants, nonmembers, unsupported currency, excess precision, or an out-of-range amount is a visible validation error with no partial write.
- Saving the expense and all shares is atomic. Repeating the same submission must not create a second expense; intentionally entering a second identical bill remains possible.
- Creating shares does not mark them paid. The payer's outlay, participants' obligations, and later repayments are separate concepts.
- Preserve the original participants/shares when membership changes.
- For the minimum correction workflow, the organizer can void an expense and re-enter it before any repayment has been recorded in that group. Retain the voided entry and exclude it from calculations. General expense editing and post-repayment corrections are deferred; block unsupported corrections clearly.

The package 1.3 migration intentionally resets all existing application data. Historical paid splits cannot be reliably distinguished from allocations and repayments; no pre-1.3 data conversion is supported. Subsequent migrations must preserve data unless a separate explicit decision says otherwise. Do not run this migration against data that must be retained.

Package 1.3 does not provide a repayment writer. The no-repayments condition for voiding therefore holds until package 1.4 adds repayments and enforces the condition against the ledger.

## Proposed detailed rules for package 1.4

Review these defaults before implementing package 1.4; any changes should update this specification and its acceptance tests.

### Balances and manual repayments

For each member, calculate:

```text
net balance = expenses paid - allocated shares
              + repayments sent - repayments received
```

- Positive means the member is owed money; negative means the member owes money.
- The sum of all member net balances is exactly zero, including after each repayment.
- Show total spent separately from outstanding repayment amounts. Total spent is the sum of nonvoid expenses, not reduced by repayments.
- Outstanding is the sum of positive net balances, equivalently the absolute sum of negative net balances. It is not total spent minus all shares marked paid.
- Show deterministic debtor-to-creditor repayment suggestions whose amounts reconcile the balances. Minimizing the number of transfers is not required.
- A repayment records the group, sender, recipient, positive USD amount, occurrence time, and recording user. Sender and recipient must be different group members.
- Allow partial repayments. For Phase 1, reject payments exceeding the sender's current debt or recipient's current credit; all calculations and validation must use one consistent ledger state.
- Repayments are recorded facts, not a checkbox that pays every expense. An allocation is not a repayment.
- Save repayments atomically and protect repeated submissions from double counting, including concurrent requests.
- Show confirmation and repayment history. Do not imply that ExpenseWise transferred money.
- Correction/reversal of repayments is deferred; require a confirmation screen before recording and state this limitation. Do not silently rewrite financial history.

### Operating expectations

- A fresh documented Compose setup builds, waits for MySQL readiness, applies the schema through a repeatable migration step, and serves the browser UI.
- Missing/invalid configuration fails clearly. No embedded credentials, generated container-name dependencies, or manual schema setup.
- After package 1.3 is installed, restarting services preserves accounts, memberships, expenses, and shares. A restart may require users to sign in again; persistent login is not a Phase 1 requirement.
- Provide a documented backup and restore procedure and prove that a restored ledger matches the original.
- Errors are logged appropriately without passwords and shown usefully to users without exposing internal exceptions.
- The local deployment is not advertised as safe for public exposure.

## Acceptance scenarios

All monetary values below are USD. Each scenario must be backed by tests at the relevant layer; the primary scenario must also pass through the Docker-hosted browser UI.

### A. Complete expense-to-settlement journey

Given registered group members Alice, Bob, and Cara, with Alice as organizer:

1. Alice records a 90.00 dinner paid by Alice and shared equally by all three: shares are 30.00 each; balances are Alice +60.00, Bob -30.00, Cara -30.00.
2. Alice records a 30.00 taxi paid by Bob and shared by Alice and Bob only: shares are 15.00 each.
3. The ledger shows total spent 120.00; balances Alice +45.00, Bob -15.00, Cara -30.00; outstanding 45.00.
4. Suggested repayments are Bob to Alice 15.00 and Cara to Alice 30.00.
5. Record Bob to Alice 5.00: balances become Alice +40.00, Bob -10.00, Cara -30.00; outstanding 40.00.
6. Retry that same repayment submission: balances and history are unchanged.
7. Record Bob's remaining 10.00 and Cara's 30.00: every balance and outstanding equal zero; total spent stays 120.00.
8. Restart the stack: the same expenses, persisted shares, repayments, and zero balances remain.

### B. Rounding and participant selection

- Allocate 10.00 across three selected members as 3.34/3.33/3.33 in the documented order; reloading does not change the allocation.
- Adding a fourth member does not change that expense.
- A payer excluded from the participants receives full credit for the outlay without an allocated share.
- One selected participant receives the whole amount. If that participant is also the payer, the net balance is zero.

### C. Invalid operations and access boundaries

- Reject invalid money, unsupported currencies, nonmember payers/participants, duplicate participants, duplicate membership, self-repayments, and overpayments without partial writes.
- Anonymous requests require sign-in/authentication; nonmembers cannot access a group; ordinary members cannot perform organizer-only writes.
- Duplicate expense/repayment submissions and concurrent repayments cannot double charge or overpay.
- Void an expense before repayments: exclude it from balances but retain its history. Reject voiding after a group has repayments, member removal with history, and unsupported repayment corrections.
- Unimplemented features are absent or explicitly unavailable; no navigation leads to a placeholder success or an unhandled exception.

### D. Installation, recovery, and failure handling

- From a clean database volume and documented local configuration, start Compose and finish scenario A without manual database commands.
- Restart with the existing volume without recreating the schema or duplicating data.
- Restore a backup into a separate disposable test database and reconcile its balances and record counts.
- An unavailable API/database results in visible errors and useful logs, not a false confirmation or an empty-looking ledger.

## Explicitly outside Phase 1

Guests without accounts; custom amounts, percentage or weighted splits; line-item receipt splitting; tax/tip calculators (enter the final bill total instead); multiple payers per expense; multiple currencies; FX; real payment collection; bank integration; receipt uploads/OCR; notifications; password reset/email verification; mobile apps; advanced debt optimization; public/cloud hosting; and unrestricted financial-history edits.

Remove or qualify existing UI claims for these capabilities until they are implemented.

## When is this done?

Phase 1 is a release, not the end of all possible features. It is done when every Phase 1 requirement above has evidence of passing, the [roadmap release gate](roadmap.md#phase-1-release-gate) is complete, and the project owner can use the documented Docker stack to finish scenario A unaided.

Later phases are optional. No additional feature is necessary to call this agreed local expense-splitting release complete.
