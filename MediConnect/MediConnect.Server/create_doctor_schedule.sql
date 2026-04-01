-- SQL Script để tạo lịch làm việc và khung giờ cho bác sĩ
-- Chạy script này trong SQL Server Management Studio (SSMS) hoặc Azure Data Studio

-- =====================================================
-- BƯỚC 1: Kiểm tra danh sách bác sĩ hiện có
-- =====================================================
SELECT user_id, full_name, email 
FROM users 
WHERE role = 'Doctor';

-- =====================================================
-- BƯỚC 2: Tạo doctor_schedules (lịch làm việc hàng tuần)
-- Thay @DoctorUserId bằng user_id của bác sĩ thực tế
-- =====================================================
DECLARE @DoctorUserId INT = 2; -- << THAY ĐỔI GIÁ TRỊ NÀY

-- Xóa lịch cũ nếu có (tùy chọn)
-- DELETE FROM time_slots WHERE user_id = @DoctorUserId;
-- DELETE FROM doctor_schedules WHERE user_id = @DoctorUserId;

-- Tạo lịch làm việc từ Thứ 2 đến Thứ 6 (8:00 - 17:00)
INSERT INTO doctor_schedules (user_id, day_of_week, start_time, end_time, slot_duration_mins, max_patients_per_slot, is_active, effective_from, effective_to, created_at, updated_at)
VALUES 
    (@DoctorUserId, 1, '08:00:00', '17:00:00', 30, 1, 1, '2026-01-01', '2026-12-31', GETDATE(), GETDATE()), -- Thứ 2
    (@DoctorUserId, 2, '08:00:00', '17:00:00', 30, 1, 1, '2026-01-01', '2026-12-31', GETDATE(), GETDATE()), -- Thứ 3
    (@DoctorUserId, 3, '08:00:00', '17:00:00', 30, 1, 1, '2026-01-01', '2026-12-31', GETDATE(), GETDATE()), -- Thứ 4
    (@DoctorUserId, 4, '08:00:00', '17:00:00', 30, 1, 1, '2026-01-01', '2026-12-31', GETDATE(), GETDATE()), -- Thứ 5
    (@DoctorUserId, 5, '08:00:00', '17:00:00', 30, 1, 1, '2026-01-01', '2026-12-31', GETDATE(), GETDATE()), -- Thứ 6
    (@DoctorUserId, 6, '08:00:00', '12:00:00', 30, 1, 1, '2026-01-01', '2026-12-31', GETDATE(), GETDATE()); -- Thứ 7 (buổi sáng)

-- =====================================================
-- BƯỚC 3: Tạo time_slots cho 30 ngày tới
-- =====================================================
DECLARE @StartDate DATE = CAST(GETDATE() AS DATE);
DECLARE @EndDate DATE = DATEADD(DAY, 30, @StartDate);
DECLARE @CurrentDate DATE = @StartDate;
DECLARE @ScheduleId INT;
DECLARE @DayOfWeekNum INT;

WHILE @CurrentDate <= @EndDate
BEGIN
    SET @DayOfWeekNum = (DATEPART(WEEKDAY, @CurrentDate) + 5) % 7; -- Chuyển sang 0=CN, 1=T2, ...
    
    -- Lấy schedule_id cho ngày trong tuần này
    SELECT @ScheduleId = schedule_id 
    FROM doctor_schedules 
    WHERE user_id = @DoctorUserId AND day_of_week = @DayOfWeekNum AND is_active = 1;
    
    IF @ScheduleId IS NOT NULL
    BEGIN
        -- Tạo các slot 30 phút từ 8:00 đến 17:00
        DECLARE @SlotStart TIME = '08:00:00';
        DECLARE @SlotEnd TIME;
        DECLARE @EndTimeLimit TIME;
        
        SELECT @EndTimeLimit = end_time FROM doctor_schedules WHERE schedule_id = @ScheduleId;
        
        WHILE @SlotStart < @EndTimeLimit
        BEGIN
            SET @SlotEnd = DATEADD(MINUTE, 30, @SlotStart);
            
            -- Kiểm tra xem slot đã tồn tại chưa
            IF NOT EXISTS (
                SELECT 1 FROM time_slots 
                WHERE user_id = @DoctorUserId 
                AND slot_date = @CurrentDate 
                AND start_time = @SlotStart
            )
            BEGIN
                INSERT INTO time_slots (schedule_id, user_id, slot_date, start_time, end_time, max_capacity, booked_count, is_available, created_at)
                VALUES (@ScheduleId, @DoctorUserId, @CurrentDate, @SlotStart, @SlotEnd, 1, 0, 1, GETDATE());
            END
            
            SET @SlotStart = @SlotEnd;
        END
    END
    
    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
    SET @ScheduleId = NULL;
END

-- =====================================================
-- BƯỚC 4: Kiểm tra kết quả
-- =====================================================
SELECT 'doctor_schedules:' AS Info;
SELECT * FROM doctor_schedules WHERE user_id = @DoctorUserId;

SELECT 'time_slots (30 ngày tới):' AS Info;
SELECT TOP 50 * FROM time_slots WHERE user_id = @DoctorUserId ORDER BY slot_date, start_time;

SELECT 'Tổng số time_slots:' AS Info, COUNT(*) AS Total FROM time_slots WHERE user_id = @DoctorUserId;
