# GameTopUp API Reference

This document provides detailed instructions on how to integrate and use the GameTopUp system APIs for the Frontend development team.

## 1. Overview

- **Base URL (Development):** `http://localhost:5089/api`
- **Standard Response Body:** All server responses are wrapped in an `ApiResponse` object.

```json
{
  "success": true, // true if the request was successful, false otherwise
  "message": "System message", // Result or error description
  "data": { ... } // Returned data (null if not applicable)
}
```

## 2. Authentication

The system uses **JWT (JSON Web Token)** for authentication.

- **Usage:** Attach the token to the Header of every request that requires authentication.
- **Header:** `Authorization: Bearer <your_access_token>`
- **Token:** Obtained from the Login API (`POST /api/auth/login`). The token contains information such as `UserId`, `Username`, and `Role`.

## 3. Endpoint List

| Module | Method | Endpoint | Description | Auth | Role |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Auth** | POST | `/api/auth/register` | Register a new user account | No | All |
| | POST | `/api/auth/login` | Login and receive an Access Token | No | All |
| | PUT | `/api/auth/password` | Change user password | Yes | User |
| **User** | GET | `/api/users` | Get user list (paginated) | Yes | Admin |
| | GET | `/api/users/{id}` | Get detailed user information | Yes | User/Admin |
| | GET | `/api/users/me` | Get detailed user information | Yes | User |
| | PUT | `/api/users/{id}` | Update user information | Yes | User/Admin |
| | DELETE | `/api/users/{id}` | Delete user from the system | Yes | Admin |
| **Game** | GET | `/api/games` | Get list of all games | No | All |
| | GET | `/api/games/{id}` | Get detailed game information | No | All |
| | POST | `/api/games` | Create a new game | Yes | Admin |
| | PUT | `/api/games/{id}` | Update game information | Yes | Admin |
| | DELETE | `/api/games/{id}` | Delete a game | Yes | Admin |
| **Package** | GET | `/api/game-packages` | Get list of all top-up packages | No | All |
| | GET | `/api/game-packages/game/{gameId}` | Get packages by Game ID | No | All |
| | GET | `/api/game-packages/{id}` | Get package details | No | All |
| | POST | `/api/game-packages` | Create a new top-up package | Yes | Admin |
| | PUT | `/api/game-packages/{id}` | Update package information | Yes | Admin |
| | DELETE | `/api/game-packages/{id}` | Delete a package | Yes | Admin |
| **Cart** | GET | `/api/cart/items` | Get list of items in cart | Yes | User |
| | GET | `/api/cart/items/summary` | Get total quantity of items in cart | Yes | User |
| | POST | `/api/cart/items` | Add product to cart | Yes | User |
| | PUT | `/api/cart/items/{productId}` | Update item quantity | Yes | User |
| | DELETE | `/api/cart/items/{productId}` | Remove item from cart | Yes | User |
| **Wallet** | POST | `/api/wallet/active` | Activate user wallet | Yes | User |
| | GET | `/api/wallet` | Check current wallet balance | Yes | User |
| | GET | `/api/wallet/transactions` | View wallet transaction history | Yes | User |
| | POST | `/api/wallet/transactions/deposit` | Deposit money into wallet | Yes | User |
| | POST | `/api/wallet/transactions/withdraw` | Withdraw money from wallet | Yes | User |
| **Order** | POST | `/api/orders/place` | Place a new order (Stock reservation) | Yes | User |
| | POST | `/api/orders/{id}/pay` | Pay for a pending order | Yes | User |
| | GET | `/api/orders/me` | Get list of my orders | Yes | User |
| | GET | `/api/orders/{id}` | View order details | Yes | User |
| | GET | `/api/orders/{id}/history` | View order status history | Yes | User |
| | GET | `/api/orders` | List all orders (filter by status) | Yes | Admin |
| | POST | `/api/orders/{id}/pick` | Claim order for processing | Yes | Admin |
| | POST | `/api/orders/{id}/complete`| Mark order as completed | Yes | Admin |
| | POST | `/api/orders/{id}/cancel` | Cancel order & auto refund | Yes | All |


## 4. Core Workflows

### Flow 1: Registration -> Login -> Get Profile
1. **Registration:** Call `POST /api/auth/register` with user details.
2. **Login:** Call `POST /api/auth/login`. The server returns an `accessToken`.
3. **Get Profile:** 
   - Decode the JWT token on the client side to get the `UserId`.
   - Call `GET /api/users/{id}` with the `Authorization` header to retrieve details.

### Flow 2: Wallet Activation -> Deposit -> Check Balance
1. **Activation:** Call `POST /api/wallet/active` (only needs to be called once after registration).
2. **Deposit:** Call `POST /api/wallet/transactions/deposit`. Send the `Amount` (decimal) in the request body. User scans the dynamic QR code (Admin provides) to transfer.
3. **Admin Approval:** Admin verifies the bank transfer and approves the deposit (Logic handled via internal admin panel/API).
4. **Check Balance:** Call `GET /api/wallet` to see the updated balance.

### Flow 3: Place Order -> Payment -> Admin Fulfillment
1. **Place Order:** Call `POST /api/orders/place`. This reserves the stock immediately.
2. **Payment:** Call `POST /api/orders/{id}/pay`. This debits the wallet balance and marks the order as `Paid`.
3. **Admin Picking:** Admin calls `POST /api/orders/{id}/pick` to assign the order to themselves.
4. **Processing & Completion:** Admin processes the top-up and calls `POST /api/orders/{id}/complete` to finish.
5. **Cancellation (Optional):** If needed, calling `POST /api/orders/{id}/cancel` will automatically restore stock and refund the wallet (if already paid).

## 5. Important Notes
- **Request Body/DTO Details:** Please refer to the [Swagger UI](http://localhost:5089/swagger) for the exact structure of each request.
- **HTTP Status Codes:** 
  - `200 OK`: Success.
  - `201 Created`: Resource created successfully.
  - `401 Unauthorized`: Invalid or expired token.
  - `403 Forbidden`: Insufficient permissions (Admin only).
  - `400 Bad Request`: Invalid input data.
  - `404 Not Found`: Resource not found.
