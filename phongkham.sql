-- =======================================================
-- BƯỚC 1: TẠO CSDL VÀ THIẾT LẬP CƠ BẢN
-- =======================================================

CREATE DATABASE QuanLyPhongKham999;
GO
USE QuanLyPhongKham999;
GO

-- =======================================================
-- BƯỚC 2: CÁC BẢNG HỆ THỐNG VÀ DANH MỤC
-- =======================================================

-- Bảng Accounts (Tài khoản hệ thống - Dùng cho RBAC và Audit)
CREATE TABLE Accounts (
    AccountID INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(256) NOT NULL,
    Role NVARCHAR(50) NOT NULL, -- Admin, Lễ tân (Receptionist), Bác sĩ (Doctor), Dược sĩ (Pharmacist), Bệnh nhân (Patient)
    IsActive BIT DEFAULT 1, -- Dùng để khóa/mở khóa tài khoản
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Ràng buộc CHECK Role (Sử dụng tên tiếng Anh để khớp với backend)
ALTER TABLE Accounts
ADD CONSTRAINT CK_Accounts_Role_New
CHECK (Role IN ('Admin', 'Receptionist', 'Doctor', 'Pharmacist', 'Patient'));

-- Bảng AuditLogs (Phục vụ NFR: Audit truy cập/thay đổi)
CREATE TABLE AuditLogs (
    LogID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME DEFAULT GETDATE(),
    AccountID INT NULL,
    ActionType NVARCHAR(50) NOT NULL, -- Ví dụ: CREATE, UPDATE, DELETE, VIEW
    TableName NVARCHAR(100) NOT NULL,
    RecordID INT NULL, -- ID của bản ghi bị ảnh hưởng
    Details NVARCHAR(MAX),
    FOREIGN KEY (AccountID) REFERENCES Accounts(AccountID)
);

-- Bảng Specialties (Chuyên khoa)
CREATE TABLE Specialties (
    SpecialtyID INT IDENTITY(1,1) PRIMARY KEY,
    SpecialtyName NVARCHAR(100) UNIQUE NOT NULL,
    IsDeleted BIT DEFAULT 0
);

-- Bảng Drugs (Thuốc)
CREATE TABLE Drugs (
    DrugID INT IDENTITY(1,1) PRIMARY KEY,
    DrugName NVARCHAR(100) NOT NULL,
    Unit NVARCHAR(50),
    Price DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedByAccountID INT NULL,
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (CreatedByAccountID) REFERENCES Accounts(AccountID)
);

-- Bảng tồn kho thuốc
CREATE TABLE Drug_Stocks (
    StockID INT IDENTITY(1,1) PRIMARY KEY,
    DrugID INT NOT NULL UNIQUE,
    QuantityAvailable INT NOT NULL CHECK (QuantityAvailable >= 0),
    LastUpdated DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (DrugID) REFERENCES Drugs(DrugID)
);

-- =======================================================
-- BƯỚC 3: CÁC BẢNG NGHIỆP VỤ CHÍNH
-- =======================================================

-- Bảng Patients (Bệnh nhân)
CREATE TABLE Patients (
    PatientID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    DOB DATETIME NULL,
    Gender NVARCHAR(10),
    -- NFR: Bảo mật PII (Masking)
    Phone VARCHAR(15) MASKED WITH (FUNCTION = 'partial(0,"***-***",4)'),
    Email VARCHAR(100) MASKED WITH (FUNCTION = 'email()'),
    Address NVARCHAR(200),
    MedicalHistory NVARCHAR(MAX),
    AccountID INT NULL UNIQUE, -- Liên kết với tài khoản hệ thống (cho bệnh nhân tự đăng nhập)
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (AccountID) REFERENCES Accounts(AccountID)
);

-- Bảng Doctors (Bác sĩ)
CREATE TABLE Doctors (
    DoctorID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    SpecialtyID INT,
    Phone VARCHAR(15),
    Email VARCHAR(100),
    AccountID INT NULL UNIQUE, -- Liên kết với tài khoản hệ thống
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (SpecialtyID) REFERENCES Specialties(SpecialtyID),
    FOREIGN KEY (AccountID) REFERENCES Accounts(AccountID)
);

-- Bảng Appointments (Lịch hẹn)
CREATE TABLE Appointments (
    AppointmentID INT IDENTITY(1,1) PRIMARY KEY,
    PatientID INT NOT NULL,
    DoctorID INT NOT NULL,
    AppointmentDate DATETIME NOT NULL,
    Status NVARCHAR(50) DEFAULT N'Đã đặt', -- Đã đặt, Đã hủy, Đã khám
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedByAccountID INT NULL, -- Ai là người tạo lịch hẹn (Lễ tân/Bệnh nhân)
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (CreatedByAccountID) REFERENCES Accounts(AccountID)
);

-- Bảng Encounters (Lần khám)
CREATE TABLE Encounters (
    EncounterID INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentID INT NOT NULL UNIQUE, -- Mỗi cuộc hẹn chỉ có 1 lần khám
    DoctorID INT NOT NULL,
    ExaminationNotes NVARCHAR(MAX), -- Ghi chú chung về quá trình khám
    EncounterDate DATETIME DEFAULT GETDATE(),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedByAccountID INT NULL, -- Bác sĩ thực hiện khám
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (CreatedByAccountID) REFERENCES Accounts(AccountID)
);

-- Bảng Diagnoses (Chẩn đoán)
CREATE TABLE Diagnoses (
    DiagnosisID INT IDENTITY(1,1) PRIMARY KEY,
    EncounterID INT NOT NULL,
    ICDCode VARCHAR(20) NULL, -- Mã ICD-10 (tùy chọn)
    Description NVARCHAR(MAX) NOT NULL,
    ResultFile VARBINARY(MAX) NULL, -- Lưu file kết quả (base64 hoặc đường dẫn file - VARBINARY(MAX) mô phỏng lưu file nhỏ)
    FOREIGN KEY (EncounterID) REFERENCES Encounters(EncounterID)
);

-- Bảng Prescriptions (Đơn thuốc)
CREATE TABLE Prescriptions (
    PrescriptionID INT IDENTITY(1,1) PRIMARY KEY,
    EncounterID INT NOT NULL,
    DrugID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    Usage NVARCHAR(200), -- Liều dùng/cách dùng
    FOREIGN KEY (EncounterID) REFERENCES Encounters(EncounterID),
    FOREIGN KEY (DrugID) REFERENCES Drugs(DrugID)
);

-- Bảng Invoices (Hóa đơn)
CREATE TABLE Invoices (
    InvoiceID INT IDENTITY(1,1) PRIMARY KEY,
    PatientID INT NOT NULL,
    EncounterID INT NOT NULL UNIQUE,
    ServiceFee DECIMAL(18,2) DEFAULT 0,
    DrugFee DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(50) NULL,
    Status NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedByAccountID INT NULL,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (EncounterID) REFERENCES Encounters(EncounterID),
    FOREIGN KEY (CreatedByAccountID) REFERENCES Accounts(AccountID)
);

-- =======================================================
-- BƯỚC 4: STORED PROCEDURES (Tập trung vào logic phức tạp)
-- =======================================================

-- 1. SP: Đăng ký tài khoản (Dùng cho Lễ tân, Bác sĩ, Dược sĩ, Bệnh nhân tự đăng ký)
-- Lưu ý: Tầng BLL sẽ tự tính PasswordHash
IF OBJECT_ID('sp_account_register', 'P') IS NOT NULL DROP PROCEDURE sp_account_register;
GO
CREATE PROCEDURE sp_account_register
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(256),
    @Role NVARCHAR(50),
    @NewAccountID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM Accounts WHERE Username = @Username)
    BEGIN
        RAISERROR(N'Tên đăng nhập đã tồn tại.', 16, 1);
        RETURN;
    END

    INSERT INTO Accounts (Username, PasswordHash, Role)
    VALUES (@Username, @PasswordHash, @Role);

    SET @NewAccountID = SCOPE_IDENTITY();
END;
GO

-- 2. SP: Tạo hồ sơ Bệnh nhân (có liên kết tài khoản)
IF OBJECT_ID('sp_patient_create_with_account', 'P') IS NOT NULL DROP PROCEDURE sp_patient_create_with_account;
GO
CREATE PROCEDURE sp_patient_create_with_account
    @FullName NVARCHAR(100),
    @DOB DATETIME = NULL,
    @Gender NVARCHAR(10) = NULL,
    @Phone VARCHAR(15) = NULL,
    @Email VARCHAR(100) = NULL,
    @Address NVARCHAR(200) = NULL,
    @MedicalHistory NVARCHAR(MAX) = NULL,
    @AccountID INT = NULL,
    @NewPatientID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Patients (FullName, DOB, Gender, Phone, Email, Address, MedicalHistory, AccountID)
    VALUES (@FullName, @DOB, @Gender, @Phone, @Email, @Address, @MedicalHistory, @AccountID);

    SET @NewPatientID = SCOPE_IDENTITY();
END;
GO

-- 3. SP: Hoàn tất Lần khám (Logic nghiệp vụ phức tạp - TRANSACTION)
-- Đây là SP mô tả logic 3. Khám bệnh: chẩn đoán, chỉ định, đơn thuốc, và tính phí dịch vụ.
-- *Chú ý: Để đơn giản, các tham số PrescriptionList và DiagnosisList sẽ được xử lý bằng code .NET.
-- Trong môi trường thực tế, SP sẽ nhận vào các bảng TVP (Table-Valued Parameters) cho đơn thuốc.
IF OBJECT_ID('sp_encounter_complete', 'P') IS NOT NULL DROP PROCEDURE sp_encounter_complete;
GO
CREATE PROCEDURE sp_encounter_complete
    @AppointmentID INT,
    @DoctorID INT,
    @ExaminationNotes NVARCHAR(MAX),
    @DiagnosisDescription NVARCHAR(MAX),
    @ServiceFee DECIMAL(18,2),
    @CurrentUserID INT,
    @NewEncounterID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @PatientID INT;
    DECLARE @TotalAmount DECIMAL(18,2) = @ServiceFee;
    DECLARE @DrugFee DECIMAL(18,2) = 0; -- Tính toán sau khi kê đơn

    -- Kiểm tra cuộc hẹn và lấy PatientID
    SELECT @PatientID = PatientID FROM Appointments WHERE AppointmentID = @AppointmentID AND Status = N'Đã đặt';

    IF @PatientID IS NULL
    BEGIN
        RAISERROR(N'Cuộc hẹn không hợp lệ hoặc đã hoàn tất.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION
    
    BEGIN TRY
        -- 1. Tạo Encounter (Lần khám)
        INSERT INTO Encounters (AppointmentID, DoctorID, ExaminationNotes, CreatedByAccountID)
        VALUES (@AppointmentID, @DoctorID, @ExaminationNotes, @CurrentUserID);
        SET @NewEncounterID = SCOPE_IDENTITY();

        -- 2. Tạo Diagnosis (Chẩn đoán)
        INSERT INTO Diagnoses (EncounterID, Description)
        VALUES (@NewEncounterID, @DiagnosisDescription);
        
        -- Cập nhật trạng thái cuộc hẹn
        UPDATE Appointments SET Status = N'Đã khám' WHERE AppointmentID = @AppointmentID;
        
        -- 3. Ghi Audit Log (Lễ tân/Bác sĩ)
        INSERT INTO AuditLogs (AccountID, ActionType, TableName, RecordID, Details)
        VALUES (@CurrentUserID, 'CREATE', 'Encounters', @NewEncounterID, N'Hoàn tất lần khám.');

        -- 4. Tạo Invoice (Hóa đơn) - Tạm thời chỉ bao gồm phí dịch vụ, phí thuốc sẽ tính ở tầng ứng dụng hoặc SP riêng
        INSERT INTO Invoices (PatientID, EncounterID, ServiceFee, DrugFee, TotalAmount, CreatedByAccountID)
        VALUES (@PatientID, @NewEncounterID, @ServiceFee, @DrugFee, @TotalAmount, @CurrentUserID);

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        -- Ghi lỗi vào log nếu cần
        THROW;
        RETURN;
    END CATCH
END;
GO

-- 4. SP: Ghi lại hành động Audit
IF OBJECT_ID('sp_audit_log_record', 'P') IS NOT NULL DROP PROCEDURE sp_audit_log_record;
GO
CREATE PROCEDURE sp_audit_log_record
    @AccountID INT,
    @ActionType NVARCHAR(50),
    @TableName NVARCHAR(100),
    @RecordID INT = NULL,
    @Details NVARCHAR(MAX) = NULL
AS
BEGIN
    INSERT INTO AuditLogs (AccountID, ActionType, TableName, RecordID, Details)
    VALUES (@AccountID, @ActionType, @TableName, @RecordID, @Details);
END;
GO

-- 5. SP: Lấy báo cáo doanh thu theo Bác sĩ (Mô phỏng chức năng Báo cáo)
IF OBJECT_ID('sp_report_revenue_by_doctor', 'P') IS NOT NULL DROP PROCEDURE sp_report_revenue_by_doctor;
GO
CREATE PROCEDURE sp_report_revenue_by_doctor
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        D.DoctorID,
        D.FullName AS DoctorName,
        S.SpecialtyName,
        COUNT(E.EncounterID) AS TotalEncounters,
        SUM(I.TotalAmount) AS TotalRevenue,
        CAST(AVG(I.TotalAmount) AS DECIMAL(18, 2)) AS AverageFee
    FROM Doctors D
    INNER JOIN Encounters E ON D.DoctorID = E.DoctorID
    INNER JOIN Invoices I ON E.EncounterID = I.EncounterID
    LEFT JOIN Specialties S ON D.SpecialtyID = S.SpecialtyID
    WHERE I.PaymentDate >= @StartDate AND I.PaymentDate <= @EndDate
      AND I.Status = N'Đã thanh toán'
    GROUP BY D.DoctorID, D.FullName, S.SpecialtyName
    ORDER BY TotalRevenue DESC;
END;
GO


-- =======================================================
-- BƯỚC 1: DROP CÁC BẢNG VÀ SP CŨ CÓ LIÊN QUAN
-- =======================================================

-- Xóa các bảng liên quan đến Encounter, Prescription, Invoice, Diagnosis trước để sửa cấu trúc
IF OBJECT_ID('Invoices', 'U') IS NOT NULL ALTER TABLE Invoices DROP CONSTRAINT FK_Invoices_EncounterID;
IF OBJECT_ID('Diagnoses', 'U') IS NOT NULL DROP TABLE Diagnoses;
IF OBJECT_ID('Prescriptions', 'U') IS NOT NULL DROP TABLE Prescriptions;
IF OBJECT_ID('Invoices', 'U') IS NOT NULL DROP TABLE Invoices;
IF OBJECT_ID('Encounters', 'U') IS NOT NULL DROP TABLE Encounters;

-- Xóa SP cũ
IF OBJECT_ID('sp_encounter_complete', 'P') IS NOT NULL DROP PROCEDURE sp_encounter_complete;

-- =======================================================
-- BƯỚC 2: CÁC BẢNG NGHIỆP VỤ (SỬA LẠI PRESCRIPTIONS)
-- =======================================================

-- Bảng Encounters (Lần khám) - KHÔNG ĐỔI
CREATE TABLE Encounters (
    EncounterID INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentID INT NOT NULL UNIQUE, -- Mỗi cuộc hẹn chỉ có 1 lần khám
    DoctorID INT NOT NULL,
    ExaminationNotes NVARCHAR(MAX), -- Ghi chú chung về quá trình khám
    EncounterDate DATETIME DEFAULT GETDATE(),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedByAccountID INT NULL, -- Bác sĩ thực hiện khám
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (CreatedByAccountID) REFERENCES Accounts(AccountID)
);

-- Bảng Diagnoses (Chẩn đoán) - KHÔNG ĐỔI
CREATE TABLE Diagnoses (
    DiagnosisID INT IDENTITY(1,1) PRIMARY KEY,
    EncounterID INT NOT NULL,
    ICDCode VARCHAR(20) NULL,
    Description NVARCHAR(MAX) NOT NULL,
    ResultFile VARBINARY(MAX) NULL,
    FOREIGN KEY (EncounterID) REFERENCES Encounters(EncounterID)
);

-- 🟢 Bảng Prescriptions (Đơn thuốc) - Bảng CHUNG cho một lần kê đơn
CREATE TABLE Prescriptions (
    PrescriptionID INT IDENTITY(1,1) PRIMARY KEY,
    EncounterID INT NOT NULL UNIQUE, -- Mỗi lần khám có 1 đơn thuốc
    PrescriptionNotes NVARCHAR(MAX) NULL, -- Ghi chú chung của đơn thuốc
    Dispensed BIT DEFAULT 0, -- Trạng thái: Đã cấp/Chưa cấp phát (dược sĩ)
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (EncounterID) REFERENCES Encounters(EncounterID)
);

-- 🟢 Bảng PrescriptionItems (Chi tiết Thuốc trong đơn)
CREATE TABLE PrescriptionItems (
    PrescriptionItemID INT IDENTITY(1,1) PRIMARY KEY,
    PrescriptionID INT NOT NULL,
    DrugID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    Usage NVARCHAR(200), -- Liều dùng/cách dùng
    FOREIGN KEY (PrescriptionID) REFERENCES Prescriptions(PrescriptionID),
    FOREIGN KEY (DrugID) REFERENCES Drugs(DrugID)
);

-- Bảng Invoices (Hóa đơn) - SỬA FOREIGN KEY LẠI
CREATE TABLE Invoices (
    InvoiceID INT IDENTITY(1,1) PRIMARY KEY,
    PatientID INT NOT NULL,
    EncounterID INT NOT NULL UNIQUE,
    ServiceFee DECIMAL(18,2) DEFAULT 0,
    DrugFee DECIMAL(18,2) DEFAULT 0, -- Phí thuốc sẽ được tính toán chính xác
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(50) NULL,
    Status NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedByAccountID INT NULL,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (EncounterID) REFERENCES Encounters(EncounterID),
    FOREIGN KEY (CreatedByAccountID) REFERENCES Accounts(AccountID)
);

-- =======================================================
-- BƯỚC 3: USER DEFINED TABLE TYPE (Dùng cho TVP)
-- =======================================================

-- Xóa nếu đã tồn tại
IF TYPE_ID(N'tt_PrescriptionItems') IS NOT NULL DROP TYPE tt_PrescriptionItems;
GO

-- 🟢 Tạo Table Type: Cấu trúc để nhận list thuốc từ C#
CREATE TYPE tt_PrescriptionItems AS TABLE
(
    DrugID INT NOT NULL,
    Quantity INT NOT NULL,
    Usage NVARCHAR(200) NULL
);
GO

-- =======================================================
-- BƯỚC 4: STORED PROCEDURE (Hoàn chỉnh Encounter)
-- =======================================================

-- 🟢 SP: Hoàn tất Lần khám (Bao gồm chẩn đoán, kê đơn, trừ kho, và tạo hóa đơn)
IF OBJECT_ID('sp_encounter_complete_v2', 'P') IS NOT NULL DROP PROCEDURE sp_encounter_complete_v2;
GO
CREATE PROCEDURE sp_encounter_complete_v2
    @AppointmentID INT,
    @DoctorID INT,
    @ExaminationNotes NVARCHAR(MAX),
    @DiagnosisDescription NVARCHAR(MAX),
    @ServiceFee DECIMAL(18,2),
    @PrescriptionList tt_PrescriptionItems READONLY, -- Tham số TVP
    @CurrentUserID INT,
    @NewEncounterID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PatientID INT;
    DECLARE @DrugFee DECIMAL(18,2) = 0;
    DECLARE @NewPrescriptionID INT;

    -- Kiểm tra cuộc hẹn và lấy PatientID
    SELECT @PatientID = PatientID 
    FROM Appointments 
    WHERE AppointmentID = @AppointmentID AND Status = N'Đã đặt';

    IF @PatientID IS NULL
    BEGIN
        RAISERROR(N'Cuộc hẹn không hợp lệ hoặc đã hoàn tất.', 16, 1);
        RETURN;
    END

    -- 1. Tính tổng phí thuốc và kiểm tra tồn kho trước khi Transaction
    SELECT @DrugFee = SUM(T.Quantity * D.Price)
    FROM @PrescriptionList T
    JOIN Drugs D ON T.DrugID = D.DrugID;

    IF @DrugFee IS NULL SET @DrugFee = 0;
    
    -- Kiểm tra tồn kho (Nên được làm ở BLL nhưng kiểm tra kép ở DB là tốt)
    IF EXISTS (
        SELECT T.DrugID
        FROM @PrescriptionList T
        JOIN Drug_Stocks DS ON T.DrugID = DS.DrugID
        WHERE T.Quantity > DS.QuantityAvailable
    )
    BEGIN
        RAISERROR(N'Thiếu tồn kho cho một số loại thuốc. Vui lòng kiểm tra lại.', 16, 1);
        RETURN;
    END

    -- Bắt đầu giao dịch
    BEGIN TRANSACTION
    
    BEGIN TRY
        -- 1. Tạo Encounter (Lần khám)
        INSERT INTO Encounters (AppointmentID, DoctorID, ExaminationNotes, CreatedByAccountID)
        VALUES (@AppointmentID, @DoctorID, @ExaminationNotes, @CurrentUserID);
        SET @NewEncounterID = SCOPE_IDENTITY();

        -- 2. Tạo Diagnosis (Chẩn đoán)
        INSERT INTO Diagnoses (EncounterID, Description)
        VALUES (@NewEncounterID, @DiagnosisDescription);
        
        -- 3. Cập nhật trạng thái cuộc hẹn
        UPDATE Appointments SET Status = N'Đã khám' WHERE AppointmentID = @AppointmentID;
        
        -- 4. Kê Đơn Thuốc
        IF EXISTS (SELECT 1 FROM @PrescriptionList)
        BEGIN
            -- Tạo đơn thuốc chung
            INSERT INTO Prescriptions (EncounterID) VALUES (@NewEncounterID);
            SET @NewPrescriptionID = SCOPE_IDENTITY();

            -- Chèn các mục thuốc chi tiết
            INSERT INTO PrescriptionItems (PrescriptionID, DrugID, Quantity, Usage)
            SELECT @NewPrescriptionID, DrugID, Quantity, Usage FROM @PrescriptionList;

            -- 5. Trừ Tồn Kho
            UPDATE DS
            SET QuantityAvailable = DS.QuantityAvailable - PL.Quantity,
                LastUpdated = GETDATE()
            FROM Drug_Stocks DS
            JOIN @PrescriptionList PL ON DS.DrugID = PL.DrugID;
        END

        -- 6. Tạo Invoice (Hóa đơn)
        DECLARE @TotalAmount DECIMAL(18,2) = @ServiceFee + @DrugFee;
        
        INSERT INTO Invoices (PatientID, EncounterID, ServiceFee, DrugFee, TotalAmount, CreatedByAccountID)
        VALUES (@PatientID, @NewEncounterID, @ServiceFee, @DrugFee, @TotalAmount, @CurrentUserID);

        -- 7. Ghi Audit Log
        INSERT INTO AuditLogs (AccountID, ActionType, TableName, RecordID, Details)
        VALUES (@CurrentUserID, 'CREATE', 'Encounters', @NewEncounterID, N'Hoàn tất lần khám, Phí thuốc: ' + CAST(@DrugFee AS NVARCHAR));

        COMMIT TRANSACTION
    END TRY
  BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION; -- Thêm dấu chấm phẩy cho chắc chắn
    
    -- Ghi lỗi và throw lại
    THROW;

    -- THÊM DÒNG NÀY VÀO
    RETURN; 
END CATCH
END;
GO

-- 6. Xem tất cả tài khoản
SELECT * FROM Accounts;
GO

-- 7. Xem tất cả bệnh nhân (Quan sát PII masking)
SELECT * FROM Patients;
GO

SELECT * FROM Doctors ;
SELECT AccountID, Username FROM Accounts WHERE Role = 'Doctor';
SELECT DoctorID, FullName FROM Doctors;

-- 8. Xem tất cả chuyên khoa
SELECT * FROM Specialties;
GO

-- 9. Xem tất cả thuốc
SELECT * FROM Drugs;
GO

-- 10. Xem tất cả lịch hẹn
SELECT * FROM Appointments;
GO


-- Gán biến cho các ID quan trọng để sử dụng sau này
DECLARE @DoctorAccountID INT;
DECLARE @PatientAccountID INT;
DECLARE @DoctorID INT;
DECLARE @PatientID INT;
DECLARE @DrugID_1 INT;
DECLARE @DrugID_2 INT;
DECLARE @AppointmentID INT;

-- 1. TẠO TÀI KHOẢN BÁC SĨ (Dùng để lấy JWT Token)
-- Password: password123 (Giả định đã hash)
IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'dr_test')
BEGIN
    INSERT INTO Accounts (Username, PasswordHash, Role)
    VALUES ('dr_test', '$2a$11$q9v0p8y0d6n9Z9q2k6Z7q.m', 'Doctor'); -- Hash mẫu
    SET @DoctorAccountID = SCOPE_IDENTITY();
END
ELSE
    SELECT @DoctorAccountID = AccountID FROM Accounts WHERE Username = 'dr_test';

-- 2. TẠO HỒ SƠ BỆNH NHÂN VÀ BÁC SĨ (Liên kết với Account)
IF NOT EXISTS (SELECT 1 FROM Patients WHERE FullName = N'Nguyễn Văn A')
BEGIN
    INSERT INTO Patients (FullName, AccountID) VALUES (N'Nguyễn Văn A', NULL); -- Không cần Account tạm thời
    SET @PatientID = SCOPE_IDENTITY();
END
ELSE
    SELECT @PatientID = PatientID FROM Patients WHERE FullName = N'Nguyễn Văn A';

IF NOT EXISTS (SELECT 1 FROM Doctors WHERE FullName = N'Bác Sĩ Test')
BEGIN
    INSERT INTO Doctors (FullName, AccountID) VALUES (N'Bác Sĩ Test', @DoctorAccountID);
    SET @DoctorID = SCOPE_IDENTITY();
END
ELSE
    SELECT @DoctorID = DoctorID FROM Doctors WHERE FullName = N'Bác Sĩ Test';


-- 3. TẠO THUỐC VÀ TỒN KHO
-- Drug 1: Paracetamol, Giá 10000, Tồn kho 100
IF NOT EXISTS (SELECT 1 FROM Drugs WHERE DrugName = N'Paracetamol 500mg')
BEGIN
    INSERT INTO Drugs (DrugName, Unit, Price) VALUES (N'Paracetamol 500mg', N'Viên', 10000.00);
    SET @DrugID_1 = SCOPE_IDENTITY();
    INSERT INTO Drug_Stocks (DrugID, QuantityAvailable) VALUES (@DrugID_1, 100);
END
ELSE
    SELECT @DrugID_1 = DrugID FROM Drugs WHERE DrugName = N'Paracetamol 500mg';

-- Drug 2: Kháng sinh, Giá 50000, Tồn kho 50
IF NOT EXISTS (SELECT 1 FROM Drugs WHERE DrugName = N'Kháng Sinh ABC')
BEGIN
    INSERT INTO Drugs (DrugName, Unit, Price) VALUES (N'Kháng Sinh ABC', N'Viên', 50000.00);
    SET @DrugID_2 = SCOPE_IDENTITY();
    INSERT INTO Drug_Stocks (DrugID, QuantityAvailable) VALUES (@DrugID_2, 50);
END
ELSE
    SELECT @DrugID_2 = DrugID FROM Drugs WHERE DrugName = N'Kháng Sinh ABC';


-- 4. TẠO LỊCH HẸN (Ở trạng thái Đã đặt)
IF NOT EXISTS (SELECT 1 FROM Appointments WHERE PatientID = @PatientID AND DoctorID = @DoctorID AND Status = N'Đã đặt')
BEGIN
    INSERT INTO Appointments (PatientID, DoctorID, AppointmentDate, Status)
    VALUES (@PatientID, @DoctorID, GETDATE(), N'Đã đặt');
    SET @AppointmentID = SCOPE_IDENTITY();
END
ELSE
    SELECT @AppointmentID = AppointmentID FROM Appointments WHERE PatientID = @PatientID AND DoctorID = @DoctorID AND Status = N'Đã đặt';


-- XUẤT ID CẦN THIẾT CHO POSTMAN
SELECT 
    @DoctorAccountID AS DoctorAccountID, 
    @AppointmentID AS AppointmentID, 
    @DrugID_1 AS DrugID_1, 
    @DrugID_2 AS DrugID_2;
GO
