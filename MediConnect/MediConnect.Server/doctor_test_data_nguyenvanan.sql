-- doctor_test_data_nguyenvanan.sql
-- Thêm dữ liệu test cho bác sĩ bs.nguyenvanan@mediconnect.vn và một số bệnh nhân
-- File này được viết để chạy an toàn trong SQL Server; nếu email đã có sẵn, chỉ chèn dữ liệu phụ trợ cần thiết.

SET NOCOUNT ON;

DECLARE @DoctorEmail NVARCHAR(256) = N'bs.nguyenvanan@mediconnect.vn';
DECLARE @DoctorId INT;
DECLARE @DepartmentName NVARCHAR(200) = N'Outpatient Department';
DECLARE @SpecialtyName NVARCHAR(200) = N'Cardiology';
DECLARE @ApproverEmail NVARCHAR(256) = N'admin.seed@mediconnect.local';

SELECT @DoctorId = user_id FROM dbo.users WHERE email = @DoctorEmail;

IF @DoctorId IS NULL
BEGIN
    INSERT INTO dbo.users (
        role_id, full_name, email, password_hash, phone_number,
        gender, date_of_birth, address, is_active, is_verified,
        created_at, updated_at
    )
    VALUES (
        (SELECT role_id FROM dbo.roles WHERE role_name = 'DOCTOR'),
        N'Nguyễn Văn An',
        @DoctorEmail,
        N'SEED_HASH_DO_NOT_USE_IN_PRODUCTION',
        N'0901000100',
        N'MALE',
        '1980-04-20',
        N'Hà Nội, Việt Nam',
        1, 1,
        GETDATE(), GETDATE()
    );
    SET @DoctorId = SCOPE_IDENTITY();
END

DECLARE @DepartmentId INT = (SELECT department_id FROM dbo.departments WHERE department_name = @DepartmentName);
IF @DepartmentId IS NULL
BEGIN
    INSERT INTO dbo.departments (department_name, description, location)
    VALUES ( @DepartmentName, N'Khoa Khám Ngoại trú', N'Tầng 2, Tòa nhà B' );
    SET @DepartmentId = SCOPE_IDENTITY();
END

IF NOT EXISTS (SELECT 1 FROM dbo.doctor_profiles WHERE user_id = @DoctorId)
BEGIN
    INSERT INTO dbo.doctor_profiles (
        user_id, department_id, license_number, years_of_experience,
        education, bio, consultation_fee, insurance_accepted, location,
        approval_status, approved_by, approved_at, average_rating, total_reviews,
        created_at, updated_at
    )
    VALUES (
        @DoctorId,
        @DepartmentId,
        N'LIC-MC-AN-0001',
        15,
        N'Tiến sĩ Y khoa chuyên tim mạch',
        N'Tôi có kinh nghiệm khám và điều trị các bệnh lý tim mạch, tăng huyết áp và rối loạn mỡ máu.',
        500000.00,
        N'Bao Viet, PTI, PVI',
        N'Phòng khám Rivergate, Hà Nội',
        'APPROVED',
        (SELECT user_id FROM dbo.users WHERE email = @ApproverEmail),
        GETDATE(),
        4.85,
        124,
        GETDATE(),
        GETDATE()
    );
END

DECLARE @SpecialtyId INT = (SELECT specialty_id FROM dbo.specialties WHERE specialty_name = @SpecialtyName);
IF @SpecialtyId IS NULL
BEGIN
    INSERT INTO dbo.specialties (specialty_name, description)
    VALUES ( @SpecialtyName, N'Bệnh lý tim mạch và huyết áp' );
    SET @SpecialtyId = SCOPE_IDENTITY();
END

IF NOT EXISTS (
    SELECT 1 FROM dbo.doctor_specialties
    WHERE user_id = @DoctorId AND specialty_id = @SpecialtyId
)
BEGIN
    INSERT INTO dbo.doctor_specialties (user_id, specialty_id, is_primary)
    VALUES (@DoctorId, @SpecialtyId, 1);
END

DECLARE @ScheduleId INT;
SELECT TOP 1 @ScheduleId = schedule_id
FROM dbo.doctor_schedules
WHERE user_id = @DoctorId AND day_of_week = DATEPART(WEEKDAY, GETDATE()) - 1;

IF @ScheduleId IS NULL
BEGIN
    INSERT INTO dbo.doctor_schedules (
        user_id, day_of_week, start_time, end_time,
        slot_duration_mins, max_patients_per_slot, is_active,
        effective_from, created_at, updated_at
    )
    VALUES (
        @DoctorId,
        DATEPART(WEEKDAY, GETDATE()) - 1,
        '08:00:00', '12:00:00',
        30, 2, 1,
        CAST(GETDATE() AS DATE), GETDATE(), GETDATE()
    );
    SET @ScheduleId = SCOPE_IDENTITY();
END

DECLARE @SlotId INT;
SELECT TOP 1 @SlotId = slot_id
FROM dbo.time_slots
WHERE user_id = @DoctorId
  AND slot_date = CAST(GETDATE() AS DATE)
  AND start_time = '08:00:00'
  AND end_time = '08:30:00';

IF @SlotId IS NULL
BEGIN
    INSERT INTO dbo.time_slots (
        schedule_id, user_id, slot_date, start_time, end_time,
        max_capacity, booked_count, is_available, created_at
    )
    VALUES (
        @ScheduleId,
        @DoctorId,
        CAST(GETDATE() AS DATE),
        '08:00:00', '08:30:00',
        1, 0, 1, GETDATE()
    );
    SET @SlotId = SCOPE_IDENTITY();
END

-- Tạo hoặc dùng bệnh nhân đã có sẵn
DECLARE @Patient1Email NVARCHAR(256) = N'patient.phamthidung@gmail.com';
DECLARE @Patient2Email NVARCHAR(256) = N'patient.nguyenthanhhung@gmail.com';
DECLARE @Patient3Email NVARCHAR(256) = N'patient.lethihoa@gmail.com';
DECLARE @Patient4Email NVARCHAR(256) = N'patient.tranvankhoa@gmail.com';
DECLARE @Patient5Email NVARCHAR(256) = N'patient.hoangthilan@gmail.com';

DECLARE @Patient1Id INT;
DECLARE @Patient2Id INT;
DECLARE @Patient3Id INT;
DECLARE @Patient4Id INT;
DECLARE @Patient5Id INT;

-- Hàm chèn bệnh nhân nếu chưa có
DECLARE @SeedRoleId INT = (SELECT role_id FROM dbo.roles WHERE role_name = 'PATIENT');

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = @Patient1Email)
BEGIN
    INSERT INTO dbo.users (
        role_id, full_name, email, password_hash, phone_number,
        gender, date_of_birth, address, is_active, is_verified,
        created_at, updated_at
    ) VALUES (
        @SeedRoleId, N'Phạm Thị Dung', @Patient1Email, N'SEED_HASH_DO_NOT_USE_IN_PRODUCTION', N'0911000101',
        N'FEMALE', '1992-02-14', N'Hà Nội', 1, 1, GETDATE(), GETDATE()
    );
END
SELECT @Patient1Id = user_id FROM dbo.users WHERE email = @Patient1Email;

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = @Patient2Email)
BEGIN
    INSERT INTO dbo.users (
        role_id, full_name, email, password_hash, phone_number,
        gender, date_of_birth, address, is_active, is_verified,
        created_at, updated_at
    ) VALUES (
        @SeedRoleId, N'Nguyễn Thanh Hưng', @Patient2Email, N'SEED_HASH_DO_NOT_USE_IN_PRODUCTION', N'0911000102',
        N'MALE', '1990-06-21', N'Hồ Chí Minh', 1, 1, GETDATE(), GETDATE()
    );
END
SELECT @Patient2Id = user_id FROM dbo.users WHERE email = @Patient2Email;

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = @Patient3Email)
BEGIN
    INSERT INTO dbo.users (
        role_id, full_name, email, password_hash, phone_number,
        gender, date_of_birth, address, is_active, is_verified,
        created_at, updated_at
    ) VALUES (
        @SeedRoleId, N'Lê Thị Hoa', @Patient3Email, N'SEED_HASH_DO_NOT_USE_IN_PRODUCTION', N'0911000103',
        N'FEMALE', '1998-10-05', N'Đà Nẵng', 1, 1, GETDATE(), GETDATE()
    );
END
SELECT @Patient3Id = user_id FROM dbo.users WHERE email = @Patient3Email;

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = @Patient4Email)
BEGIN
    INSERT INTO dbo.users (
        role_id, full_name, email, password_hash, phone_number,
        gender, date_of_birth, address, is_active, is_verified,
        created_at, updated_at
    ) VALUES (
        @SeedRoleId, N'Trần Văn Khoa', @Patient4Email, N'SEED_HASH_DO_NOT_USE_IN_PRODUCTION', N'0911000104',
        N'MALE', '1988-12-09', N'Hải Phòng', 1, 1, GETDATE(), GETDATE()
    );
END
SELECT @Patient4Id = user_id FROM dbo.users WHERE email = @Patient4Email;

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE email = @Patient5Email)
BEGIN
    INSERT INTO dbo.users (
        role_id, full_name, email, password_hash, phone_number,
        gender, date_of_birth, address, is_active, is_verified,
        created_at, updated_at
    ) VALUES (
        @SeedRoleId, N'Hoàng Thị Lan', @Patient5Email, N'SEED_HASH_DO_NOT_USE_IN_PRODUCTION', N'0911000105',
        N'FEMALE', '1994-03-17', N'Bắc Ninh', 1, 1, GETDATE(), GETDATE()
    );
END
SELECT @Patient5Id = user_id FROM dbo.users WHERE email = @Patient5Email;

-- Tạo appointment test
DECLARE @Appointment1Id INT;
DECLARE @Appointment2Id INT;
DECLARE @Appointment3Id INT;
DECLARE @Appointment4Id INT;

IF NOT EXISTS (
    SELECT 1 FROM dbo.appointments
    WHERE patient_id = @Patient1Id AND doctor_id = @DoctorId AND appointment_date = CAST(GETDATE() AS DATE) AND start_time = '08:00:00'
)
BEGIN
    INSERT INTO dbo.appointments (
        patient_id, doctor_id, slot_id, specialty_id,
        appointment_date, start_time, end_time,
        reason, status, created_at, updated_at
    ) VALUES (
        @Patient1Id, @DoctorId, @SlotId, @SpecialtyId,
        CAST(GETDATE() AS DATE), '08:00:00', '08:30:00',
        N'Đau tức ngực và hồi hộp', 'PENDING', GETDATE(), GETDATE()
    );
    SET @Appointment1Id = SCOPE_IDENTITY();
END
ELSE
    SELECT @Appointment1Id = appointment_id FROM dbo.appointments
    WHERE patient_id = @Patient1Id AND doctor_id = @DoctorId AND appointment_date = CAST(GETDATE() AS DATE) AND start_time = '08:00:00';

IF NOT EXISTS (
    SELECT 1 FROM dbo.appointments
    WHERE patient_id = @Patient2Id AND doctor_id = @DoctorId AND appointment_date = DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
)
BEGIN
    INSERT INTO dbo.appointments (
        patient_id, doctor_id, slot_id, specialty_id,
        appointment_date, start_time, end_time,
        reason, status, created_at, updated_at
    ) VALUES (
        @Patient2Id, @DoctorId, @SlotId, @SpecialtyId,
        DATEADD(DAY, 1, CAST(GETDATE() AS DATE)), '08:00:00', '08:30:00',
        N'Tim đập nhanh khi chạy bộ', 'CONFIRMED', GETDATE(), GETDATE()
    );
    SET @Appointment2Id = SCOPE_IDENTITY();
END
ELSE
    SELECT @Appointment2Id = appointment_id FROM dbo.appointments
    WHERE patient_id = @Patient2Id AND doctor_id = @DoctorId AND appointment_date = DATEADD(DAY, 1, CAST(GETDATE() AS DATE));

IF NOT EXISTS (
    SELECT 1 FROM dbo.appointments
    WHERE patient_id = @Patient3Id AND doctor_id = @DoctorId AND appointment_date = DATEADD(DAY, -3, CAST(GETDATE() AS DATE))
)
BEGIN
    INSERT INTO dbo.appointments (
        patient_id, doctor_id, slot_id, specialty_id,
        appointment_date, start_time, end_time,
        reason, status, created_at, updated_at
    ) VALUES (
        @Patient3Id, @DoctorId, @SlotId, @SpecialtyId,
        DATEADD(DAY, -3, CAST(GETDATE() AS DATE)), '08:00:00', '08:30:00',
        N'Đau ngực sau ăn', 'COMPLETED', GETDATE(), GETDATE()
    );
    SET @Appointment3Id = SCOPE_IDENTITY();
END
ELSE
    SELECT @Appointment3Id = appointment_id FROM dbo.appointments
    WHERE patient_id = @Patient3Id AND doctor_id = @DoctorId AND appointment_date = DATEADD(DAY, -3, CAST(GETDATE() AS DATE));

IF NOT EXISTS (
    SELECT 1 FROM dbo.appointments
    WHERE patient_id = @Patient4Id AND doctor_id = @DoctorId AND appointment_date = DATEADD(DAY, 2, CAST(GETDATE() AS DATE))
)
BEGIN
    INSERT INTO dbo.appointments (
        patient_id, doctor_id, slot_id, specialty_id,
        appointment_date, start_time, end_time,
        reason, status, created_at, updated_at
    ) VALUES (
        @Patient4Id, @DoctorId, @SlotId, @SpecialtyId,
        DATEADD(DAY, 2, CAST(GETDATE() AS DATE)), '08:00:00', '08:30:00',
        N'Mệt mỏi, khó thở khi đi bộ', 'PENDING', GETDATE(), GETDATE()
    );
    SET @Appointment4Id = SCOPE_IDENTITY();
END
ELSE
    SELECT @Appointment4Id = appointment_id FROM dbo.appointments
    WHERE patient_id = @Patient4Id AND doctor_id = @DoctorId AND appointment_date = DATEADD(DAY, 2, CAST(GETDATE() AS DATE));

-- Tạo một lịch chờ cho bệnh nhân thứ 5
IF NOT EXISTS (
    SELECT 1 FROM dbo.appointment_waitlists
    WHERE patient_id = @Patient5Id AND doctor_id = @DoctorId AND preferred_date = DATEADD(DAY, 3, CAST(GETDATE() AS DATE))
)
BEGIN
    INSERT INTO dbo.appointment_waitlists (
        patient_id, doctor_id, specialty_id, preferred_date, preferred_time,
        status, created_at, updated_at
    ) VALUES (
        @Patient5Id, @DoctorId, @SpecialtyId,
        DATEADD(DAY, 3, CAST(GETDATE() AS DATE)), '09:00:00',
        'WAITING', GETDATE(), GETDATE()
    );
END

-- Medical record cho cuộc hẹn đã hoàn thành
IF @Appointment3Id IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.medical_records WHERE appointment_id = @Appointment3Id
)
BEGIN
    INSERT INTO dbo.medical_records (
        patient_id, doctor_id, appointment_id,
        diagnosis, symptoms, treatment_plan, prescription, notes,
        record_date, created_at, updated_at
    ) VALUES (
        @Patient3Id, @DoctorId, @Appointment3Id,
        N'Rối loạn nhịp tim', N'Đau ngực nhẹ, khó thở, tim đập nhanh',
        N'Khuyến nghị nghỉ ngơi và xét nghiệm ECG.',
        N'Thuốc giảm đau ngực, theo dõi huyết áp',
        N'Bệnh nhân cần tái khám sau 7 ngày.',
        DATEADD(DAY, -3, CAST(GETDATE() AS DATE)), GETDATE(), GETDATE()
    );
END

-- Review cho cuộc hẹn đã hoàn thành
IF @Appointment3Id IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.reviews WHERE appointment_id = @Appointment3Id
)
BEGIN
    INSERT INTO dbo.reviews (
        appointment_id, patient_id, doctor_id,
        rating, comment, sentiment_score, sentiment_label,
        is_visible, created_at, updated_at
    ) VALUES (
        @Appointment3Id, @Patient3Id, @DoctorId,
        5, N'BS Nguyễn Văn An rất tận tâm và giải thích rõ ràng.', 0.95, N'POSITIVE',
        1, GETDATE(), GETDATE()
    );
END

-- Thông báo test cho bệnh nhân
IF @Appointment1Id IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.notifications
    WHERE user_id = @Patient1Id AND appointment_id = @Appointment1Id AND notification_type = 'APPOINTMENT_CONFIRMATION'
)
BEGIN
    INSERT INTO dbo.notifications (
        user_id, appointment_id, notification_type, channel,
        title, body, is_read, sent_at, status, created_at, updated_at
    ) VALUES (
        @Patient1Id, @Appointment1Id, N'APPOINTMENT_CONFIRMATION', N'IN_APP',
        N'Yêu cầu đặt lịch đã được gửi',
        N'Yêu cầu khám ngày hôm nay đang chờ bác sĩ xác nhận.',
        0, GETDATE(), N'SENT', GETDATE(), GETDATE()
    );
END

IF NOT EXISTS (
    SELECT 1 FROM dbo.notifications
    WHERE user_id = @DoctorId AND notification_type = 'NEW_WAITLIST_PATIENT'
)
BEGIN
    INSERT INTO dbo.notifications (
        user_id, appointment_id, notification_type, channel,
        title, body, is_read, sent_at, status, created_at, updated_at
    ) VALUES (
        @DoctorId, NULL, N'NEW_WAITLIST_PATIENT', N'IN_APP',
        N'Có bệnh nhân mới trong danh sách chờ',
        N'Bệnh nhân Hoàng Thị Lan đang chờ lịch khám.',
        0, GETDATE(), N'SENT', GETDATE(), GETDATE()
    );
END

PRINT 'Đã thêm dữ liệu test cho bác sĩ bs.nguyenvanan@mediconnect.vn và các bệnh nhân mẫu.';
