# 🧠 System Motivation & Goals

## 🧠 System Motivation (Game Top-up Intermediary Platform)

This system was designed to replace and improve a chat-based game top-up intermediary workflow, commonly used by small online game shops operating on commission-based or discounted top-up services.

In the traditional model, orders are handled manually via chat applications between users and admins. While flexible, this approach introduces several operational and financial risks:

- ❌ **Order loss and human error**: Orders can be forgotten or missed in high chat volume environments.
- ❌ **State inconsistency**: Users often do not have a clear understanding of whether their order is being processed, completed, or delayed.
- ❌ **Delayed fulfillment**: Some top-up requests may take hours or even a full day to process depending on admin availability, affecting user experience and in-game timing.
- ❌ **Lack of centralized tracking**: No single system exists to manage order lifecycle, pricing, or commission tracking.
- ❌ **Operational fragmentation**: Payment confirmation, order handling, and wallet updates are spread across chat messages, making auditing and reconciliation difficult.

In a discount-based intermediary top-up model, these issues become more critical because:
1. Orders are time-sensitive (users want immediate in-game value)
2. Admins act as intermediaries handling bulk transactions
3. Financial accuracy (price difference / commission margin) must be strictly controlled

---

## 🎯 System Goal

This system centralizes the entire game top-up intermediary flow with discount/commission handling into a structured backend platform.

It provides:

### 🧾 Order Management System
- Centralized order queue instead of chat-based tracking
- Explicit state transitions: `Pending` → `Paid` → `Processing` → `Completed` / `Cancelled`
- Admin dashboard capability for order assignment and processing

### 👤 User Order Flow
- Users place top-up requests through a structured API
- Real-time order status tracking instead of manual chat confirmation

### 💰 Internal Wallet System
- **Unified financial layer**: Centralized wallet system for handling deposits, payments, and withdrawals.
- **QR-based Deposit Flow**: Supports deposit requests via QR code with unique transaction identifiers for reconciliation.
- **Admin Approval Workflow**: Deposit and withdrawal requests are processed through an admin approval system to ensure correctness and fraud prevention.
- **Audit Integrity**: Maintains full transaction history with `BalanceBefore` and `BalanceAfter` for traceability and financial consistency.

### 💸 Intermediary Pricing Model

The system supports a structured pricing model designed for intermediary game top-up services, where revenue is derived from the difference between wholesale and retail pricing.

- **Discount Mechanism**: Supports configurable discount rates applied to game packages, enabling flexible pricing strategies for different user segments.
- **Margin Management**: Clearly separates input (wholesale/import cost) and output (retail/sale price) to ensure transparent profit calculation per transaction.
- **Profit Tracking**: Enables consistent monitoring of commission and profit margins across all orders, supporting sustainable business operations and financial traceability.

### ⚙️ Admin Processing Workflow
- Admin explicitly picks and processes orders
- Prevents order duplication or missed requests
- Ensures controlled execution of top-up operations

---

## 🔐 Core Design Principles

### 1. Centralized Truth
All orders and financial flows are stored in a single system instead of fragmented chat logs.

### 2. Consistent State Machine
Every order follows a strict lifecycle, eliminating ambiguous or partially processed states.

### 3. Financial Integrity
Wallet operations and order payments are designed to prevent:
- Double spending
- Inconsistent balances
- Missing transaction records

### 4. Auditability
Every financial and order-related action is fully traceable for reconciliation and debugging.

---

## 🧩 Outcome

By transforming a chat-based intermediary workflow into a structured backend system:
- Order processing becomes deterministic and trackable
- Admin workload is reduced and centralized
- Users gain transparency and trust in fulfillment status
- Financial operations become safe, auditable, and consistent under concurrency
