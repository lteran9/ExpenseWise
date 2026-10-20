#!/usr/bin/env bash
# Phase 1 HTTP acceptance checks against a running ExpenseWise API.
#
# Exercises specification scenarios A (steps 1-8), B, and the API-observable parts of C against a real
# API and MySQL database. Each run registers new disposable accounts, so it can be repeated against the
# same database. Run from the repository root with the Compose stack started:
#
#   scripts/acceptance-api.sh
#
# Environment:
#   API_BASE_URL   API address (default http://localhost:${API_PORT:-8081}).
#   SKIP_RESTART=1 Skip scenario A step 8, which restarts the Compose mysql and webapi services.
#
# Requires curl, jq, and uuidgen. Exits nonzero on the first failed check.

set -euo pipefail

API_BASE_URL="${API_BASE_URL:-http://localhost:${API_PORT:-8081}}"
API_BASE_URL="${API_BASE_URL%/}"
RUN_ID="$(date +%s)$RANDOM"
PASSWORD='Accept@nce1'
AUTH_SCHEME='Bearer'
CHECKS=0
WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

for tool in curl jq uuidgen; do
    command -v "$tool" >/dev/null || { echo "Missing required tool: $tool" >&2; exit 2; }
done

fail() {
    echo "FAIL: $*" >&2
    if [[ -n "${BODY:-}" ]]; then
        echo "Last response (HTTP ${STATUS:-?}): $BODY" >&2
    fi
    exit 1
}

pass() {
    CHECKS=$((CHECKS + 1))
    echo "PASS: $*"
}

# request METHOD PATH TOKEN [JSON_BODY]; sets STATUS and BODY.
request() {
    local method="$1" path="$2" token="$3" data="${4:-}"
    local args=(--silent --show-error --output "$WORK_DIR/body" --write-out '%{http_code}' -X "$method"
        -H 'Accept: application/json')
    if [[ -n "$token" ]]; then
        args+=(-H "Authorization: $AUTH_SCHEME $token")
    fi
    if [[ -n "$data" ]]; then
        args+=(-H 'Content-Type: application/json' --data "$data")
    fi
    STATUS="$(curl "${args[@]}" "$API_BASE_URL$path")"
    BODY="$(cat "$WORK_DIR/body")"
}

expect_status() {
    local expected="$1" description="$2"
    [[ "$STATUS" == "$expected" ]] || fail "$description: expected HTTP $expected, got $STATUS"
}

expect_rejected() {
    local description="$1"
    [[ "$STATUS" =~ ^4[0-9][0-9]$ ]] || fail "$description: expected a 4xx rejection, got HTTP $STATUS"
}

# expect_json JQ_FILTER DESCRIPTION; the filter must evaluate to true against BODY.
expect_json() {
    local filter="$1" description="$2"
    jq -e "$filter" >/dev/null <<<"$BODY" || fail "$description (jq: $filter)"
}

new_key() {
    uuidgen | tr '[:upper:]' '[:lower:]'
}

wait_for_api() {
    for _ in $(seq 1 60); do
        if curl --silent --fail "$API_BASE_URL/Health" >/dev/null; then
            return 0
        fi
        sleep 2
    done
    fail "API did not become reachable at $API_BASE_URL"
}

# register NAME; sets REG_KEY, REG_PHONE, and REG_EMAIL.
register() {
    local name="$1"
    REG_PHONE="$(printf '%010d' $(( (RUN_ID + RANDOM * 32768 + RANDOM) % 10000000000 )))"
    REG_EMAIL="${name,,}.${RUN_ID}@example.test"
    request POST /User/Create "" "$(jq -nc --arg n "$name Acceptance" --arg e "$REG_EMAIL" --arg p "$REG_PHONE" \
        --arg pw "$PASSWORD" '{name:$n,email:$e,phone:$p,countryCode:"+1",password:$pw,confirmPassword:$pw}')"
    expect_status 200 "register $name"
    REG_KEY="$(jq -r '.uniqueKey' <<<"$BODY")"
}

sign_in() {
    local email="$1"
    request POST /User/Authenticate "" "$(jq -nc --arg e "$email" --arg pw "$PASSWORD" '{email:$e,password:$pw}')"
    expect_status 200 "sign in $email"
    jq -r '.accessToken' <<<"$BODY"
}

create_group() {
    local token="$1" name="$2"
    request POST /Group/Create "$token" "$(jq -nc --arg n "$name" \
        '{name:$n,startDate:"2026-01-01T00:00:00",endDate:"2026-12-31T00:00:00"}')"
    expect_status 200 "create group $name"
    jq -r '.uniqueKey' <<<"$BODY"
}

add_member() {
    local token="$1" group="$2" phone="$3"
    request POST /Group/AddMember "$token" "$(jq -nc --arg g "$group" --arg p "$phone" \
        '{groupUniqueKey:$g,phone:$p,countryCode:"+1"}')"
}

# create_expense TOKEN GROUP DESCRIPTION AMOUNT PAYER IDEMPOTENCY_KEY PARTICIPANT...
create_expense() {
    local token="$1" group="$2" description="$3" amount="$4" payer="$5" key="$6"
    shift 6
    local participants
    participants="$(printf '%s\n' "$@" | jq -R . | jq -sc .)"
    request POST /Expense/Create "$token" "$(jq -nc --arg g "$group" --arg d "$description" --argjson a "$amount" \
        --arg p "$payer" --arg k "$key" --argjson ps "$participants" \
        '{groupKey:$g,description:$d,currency:"USD",amount:$a,payerKey:$p,participantKeys:$ps,idempotencyKey:$k}')"
}

record_repayment() {
    local token="$1" group="$2" sender="$3" recipient="$4" amount="$5" key="$6"
    request POST /Repayment/Record "$token" "$(jq -nc --arg g "$group" --arg s "$sender" --arg r "$recipient" \
        --argjson a "$amount" --arg k "$key" \
        '{groupKey:$g,senderKey:$s,recipientKey:$r,currency:"USD",amount:$a,idempotencyKey:$k}')"
}

ledger() {
    request GET "/Repayment/Ledger?groupKey=$2" "$1"
    expect_status 200 "load ledger"
}

expense_list() {
    request GET "/Expense/List?groupKey=$2" "$1"
    expect_status 200 "load expense list"
}

# balance_filter KEY AMOUNT -> jq filter asserting a member's net balance.
balance_filter() {
    echo "(.balances[] | select(.uniqueKey == \"$1\") | .netBalance) == $2"
}

balance_of() {
    jq "(.balances[] | select(.uniqueKey == \"$1\") | .netBalance)" <<<"$BODY"
}

# expect_balances DESCRIPTION KEY AMOUNT [KEY AMOUNT...]
expect_balances() {
    local description="$1"
    shift
    while (($# > 0)); do
        expect_json "$(balance_filter "$1" "$2")" "$description: balance for $1 should be $2"
        shift 2
    done
    expect_json '([.balances[].netBalance] | add) == 0' "$description: balances should sum to zero"
}

wait_for_api
echo "Running Phase 1 API acceptance checks against $API_BASE_URL (run $RUN_ID)"

register Alice; ALICE="$REG_KEY"; ALICE_EMAIL="$REG_EMAIL"
register Bob; BOB="$REG_KEY"; BOB_PHONE="$REG_PHONE"; BOB_EMAIL="$REG_EMAIL"
register Cara; CARA="$REG_KEY"; CARA_PHONE="$REG_PHONE"
register Dan; DAN="$REG_KEY"; DAN_PHONE="$REG_PHONE"; DAN_EMAIL="$REG_EMAIL"
register Erin; ERIN="$REG_KEY"; ERIN_PHONE="$REG_PHONE"
pass "registered five accounts"

ALICE_TOKEN="$(sign_in "$ALICE_EMAIL")"
BOB_TOKEN="$(sign_in "$BOB_EMAIL")"
DAN_TOKEN="$(sign_in "$DAN_EMAIL")"
pass "signed in and received bearer tokens"

# ---------------------------------------------------------------- Scenario A
GROUP_A="$(create_group "$ALICE_TOKEN" "Scenario A $RUN_ID")"
add_member "$ALICE_TOKEN" "$GROUP_A" "$BOB_PHONE"; expect_status 200 "A: add Bob"
add_member "$ALICE_TOKEN" "$GROUP_A" "$CARA_PHONE"; expect_status 200 "A: add Cara"
pass "A: organizer created a group and added two members"

create_expense "$ALICE_TOKEN" "$GROUP_A" "Dinner" 90.00 "$ALICE" "$(new_key)" "$ALICE" "$BOB" "$CARA"
expect_status 200 "A1: record dinner"
expense_list "$ALICE_TOKEN" "$GROUP_A"
expect_json '.expenses | length == 1' "A1: one expense"
expect_json '[.expenses[0].shares[].amount] == [30,30,30]' "A1: three 30.00 shares"
ledger "$ALICE_TOKEN" "$GROUP_A"
expect_balances "A1" "$ALICE" 60 "$BOB" -30 "$CARA" -30
pass "A1: dinner shares are 30.00 each; Alice +60, Bob -30, Cara -30"

create_expense "$ALICE_TOKEN" "$GROUP_A" "Taxi" 30.00 "$BOB" "$(new_key)" "$ALICE" "$BOB"
expect_status 200 "A2: record taxi"
expense_list "$ALICE_TOKEN" "$GROUP_A"
expect_json '[.expenses[] | select(.description == "Taxi") | .shares[].amount] == [15,15]' "A2: two 15.00 shares"
pass "A2: taxi paid by Bob is shared by Alice and Bob at 15.00 each"

ledger "$ALICE_TOKEN" "$GROUP_A"
expect_json '.totalSpent == 120' "A3: total spent 120"
expect_json '.outstanding == 45' "A3: outstanding 45"
expect_balances "A3" "$ALICE" 45 "$BOB" -15 "$CARA" -30
pass "A3: total 120.00; Alice +45, Bob -15, Cara -30; outstanding 45.00"

expect_json "[.suggestions[] | {fromKey, toKey, amount}] | sort_by(.amount) == [
    {fromKey:\"$BOB\",toKey:\"$ALICE\",amount:15},
    {fromKey:\"$CARA\",toKey:\"$ALICE\",amount:30}]" "A4: suggestions"
pass "A4: suggestions are Bob->Alice 15.00 and Cara->Alice 30.00"

RETRY_KEY="$(new_key)"
record_repayment "$ALICE_TOKEN" "$GROUP_A" "$BOB" "$ALICE" 5.00 "$RETRY_KEY"
expect_status 200 "A5: record Bob->Alice 5"
expect_json '.created == true' "A5: repayment created"
ledger "$ALICE_TOKEN" "$GROUP_A"
expect_json '.outstanding == 40' "A5: outstanding 40"
expect_balances "A5" "$ALICE" 40 "$BOB" -10 "$CARA" -30
pass "A5: Bob->Alice 5.00; Alice +40, Bob -10, Cara -30; outstanding 40.00"

record_repayment "$ALICE_TOKEN" "$GROUP_A" "$BOB" "$ALICE" 5.00 "$RETRY_KEY"
expect_status 200 "A6: retry same repayment"
expect_json '.created == false' "A6: retry is not a new repayment"
ledger "$ALICE_TOKEN" "$GROUP_A"
expect_json '.repayments | length == 1' "A6: history unchanged"
expect_balances "A6" "$ALICE" 40 "$BOB" -10 "$CARA" -30
pass "A6: retrying the same submission leaves balances and history unchanged"

record_repayment "$ALICE_TOKEN" "$GROUP_A" "$BOB" "$ALICE" 10.00 "$(new_key)"
expect_status 200 "A7: Bob pays remaining 10"
record_repayment "$ALICE_TOKEN" "$GROUP_A" "$CARA" "$ALICE" 30.00 "$(new_key)"
expect_status 200 "A7: Cara pays 30"
ledger "$ALICE_TOKEN" "$GROUP_A"
expect_json '.totalSpent == 120 and .outstanding == 0' "A7: total 120 and outstanding 0"
expect_json '.suggestions | length == 0' "A7: no suggestions"
expect_json '.repayments | length == 3' "A7: three repayments"
expect_balances "A7" "$ALICE" 0 "$BOB" 0 "$CARA" 0
pass "A7: all balances and outstanding are zero; total spent stays 120.00"

# ---------------------------------------------------------------- Scenario B
GROUP_B="$(create_group "$ALICE_TOKEN" "Scenario B $RUN_ID")"
add_member "$ALICE_TOKEN" "$GROUP_B" "$BOB_PHONE"; expect_status 200 "B: add Bob"
add_member "$ALICE_TOKEN" "$GROUP_B" "$CARA_PHONE"; expect_status 200 "B: add Cara"

create_expense "$ALICE_TOKEN" "$GROUP_B" "Ten split three ways" 10.00 "$ALICE" "$(new_key)" "$CARA" "$BOB" "$ALICE"
expect_status 200 "B: record 10.00"
TEN_KEY="$(jq -r '.uniqueKey' <<<"$BODY")"
request GET "/Expense/Detail?expenseKey=$TEN_KEY" "$ALICE_TOKEN"
expect_status 200 "B: load expense detail"
expect_json '[.shares[].amount] | sort == [3.33,3.33,3.34]' "B: 3.34/3.33/3.33 allocation"
expect_json '([.shares[].amount] | add) == 10' "B: shares sum to 10.00"
ORIGINAL_SHARES="$(jq -c '.shares' <<<"$BODY")"
request GET "/Expense/Detail?expenseKey=$TEN_KEY" "$ALICE_TOKEN"
[[ "$(jq -c '.shares' <<<"$BODY")" == "$ORIGINAL_SHARES" ]] || fail "B: reloading changed the allocation"
pass "B: 10.00 across three members is 3.34/3.33/3.33 and stable on reload"

add_member "$ALICE_TOKEN" "$GROUP_B" "$ERIN_PHONE"; expect_status 200 "B: add Erin"
request GET "/Expense/Detail?expenseKey=$TEN_KEY" "$ALICE_TOKEN"
[[ "$(jq -c '.shares' <<<"$BODY")" == "$ORIGINAL_SHARES" ]] || fail "B: adding a member changed the allocation"
pass "B: adding a fourth member does not change the existing expense"

create_expense "$ALICE_TOKEN" "$GROUP_B" "Excluded payer" 6.00 "$ERIN" "$(new_key)" "$BOB" "$CARA"
expect_status 200 "B: record expense with excluded payer"
ledger "$ALICE_TOKEN" "$GROUP_B"
expect_json "$(balance_filter "$ERIN" 6)" "B: excluded payer receives full credit"
pass "B: a payer excluded from the participants is credited the full outlay without a share"

BOB_BEFORE="$(balance_of "$BOB")"
create_expense "$ALICE_TOKEN" "$GROUP_B" "Solo" 7.00 "$BOB" "$(new_key)" "$BOB"
expect_status 200 "B: record single-participant payer expense"
request GET "/Expense/Detail?expenseKey=$(jq -r '.uniqueKey' <<<"$BODY")" "$ALICE_TOKEN"
expect_json '[.shares[].amount] == [7]' "B: single participant receives the whole amount"
ledger "$ALICE_TOKEN" "$GROUP_B"
expect_json "$(balance_filter "$BOB" "$BOB_BEFORE")" "B: payer-only participant has no net change"
expect_json '([.balances[].netBalance] | add) == 0' "B: balances sum to zero"
pass "B: a single participant who is also the payer nets to zero for that expense"

# ---------------------------------------------------------------- Scenario C
GROUP_C="$(create_group "$ALICE_TOKEN" "Scenario C $RUN_ID")"
add_member "$ALICE_TOKEN" "$GROUP_C" "$BOB_PHONE"; expect_status 200 "C: add Bob"
add_member "$ALICE_TOKEN" "$GROUP_C" "$CARA_PHONE"; expect_status 200 "C: add Cara"

add_member "$ALICE_TOKEN" "$GROUP_C" "$BOB_PHONE"
expect_rejected "C: duplicate membership"
pass "C: duplicate membership is rejected"

INVALID_EXPENSES=(
    "zero amount|0|USD|$ALICE|[\"$ALICE\"]"
    "negative amount|-5|USD|$ALICE|[\"$ALICE\"]"
    "excess precision|1.001|USD|$ALICE|[\"$ALICE\"]"
    "out of range|100000000000000000|USD|$ALICE|[\"$ALICE\"]"
    "unsupported currency|5|EUR|$ALICE|[\"$ALICE\"]"
    "nonmember payer|5|USD|$DAN|[\"$ALICE\"]"
    "nonmember participant|5|USD|$ALICE|[\"$ALICE\",\"$DAN\"]"
    "duplicate participants|5|USD|$ALICE|[\"$BOB\",\"$BOB\"]"
    "no participants|5|USD|$ALICE|[]"
)
for entry in "${INVALID_EXPENSES[@]}"; do
    IFS='|' read -r label amount currency payer participants <<<"$entry"
    request POST /Expense/Create "$ALICE_TOKEN" "$(jq -nc --arg g "$GROUP_C" --arg c "$currency" --argjson a "$amount" \
        --arg p "$payer" --argjson ps "$participants" --arg k "$(new_key)" \
        '{groupKey:$g,description:"Invalid",currency:$c,amount:$a,payerKey:$p,participantKeys:$ps,idempotencyKey:$k}')"
    expect_rejected "C: $label"
done
expense_list "$ALICE_TOKEN" "$GROUP_C"
expect_json '.expenses | length == 0' "C: invalid expenses wrote nothing"
pass "C: invalid money, currency, nonmembers, and duplicate or empty participants are rejected without writes"

request GET /Group/List ""
expect_status 401 "C: anonymous group list"
request GET "/Repayment/Ledger?groupKey=$GROUP_C" ""
expect_status 401 "C: anonymous ledger"
pass "C: anonymous requests require authentication"

request GET "/Group/Retrieve?uniqueKey=$GROUP_C" "$DAN_TOKEN"; expect_rejected "C: nonmember group read"
request GET "/Expense/List?groupKey=$GROUP_C" "$DAN_TOKEN"; expect_rejected "C: nonmember expense read"
request GET "/Repayment/Ledger?groupKey=$GROUP_C" "$DAN_TOKEN"; expect_rejected "C: nonmember ledger read"
request GET /Group/List "$DAN_TOKEN"; expect_status 200 "C: nonmember group list"
expect_json "[.groups[].uniqueKey] | index(\"$GROUP_C\") == null" "C: nonmember list excludes the group"
pass "C: nonmembers cannot read the group, expenses, or ledger"

create_expense "$BOB_TOKEN" "$GROUP_C" "Member write" 5.00 "$BOB" "$(new_key)" "$BOB"
expect_rejected "C: ordinary member records expense"
add_member "$BOB_TOKEN" "$GROUP_C" "$DAN_PHONE"
expect_rejected "C: ordinary member adds member"
record_repayment "$BOB_TOKEN" "$GROUP_C" "$BOB" "$ALICE" 1.00 "$(new_key)"
expect_rejected "C: ordinary member records repayment"
pass "C: ordinary members cannot perform organizer-only writes"

DUPLICATE_KEY="$(new_key)"
create_expense "$ALICE_TOKEN" "$GROUP_C" "Groceries" 30.00 "$ALICE" "$DUPLICATE_KEY" "$ALICE" "$BOB" "$CARA"
expect_status 200 "C: record groceries"
GROCERIES_KEY="$(jq -r '.uniqueKey' <<<"$BODY")"
create_expense "$ALICE_TOKEN" "$GROUP_C" "Groceries" 30.00 "$ALICE" "$DUPLICATE_KEY" "$ALICE" "$BOB" "$CARA"
expect_status 200 "C: resubmit groceries"
expect_json '.replayed == true' "C: resubmission is replayed"
expense_list "$ALICE_TOKEN" "$GROUP_C"
expect_json '.expenses | length == 1' "C: duplicate submission stored once"
pass "C: a duplicate expense submission does not create a second expense"

create_expense "$ALICE_TOKEN" "$GROUP_C" "Mistake" 12.00 "$ALICE" "$(new_key)" "$ALICE" "$BOB"
expect_status 200 "C: record mistaken expense"
MISTAKE_KEY="$(jq -r '.uniqueKey' <<<"$BODY")"
request POST /Expense/Void "$ALICE_TOKEN" "$(jq -nc --arg g "$GROUP_C" --arg e "$MISTAKE_KEY" '{groupKey:$g,expenseKey:$e}')"
expect_status 200 "C: void before repayments"
expense_list "$ALICE_TOKEN" "$GROUP_C"
expect_json "[.expenses[] | select(.uniqueKey == \"$MISTAKE_KEY\")] | length == 1 and .[0].voided == true and (.[0].shares | length == 2)" \
    "C: voided expense retained with shares"
ledger "$ALICE_TOKEN" "$GROUP_C"
expect_json '.totalSpent == 30' "C: voided expense excluded from totals"
expect_balances "C: after void" "$ALICE" 20 "$BOB" -10 "$CARA" -10
pass "C: voiding before repayments keeps history and excludes the expense from balances"

record_repayment "$ALICE_TOKEN" "$GROUP_C" "$BOB" "$BOB" 1.00 "$(new_key)"
expect_rejected "C: self-repayment"
record_repayment "$ALICE_TOKEN" "$GROUP_C" "$BOB" "$ALICE" 10.01 "$(new_key)"
expect_rejected "C: overpayment"
ledger "$ALICE_TOKEN" "$GROUP_C"
expect_json '.repayments | length == 0' "C: rejected repayments wrote nothing"
pass "C: self-repayments and overpayments are rejected without writes"

# Two different submissions that each settle Cara's whole debt race; only one may be stored.
for i in 1 2; do
    curl --silent --output /dev/null --write-out '%{http_code}\n' -X POST \
        -H "Authorization: $AUTH_SCHEME $ALICE_TOKEN" -H 'Content-Type: application/json' \
        --data "$(jq -nc --arg g "$GROUP_C" --arg s "$CARA" --arg r "$ALICE" --arg k "$(new_key)" \
            '{groupKey:$g,senderKey:$s,recipientKey:$r,currency:"USD",amount:10,idempotencyKey:$k}')" \
        "$API_BASE_URL/Repayment/Record" >"$WORK_DIR/concurrent-$i" &
done
wait
CONCURRENT_STATUSES="$(cat "$WORK_DIR"/concurrent-* | sort | tr '\n' ' ')"
[[ "$CONCURRENT_STATUSES" == "200 400 " ]] || fail "C: concurrent repayments returned $CONCURRENT_STATUSES; expected one 200 and one 400"
ledger "$ALICE_TOKEN" "$GROUP_C"
expect_json '.repayments | length == 1' "C: one concurrent repayment stored"
expect_balances "C: after concurrent repayments" "$ALICE" 10 "$BOB" -10 "$CARA" 0
pass "C: concurrent repayments cannot overpay"

request POST /Expense/Void "$ALICE_TOKEN" "$(jq -nc --arg g "$GROUP_C" --arg e "$GROCERIES_KEY" '{groupKey:$g,expenseKey:$e}')"
expect_rejected "C: void after repayments"
expense_list "$ALICE_TOKEN" "$GROUP_C"
expect_json "[.expenses[] | select(.uniqueKey == \"$GROCERIES_KEY\")][0].voided == false" "C: rejected void wrote nothing"
pass "C: voiding is rejected once the group has repayments"

request DELETE /User/Delete "$BOB_TOKEN"
expect_status 409 "C: account deletion"
request PUT /Expense/Update "$ALICE_TOKEN" '{}'
expect_rejected "C: removed expense update route"
pass "C: account deletion and expense editing are explicitly unavailable"

# ---------------------------------------------------------------- Scenario A step 8
if [[ "${SKIP_RESTART:-}" == "1" ]]; then
    echo "SKIP: A8 restart check (SKIP_RESTART=1)"
else
    docker compose restart mysql webapi >/dev/null
    wait_for_api
    ALICE_TOKEN=""
    for _ in $(seq 1 30); do
        request POST /User/Authenticate "" "$(jq -nc --arg e "$ALICE_EMAIL" --arg pw "$PASSWORD" '{email:$e,password:$pw}')"
        if [[ "$STATUS" == "200" ]]; then
            ALICE_TOKEN="$(jq -r '.accessToken' <<<"$BODY")"
            break
        fi
        sleep 2
    done
    [[ -n "$ALICE_TOKEN" ]] || fail "A8: sign in after restart"
    expense_list "$ALICE_TOKEN" "$GROUP_A"
    expect_json '.expenses | length == 2' "A8: expenses retained"
    expect_json '[.expenses[] | select(.description == "Dinner") | .shares[].amount] == [30,30,30]' "A8: dinner shares retained"
    expect_json '[.expenses[] | select(.description == "Taxi") | .shares[].amount] == [15,15]' "A8: taxi shares retained"
    ledger "$ALICE_TOKEN" "$GROUP_A"
    expect_json '.totalSpent == 120 and .outstanding == 0 and (.repayments | length == 3)' "A8: ledger retained"
    expect_balances "A8" "$ALICE" 0 "$BOB" 0 "$CARA" 0
    pass "A8: after restarting MySQL and the API, expenses, shares, repayments, and zero balances remain"
fi

echo "All $CHECKS acceptance checks passed (run $RUN_ID)."
