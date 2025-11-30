use booker
go
-- =====================================================
-- Doctor Appointment Scheduling System
-- Based on FHIR Scheduling Pattern
-- Microsoft SQL Server DDL Script
-- =====================================================

-- Drop tables if they exist (in reverse order of dependencies)
IF OBJECT_ID('dbo.Appointment', 'U') IS NOT NULL DROP TABLE dbo.Appointment;
IF OBJECT_ID('dbo.Slot', 'U') IS NOT NULL DROP TABLE dbo.Slot;
IF OBJECT_ID('dbo.Schedule', 'U') IS NOT NULL DROP TABLE dbo.Schedule;
IF OBJECT_ID('dbo.Patient', 'U') IS NOT NULL DROP TABLE dbo.Patient;
IF OBJECT_ID('dbo.Practitioner', 'U') IS NOT NULL DROP TABLE dbo.Practitioner;
GO

-- =====================================================
-- Table: Practitioner (Doctor)
-- Represents healthcare providers who offer services
-- =====================================================
CREATE TABLE dbo.Practitioner (
    PractitionerId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Specialty NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- =====================================================
-- Table: Patient
-- Represents individuals receiving healthcare services
-- =====================================================
CREATE TABLE dbo.Patient (
    PatientId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- =====================================================
-- Table: Schedule
-- Defines the broader availability context for a practitioner
-- One schedule per practitioner covering a time period
-- =====================================================
CREATE TABLE dbo.Schedule (
    ScheduleId INT IDENTITY(1,1) PRIMARY KEY,
    PractitionerId INT NOT NULL,
    ScheduleName NVARCHAR(200) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Schedule_Practitioner 
        FOREIGN KEY (PractitionerId) 
        REFERENCES dbo.Practitioner(PractitionerId)
);
GO

-- =====================================================
-- Table: Slot
-- Granular bookable time units within a Schedule
-- Represents specific time periods that can be booked
-- =====================================================
CREATE TABLE dbo.Slot (
    SlotId INT IDENTITY(1,1) PRIMARY KEY,
    ScheduleId INT NOT NULL,
    StartDateTime DATETIME2 NOT NULL,
    EndDateTime DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'free',
    -- Status values: 'free', 'busy', 'busy-unavailable', 'busy-tentative'
    IsEnabled BIT NOT NULL DEFAULT 1,
    -- IsEnabled: false for holidays, true for regular working days
    Comment NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Slot_Schedule 
        FOREIGN KEY (ScheduleId) 
        REFERENCES dbo.Schedule(ScheduleId),
    CONSTRAINT CHK_Slot_Status 
        CHECK (Status IN ('free', 'busy', 'busy-unavailable', 'busy-tentative','not-available')),
    CONSTRAINT CHK_Slot_DateTime 
        CHECK (EndDateTime > StartDateTime)
);
GO

-- =====================================================
-- Table: Appointment
-- The outcome of the scheduling process
-- Links a Patient to a specific Slot
-- =====================================================
CREATE TABLE dbo.Appointment (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    SlotId INT NOT NULL,
    PatientId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'booked',
    -- Status values: 'proposed', 'pending', 'booked', 'arrived', 'fulfilled', 'cancelled', 'noshow'
    AppointmentType NVARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Appointment_Slot 
        FOREIGN KEY (SlotId) 
        REFERENCES dbo.Slot(SlotId),
    CONSTRAINT FK_Appointment_Patient 
        FOREIGN KEY (PatientId) 
        REFERENCES dbo.Patient(PatientId),
    CONSTRAINT CHK_Appointment_Status 
        CHECK (Status IN ('proposed', 'pending', 'booked', 'arrived', 'fulfilled', 'cancelled', 'noshow'))
);
GO

-- =====================================================
-- Indexes for performance optimization
-- =====================================================

-- Index on Schedule by Practitioner and Date Range
CREATE NONCLUSTERED INDEX IX_Schedule_Practitioner_DateRange
ON dbo.Schedule(PractitionerId, StartDate, EndDate)
INCLUDE (IsActive);
GO

-- Index on Slot by Schedule and DateTime
CREATE NONCLUSTERED INDEX IX_Slot_Schedule_DateTime
ON dbo.Slot(ScheduleId, StartDateTime, EndDateTime)
INCLUDE (Status, IsEnabled);
GO

-- Index on Slot by Status and DateTime (for finding available slots)
CREATE NONCLUSTERED INDEX IX_Slot_Status_DateTime
ON dbo.Slot(Status, StartDateTime)
WHERE IsEnabled = 1;
GO

-- Index on Appointment by Slot
CREATE NONCLUSTERED INDEX IX_Appointment_Slot
ON dbo.Appointment(SlotId)
INCLUDE (PatientId, Status);
GO

-- Index on Appointment by Patient
CREATE NONCLUSTERED INDEX IX_Appointment_Patient
ON dbo.Appointment(PatientId)
INCLUDE (SlotId, Status);
GO

PRINT 'DDL Script completed successfully. All tables and indexes created.';
GO