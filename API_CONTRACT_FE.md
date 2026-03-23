# FpolyCafe API Contract For Frontend

Tai lieu nay tong hop cac API va endpoint hien co de team Frontend tich hop.

Base URL vi du:
- `http://localhost:5189`

Auth:
- Hầu het API can `Authorization: Bearer <token>`
- Public:
  - `POST /api/auth/login`
  - `GET /api/products`
  - `GET /api/products/{id}`
  - `GET /api/lookups/sizes`
  - `GET /api/lookups/toppings`
  - `GET /api/lookups/ingredients`

## 1. Auth

### `POST /api/auth/login`

Request:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

Response:

```json
{
  "token": "eyJhbGciOi...",
  "username": "admin",
  "fullName": "Administrator",
  "role": 0
}
```

Tai khoan seed:
- `admin/admin123`
- `manager1/manager123`
- `staff1/staff123`

## 2. Lookups Cho Order

### `GET /api/lookups/sizes`

Response:

```json
[
  {
    "sizeId": 1,
    "sizeName": "M"
  },
  {
    "sizeId": 2,
    "sizeName": "L"
  }
]
```

### `GET /api/lookups/toppings`

Response:

```json
[
  {
    "toppingId": 1,
    "toppingName": "Tran chau den",
    "price": 8000,
    "isActive": true
  }
]
```

### `GET /api/lookups/ingredients`

Response:

```json
[
  {
    "ingredientId": 1,
    "ingredientName": "Ca phe hat",
    "unit": "gram",
    "stockQuantity": 5000
  }
]
```

## 3. Categories

Endpoints:
- `GET /api/categories`
- `GET /api/categories/{id}`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`

Create request:

```json
{
  "name": "Sinh to",
  "description": "Nhom do uong xay"
}
```

Response:

```json
{
  "categoryId": 1,
  "name": "Ca Phe",
  "description": null,
  "isActive": true
}
```

## 4. Products

Public endpoints:
- `GET /api/products`
- `GET /api/products/{id}`

Response:

```json
{
  "productId": 1,
  "name": "Ca Phe Den",
  "categoryId": 1,
  "categoryName": "Ca Phe",
  "price": 25000,
  "imageUrl": null,
  "isActive": true
}
```

Admin only:
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`

Create request:

```json
{
  "name": "Matcha Latte",
  "categoryId": 2,
  "price": 42000,
  "imageUrl": "https://example.com/matcha.png"
}
```

## 5. Bills / POS

Endpoints:
- `GET /api/bills`
- `GET /api/bills/{id}`
- `POST /api/bills`
- `POST /api/bills/{id}/items`
- `PUT /api/bills/items/{billDetailId}`
- `DELETE /api/bills/items/{billDetailId}`
- `POST /api/bills/{id}/checkout`
- `POST /api/bills/{id}/cancel`

### `POST /api/bills`

Request:

```json
{
  "userId": 2
}
```

Response:

```json
1
```

### `POST /api/bills/{id}/items`

Request:

```json
{
  "productId": 1,
  "sizeId": 1,
  "toppingIds": [1],
  "quantity": 2,
  "note": "It da"
}
```

### `PUT /api/bills/items/{billDetailId}`

Request:

```json
{
  "quantity": 3,
  "note": "Khong duong"
}
```

Bill response:

```json
{
  "billId": 1,
  "orderDate": "2026-03-23T06:30:00Z",
  "totalAmount": 66000,
  "status": "Waiting",
  "customerName": "Nhan vien Ban Hang 1",
  "items": [
    {
      "billDetailId": 10,
      "productId": 1,
      "productName": "Ca Phe Den",
      "sizeId": 1,
      "sizeName": "M",
      "quantity": 2,
      "price": 25000,
      "note": "It da",
      "toppings": [
        {
          "toppingId": 1,
          "toppingName": "Tran chau den",
          "price": 8000
        }
      ]
    }
  ]
}
```

## 6. Users

Endpoints:
- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`

Create request:

```json
{
  "username": "staff2",
  "password": "staff123",
  "fullName": "Nhan vien 2",
  "email": "staff2@fpolycafe.local",
  "role": "Staff"
}
```

Response:

```json
{
  "userId": 2,
  "username": "staff1",
  "fullName": "Nhan vien Ban Hang 1",
  "email": "",
  "role": "Staff",
  "isActive": true
}
```

## 7. Attendance Cho Nhan Vien

Endpoints:
- `POST /api/attendance/check-in`
- `POST /api/attendance/break/start`
- `POST /api/attendance/break/end`
- `POST /api/attendance/check-out`
- `GET /api/attendance/me/today`
- `GET /api/attendance/me/open-shift`
- `GET /api/attendance/me/history?from=2026-03-01&to=2026-03-31`

### `POST /api/attendance/check-in`

Request:

```json
{
  "source": "Web",
  "notes": "Vao ca sang"
}
```

Response:

```json
{
  "attendanceId": 1,
  "employeeId": 2,
  "employeeName": "Nhan vien Ban Hang 1",
  "checkInTime": "2026-03-23T01:00:00Z",
  "checkOutTime": null,
  "workedMinutes": 0,
  "breakMinutes": 0,
  "overtimeMinutes": 0,
  "salaryAmount": 0,
  "status": "Working",
  "notes": "Vao ca sang",
  "breaks": []
}
```

### `POST /api/attendance/break/start`

Request:

```json
{
  "note": "Nghi an trua"
}
```

### `POST /api/attendance/break/end`

Request:

```json
{
  "note": "Quay lai lam"
}
```

### `POST /api/attendance/check-out`

Request:

```json
{
  "source": "Web",
  "notes": "Ket thuc ca"
}
```

Response:

```json
{
  "attendanceId": 1,
  "employeeId": 2,
  "employeeName": "Nhan vien Ban Hang 1",
  "checkInTime": "2026-03-23T01:00:00Z",
  "checkOutTime": "2026-03-23T10:30:00Z",
  "workedMinutes": 510,
  "breakMinutes": 30,
  "overtimeMinutes": 30,
  "salaryAmount": 221250,
  "status": "Completed",
  "notes": "Ket thuc ca",
  "breaks": [
    {
      "breakId": 1,
      "startTime": "2026-03-23T05:00:00Z",
      "endTime": "2026-03-23T05:30:00Z",
      "durationMinutes": 30,
      "status": "Completed",
      "note": "Nghi an trua"
    }
  ]
}
```

### `GET /api/attendance/me/today`

Response:

```json
{
  "currentShift": null,
  "totalWorkedMinutesToday": 510,
  "totalOvertimeMinutesToday": 30,
  "totalCompletedShiftsToday": 1
}
```

## 8. Attendance Cho Admin / Manager

Endpoints:
- `GET /api/attendance?employeeId=2&from=2026-03-01&to=2026-03-31&status=Completed`
- `PUT /api/attendance/{attendanceId}`
- `POST /api/attendance/auto-close?cutoffTime=2026-03-23T16:59:00Z`
- `GET /api/attendance/dashboard?date=2026-03-23`
- `GET /api/attendance/employee-summaries?from=2026-03-01&to=2026-03-31`

### `PUT /api/attendance/{attendanceId}`

Request:

```json
{
  "checkInTime": "2026-03-23T01:00:00Z",
  "checkOutTime": "2026-03-23T10:00:00Z",
  "reason": "Sua cong do nhan vien quen checkout",
  "notes": "Admin adjusted"
}
```

### `POST /api/attendance/auto-close`

Response:

```json
{
  "updated": 3
}
```

### `GET /api/attendance/dashboard`

Response:

```json
{
  "date": "2026-03-23T00:00:00Z",
  "activeEmployees": 2,
  "employeesOnBreak": 1,
  "completedShifts": 5,
  "missingCheckoutShifts": 1,
  "totalWorkedMinutes": 2400,
  "totalOvertimeMinutes": 180,
  "totalSalaryAmount": 1450000
}
```

### `GET /api/attendance/employee-summaries`

Response:

```json
[
  {
    "employeeId": 2,
    "employeeName": "Nhan vien Ban Hang 1",
    "shiftCount": 24,
    "workedMinutes": 11040,
    "overtimeMinutes": 420,
    "salaryAmount": 5200000
  }
]
```

## 9. Salary Rules

Endpoints:
- `GET /api/salaryrules`
- `GET /api/salaryrules/{id}`
- `POST /api/salaryrules`
- `PUT /api/salaryrules/{id}`

Response:

```json
{
  "salaryRuleId": 1,
  "employeeId": null,
  "employeeName": null,
  "role": "Staff",
  "hourlyRate": 25000,
  "overtimeRate": 35000,
  "nightShiftMultiplier": 1.2,
  "maxHoursPerShift": 12,
  "standardHoursPerShift": 8,
  "effectiveFrom": "2026-03-23T00:00:00Z",
  "isActive": true
}
```

### `POST /api/salaryrules`

Request:

```json
{
  "employeeId": null,
  "role": "Staff",
  "hourlyRate": 28000,
  "overtimeRate": 40000,
  "nightShiftMultiplier": 1.2,
  "maxHoursPerShift": 12,
  "standardHoursPerShift": 8,
  "effectiveFrom": "2026-04-01T00:00:00Z",
  "isActive": true
}
```

### `PUT /api/salaryrules/{id}`

Request:

```json
{
  "hourlyRate": 30000,
  "overtimeRate": 45000,
  "nightShiftMultiplier": 1.3,
  "maxHoursPerShift": 12,
  "standardHoursPerShift": 8,
  "effectiveFrom": "2026-04-01T00:00:00Z",
  "isActive": true
}
```

## 10. Payroll

Endpoints:
- `POST /api/payroll/generate`
- `GET /api/payroll/monthly?month=3&year=2026`
- `GET /api/payroll/{employeeId}/{year}/{month}`

### `POST /api/payroll/generate`

Request:

```json
{
  "month": 3,
  "year": 2026,
  "employeeId": null
}
```

Response:

```json
[
  {
    "payrollId": 1,
    "employeeId": 2,
    "employeeName": "Nhan vien Ban Hang 1",
    "month": 3,
    "year": 2026,
    "totalWorkedMinutes": 11040,
    "totalOvertimeMinutes": 420,
    "totalNormalSalary": 4800000,
    "totalOvertimeSalary": 400000,
    "totalSalary": 5200000,
    "status": "Generated",
    "generatedAt": "2026-03-23T07:00:00Z",
    "details": [
      {
        "attendanceId": 12,
        "checkInTime": "2026-03-12T01:00:00Z",
        "checkOutTime": "2026-03-12T10:00:00Z",
        "workedMinutes": 510,
        "overtimeMinutes": 30,
        "salaryAmount": 221250
      }
    ]
  }
]
```

## 11. Reports Kinh Doanh

Endpoints:
- `GET /api/reports/dashboard`
- `GET /api/reports/top-products?count=5`
- `GET /api/reports/revenue?days=7`

### `GET /api/reports/dashboard`

Response:

```json
{
  "totalOrders": 12,
  "totalRevenue": 345000,
  "totalProducts": 4,
  "totalUsers": 3
}
```

## 12. Reports Cham Cong Cho Do An

Endpoints:
- `GET /api/reports/attendance-dashboard?date=2026-03-23`
- `GET /api/reports/late-employees?date=2026-03-23&thresholdHour=8&thresholdMinute=15`
- `GET /api/reports/overtime-summary?from=2026-03-01&to=2026-03-31`
- `GET /api/reports/monthly-attendance-summary?month=3&year=2026`

### `GET /api/reports/attendance-dashboard`

Response:

```json
{
  "date": "2026-03-23T00:00:00Z",
  "activeEmployees": 2,
  "employeesOnBreak": 1,
  "completedShifts": 5,
  "missingCheckoutShifts": 1,
  "lateEmployees": 2,
  "totalWorkedMinutes": 2400,
  "totalOvertimeMinutes": 180,
  "totalSalaryAmount": 1450000
}
```

### `GET /api/reports/late-employees`

Response:

```json
[
  {
    "employeeId": 2,
    "employeeName": "Nhan vien Ban Hang 1",
    "checkInTime": "2026-03-23T01:30:00Z",
    "lateMinutes": 15,
    "status": "Completed"
  }
]
```

### `GET /api/reports/overtime-summary`

Response:

```json
[
  {
    "employeeId": 2,
    "employeeName": "Nhan vien Ban Hang 1",
    "shiftCount": 8,
    "totalWorkedMinutes": 4200,
    "totalOvertimeMinutes": 240,
    "totalSalaryAmount": 2100000
  }
]
```

### `GET /api/reports/monthly-attendance-summary`

Response:

```json
[
  {
    "employeeId": 2,
    "employeeName": "Nhan vien Ban Hang 1",
    "month": 3,
    "year": 2026,
    "shiftCount": 24,
    "workedMinutes": 11040,
    "overtimeMinutes": 420,
    "missingCheckoutCount": 1,
    "totalSalaryAmount": 5200000
  }
]
```

## 13. Audit Logs

### `GET /api/auditlogs?action=Attendance&entityName=Attendance&userId=1&from=2026-03-01&to=2026-03-31`

Response:

```json
[
  {
    "auditLogId": 1,
    "userId": 1,
    "userName": "Administrator",
    "action": "Attendance.Adjust",
    "entityName": "Attendance",
    "entityId": "12",
    "oldValueJson": "{\"checkInTime\":\"...\"}",
    "newValueJson": "{\"attendanceId\":12,\"employeeId\":2}",
    "createdAt": "2026-03-23T07:10:00Z",
    "ipAddress": "::1"
  }
]
```

## 14. Error Format

Backend tra loi theo format:

```json
{
  "code": "bad_request",
  "message": "Nhan vien dang co ca chua dong."
}
```

Ma loi chinh:
- `bad_request`
- `not_found`
- `unauthorized`
- `forbidden`
- `internal_error`

## 15. Khuyen Nghi Tich Hop FE

- Login xong luu `token`, `username`, `fullName`, `role`
- Man hinh cham cong:
  - goi `GET /api/attendance/me/open-shift` khi load
  - goi `GET /api/attendance/me/today` de hien thi tong gio
- Man hinh POS:
  - goi `GET /api/products`
  - goi `GET /api/lookups/sizes`
  - goi `GET /api/lookups/toppings`
- Man hinh admin:
  - dashboard: `GET /api/reports/attendance-dashboard`
  - di tre: `GET /api/reports/late-employees`
  - OT: `GET /api/reports/overtime-summary`
  - payroll: `GET /api/payroll/monthly`

