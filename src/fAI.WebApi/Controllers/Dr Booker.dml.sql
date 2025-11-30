use booker
go
-- =====================================================
-- Doctor Appointment Scheduling System
-- Based on FHIR Scheduling Pattern
-- Microsoft SQL Server DML Script
-- Populates data for December 2025
-- =====================================================


SET IDENTITY_INSERT dbo.Patient ON;
GO

INSERT INTO dbo.Patient (PatientId, FirstName, LastName, DateOfBirth, Phone, Email, IsActive)
VALUES 
    (1, 'Sarah', 'Johnson', '1985-03-15', '555-0101', 'sarah.johnson@email.com', 1),
    (2, 'Michael', 'Chen', '1992-07-22', '555-0102', 'michael.chen@email.com', 1),
    (3, 'Emily', 'Rodriguez', '1978-11-30', '555-0103', 'emily.rodriguez@email.com', 1),
    (4, 'David', 'Williams', '1965-05-18', '555-0104', 'david.williams@email.com', 1),
    (5, 'Jessica', 'Martinez', '1990-09-25', '555-0105', 'jessica.martinez@email.com', 1),
    (6, 'James', 'Anderson', '1982-01-12', '555-0106', 'james.anderson@email.com', 1),
    (7, 'Maria', 'Garcia', '1975-12-08', '555-0107', 'maria.garcia@email.com', 1),
    (8, 'Robert', 'Taylor', '1988-04-03', '555-0108', 'robert.taylor@email.com', 1),
    (9, 'Jennifer', 'Thomas', '1995-06-17', '555-0109', 'jennifer.thomas@email.com', 1),
    (10, 'William', 'Moore', '1970-10-21', '555-0110', 'william.moore@email.com', 1),
    (11, 'Linda', 'Jackson', '1983-02-14', '555-0111', 'linda.jackson@email.com', 1),
    (12, 'Christopher', 'White', '1991-08-09', '555-0112', 'christopher.white@email.com', 1),
    (13, 'Patricia', 'Harris', '1968-03-27', '555-0113', 'patricia.harris@email.com', 1),
    (14, 'Daniel', 'Martin', '1987-11-05', '555-0114', 'daniel.martin@email.com', 1),
    (15, 'Nancy', 'Thompson', '1993-01-19', '555-0115', 'nancy.thompson@email.com', 1),
    (16, 'Matthew', 'Lee', '1979-07-31', '555-0116', 'matthew.lee@email.com', 1),
    (17, 'Karen', 'Walker', '1986-09-13', '555-0117', 'karen.walker@email.com', 1),
    (18, 'Paul', 'Hall', '1972-12-22', '555-0118', 'paul.hall@email.com', 1),
    (19, 'Lisa', 'Allen', '1989-05-08', '555-0119', 'lisa.allen@email.com', 1),
    (20, 'Mark', 'Young', '1994-02-28', '555-0120', 'mark.young@email.com', 1);
GO

SET IDENTITY_INSERT dbo.Patient OFF;
GO

PRINT 'Inserted 20 sample patients';
GO

-- =====================================================
-- Insert Practitioners (Doctors)
-- =====================================================
SET IDENTITY_INSERT dbo.Practitioner ON;
GO

INSERT INTO dbo.Practitioner (PractitionerId, FirstName, LastName, Specialty, IsActive)
VALUES 
    (1, 'Joe', 'Moreau', 'Internal Medicine', 1),
    (2, 'Strange', 'Love', 'Cardiology', 1),
    (3, 'Yuri', 'Zhivago', 'Family Medicine', 1);
GO

SET IDENTITY_INSERT dbo.Practitioner OFF;
GO

PRINT 'Inserted 3 Practitioners';
GO



    PatientId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE()

-- select * from Practitioner
-- select PractitionerId,FirstName,LastName,Specialty,IsActive,CreatedDate,ModifiedDate from Practitioner


-- =====================================================
-- Insert Schedules for December 2025
-- One schedule per practitioner for the entire month
-- =====================================================
SET IDENTITY_INSERT dbo.Schedule ON;
GO

INSERT INTO dbo.Schedule (ScheduleId, PractitionerId, ScheduleName, StartDate, EndDate, IsActive)
VALUES 
    (1, 1, 'Dr. Joe Moreau - December 2025', '2025-12-01', '2025-12-31', 1),
    (2, 2, 'Dr. Strange Love - December 2025', '2025-12-01', '2025-12-31', 1),
    (3, 3, 'Dr. Yuri Zhivago - December 2025', '2025-12-01', '2025-12-31', 1);
GO

SET IDENTITY_INSERT dbo.Schedule OFF;
GO

PRINT 'Inserted 3 Schedules for December 2025';
GO

-- select * from Schedule

-- =====================================================
-- Insert Slots for December 2025
-- Generate 20-minute slots from 9:00 AM to 5:00 PM
-- Monday through Friday only
-- Mark holidays as IsEnabled = 0
-- =====================================================

SELECT * FROM dbo.Schedule
SELECT * FROM Slot
--delete FROM Slot




-- Declare variables for slot generation
DECLARE @ScheduleId INT;
DECLARE @CurrentDate DATE = '2025-12-01';
DECLARE @EndDate DATE = '2025-12-31';
DECLARE @StartTime TIME;
DECLARE @EndTime TIME;
DECLARE @SlotStartDateTime DATETIME2;
DECLARE @SlotEndDateTime DATETIME2;
DECLARE @IsEnabled BIT;
DECLARE @DayOfWeek INT;
DECLARE @RandomValue float;

-- select CHECKSUM(NEWID())
-- delete from slot
-- Holiday dates in December 2025
DECLARE @Holidays TABLE (HolidayDate DATE);
INSERT INTO @Holidays VALUES
    ('2025-12-25'), -- Christmas Day
    ('2025-12-26'); -- Day after Christmas (observed)

-- Loop through each practitioner's schedule
DECLARE schedule_cursor CURSOR FOR
	SELECT ScheduleId FROM dbo.Schedule WHERE StartDate = '2025-12-01';

OPEN schedule_cursor;
FETCH NEXT FROM schedule_cursor INTO @ScheduleId;
WHILE @@FETCH_STATUS = 0 BEGIN -- Loop on the schedule 1,2,3 for each doctors

    SET @CurrentDate = '2025-12-01';
    
    -- Loop through each day in December 2025
    WHILE @CurrentDate <= @EndDate BEGIN
    
        SET @DayOfWeek = DATEPART(WEEKDAY, @CurrentDate);
        -- Only process Monday (2) through Friday (6) - adjust based on DATEFIRST setting
        -- Assuming DATEFIRST 7 (US default): Sunday=1, Monday=2, ..., Saturday=7
        IF @DayOfWeek BETWEEN 2 AND 6 BEGIN
        
            -- Check if current date is a holiday
            SET @IsEnabled = CASE WHEN EXISTS (SELECT 1 FROM @Holidays WHERE HolidayDate = @CurrentDate) THEN 0  ELSE 1 END;
            
            -- Generate slots from 9:00 AM to 5:00 PM (20-minute intervals)
            SET @StartTime = '09:00:00';
            WHILE @StartTime < '17:00:00' BEGIN
            
				SET @SlotStartDateTime = CAST(CAST(@CurrentDate AS DATETIME) + CAST(@StartTime AS DATETIME) AS DATETIME2);
                SET @SlotEndDateTime = DATEADD(MINUTE, 20, @SlotStartDateTime);
				SET @RandomValue = RAND() 
				--select @RandomValue
                
                IF CAST(@SlotEndDateTime AS TIME) <= '17:00:00' BEGIN -- Only insert if end time doesn't exceed 5:00 PM
                
                    INSERT INTO dbo.Slot (ScheduleId, StartDateTime, EndDateTime, Status, IsEnabled, Comment)
                    VALUES (
                        @ScheduleId, 
                        @SlotStartDateTime, 
                        @SlotEndDateTime, 
                        iif(@RandomValue < 0.5, 'free', 'not-available'),
                        @IsEnabled,
                        CASE WHEN @IsEnabled = 0 THEN 'Holiday - Not Available' ELSE NULL END
                    );
					-- SELECT @ScheduleId,  @SlotStartDateTime, @SlotEndDateTime
					print('.')
                END
                
                -- Move to next 20-minute slot
                SET @StartTime = DATEADD(MINUTE, 20, CAST(@StartTime AS DATETIME2));
            END
        END
        
        -- Move to next day
        SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
		select @CurrentDate
    END
    
    FETCH NEXT FROM schedule_cursor INTO @ScheduleId;
END

CLOSE schedule_cursor;
DEALLOCATE schedule_cursor;
GO

PRINT 'Inserted Slots for December 2025 (20-minute intervals, Mon-Fri, 9 AM - 5 PM)';
GO

-- =====================================================
-- Summary Query: Verify Data Inserted
-- =====================================================

PRINT '========================================';
PRINT 'DATA SUMMARY';
PRINT '========================================';

-- Practitioner Count
SELECT COUNT(*) AS PractitionerCount FROM dbo.Practitioner;
PRINT 'Practitioners created: 3';

-- Schedule Count
SELECT COUNT(*) AS ScheduleCount FROM dbo.Schedule;
PRINT 'Schedules created: 3 (one per practitioner for December 2025)';


-- Slot Count by Practitioner
SELECT 
    p.FirstName + ' ' + p.LastName AS DoctorName,
    COUNT(sl.SlotId) AS TotalSlots,
    SUM(CASE WHEN sl.IsEnabled = 1 THEN 1 ELSE 0 END) AS EnabledSlots,
    SUM(CASE WHEN sl.IsEnabled = 0 THEN 1 ELSE 0 END) AS DisabledSlots
FROM dbo.Practitioner p
INNER JOIN dbo.Schedule sch ON p.PractitionerId = sch.PractitionerId
INNER JOIN dbo.Slot sl ON sch.ScheduleId = sl.ScheduleId
GROUP BY p.FirstName, p.LastName
--ORDER BY p.PractitionerId;


-- Find the next appointement available
select 
p.FirstName + ' ' + p.LastName AS DoctorName,
sl.SlotId, sl.status,
DATENAME(WEEKDAY, sl.StartDateTime) AS DayOfWeek,
sl.startDateTime--, sl.endDateTime
from Schedule sch
join Practitioner p on p.PractitionerId = sch.PractitionerId
JOIN dbo.Slot sl ON sch.ScheduleId = sl.ScheduleId  and sl.isEnabled = 1
where 
sl.startDateTime > getdate() and
--p.LastName like '%love%'
p.LastName like '%Moreau%'
and sl.status = 'free'






-- Find the next appointement booked
select 
p.FirstName + ' ' + p.LastName AS DoctorName,
sl.SlotId, 
DATENAME(WEEKDAY, sl.StartDateTime) AS DayOfWeek,
sl.startDateTime--, sl.endDateTime
from Schedule sch
join Practitioner p on p.PractitionerId = sch.PractitionerId
JOIN dbo.Slot sl ON sch.ScheduleId = sl.ScheduleId and sl.status = 'busy' and sl.isEnabled = 1
where 
sl.startDateTime > getdate() and
p.LastName like '%love%'




-- Total Slots Summary
SELECT 
    COUNT(*) AS TotalSlots,
    SUM(CASE WHEN Status = 'free' THEN 1 ELSE 0 END) AS FreeSlots,
    SUM(CASE WHEN Status = 'busy' THEN 1 ELSE 0 END) AS BusySlots,
    SUM(CASE WHEN IsEnabled = 1 THEN 1 ELSE 0 END) AS EnabledSlots,
    SUM(CASE WHEN IsEnabled = 0 THEN 1 ELSE 0 END) AS HolidaySlots
FROM dbo.Slot;

-- Slots by Date and Status
SELECT 
    CAST(StartDateTime AS DATE) AS SlotDate,
    DATENAME(WEEKDAY, StartDateTime) AS DayOfWeek,
    COUNT(*) AS SlotCount,
    SUM(CASE WHEN IsEnabled = 0 THEN 1 ELSE 0 END) AS HolidaySlots
FROM dbo.Slot
GROUP BY CAST(StartDateTime AS DATE), DATENAME(WEEKDAY, StartDateTime)
ORDER BY SlotDate;

PRINT '========================================';
PRINT 'DML Script completed successfully.';
PRINT '========================================';
GO