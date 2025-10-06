-- Tạo CSDL
CREATE DATABASE QuanLyPhongKham;
GO
USE QuanLyPhongKham;
GO

-- ===============================
-- Bảng Patients (Bệnh nhân)
-- ===============================
CREATE TABLE Patients (
    PatientID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    DOB DATE,
    Gender NVARCHAR(10),
    Phone VARCHAR(15) MASKED WITH (FUNCTION = 'partial(0,"***-***-",3)'), -- Masking số điện thoại
    Email VARCHAR(100) MASKED WITH (FUNCTION = 'email()'), -- Masking email
    Address NVARCHAR(200),
    MedicalHistory NVARCHAR(MAX)
);

-- ===============================
-- Bảng Specialties (Chuyên khoa)
-- ===============================
CREATE TABLE Specialties (
    SpecialtyID INT IDENTITY(1,1) PRIMARY KEY,
    SpecialtyName NVARCHAR(100) UNIQUE
);

-- ===============================
-- Bảng Doctors (Bác sĩ)
-- ===============================
CREATE TABLE Doctors (
    DoctorID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    SpecialtyID INT,
    Phone VARCHAR(15),
    Email VARCHAR(100),
    FOREIGN KEY (SpecialtyID) REFERENCES Specialties(SpecialtyID)
);

-- ===============================
-- Bảng Appointments (Lịch hẹn)
-- ===============================
CREATE TABLE Appointments (
    AppointmentID INT IDENTITY(1,1) PRIMARY KEY,
    PatientID INT NOT NULL,
    DoctorID INT NOT NULL,
    AppointmentDate DATETIME NOT NULL,
    Status NVARCHAR(50) DEFAULT N'Đã đặt',
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID)
);

-- ===============================
-- Bảng Encounters (Lần khám)
-- ===============================
CREATE TABLE Encounters (
    EncounterID INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentID INT NOT NULL,
    DoctorID INT NOT NULL,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID)
);

-- ===============================
-- Bảng Diagnoses (Chẩn đoán)
-- ===============================
CREATE TABLE Diagnoses (
    DiagnosisID INT IDENTITY(1,1) PRIMARY KEY,
    EncounterID INT NOT NULL,
    Description NVARCHAR(MAX),
    ResultFile VARBINARY(MAX), -- lưu file kết quả (scan, pdf…)
    FOREIGN KEY (EncounterID) REFERENCES Encounters(EncounterID)
);

-- ===============================
-- Bảng Drugs (Thuốc)
-- ===============================
CREATE TABLE Drugs (
    DrugID INT IDENTITY(1,1) PRIMARY KEY,
    DrugName NVARCHAR(100) NOT NULL,
    Unit NVARCHAR(50),
    Price DECIMAL(18,2)
);

-- ===============================
-- Bảng tồn kho thuốc
-- ===============================
CREATE TABLE Drug_Stocks (
    StockID INT IDENTITY(1,1) PRIMARY KEY,
    DrugID INT NOT NULL,
    QuantityAvailable INT,
    LastUpdated DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (DrugID) REFERENCES Drugs(DrugID)
);

-- ===============================
-- Bảng Prescriptions (Đơn thuốc)
-- ===============================
CREATE TABLE Prescriptions (
    PrescriptionID INT IDENTITY(1,1) PRIMARY KEY,
    EncounterID INT NOT NULL,
    DrugID INT NOT NULL,
    Quantity INT,
    Usage NVARCHAR(200),
    FOREIGN KEY (EncounterID) REFERENCES Encounters(EncounterID),
    FOREIGN KEY (DrugID) REFERENCES Drugs(DrugID)
);

-- ===============================
-- Bảng Invoices (Hóa đơn)
-- ===============================
CREATE TABLE Invoices (
    InvoiceID INT IDENTITY(1,1) PRIMARY KEY,
    PatientID INT NOT NULL,
    EncounterID INT NOT NULL,
    TotalAmount DECIMAL(18,2),
    PaymentDate DATETIME DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(50),
    Status NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (EncounterID) REFERENCES Encounters(EncounterID)
);

-- ===============================
-- Bảng Accounts (Tài khoản hệ thống)
-- ===============================
CREATE TABLE Accounts (
    AccountID INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(256) NOT NULL,
    Role NVARCHAR(50) CHECK (Role IN (N'Admin',N'Lễ tân',N'Bác sĩ',N'Dược sĩ',N'Bệnh nhân'))
);

-- ===============================
-- Bảng AuditLogs (ghi lại truy cập hồ sơ / hành động)
-- ===============================
CREATE TABLE AuditLogs (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    AccountID INT,
    Action NVARCHAR(200),
    TableName NVARCHAR(100),
    RecordID INT,
    AccessTime DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AccountID) REFERENCES Accounts(AccountID)
);

-- 1️⃣ Xóa constraint cũ
ALTER TABLE Accounts DROP CONSTRAINT CK__Accounts__Role__59FA5E80;
-- (Tên constraint có thể khác, nên bạn có thể chạy sp_helpconstraint trước:
-- EXEC sp_helpconstraint 'Accounts'; )

-- 2️⃣ Tạo lại constraint chuẩn khớp với backend
ALTER TABLE Accounts
ADD CONSTRAINT CK_Accounts_Role
CHECK (Role IN ('Admin', 'Doctor', 'Receptionist', 'Pharmacist', 'Patient'));

SELECT * FROM Doctors;
