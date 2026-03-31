-- File Script Thêm Dữ liệu Khách hàng mẫu cho Fpoly Cafe
-- Hướng dẫn: Mở SQL Server Management Studio (SSMS), chọn Database FpolyCafeDb và ấn Execute script này.

USE [FpolyCafeDb];
GO

-- Xóa dữ liệu cũ nếu muốn chạy lại nhiều lần (không bắt buộc)
-- TRUNCATE TABLE [Customers];

-- Thêm Data Mẫu
INSERT INTO [Customers] ([FullName], [PhoneNumber], [RewardPoints], [CreatedAt])
VALUES 
(N'Khách vãng lai', '0000000000', 0, GETDATE()),
(N'Nguyễn Văn A', '0987654321', 1500, GETDATE()),
(N'Trần Thị B', '0912345678', 500, GETDATE()),
(N'Lê Minh C', '0933445566', 250, GETDATE()),
(N'Hoàng Ngọc D', '0901223344', 0, GETDATE());

GO
PRINT N'[SUCCESS] Đã nhập 5 dòng dữ liệu Khách hàng mẫu thành công!';
