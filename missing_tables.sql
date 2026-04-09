BEGIN TRANSACTION;
CREATE TABLE [Attendances] (
    [AttendanceId] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [CheckInTime] datetime2 NOT NULL,
    [CheckOutTime] datetime2 NULL,
    [WorkedMinutes] int NOT NULL,
    [BreakMinutes] int NOT NULL,
    [OvertimeMinutes] int NOT NULL,
    [SalaryAmount] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [CheckInSource] nvarchar(50) NULL,
    [CheckOutSource] nvarchar(50) NULL,
    [CheckInIp] nvarchar(50) NULL,
    [CheckOutIp] nvarchar(50) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Attendances] PRIMARY KEY ([AttendanceId]),
    CONSTRAINT [FK_Attendances_Users_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [AuditLogs] (
    [AuditLogId] int NOT NULL IDENTITY,
    [UserId] int NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityName] nvarchar(100) NOT NULL,
    [EntityId] nvarchar(50) NOT NULL,
    [OldValueJson] nvarchar(max) NULL,
    [NewValueJson] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IpAddress] nvarchar(50) NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([AuditLogId]),
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);

CREATE TABLE [MonthlyPayrolls] (
    [PayrollId] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Month] int NOT NULL,
    [Year] int NOT NULL,
    [TotalWorkedMinutes] int NOT NULL,
    [TotalOvertimeMinutes] int NOT NULL,
    [TotalNormalSalary] decimal(18,2) NOT NULL,
    [TotalOvertimeSalary] decimal(18,2) NOT NULL,
    [TotalSalary] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [GeneratedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_MonthlyPayrolls] PRIMARY KEY ([PayrollId]),
    CONSTRAINT [FK_MonthlyPayrolls_Users_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [SalaryRules] (
    [SalaryRuleId] int NOT NULL IDENTITY,
    [EmployeeId] int NULL,
    [Role] int NULL,
    [HourlyRate] decimal(18,2) NOT NULL,
    [OvertimeRate] decimal(18,2) NOT NULL,
    [NightShiftMultiplier] decimal(18,2) NOT NULL,
    [MaxHoursPerShift] int NOT NULL,
    [StandardHoursPerShift] int NOT NULL,
    [EffectiveFrom] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_SalaryRules] PRIMARY KEY ([SalaryRuleId]),
    CONSTRAINT [FK_SalaryRules_Users_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [AttendanceAdjustments] (
    [AdjustmentId] int NOT NULL IDENTITY,
    [AttendanceId] int NOT NULL,
    [AdjustedByUserId] int NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [OldCheckInTime] datetime2 NULL,
    [NewCheckInTime] datetime2 NULL,
    [OldCheckOutTime] datetime2 NULL,
    [NewCheckOutTime] datetime2 NULL,
    [OldWorkedMinutes] int NOT NULL,
    [NewWorkedMinutes] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AttendanceAdjustments] PRIMARY KEY ([AdjustmentId]),
    CONSTRAINT [FK_AttendanceAdjustments_Attendances_AttendanceId] FOREIGN KEY ([AttendanceId]) REFERENCES [Attendances] ([AttendanceId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AttendanceAdjustments_Users_AdjustedByUserId] FOREIGN KEY ([AdjustedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [AttendanceBreaks] (
    [BreakId] int NOT NULL IDENTITY,
    [AttendanceId] int NOT NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NULL,
    [DurationMinutes] int NOT NULL,
    [Status] int NOT NULL,
    [Note] nvarchar(250) NULL,
    CONSTRAINT [PK_AttendanceBreaks] PRIMARY KEY ([BreakId]),
    CONSTRAINT [FK_AttendanceBreaks_Attendances_AttendanceId] FOREIGN KEY ([AttendanceId]) REFERENCES [Attendances] ([AttendanceId]) ON DELETE CASCADE
);

CREATE TABLE [MonthlyPayrollDetails] (
    [PayrollDetailId] int NOT NULL IDENTITY,
    [PayrollId] int NOT NULL,
    [AttendanceId] int NOT NULL,
    [WorkedMinutes] int NOT NULL,
    [OvertimeMinutes] int NOT NULL,
    [SalaryAmount] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_MonthlyPayrollDetails] PRIMARY KEY ([PayrollDetailId]),
    CONSTRAINT [FK_MonthlyPayrollDetails_Attendances_AttendanceId] FOREIGN KEY ([AttendanceId]) REFERENCES [Attendances] ([AttendanceId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MonthlyPayrollDetails_MonthlyPayrolls_PayrollId] FOREIGN KEY ([PayrollId]) REFERENCES [MonthlyPayrolls] ([PayrollId]) ON DELETE CASCADE
);

CREATE INDEX [IX_AttendanceAdjustments_AdjustedByUserId] ON [AttendanceAdjustments] ([AdjustedByUserId]);

CREATE INDEX [IX_AttendanceAdjustments_AttendanceId] ON [AttendanceAdjustments] ([AttendanceId]);

CREATE INDEX [IX_AttendanceBreaks_AttendanceId] ON [AttendanceBreaks] ([AttendanceId]);

CREATE INDEX [IX_Attendances_EmployeeId] ON [Attendances] ([EmployeeId]);

CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);

CREATE INDEX [IX_MonthlyPayrollDetails_AttendanceId] ON [MonthlyPayrollDetails] ([AttendanceId]);

CREATE INDEX [IX_MonthlyPayrollDetails_PayrollId] ON [MonthlyPayrollDetails] ([PayrollId]);

CREATE UNIQUE INDEX [IX_MonthlyPayrolls_EmployeeId_Month_Year] ON [MonthlyPayrolls] ([EmployeeId], [Month], [Year]);

CREATE INDEX [IX_SalaryRules_EmployeeId] ON [SalaryRules] ([EmployeeId]);

COMMIT;
GO

BEGIN TRANSACTION;
COMMIT;
GO
