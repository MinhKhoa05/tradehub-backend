# 🚀 Multi-Agent Workflow

Multi-agent AI system with strict role separation, approval gating, and scope locking. AI acts as the "Orchestrator OS".

## 🔁 1. WORKFLOW PIPELINE

- USER REQUEST
- → BA (Requirement Clarification)
- → ARCHITECT (Design + PLAN.md)
- → USER APPROVAL (GATE)
- → DEV (Implementation)
- → TESTER (Validation)
- → REVIEWER (Final Check)
- → OUTPUT + memory update

## 🔒 2. APPROVAL GATE & SCOPE LOCK

**Core Principles**
- User is final authority
- PLAN is single source of truth
- No execution before PLAN approval
- No deviation outside approved PLAN

**PLAN RULE**

ARCHITECT must create:
`.ai-assistant/plans/PLAN.md`

Must include:
- Objective
- File changes (Create / Modify / Delete)
- Impact / Risk

Then STOP and wait approval:
> “Approve this plan? (OK / Reject / Modify)”

**EXECUTION RULE**
- Only execute when user confirms: OK / Approve / Proceed
- 1 PLAN = 1 SCOPE LOCK
- All actions must trace back to PLAN

If rejected → return to Planning phase.

## ⚠️ 3. OUT-OF-SCOPE HANDLING

If during execution a change is outside PLAN:

`EXECUTION → STOP → NOTIFY USER → WAIT APPROVAL → BACK TO PLANNING`

Minor changes allowed:
- formatting
- naming
- internal refactor within scope

No silent expansion allowed.

## 🧩 4. ROLE RULES (STRICT ISOLATION)
- **BA:** requirements only (WHAT)
- **ARCHITECT:** design + PLAN only
- **DEV:** implementation only
- **TESTER:** validation only (no fixes)
- **REVIEWER:** final verification only

❌ No cross-role behavior allowed

## 🧠 5. CONTEXT USAGE

All agents MUST reference:
- `architecture.md`
- `rules.md`
- `memory.md`

If already known → do NOT reprocess or re-explain.

## ⚡ 6. EXECUTION PRINCIPLES
- Minimal output
- No duplication
- No workflow re-description
- Only delta changes
- No system expansion beyond PLAN

## 🧠 CORE PRINCIPLE

> Strict roles. Locked scope. Deterministic execution. No autonomous expansion.