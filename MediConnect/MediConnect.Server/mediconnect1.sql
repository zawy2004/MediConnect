IF DB_ID('mediconnect') IS NULL
    CREATE DATABASE mediconnect;
GO

USE mediconnect;
GO

-- ============================================================
-- TABLE: roles
-- ============================================================
IF OBJECT_ID('dbo.roles', 'U') IS NULL
CREATE TABLE dbo.roles (
    role_id     INT             NOT NULL IDENTITY(1,1),
    role_name   NVARCHAR(50)    NOT NULL,
    description NVARCHAR(255)   NULL,
    CONSTRAINT PK_roles         PRIMARY KEY (role_id),
    CONSTRAINT UQ_role_name     UNIQUE      (role_name)
);
GO

-- ============================================================
-- TABLE: users
-- ============================================================
IF OBJECT_ID('dbo.users', 'U') IS NULL
CREATE TABLE dbo.users (
    user_id         INT             NOT NULL IDENTITY(1,1),
    role_id         INT             NOT NULL,
    full_name       NVARCHAR(150)   NOT NULL,
    email           NVARCHAR(150)   NOT NULL,
    password_hash   NVARCHAR(255)   NOT NULL,
    phone_number    NVARCHAR(20)    NULL,
    avatar_url      NVARCHAR(500)   NULL,
    gender          NVARCHAR(10)    NULL,
    date_of_birth   DATE            NULL,
    address         NVARCHAR(300)   NULL,
    is_active       BIT             NOT NULL CONSTRAINT DF_users_is_active    DEFAULT 1,
    is_verified     BIT             NOT NULL CONSTRAINT DF_users_is_verified  DEFAULT 0,
    created_at      DATETIME        NOT NULL CONSTRAINT DF_users_created_at   DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME        NOT NULL CONSTRAINT DF_users_updated_at   DEFAULT CURRENT_TIMESTAMP,
    last_login_at   DATETIME        NULL,
    CONSTRAINT PK_users         PRIMARY KEY (user_id),
    CONSTRAINT UQ_users_email   UNIQUE      (email),
    CONSTRAINT CK_users_gender  CHECK       (gender IN ('MALE','FEMALE','OTHER')),
    CONSTRAINT FK_users_role    FOREIGN KEY (role_id) REFERENCES dbo.roles (role_id)
);
GO
CREATE INDEX IX_users_role_id ON dbo.users (role_id);
GO

-- ============================================================
-- TABLE: password_reset_tokens
-- ============================================================
IF OBJECT_ID('dbo.password_reset_tokens', 'U') IS NULL
CREATE TABLE dbo.password_reset_tokens (
    token_id    INT             NOT NULL IDENTITY(1,1),
    user_id     INT             NOT NULL,
    token       NVARCHAR(255)   NOT NULL,
    expires_at  DATETIME        NOT NULL,
    is_used     BIT             NOT NULL CONSTRAINT DF_prt_is_used    DEFAULT 0,
    created_at  DATETIME        NOT NULL CONSTRAINT DF_prt_created_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_prt       PRIMARY KEY (token_id),
    CONSTRAINT FK_prt_user  FOREIGN KEY (user_id) REFERENCES dbo.users (user_id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_prt_token   ON dbo.password_reset_tokens (token);
CREATE INDEX IX_prt_user_id ON dbo.password_reset_tokens (user_id);
GO

-- ============================================================
-- TABLE: specialties
-- ============================================================
IF OBJECT_ID('dbo.specialties', 'U') IS NULL
CREATE TABLE dbo.specialties (
    specialty_id    INT             NOT NULL IDENTITY(1,1),
    specialty_name  NVARCHAR(150)   NOT NULL,
    description     NVARCHAR(MAX)   NULL,
    icon_url        NVARCHAR(500)   NULL,
    is_active       BIT             NOT NULL CONSTRAINT DF_spec_is_active   DEFAULT 1,
    created_at      DATETIME        NOT NULL CONSTRAINT DF_spec_created_at  DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME        NOT NULL CONSTRAINT DF_spec_updated_at  DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_specialties       PRIMARY KEY (specialty_id),
    CONSTRAINT UQ_specialty_name    UNIQUE      (specialty_name)
);
GO

-- ============================================================
-- TABLE: departments
-- ============================================================
IF OBJECT_ID('dbo.departments', 'U') IS NULL
CREATE TABLE dbo.departments (
    department_id   INT             NOT NULL IDENTITY(1,1),
    department_name NVARCHAR(150)   NOT NULL,
    description     NVARCHAR(MAX)   NULL,
    location        NVARCHAR(200)   NULL,
    is_active       BIT             NOT NULL CONSTRAINT DF_dept_is_active   DEFAULT 1,
    created_at      DATETIME        NOT NULL CONSTRAINT DF_dept_created_at  DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME        NOT NULL CONSTRAINT DF_dept_updated_at  DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_departments       PRIMARY KEY (department_id),
    CONSTRAINT UQ_department_name   UNIQUE      (department_name)
);
GO

-- ============================================================
-- TABLE: doctor_profiles
-- ============================================================
IF OBJECT_ID('dbo.doctor_profiles', 'U') IS NULL
CREATE TABLE dbo.doctor_profiles (
    doctor_profile_id       INT             NOT NULL IDENTITY(1,1),
    user_id                 INT             NOT NULL,
    department_id           INT             NULL,
    license_number          NVARCHAR(100)   NOT NULL,
    years_of_experience     INT             NOT NULL CONSTRAINT DF_dp_exp        DEFAULT 0,
    education               NVARCHAR(MAX)   NULL,
    bio                     NVARCHAR(MAX)   NULL,
    consultation_fee        DECIMAL(12,2)   NOT NULL CONSTRAINT DF_dp_fee        DEFAULT 0.00,
    insurance_accepted      NVARCHAR(500)   NULL,
    location                NVARCHAR(300)   NULL,
    approval_status         NVARCHAR(20)    NOT NULL CONSTRAINT DF_dp_status     DEFAULT 'PENDING',
    approved_by             INT             NULL,
    approved_at             DATETIME        NULL,
    average_rating          DECIMAL(3,2)    NOT NULL CONSTRAINT DF_dp_rating     DEFAULT 0.00,
    total_reviews           INT             NOT NULL CONSTRAINT DF_dp_reviews    DEFAULT 0,
    created_at              DATETIME        NOT NULL CONSTRAINT DF_dp_created_at DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME        NOT NULL CONSTRAINT DF_dp_updated_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_doctor_profiles       PRIMARY KEY (doctor_profile_id),
    CONSTRAINT UQ_dp_user_id            UNIQUE (user_id),
    CONSTRAINT UQ_dp_license_number     UNIQUE (license_number),
    CONSTRAINT CK_dp_approval_status    CHECK  (approval_status IN ('PENDING','APPROVED','REJECTED')),
    CONSTRAINT FK_dp_user               FOREIGN KEY (user_id)       REFERENCES dbo.users       (user_id) ON DELETE CASCADE,
    CONSTRAINT FK_dp_department         FOREIGN KEY (department_id) REFERENCES dbo.departments (department_id),
    CONSTRAINT FK_dp_approved_by        FOREIGN KEY (approved_by)   REFERENCES dbo.users       (user_id)
);
GO
CREATE INDEX IX_dp_department_id   ON dbo.doctor_profiles (department_id);
CREATE INDEX IX_dp_approval_status ON dbo.doctor_profiles (approval_status);
GO

-- ============================================================
-- TABLE: doctor_specialties
-- ============================================================
IF OBJECT_ID('dbo.doctor_specialties', 'U') IS NULL
CREATE TABLE dbo.doctor_specialties (
    doctor_specialty_id INT  NOT NULL IDENTITY(1,1),
    user_id             INT  NOT NULL,
    specialty_id        INT  NOT NULL,
    is_primary          BIT  NOT NULL CONSTRAINT DF_ds_is_primary DEFAULT 0,
    CONSTRAINT PK_doctor_specialties PRIMARY KEY (doctor_specialty_id),
    CONSTRAINT UQ_doctor_specialty   UNIQUE (user_id, specialty_id),
    CONSTRAINT FK_ds_user            FOREIGN KEY (user_id)      REFERENCES dbo.users       (user_id)      ON DELETE CASCADE,
    CONSTRAINT FK_ds_specialty       FOREIGN KEY (specialty_id) REFERENCES dbo.specialties (specialty_id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_ds_specialty_id ON dbo.doctor_specialties (specialty_id);
GO

-- ============================================================
-- TABLE: doctor_schedules
-- ============================================================
IF OBJECT_ID('dbo.doctor_schedules', 'U') IS NULL
CREATE TABLE dbo.doctor_schedules (
    schedule_id             INT      NOT NULL IDENTITY(1,1),
    user_id                 INT      NOT NULL,
    day_of_week             TINYINT  NOT NULL,
    start_time              TIME     NOT NULL,
    end_time                TIME     NOT NULL,
    slot_duration_mins      INT      NOT NULL CONSTRAINT DF_sched_duration   DEFAULT 30,
    max_patients_per_slot   INT      NOT NULL CONSTRAINT DF_sched_capacity   DEFAULT 1,
    is_active               BIT      NOT NULL CONSTRAINT DF_sched_active     DEFAULT 1,
    effective_from          DATE     NULL,
    effective_to            DATE     NULL,
    created_at              DATETIME NOT NULL CONSTRAINT DF_sched_created_at DEFAULT CURRENT_TIMESTAMP,
    updated_at              DATETIME NOT NULL CONSTRAINT DF_sched_updated_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_doctor_schedules PRIMARY KEY (schedule_id),
    CONSTRAINT FK_sched_user       FOREIGN KEY (user_id) REFERENCES dbo.users (user_id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_sched_user        ON dbo.doctor_schedules (user_id);
CREATE INDEX IX_sched_day_of_week ON dbo.doctor_schedules (day_of_week);
GO

-- ============================================================
-- TABLE: time_slots
-- ============================================================
IF OBJECT_ID('dbo.time_slots', 'U') IS NULL
CREATE TABLE dbo.time_slots (
    slot_id         INT      NOT NULL IDENTITY(1,1),
    schedule_id     INT      NOT NULL,
    user_id         INT      NOT NULL,
    slot_date       DATE     NOT NULL,
    start_time      TIME     NOT NULL,
    end_time        TIME     NOT NULL,
    max_capacity    INT      NOT NULL CONSTRAINT DF_ts_max_capacity DEFAULT 1,
    booked_count    INT      NOT NULL CONSTRAINT DF_ts_booked_count DEFAULT 0,
    is_available    BIT      NOT NULL CONSTRAINT DF_ts_is_available DEFAULT 1,
    created_at      DATETIME NOT NULL CONSTRAINT DF_ts_created_at   DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_time_slots  PRIMARY KEY (slot_id),
    CONSTRAINT FK_ts_schedule FOREIGN KEY (schedule_id) REFERENCES dbo.doctor_schedules (schedule_id),
    CONSTRAINT FK_ts_user     FOREIGN KEY (user_id)     REFERENCES dbo.users            (user_id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_ts_doctor_date ON dbo.time_slots (user_id, slot_date);
CREATE INDEX IX_ts_schedule    ON dbo.time_slots (schedule_id);
GO

-- ============================================================
-- TABLE: appointments
-- ============================================================
IF OBJECT_ID('dbo.appointments', 'U') IS NULL
CREATE TABLE dbo.appointments (
    appointment_id      INT             NOT NULL IDENTITY(1,1),
    patient_id          INT             NOT NULL,
    doctor_id           INT             NOT NULL,
    slot_id             INT             NOT NULL,
    specialty_id        INT             NULL,
    appointment_date    DATE            NOT NULL,
    start_time          TIME            NOT NULL,
    end_time            TIME            NOT NULL,
    reason              NVARCHAR(MAX)   NULL,
    status              NVARCHAR(30)    NOT NULL CONSTRAINT DF_appt_status     DEFAULT 'PENDING',
    confirmed_at        DATETIME        NULL,
    cancelled_at        DATETIME        NULL,
    cancel_reason       NVARCHAR(MAX)   NULL,
    no_show_risk_score  DECIMAL(5,4)    NULL,
    notes               NVARCHAR(MAX)   NULL,
    created_at          DATETIME        NOT NULL CONSTRAINT DF_appt_created_at DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME        NOT NULL CONSTRAINT DF_appt_updated_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_appointments   PRIMARY KEY (appointment_id),
    CONSTRAINT CK_appt_status    CHECK (status IN (
                                    'PENDING','CONFIRMED',
                                    'CANCELLED_BY_PATIENT','CANCELLED_BY_DOCTOR',
                                    'COMPLETED','NO_SHOW')),
    CONSTRAINT FK_appt_patient   FOREIGN KEY (patient_id)   REFERENCES dbo.users       (user_id),
    CONSTRAINT FK_appt_doctor    FOREIGN KEY (doctor_id)    REFERENCES dbo.users       (user_id),
    CONSTRAINT FK_appt_slot      FOREIGN KEY (slot_id)      REFERENCES dbo.time_slots  (slot_id),
    CONSTRAINT FK_appt_specialty FOREIGN KEY (specialty_id) REFERENCES dbo.specialties (specialty_id)
);
GO
CREATE INDEX IX_appt_patient ON dbo.appointments (patient_id);
CREATE INDEX IX_appt_doctor  ON dbo.appointments (doctor_id);
CREATE INDEX IX_appt_slot    ON dbo.appointments (slot_id);
CREATE INDEX IX_appt_date    ON dbo.appointments (appointment_date);
CREATE INDEX IX_appt_status  ON dbo.appointments (status);
GO

-- ============================================================
-- TABLE: appointment_waitlists
-- ============================================================
IF OBJECT_ID('dbo.appointment_waitlists', 'U') IS NULL
CREATE TABLE dbo.appointment_waitlists (
    waitlist_id     INT             NOT NULL IDENTITY(1,1),
    patient_id      INT             NOT NULL,
    doctor_id       INT             NOT NULL,
    specialty_id    INT             NULL,
    preferred_date  DATE            NULL,
    preferred_time  TIME            NULL,
    status          NVARCHAR(20)    NOT NULL CONSTRAINT DF_wl_status     DEFAULT 'WAITING',
    notified_at     DATETIME        NULL,
    expires_at      DATETIME        NULL,
    created_at      DATETIME        NOT NULL CONSTRAINT DF_wl_created_at DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME        NOT NULL CONSTRAINT DF_wl_updated_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_appointment_waitlists PRIMARY KEY (waitlist_id),
    CONSTRAINT CK_wl_status    CHECK  (status IN ('WAITING','NOTIFIED','BOOKED','EXPIRED','CANCELLED')),
    CONSTRAINT FK_wl_patient   FOREIGN KEY (patient_id)  REFERENCES dbo.users       (user_id),
    CONSTRAINT FK_wl_doctor    FOREIGN KEY (doctor_id)   REFERENCES dbo.users       (user_id),
    CONSTRAINT FK_wl_specialty FOREIGN KEY (specialty_id) REFERENCES dbo.specialties (specialty_id)
);
GO
CREATE INDEX IX_wl_patient ON dbo.appointment_waitlists (patient_id);
CREATE INDEX IX_wl_doctor  ON dbo.appointment_waitlists (doctor_id);
GO

-- ============================================================
-- TABLE: payments
-- ============================================================
IF OBJECT_ID('dbo.payments', 'U') IS NULL
CREATE TABLE dbo.payments (
    payment_id      INT             NOT NULL IDENTITY(1,1),
    appointment_id  INT             NOT NULL,
    patient_id      INT             NOT NULL,
    amount          DECIMAL(12,2)   NOT NULL,
    currency        NVARCHAR(10)    NOT NULL CONSTRAINT DF_pay_currency DEFAULT 'VND',
    payment_method  NVARCHAR(30)    NOT NULL,
    payment_status  NVARCHAR(20)    NOT NULL CONSTRAINT DF_pay_status   DEFAULT 'PENDING',
    transaction_id  NVARCHAR(200)   NULL,
    paid_at         DATETIME        NULL,
    refunded_at     DATETIME        NULL,
    refund_amount   DECIMAL(12,2)   NULL,
    created_at      DATETIME        NOT NULL CONSTRAINT DF_pay_created_at DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME        NOT NULL CONSTRAINT DF_pay_updated_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_payments        PRIMARY KEY (payment_id),
    CONSTRAINT CK_pay_method      CHECK (payment_method IN ('CASH','BANK_TRANSFER','MOMO','VNPAY','ZALOPAY','CREDIT_CARD')),
    CONSTRAINT CK_pay_status      CHECK (payment_status IN ('PENDING','COMPLETED','FAILED','REFUNDED')),
    CONSTRAINT FK_pay_appointment FOREIGN KEY (appointment_id) REFERENCES dbo.appointments (appointment_id),
    CONSTRAINT FK_pay_patient     FOREIGN KEY (patient_id)     REFERENCES dbo.users        (user_id)
);
GO
CREATE INDEX IX_pay_appointment ON dbo.payments (appointment_id);
CREATE INDEX IX_pay_patient     ON dbo.payments (patient_id);
GO

-- ============================================================
-- TABLE: reviews
-- ============================================================
IF OBJECT_ID('dbo.reviews', 'U') IS NULL
CREATE TABLE dbo.reviews (
    review_id       INT             NOT NULL IDENTITY(1,1),
    appointment_id  INT             NOT NULL,
    patient_id      INT             NOT NULL,
    doctor_id       INT             NOT NULL,
    rating          TINYINT         NOT NULL,
    comment         NVARCHAR(MAX)   NULL,
    sentiment_score DECIMAL(5,4)    NULL,
    sentiment_label NVARCHAR(20)    NULL,
    is_visible      BIT             NOT NULL CONSTRAINT DF_rev_is_visible DEFAULT 1,
    created_at      DATETIME        NOT NULL CONSTRAINT DF_rev_created_at DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME        NOT NULL CONSTRAINT DF_rev_updated_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_reviews            PRIMARY KEY (review_id),
    CONSTRAINT UQ_appt_review        UNIQUE (appointment_id),
    CONSTRAINT CK_rev_rating         CHECK (rating BETWEEN 1 AND 5),
    CONSTRAINT CK_rev_sentiment      CHECK (sentiment_label IN ('POSITIVE','NEUTRAL','NEGATIVE')),
    CONSTRAINT FK_rev_appointment    FOREIGN KEY (appointment_id) REFERENCES dbo.appointments (appointment_id),
    CONSTRAINT FK_rev_patient        FOREIGN KEY (patient_id)     REFERENCES dbo.users        (user_id),
    CONSTRAINT FK_rev_doctor         FOREIGN KEY (doctor_id)      REFERENCES dbo.users        (user_id)
);
GO
CREATE INDEX IX_rev_doctor  ON dbo.reviews (doctor_id);
CREATE INDEX IX_rev_patient ON dbo.reviews (patient_id);
GO

-- ============================================================
-- TABLE: medical_records
-- ============================================================
IF OBJECT_ID('dbo.medical_records', 'U') IS NULL
CREATE TABLE dbo.medical_records (
    record_id       INT             NOT NULL IDENTITY(1,1),
    patient_id      INT             NOT NULL,
    doctor_id       INT             NOT NULL,
    appointment_id  INT             NULL,
    diagnosis       NVARCHAR(MAX)   NULL,
    symptoms        NVARCHAR(MAX)   NULL,
    treatment_plan  NVARCHAR(MAX)   NULL,
    prescription    NVARCHAR(MAX)   NULL,
    notes           NVARCHAR(MAX)   NULL,
    record_date     DATE            NOT NULL,
    created_at      DATETIME        NOT NULL CONSTRAINT DF_mr_created_at DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME        NOT NULL CONSTRAINT DF_mr_updated_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_medical_records PRIMARY KEY (record_id),
    CONSTRAINT FK_mr_patient      FOREIGN KEY (patient_id)     REFERENCES dbo.users        (user_id),
    CONSTRAINT FK_mr_doctor       FOREIGN KEY (doctor_id)      REFERENCES dbo.users        (user_id),
    CONSTRAINT FK_mr_appointment  FOREIGN KEY (appointment_id) REFERENCES dbo.appointments (appointment_id)
);
GO
CREATE INDEX IX_mr_patient     ON dbo.medical_records (patient_id);
CREATE INDEX IX_mr_doctor      ON dbo.medical_records (doctor_id);
CREATE INDEX IX_mr_appointment ON dbo.medical_records (appointment_id);
GO

-- ============================================================
-- TABLE: notifications
-- ============================================================
IF OBJECT_ID('dbo.notifications', 'U') IS NULL
CREATE TABLE dbo.notifications (
    notification_id     INT             NOT NULL IDENTITY(1,1),
    user_id             INT             NOT NULL,
    appointment_id      INT             NULL,
    notification_type   NVARCHAR(40)    NOT NULL,
    channel             NVARCHAR(20)    NOT NULL,
    title               NVARCHAR(300)   NOT NULL,
    body                NVARCHAR(MAX)   NOT NULL,
    is_read             BIT             NOT NULL CONSTRAINT DF_notif_is_read   DEFAULT 0,
    sent_at             DATETIME        NULL,
    read_at             DATETIME        NULL,
    status              NVARCHAR(20)    NOT NULL CONSTRAINT DF_notif_status    DEFAULT 'PENDING',
    created_at          DATETIME        NOT NULL CONSTRAINT DF_notif_created_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_notifications     PRIMARY KEY (notification_id),
    CONSTRAINT CK_notif_type        CHECK (notification_type IN (
                                        'BOOKING_CONFIRMATION','APPOINTMENT_REMINDER',
                                        'APPOINTMENT_CANCELLATION','APPOINTMENT_RESCHEDULE',
                                        'DOCTOR_CONFIRMATION','AI_SMART_REMINDER',
                                        'NO_SHOW_ALERT','PROMOTIONAL','SYSTEM')),
    CONSTRAINT CK_notif_channel     CHECK (channel IN ('EMAIL','SMS','ZALO','IN_APP','PUSH')),
    CONSTRAINT CK_notif_status      CHECK (status  IN ('PENDING','SENT','FAILED')),
    CONSTRAINT FK_notif_user        FOREIGN KEY (user_id)        REFERENCES dbo.users        (user_id) ON DELETE CASCADE,
    CONSTRAINT FK_notif_appointment FOREIGN KEY (appointment_id) REFERENCES dbo.appointments (appointment_id) ON DELETE SET NULL
);
GO
CREATE INDEX IX_notif_user        ON dbo.notifications (user_id);
CREATE INDEX IX_notif_appointment ON dbo.notifications (appointment_id);
CREATE INDEX IX_notif_status      ON dbo.notifications (status);
GO

-- ============================================================
-- TABLE: complaints
-- ============================================================
IF OBJECT_ID('dbo.complaints', 'U') IS NULL
CREATE TABLE dbo.complaints (
    complaint_id    INT             NOT NULL IDENTITY(1,1),
    patient_id      INT             NOT NULL,
    doctor_id       INT             NULL,
    appointment_id  INT             NULL,
    subject         NVARCHAR(300)   NOT NULL,
    description     NVARCHAR(MAX)   NOT NULL,
    status          NVARCHAR(20)    NOT NULL CONSTRAINT DF_comp_status     DEFAULT 'OPEN',
    resolved_by     INT             NULL,
    resolution_note NVARCHAR(MAX)   NULL,
    resolved_at     DATETIME        NULL,
    created_at      DATETIME        NOT NULL CONSTRAINT DF_comp_created_at DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME        NOT NULL CONSTRAINT DF_comp_updated_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_complaints       PRIMARY KEY (complaint_id),
    CONSTRAINT CK_comp_status      CHECK (status IN ('OPEN','IN_REVIEW','RESOLVED','CLOSED')),
    CONSTRAINT FK_comp_patient     FOREIGN KEY (patient_id)     REFERENCES dbo.users        (user_id),
    CONSTRAINT FK_comp_doctor      FOREIGN KEY (doctor_id)      REFERENCES dbo.users        (user_id),
    CONSTRAINT FK_comp_appointment FOREIGN KEY (appointment_id) REFERENCES dbo.appointments (appointment_id),
    CONSTRAINT FK_comp_resolved_by FOREIGN KEY (resolved_by)    REFERENCES dbo.users        (user_id)
);
GO
CREATE INDEX IX_comp_patient     ON dbo.complaints (patient_id);
CREATE INDEX IX_comp_doctor      ON dbo.complaints (doctor_id);
CREATE INDEX IX_comp_appointment ON dbo.complaints (appointment_id);
GO

-- ============================================================
-- TABLE: ai_recommendations
-- ============================================================
IF OBJECT_ID('dbo.ai_recommendations', 'U') IS NULL
CREATE TABLE dbo.ai_recommendations (
    recommendation_id   INT             NOT NULL IDENTITY(1,1),
    patient_id          INT             NOT NULL,
    input_symptoms      NVARCHAR(MAX)   NOT NULL,
    recommended_doctors NVARCHAR(MAX)   NULL,
    recommended_slots   NVARCHAR(MAX)   NULL,
    specialty_suggested INT             NULL,
    model_version       NVARCHAR(50)    NULL,
    created_at          DATETIME        NOT NULL CONSTRAINT DF_airec_created_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_ai_recommendations PRIMARY KEY (recommendation_id),
    CONSTRAINT FK_airec_patient      FOREIGN KEY (patient_id)          REFERENCES dbo.users       (user_id) ON DELETE CASCADE,
    CONSTRAINT FK_airec_specialty    FOREIGN KEY (specialty_suggested)  REFERENCES dbo.specialties (specialty_id)
);
GO
CREATE INDEX IX_airec_patient   ON dbo.ai_recommendations (patient_id);
CREATE INDEX IX_airec_specialty ON dbo.ai_recommendations (specialty_suggested);
GO

-- ============================================================
-- TABLE: system_logs
-- ============================================================
IF OBJECT_ID('dbo.system_logs', 'U') IS NULL
CREATE TABLE dbo.system_logs (
    log_id      BIGINT          NOT NULL IDENTITY(1,1),
    user_id     INT             NULL,
    action      NVARCHAR(200)   NOT NULL,
    entity_type NVARCHAR(100)   NULL,
    entity_id   INT             NULL,
    description NVARCHAR(MAX)   NULL,
    ip_address  NVARCHAR(45)    NULL,
    user_agent  NVARCHAR(500)   NULL,
    severity    NVARCHAR(20)    NOT NULL CONSTRAINT DF_slog_severity   DEFAULT 'INFO',
    created_at  DATETIME        NOT NULL CONSTRAINT DF_slog_created_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_system_logs   PRIMARY KEY (log_id),
    CONSTRAINT CK_slog_severity CHECK (severity IN ('INFO','WARNING','ERROR','CRITICAL')),
    CONSTRAINT FK_slog_user     FOREIGN KEY (user_id) REFERENCES dbo.users (user_id) ON DELETE SET NULL
);
GO
CREATE INDEX IX_slog_user       ON dbo.system_logs (user_id);
CREATE INDEX IX_slog_action     ON dbo.system_logs (action);
CREATE INDEX IX_slog_severity   ON dbo.system_logs (severity);
CREATE INDEX IX_slog_created_at ON dbo.system_logs (created_at);
GO

-- ============================================================
-- TABLE: external_calendar_integrations
-- ============================================================
IF OBJECT_ID('dbo.external_calendar_integrations', 'U') IS NULL
CREATE TABLE dbo.external_calendar_integrations (
    integration_id   INT             NOT NULL IDENTITY(1,1),
    user_id          INT             NOT NULL,
    provider         NVARCHAR(50)    NOT NULL CONSTRAINT DF_eci_provider  DEFAULT 'GOOGLE_CALENDAR',
    access_token     NVARCHAR(MAX)   NULL,
    refresh_token    NVARCHAR(MAX)   NULL,
    token_expires_at DATETIME        NULL,
    calendar_id      NVARCHAR(200)   NULL,
    is_active        BIT             NOT NULL CONSTRAINT DF_eci_is_active  DEFAULT 1,
    last_synced_at   DATETIME        NULL,
    created_at       DATETIME        NOT NULL CONSTRAINT DF_eci_created_at DEFAULT CURRENT_TIMESTAMP,
    updated_at       DATETIME        NOT NULL CONSTRAINT DF_eci_updated_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_eci               PRIMARY KEY (integration_id),
    CONSTRAINT UQ_eci_user_provider UNIQUE (user_id, provider),
    CONSTRAINT FK_eci_user          FOREIGN KEY (user_id) REFERENCES dbo.users (user_id) ON DELETE CASCADE
);
GO

-- ============================================================
-- TABLE: promotional_campaigns
-- ============================================================
IF OBJECT_ID('dbo.promotional_campaigns', 'U') IS NULL
CREATE TABLE dbo.promotional_campaigns (
    campaign_id INT             NOT NULL IDENTITY(1,1),
    title       NVARCHAR(300)   NOT NULL,
    message     NVARCHAR(MAX)   NOT NULL,
    target_role NVARCHAR(20)    NOT NULL CONSTRAINT DF_promo_target  DEFAULT 'PATIENT',
    start_date  DATE            NULL,
    end_date    DATE            NULL,
    created_by  INT             NOT NULL,
    is_sent     BIT             NOT NULL CONSTRAINT DF_promo_is_sent  DEFAULT 0,
    sent_at     DATETIME        NULL,
    created_at  DATETIME        NOT NULL CONSTRAINT DF_promo_created_at DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_promotional_campaigns PRIMARY KEY (campaign_id),
    CONSTRAINT CK_promo_target_role     CHECK (target_role IN ('PATIENT','DOCTOR','ALL')),
    CONSTRAINT FK_promo_created_by      FOREIGN KEY (created_by) REFERENCES dbo.users (user_id)
);
GO
CREATE INDEX IX_promo_created_by ON dbo.promotional_campaigns (created_by);
GO

-- ============================================================
-- SEED DATA: Roles
-- ============================================================
SET IDENTITY_INSERT dbo.roles ON;
INSERT INTO dbo.roles (role_id, role_name, description) VALUES
    (1, 'PATIENT', 'Registered patient who can book appointments'),
    (2, 'DOCTOR',  'Healthcare professional managing schedules and appointments'),
    (3, 'ADMIN',   'System administrator with full access');
SET IDENTITY_INSERT dbo.roles OFF;
GO

-- ============================================================
-- SEED DATA: Specialties
-- ============================================================
INSERT INTO dbo.specialties (specialty_name, description) VALUES
    (N'General Medicine',        N'Primary care and general health checkups'),
    (N'Cardiology',              N'Heart and cardiovascular system disorders'),
    (N'Dermatology',             N'Skin, hair, and nail conditions'),
    (N'Neurology',               N'Disorders of the nervous system'),
    (N'Orthopedics',             N'Musculoskeletal system and bone disorders'),
    (N'Pediatrics',              N'Medical care for infants, children, and adolescents'),
    (N'Gynecology & Obstetrics', N'Female reproductive health and pregnancy'),
    (N'Ophthalmology',           N'Eye and vision disorders'),
    (N'ENT',                     N'Ear, nose, and throat conditions'),
    (N'Gastroenterology',        N'Digestive system disorders'),
    (N'Endocrinology',           N'Hormone and metabolic disorders'),
    (N'Oncology',                N'Cancer diagnosis and treatment'),
    (N'Psychiatry',              N'Mental health and behavioral disorders'),
    (N'Urology',                 N'Urinary tract and male reproductive disorders'),
    (N'Radiology',               N'Medical imaging and diagnosis');
GO

-- ============================================================
-- SEED DATA: Departments
-- ============================================================
INSERT INTO dbo.departments (department_name, description, location) VALUES
    (N'Emergency Department',  N'Handles emergency and critical care',    N'Ground Floor, Block A'),
    (N'Outpatient Department', N'Scheduled consultations and follow-ups', N'2nd Floor, Block B'),
    (N'Inpatient Department',  N'Hospitalized patient care',              N'3rd Floor, Block B'),
    (N'Diagnostic Center',     N'Lab tests, imaging, and diagnostics',    N'1st Floor, Block C'),
    (N'Pharmacy',              N'Medication dispensing',                  N'Ground Floor, Block C');
GO

-- ============================================================
-- ADDITIONAL SEED DATA: ~10 records/table (idempotent)
-- ============================================================

-- Roles: add test roles to reach richer sample data.
INSERT INTO dbo.roles (role_name, description)
SELECT v.role_name, v.description
FROM (VALUES
    ('NURSE',            N'Nursing staff role for operational testing'),
    ('RECEPTIONIST',     N'Front desk scheduling support role'),
    ('LAB_TECH',         N'Laboratory technician role'),
    ('PHARMACIST',       N'Pharmacy operations role'),
    ('INSURANCE_AGENT',  N'Insurance verification role'),
    ('SUPPORT_STAFF',    N'General support staff role'),
    ('AUDITOR',          N'Compliance and audit role'),
    ('SUPER_ADMIN',      N'Extended administration role for testing'),
    ('CARE_COORDINATOR', N'Patient care coordination role'),
    ('TRIAGE_BOT',       N'AI triage integration role')
) v(role_name, description)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.roles r WHERE r.role_name = v.role_name
);
GO

-- Specialties: add more options beyond the initial catalog.
INSERT INTO dbo.specialties (specialty_name, description)
SELECT v.specialty_name, v.description
FROM (VALUES
    (N'Sports Medicine',         N'Prevention and treatment of sports-related injuries'),
    (N'Nephrology',              N'Kidney care and renal disorders'),
    (N'Hematology',              N'Blood disorders and blood-related diseases'),
    (N'Pulmonology',             N'Lung and respiratory system conditions'),
    (N'Rheumatology',            N'Autoimmune and joint disorders'),
    (N'Geriatrics',              N'Comprehensive care for older adults'),
    (N'Infectious Diseases',     N'Diagnosis and treatment of infections'),
    (N'Physical Rehabilitation', N'Rehabilitation and functional recovery'),
    (N'Pain Management',         N'Chronic and acute pain treatment'),
    (N'Allergy & Immunology',    N'Allergic and immune system disorders')
) v(specialty_name, description)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.specialties s WHERE s.specialty_name = v.specialty_name
);
GO

-- Departments: add 10 more departments.
INSERT INTO dbo.departments (department_name, description, location)
SELECT v.department_name, v.description, v.location
FROM (VALUES
    (N'Cardiac Center',            N'Advanced heart care and monitoring',          N'4th Floor, Block A'),
    (N'Neuro Center',              N'Neurology and neurosurgery consultations',    N'5th Floor, Block A'),
    (N'Maternal Care Unit',        N'Pregnancy and prenatal services',             N'3rd Floor, Block D'),
    (N'Pediatric Care Unit',       N'Child-focused clinical services',              N'2nd Floor, Block D'),
    (N'Dermatology Clinic',        N'Skin treatment and cosmetic dermatology',      N'2nd Floor, Block E'),
    (N'Rehabilitation Unit',       N'Physical therapy and post-op rehabilitation',  N'1st Floor, Block E'),
    (N'Endoscopy Unit',            N'GI diagnostics and endoscopic procedures',     N'1st Floor, Block F'),
    (N'Oncology Day Care',         N'Cancer consultation and day treatment',        N'6th Floor, Block B'),
    (N'Mental Health Clinic',      N'Psychiatry and behavioral health support',     N'4th Floor, Block C'),
    (N'Vaccination Center',        N'Immunization and preventive care programs',    N'Ground Floor, Block D')
) v(department_name, description, location)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.departments d WHERE d.department_name = v.department_name
);
GO

-- Users: 1 admin + 10 doctors + 10 patients.
;WITH seed_users AS (
    SELECT * FROM (VALUES
        ('ADMIN',  N'System Admin',     'admin.seed@mediconnect.local',       '0900000000', 'OTHER', CAST('1988-01-01' AS DATE), N'HQ Office',      1),
        ('DOCTOR', N'Dr. An Pham',      'doctor01.seed@mediconnect.local',    '0910000001', 'MALE',  CAST('1984-02-10' AS DATE), N'District 1',     1),
        ('DOCTOR', N'Dr. Binh Tran',    'doctor02.seed@mediconnect.local',    '0910000002', 'MALE',  CAST('1983-03-12' AS DATE), N'Binh Thanh',     1),
        ('DOCTOR', N'Dr. Chau Nguyen',  'doctor03.seed@mediconnect.local',    '0910000003', 'FEMALE',CAST('1986-04-14' AS DATE), N'Thu Duc',        1),
        ('DOCTOR', N'Dr. Dung Le',      'doctor04.seed@mediconnect.local',    '0910000004', 'FEMALE',CAST('1982-05-16' AS DATE), N'Go Vap',         1),
        ('DOCTOR', N'Dr. Giang Vo',     'doctor05.seed@mediconnect.local',    '0910000005', 'FEMALE',CAST('1985-06-18' AS DATE), N'Phu Nhuan',      1),
        ('DOCTOR', N'Dr. Huy Do',       'doctor06.seed@mediconnect.local',    '0910000006', 'MALE',  CAST('1981-07-20' AS DATE), N'Tan Binh',       1),
        ('DOCTOR', N'Dr. Khanh Hoang',  'doctor07.seed@mediconnect.local',    '0910000007', 'MALE',  CAST('1987-08-22' AS DATE), N'Tan Phu',        1),
        ('DOCTOR', N'Dr. Linh Bui',     'doctor08.seed@mediconnect.local',    '0910000008', 'FEMALE',CAST('1989-09-24' AS DATE), N'District 7',     1),
        ('DOCTOR', N'Dr. Minh Dang',    'doctor09.seed@mediconnect.local',    '0910000009', 'MALE',  CAST('1980-10-26' AS DATE), N'District 3',     1),
        ('DOCTOR', N'Dr. Nga Phan',     'doctor10.seed@mediconnect.local',    '0910000010', 'FEMALE',CAST('1988-11-28' AS DATE), N'District 5',     1),
        ('PATIENT',N'Alice Nguyen',     'patient01.seed@mediconnect.local',   '0920000001', 'FEMALE',CAST('1995-01-05' AS DATE), N'District 10',    1),
        ('PATIENT',N'Bao Tran',         'patient02.seed@mediconnect.local',   '0920000002', 'MALE',  CAST('1992-02-06' AS DATE), N'District 11',    1),
        ('PATIENT',N'Chi Le',           'patient03.seed@mediconnect.local',   '0920000003', 'FEMALE',CAST('1998-03-07' AS DATE), N'District 12',    1),
        ('PATIENT',N'Duc Pham',         'patient04.seed@mediconnect.local',   '0920000004', 'MALE',  CAST('1991-04-08' AS DATE), N'Thu Duc',        1),
        ('PATIENT',N'Emi Ho',           'patient05.seed@mediconnect.local',   '0920000005', 'FEMALE',CAST('1997-05-09' AS DATE), N'Binh Tan',       1),
        ('PATIENT',N'Gia Vu',           'patient06.seed@mediconnect.local',   '0920000006', 'OTHER', CAST('1994-06-10' AS DATE), N'Phu Nhuan',      1),
        ('PATIENT',N'Hai Bui',          'patient07.seed@mediconnect.local',   '0920000007', 'MALE',  CAST('1990-07-11' AS DATE), N'Go Vap',         1),
        ('PATIENT',N'Iris Do',          'patient08.seed@mediconnect.local',   '0920000008', 'FEMALE',CAST('1996-08-12' AS DATE), N'District 8',     1),
        ('PATIENT',N'Khoa Dang',        'patient09.seed@mediconnect.local',   '0920000009', 'MALE',  CAST('1993-09-13' AS DATE), N'District 4',     1),
        ('PATIENT',N'Linh Truong',      'patient10.seed@mediconnect.local',   '0920000010', 'FEMALE',CAST('1999-10-14' AS DATE), N'District 6',     1)
    ) v(role_name, full_name, email, phone_number, gender, date_of_birth, address, is_verified)
)
INSERT INTO dbo.users (
    role_id, full_name, email, password_hash, phone_number, avatar_url, gender,
    date_of_birth, address, is_active, is_verified, last_login_at
)
SELECT
    r.role_id,
    s.full_name,
    s.email,
    N'SEED_HASH_DO_NOT_USE_IN_PRODUCTION',
    s.phone_number,
    NULL,
    s.gender,
    s.date_of_birth,
    s.address,
    1,
    s.is_verified,
    DATEADD(MINUTE, -10, CURRENT_TIMESTAMP)
FROM seed_users s
JOIN dbo.roles r ON r.role_name = s.role_name
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.users u WHERE u.email = s.email
);
GO

-- Password reset tokens: 10 records.
INSERT INTO dbo.password_reset_tokens (user_id, token, expires_at, is_used)
SELECT u.user_id, v.token, DATEADD(DAY, 2, CURRENT_TIMESTAMP), v.is_used
FROM (VALUES
    ('patient01.seed@mediconnect.local', N'seed-reset-token-01', 0),
    ('patient02.seed@mediconnect.local', N'seed-reset-token-02', 0),
    ('patient03.seed@mediconnect.local', N'seed-reset-token-03', 1),
    ('patient04.seed@mediconnect.local', N'seed-reset-token-04', 0),
    ('patient05.seed@mediconnect.local', N'seed-reset-token-05', 0),
    ('patient06.seed@mediconnect.local', N'seed-reset-token-06', 1),
    ('patient07.seed@mediconnect.local', N'seed-reset-token-07', 0),
    ('patient08.seed@mediconnect.local', N'seed-reset-token-08', 0),
    ('patient09.seed@mediconnect.local', N'seed-reset-token-09', 0),
    ('patient10.seed@mediconnect.local', N'seed-reset-token-10', 1)
) v(email, token, is_used)
JOIN dbo.users u ON u.email = v.email
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.password_reset_tokens prt WHERE prt.token = v.token
);
GO

-- Doctor profiles: 10 approved profiles.
INSERT INTO dbo.doctor_profiles (
    user_id, department_id, license_number, years_of_experience, education, bio,
    consultation_fee, insurance_accepted, location, approval_status, approved_by,
    approved_at, average_rating, total_reviews
)
SELECT
    du.user_id,
    d.department_id,
    v.license_number,
    v.years_of_experience,
    v.education,
    v.bio,
    v.consultation_fee,
    v.insurance_accepted,
    v.location,
    'APPROVED',
    au.user_id,
    CURRENT_TIMESTAMP,
    v.average_rating,
    v.total_reviews
FROM (VALUES
    ('doctor01.seed@mediconnect.local', N'LIC-MC-0001', 12, N'MD Internal Medicine', N'Focus on chronic disease management', 300000.00, N'Bao Viet, PTI', N'Room A101', 4.80, 120, N'Outpatient Department'),
    ('doctor02.seed@mediconnect.local', N'LIC-MC-0002', 14, N'MD Cardiology', N'Preventive cardiology specialist', 450000.00, N'Bao Minh, PVI', N'Room A201', 4.70, 98, N'Cardiac Center'),
    ('doctor03.seed@mediconnect.local', N'LIC-MC-0003', 10, N'MD Dermatology', N'Acne and eczema treatment', 350000.00, N'Bao Viet, VBI', N'Room E203', 4.60, 84, N'Dermatology Clinic'),
    ('doctor04.seed@mediconnect.local', N'LIC-MC-0004', 16, N'MD Neurology', N'Headache and stroke follow-up', 500000.00, N'PVI, PTI', N'Room A501', 4.90, 160, N'Neuro Center'),
    ('doctor05.seed@mediconnect.local', N'LIC-MC-0005', 11, N'MD Pediatrics', N'General pediatric care', 320000.00, N'Bao Viet, MIC', N'Room D205', 4.75, 110, N'Pediatric Care Unit'),
    ('doctor06.seed@mediconnect.local', N'LIC-MC-0006', 13, N'MD Orthopedics', N'Sports injury and bone health', 420000.00, N'PVI, VBI', N'Room B304', 4.65, 90, N'Rehabilitation Unit'),
    ('doctor07.seed@mediconnect.local', N'LIC-MC-0007', 9, N'MD ENT', N'Sinus and allergy consultations', 300000.00, N'Bao Minh, PTI', N'Room C112', 4.50, 70, N'Outpatient Department'),
    ('doctor08.seed@mediconnect.local', N'LIC-MC-0008', 15, N'MD Obstetrics', N'Prenatal and postnatal care', 480000.00, N'Bao Viet, PVI', N'Room D310', 4.85, 130, N'Maternal Care Unit'),
    ('doctor09.seed@mediconnect.local', N'LIC-MC-0009', 8, N'MD Psychiatry', N'Anxiety and sleep disorders', 380000.00, N'VBI, MIC', N'Room C405', 4.55, 60, N'Mental Health Clinic'),
    ('doctor10.seed@mediconnect.local', N'LIC-MC-0010', 17, N'MD Gastroenterology', N'Digestive disease diagnostics', 460000.00, N'Bao Viet, PTI', N'Room F109', 4.78, 140, N'Endoscopy Unit')
) v(email, license_number, years_of_experience, education, bio, consultation_fee, insurance_accepted, location, average_rating, total_reviews, department_name)
JOIN dbo.users du ON du.email = v.email
JOIN dbo.users au ON au.email = 'admin.seed@mediconnect.local'
JOIN dbo.departments d ON d.department_name = v.department_name
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.doctor_profiles dp WHERE dp.user_id = du.user_id
);
GO

-- Doctor specialties: 10 mappings.
INSERT INTO dbo.doctor_specialties (user_id, specialty_id, is_primary)
SELECT u.user_id, s.specialty_id, 1
FROM (VALUES
    ('doctor01.seed@mediconnect.local', N'General Medicine'),
    ('doctor02.seed@mediconnect.local', N'Cardiology'),
    ('doctor03.seed@mediconnect.local', N'Dermatology'),
    ('doctor04.seed@mediconnect.local', N'Neurology'),
    ('doctor05.seed@mediconnect.local', N'Pediatrics'),
    ('doctor06.seed@mediconnect.local', N'Orthopedics'),
    ('doctor07.seed@mediconnect.local', N'ENT'),
    ('doctor08.seed@mediconnect.local', N'Gynecology & Obstetrics'),
    ('doctor09.seed@mediconnect.local', N'Psychiatry'),
    ('doctor10.seed@mediconnect.local', N'Gastroenterology')
) v(email, specialty_name)
JOIN dbo.users u ON u.email = v.email
JOIN dbo.specialties s ON s.specialty_name = v.specialty_name
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.doctor_specialties ds
    WHERE ds.user_id = u.user_id AND ds.specialty_id = s.specialty_id
);
GO

-- Doctor schedules: 10 weekly schedules.
INSERT INTO dbo.doctor_schedules (
    user_id, day_of_week, start_time, end_time, slot_duration_mins,
    max_patients_per_slot, is_active, effective_from, effective_to
)
SELECT
    u.user_id,
    v.day_of_week,
    v.start_time,
    v.end_time,
    30,
    2,
    1,
    CAST('2026-01-01' AS DATE),
    NULL
FROM (VALUES
    ('doctor01.seed@mediconnect.local', 1, CAST('08:00:00' AS TIME), CAST('11:00:00' AS TIME)),
    ('doctor02.seed@mediconnect.local', 2, CAST('08:30:00' AS TIME), CAST('11:30:00' AS TIME)),
    ('doctor03.seed@mediconnect.local', 3, CAST('09:00:00' AS TIME), CAST('12:00:00' AS TIME)),
    ('doctor04.seed@mediconnect.local', 4, CAST('13:00:00' AS TIME), CAST('16:00:00' AS TIME)),
    ('doctor05.seed@mediconnect.local', 5, CAST('08:00:00' AS TIME), CAST('11:00:00' AS TIME)),
    ('doctor06.seed@mediconnect.local', 1, CAST('13:30:00' AS TIME), CAST('16:30:00' AS TIME)),
    ('doctor07.seed@mediconnect.local', 2, CAST('14:00:00' AS TIME), CAST('17:00:00' AS TIME)),
    ('doctor08.seed@mediconnect.local', 3, CAST('08:00:00' AS TIME), CAST('11:00:00' AS TIME)),
    ('doctor09.seed@mediconnect.local', 4, CAST('09:30:00' AS TIME), CAST('12:30:00' AS TIME)),
    ('doctor10.seed@mediconnect.local', 5, CAST('13:00:00' AS TIME), CAST('16:00:00' AS TIME))
) v(email, day_of_week, start_time, end_time)
JOIN dbo.users u ON u.email = v.email
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.doctor_schedules ds
    WHERE ds.user_id = u.user_id
      AND ds.day_of_week = v.day_of_week
      AND ds.start_time = v.start_time
      AND ds.end_time = v.end_time
);
GO

-- Time slots: 10 concrete slots.
INSERT INTO dbo.time_slots (
    schedule_id, user_id, slot_date, start_time, end_time,
    max_capacity, booked_count, is_available
)
SELECT
    ds.schedule_id,
    du.user_id,
    v.slot_date,
    v.start_time,
    v.end_time,
    2,
    0,
    1
FROM (VALUES
    ('doctor01.seed@mediconnect.local', CAST('2026-04-06' AS DATE), CAST('08:00:00' AS TIME), CAST('08:30:00' AS TIME)),
    ('doctor02.seed@mediconnect.local', CAST('2026-04-07' AS DATE), CAST('08:30:00' AS TIME), CAST('09:00:00' AS TIME)),
    ('doctor03.seed@mediconnect.local', CAST('2026-04-08' AS DATE), CAST('09:00:00' AS TIME), CAST('09:30:00' AS TIME)),
    ('doctor04.seed@mediconnect.local', CAST('2026-04-09' AS DATE), CAST('13:00:00' AS TIME), CAST('13:30:00' AS TIME)),
    ('doctor05.seed@mediconnect.local', CAST('2026-04-10' AS DATE), CAST('08:00:00' AS TIME), CAST('08:30:00' AS TIME)),
    ('doctor06.seed@mediconnect.local', CAST('2026-04-13' AS DATE), CAST('13:30:00' AS TIME), CAST('14:00:00' AS TIME)),
    ('doctor07.seed@mediconnect.local', CAST('2026-04-14' AS DATE), CAST('14:00:00' AS TIME), CAST('14:30:00' AS TIME)),
    ('doctor08.seed@mediconnect.local', CAST('2026-04-15' AS DATE), CAST('08:00:00' AS TIME), CAST('08:30:00' AS TIME)),
    ('doctor09.seed@mediconnect.local', CAST('2026-04-16' AS DATE), CAST('09:30:00' AS TIME), CAST('10:00:00' AS TIME)),
    ('doctor10.seed@mediconnect.local', CAST('2026-04-17' AS DATE), CAST('13:00:00' AS TIME), CAST('13:30:00' AS TIME))
) v(email, slot_date, start_time, end_time)
JOIN dbo.users du ON du.email = v.email
JOIN dbo.doctor_schedules ds ON ds.user_id = du.user_id
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.time_slots ts
    WHERE ts.user_id = du.user_id
      AND ts.slot_date = v.slot_date
      AND ts.start_time = v.start_time
      AND ts.end_time = v.end_time
);
GO

-- Appointments: 10 records.
INSERT INTO dbo.appointments (
    patient_id, doctor_id, slot_id, specialty_id, appointment_date,
    start_time, end_time, reason, status, confirmed_at, notes
)
SELECT
    pu.user_id,
    du.user_id,
    ts.slot_id,
    ds.specialty_id,
    v.slot_date,
    v.start_time,
    v.end_time,
    v.reason,
    v.status,
    CASE WHEN v.status IN ('CONFIRMED','COMPLETED') THEN CURRENT_TIMESTAMP ELSE NULL END,
    N'Seed appointment record'
FROM (VALUES
    ('patient01.seed@mediconnect.local','doctor01.seed@mediconnect.local', CAST('2026-04-06' AS DATE), CAST('08:00:00' AS TIME), CAST('08:30:00' AS TIME), N'Routine checkup',           'COMPLETED'),
    ('patient02.seed@mediconnect.local','doctor02.seed@mediconnect.local', CAST('2026-04-07' AS DATE), CAST('08:30:00' AS TIME), CAST('09:00:00' AS TIME), N'Chest discomfort',          'COMPLETED'),
    ('patient03.seed@mediconnect.local','doctor03.seed@mediconnect.local', CAST('2026-04-08' AS DATE), CAST('09:00:00' AS TIME), CAST('09:30:00' AS TIME), N'Skin irritation',           'COMPLETED'),
    ('patient04.seed@mediconnect.local','doctor04.seed@mediconnect.local', CAST('2026-04-09' AS DATE), CAST('13:00:00' AS TIME), CAST('13:30:00' AS TIME), N'Persistent headaches',       'COMPLETED'),
    ('patient05.seed@mediconnect.local','doctor05.seed@mediconnect.local', CAST('2026-04-10' AS DATE), CAST('08:00:00' AS TIME), CAST('08:30:00' AS TIME), N'Child fever follow-up',      'COMPLETED'),
    ('patient06.seed@mediconnect.local','doctor06.seed@mediconnect.local', CAST('2026-04-13' AS DATE), CAST('13:30:00' AS TIME), CAST('14:00:00' AS TIME), N'Knee pain after exercise',   'COMPLETED'),
    ('patient07.seed@mediconnect.local','doctor07.seed@mediconnect.local', CAST('2026-04-14' AS DATE), CAST('14:00:00' AS TIME), CAST('14:30:00' AS TIME), N'Chronic sinus symptoms',     'CONFIRMED'),
    ('patient08.seed@mediconnect.local','doctor08.seed@mediconnect.local', CAST('2026-04-15' AS DATE), CAST('08:00:00' AS TIME), CAST('08:30:00' AS TIME), N'Prenatal consultation',      'CONFIRMED'),
    ('patient09.seed@mediconnect.local','doctor09.seed@mediconnect.local', CAST('2026-04-16' AS DATE), CAST('09:30:00' AS TIME), CAST('10:00:00' AS TIME), N'Sleep quality issues',       'PENDING'),
    ('patient10.seed@mediconnect.local','doctor10.seed@mediconnect.local', CAST('2026-04-17' AS DATE), CAST('13:00:00' AS TIME), CAST('13:30:00' AS TIME), N'Upper abdominal discomfort', 'PENDING')
) v(patient_email, doctor_email, slot_date, start_time, end_time, reason, status)
JOIN dbo.users pu ON pu.email = v.patient_email
JOIN dbo.users du ON du.email = v.doctor_email
JOIN dbo.time_slots ts
    ON ts.user_id = du.user_id
   AND ts.slot_date = v.slot_date
   AND ts.start_time = v.start_time
   AND ts.end_time = v.end_time
LEFT JOIN dbo.doctor_specialties ds ON ds.user_id = du.user_id AND ds.is_primary = 1
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.appointments a WHERE a.slot_id = ts.slot_id
);
GO

-- Appointment waitlists: 10 records.
INSERT INTO dbo.appointment_waitlists (
    patient_id, doctor_id, specialty_id, preferred_date, preferred_time,
    status, notified_at, expires_at
)
SELECT
    pu.user_id,
    du.user_id,
    ds.specialty_id,
    v.preferred_date,
    v.preferred_time,
    v.status,
    CASE WHEN v.status = 'NOTIFIED' THEN CURRENT_TIMESTAMP ELSE NULL END,
    DATEADD(DAY, 2, CURRENT_TIMESTAMP)
FROM (VALUES
    ('patient01.seed@mediconnect.local','doctor02.seed@mediconnect.local', CAST('2026-04-20' AS DATE), CAST('09:00:00' AS TIME), 'WAITING'),
    ('patient02.seed@mediconnect.local','doctor03.seed@mediconnect.local', CAST('2026-04-21' AS DATE), CAST('10:00:00' AS TIME), 'WAITING'),
    ('patient03.seed@mediconnect.local','doctor04.seed@mediconnect.local', CAST('2026-04-22' AS DATE), CAST('14:00:00' AS TIME), 'NOTIFIED'),
    ('patient04.seed@mediconnect.local','doctor05.seed@mediconnect.local', CAST('2026-04-23' AS DATE), CAST('08:30:00' AS TIME), 'WAITING'),
    ('patient05.seed@mediconnect.local','doctor06.seed@mediconnect.local', CAST('2026-04-24' AS DATE), CAST('15:00:00' AS TIME), 'WAITING'),
    ('patient06.seed@mediconnect.local','doctor07.seed@mediconnect.local', CAST('2026-04-25' AS DATE), CAST('16:00:00' AS TIME), 'WAITING'),
    ('patient07.seed@mediconnect.local','doctor08.seed@mediconnect.local', CAST('2026-04-26' AS DATE), CAST('09:30:00' AS TIME), 'NOTIFIED'),
    ('patient08.seed@mediconnect.local','doctor09.seed@mediconnect.local', CAST('2026-04-27' AS DATE), CAST('10:30:00' AS TIME), 'WAITING'),
    ('patient09.seed@mediconnect.local','doctor10.seed@mediconnect.local', CAST('2026-04-28' AS DATE), CAST('13:30:00' AS TIME), 'WAITING'),
    ('patient10.seed@mediconnect.local','doctor01.seed@mediconnect.local', CAST('2026-04-29' AS DATE), CAST('08:30:00' AS TIME), 'WAITING')
) v(patient_email, doctor_email, preferred_date, preferred_time, status)
JOIN dbo.users pu ON pu.email = v.patient_email
JOIN dbo.users du ON du.email = v.doctor_email
LEFT JOIN dbo.doctor_specialties ds ON ds.user_id = du.user_id AND ds.is_primary = 1
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.appointment_waitlists wl
    WHERE wl.patient_id = pu.user_id
      AND wl.doctor_id = du.user_id
      AND wl.preferred_date = v.preferred_date
);
GO

-- Payments: 10 records.
INSERT INTO dbo.payments (
    appointment_id, patient_id, amount, currency, payment_method, payment_status,
    transaction_id, paid_at, refunded_at, refund_amount
)
SELECT
    a.appointment_id,
    a.patient_id,
    v.amount,
    'VND',
    v.payment_method,
    v.payment_status,
    v.transaction_id,
    CASE WHEN v.payment_status IN ('COMPLETED','REFUNDED') THEN CURRENT_TIMESTAMP ELSE NULL END,
    CASE WHEN v.payment_status = 'REFUNDED' THEN DATEADD(HOUR, 1, CURRENT_TIMESTAMP) ELSE NULL END,
    CASE WHEN v.payment_status = 'REFUNDED' THEN v.amount ELSE NULL END
FROM (VALUES
    ('patient01.seed@mediconnect.local','doctor01.seed@mediconnect.local', CAST('2026-04-06' AS DATE), 300000.00, 'MOMO',         'COMPLETED', N'TXN-SEED-0001'),
    ('patient02.seed@mediconnect.local','doctor02.seed@mediconnect.local', CAST('2026-04-07' AS DATE), 450000.00, 'VNPAY',        'COMPLETED', N'TXN-SEED-0002'),
    ('patient03.seed@mediconnect.local','doctor03.seed@mediconnect.local', CAST('2026-04-08' AS DATE), 350000.00, 'BANK_TRANSFER','COMPLETED', N'TXN-SEED-0003'),
    ('patient04.seed@mediconnect.local','doctor04.seed@mediconnect.local', CAST('2026-04-09' AS DATE), 500000.00, 'CREDIT_CARD',  'COMPLETED', N'TXN-SEED-0004'),
    ('patient05.seed@mediconnect.local','doctor05.seed@mediconnect.local', CAST('2026-04-10' AS DATE), 320000.00, 'ZALOPAY',      'COMPLETED', N'TXN-SEED-0005'),
    ('patient06.seed@mediconnect.local','doctor06.seed@mediconnect.local', CAST('2026-04-13' AS DATE), 420000.00, 'MOMO',         'REFUNDED',  N'TXN-SEED-0006'),
    ('patient07.seed@mediconnect.local','doctor07.seed@mediconnect.local', CAST('2026-04-14' AS DATE), 300000.00, 'CASH',         'PENDING',   N'TXN-SEED-0007'),
    ('patient08.seed@mediconnect.local','doctor08.seed@mediconnect.local', CAST('2026-04-15' AS DATE), 480000.00, 'VNPAY',        'PENDING',   N'TXN-SEED-0008'),
    ('patient09.seed@mediconnect.local','doctor09.seed@mediconnect.local', CAST('2026-04-16' AS DATE), 380000.00, 'BANK_TRANSFER','FAILED',    N'TXN-SEED-0009'),
    ('patient10.seed@mediconnect.local','doctor10.seed@mediconnect.local', CAST('2026-04-17' AS DATE), 460000.00, 'MOMO',         'PENDING',   N'TXN-SEED-0010')
) v(patient_email, doctor_email, appointment_date, amount, payment_method, payment_status, transaction_id)
JOIN dbo.users pu ON pu.email = v.patient_email
JOIN dbo.users du ON du.email = v.doctor_email
JOIN dbo.appointments a
    ON a.patient_id = pu.user_id
   AND a.doctor_id = du.user_id
   AND a.appointment_date = v.appointment_date
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.payments p WHERE p.appointment_id = a.appointment_id
);
GO

-- Reviews: 10 records (1 per appointment).
INSERT INTO dbo.reviews (
    appointment_id, patient_id, doctor_id, rating, comment,
    sentiment_score, sentiment_label, is_visible
)
SELECT
    a.appointment_id,
    a.patient_id,
    a.doctor_id,
    v.rating,
    v.comment,
    v.sentiment_score,
    v.sentiment_label,
    1
FROM (VALUES
    ('patient01.seed@mediconnect.local','doctor01.seed@mediconnect.local', CAST('2026-04-06' AS DATE), 5, N'Great experience',             0.9300, 'POSITIVE'),
    ('patient02.seed@mediconnect.local','doctor02.seed@mediconnect.local', CAST('2026-04-07' AS DATE), 5, N'Clear and helpful advice',     0.9100, 'POSITIVE'),
    ('patient03.seed@mediconnect.local','doctor03.seed@mediconnect.local', CAST('2026-04-08' AS DATE), 4, N'Treatment worked well',        0.7600, 'POSITIVE'),
    ('patient04.seed@mediconnect.local','doctor04.seed@mediconnect.local', CAST('2026-04-09' AS DATE), 5, N'Very professional doctor',      0.9400, 'POSITIVE'),
    ('patient05.seed@mediconnect.local','doctor05.seed@mediconnect.local', CAST('2026-04-10' AS DATE), 4, N'Friendly and patient-focused',  0.8100, 'POSITIVE'),
    ('patient06.seed@mediconnect.local','doctor06.seed@mediconnect.local', CAST('2026-04-13' AS DATE), 3, N'Average waiting time',          0.0500, 'NEUTRAL'),
    ('patient07.seed@mediconnect.local','doctor07.seed@mediconnect.local', CAST('2026-04-14' AS DATE), 4, N'Good explanation provided',     0.6700, 'POSITIVE'),
    ('patient08.seed@mediconnect.local','doctor08.seed@mediconnect.local', CAST('2026-04-15' AS DATE), 5, N'Excellent prenatal support',    0.9600, 'POSITIVE'),
    ('patient09.seed@mediconnect.local','doctor09.seed@mediconnect.local', CAST('2026-04-16' AS DATE), 3, N'Needs shorter queue time',      -0.1200, 'NEUTRAL'),
    ('patient10.seed@mediconnect.local','doctor10.seed@mediconnect.local', CAST('2026-04-17' AS DATE), 4, N'Accurate diagnosis and plan',   0.7200, 'POSITIVE')
) v(patient_email, doctor_email, appointment_date, rating, comment, sentiment_score, sentiment_label)
JOIN dbo.users pu ON pu.email = v.patient_email
JOIN dbo.users du ON du.email = v.doctor_email
JOIN dbo.appointments a
    ON a.patient_id = pu.user_id
   AND a.doctor_id = du.user_id
   AND a.appointment_date = v.appointment_date
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.reviews r WHERE r.appointment_id = a.appointment_id
);
GO

-- Medical records: 10 records.
INSERT INTO dbo.medical_records (
    patient_id, doctor_id, appointment_id, diagnosis, symptoms,
    treatment_plan, prescription, notes, record_date
)
SELECT
    a.patient_id,
    a.doctor_id,
    a.appointment_id,
    v.diagnosis,
    v.symptoms,
    v.treatment_plan,
    v.prescription,
    N'Seeded medical record',
    a.appointment_date
FROM (VALUES
    ('patient01.seed@mediconnect.local','doctor01.seed@mediconnect.local', CAST('2026-04-06' AS DATE), N'Mild gastritis',       N'Abdominal discomfort',      N'Diet adjustment + follow-up', N'Antacid x 7 days'),
    ('patient02.seed@mediconnect.local','doctor02.seed@mediconnect.local', CAST('2026-04-07' AS DATE), N'Hypertension stage 1', N'Intermittent chest tightness',N'Lifestyle changes + monitor', N'ARB daily'),
    ('patient03.seed@mediconnect.local','doctor03.seed@mediconnect.local', CAST('2026-04-08' AS DATE), N'Contact dermatitis',   N'Rash and itching',          N'Avoid trigger and topical',   N'Topical steroid cream'),
    ('patient04.seed@mediconnect.local','doctor04.seed@mediconnect.local', CAST('2026-04-09' AS DATE), N'Migraine',             N'Recurrent headache',        N'Headache diary + treatment',  N'Triptan as needed'),
    ('patient05.seed@mediconnect.local','doctor05.seed@mediconnect.local', CAST('2026-04-10' AS DATE), N'Viral fever',          N'Fever and fatigue',         N'Symptomatic care',            N'Paracetamol'),
    ('patient06.seed@mediconnect.local','doctor06.seed@mediconnect.local', CAST('2026-04-13' AS DATE), N'Knee tendon strain',   N'Pain with movement',        N'Rest + physiotherapy',        N'NSAID short course'),
    ('patient07.seed@mediconnect.local','doctor07.seed@mediconnect.local', CAST('2026-04-14' AS DATE), N'Allergic rhinitis',    N'Nasal congestion',          N'Allergy management plan',     N'Antihistamine'),
    ('patient08.seed@mediconnect.local','doctor08.seed@mediconnect.local', CAST('2026-04-15' AS DATE), N'Normal pregnancy',     N'Routine prenatal check',    N'Continue prenatal vitamins',  N'Folic acid'),
    ('patient09.seed@mediconnect.local','doctor09.seed@mediconnect.local', CAST('2026-04-16' AS DATE), N'Insomnia',             N'Difficulty sleeping',       N'Sleep hygiene counseling',    N'Melatonin low dose'),
    ('patient10.seed@mediconnect.local','doctor10.seed@mediconnect.local', CAST('2026-04-17' AS DATE), N'Functional dyspepsia', N'Upper abdominal fullness',  N'Diet + medication trial',     N'Prokinetic + antacid')
) v(patient_email, doctor_email, appointment_date, diagnosis, symptoms, treatment_plan, prescription)
JOIN dbo.users pu ON pu.email = v.patient_email
JOIN dbo.users du ON du.email = v.doctor_email
JOIN dbo.appointments a
    ON a.patient_id = pu.user_id
   AND a.doctor_id = du.user_id
   AND a.appointment_date = v.appointment_date
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.medical_records mr WHERE mr.appointment_id = a.appointment_id
);
GO

-- Notifications: 10 records.
INSERT INTO dbo.notifications (
    user_id, appointment_id, notification_type, channel,
    title, body, is_read, sent_at, read_at, status
)
SELECT
    u.user_id,
    a.appointment_id,
    v.notification_type,
    v.channel,
    v.title,
    v.body,
    v.is_read,
    CASE WHEN v.status = 'SENT' THEN CURRENT_TIMESTAMP ELSE NULL END,
    CASE WHEN v.is_read = 1 THEN CURRENT_TIMESTAMP ELSE NULL END,
    v.status
FROM (VALUES
    ('patient01.seed@mediconnect.local','doctor01.seed@mediconnect.local', CAST('2026-04-06' AS DATE), 'BOOKING_CONFIRMATION',  'IN_APP', N'Appointment confirmed', N'Your appointment has been confirmed.', 1, 'SENT'),
    ('patient02.seed@mediconnect.local','doctor02.seed@mediconnect.local', CAST('2026-04-07' AS DATE), 'APPOINTMENT_REMINDER',   'EMAIL',  N'Reminder',              N'Please arrive 15 minutes early.',      0, 'SENT'),
    ('patient03.seed@mediconnect.local','doctor03.seed@mediconnect.local', CAST('2026-04-08' AS DATE), 'APPOINTMENT_REMINDER',   'SMS',    N'Reminder',              N'Appointment starts in 1 hour.',         0, 'SENT'),
    ('patient04.seed@mediconnect.local','doctor04.seed@mediconnect.local', CAST('2026-04-09' AS DATE), 'APPOINTMENT_RESCHEDULE', 'IN_APP', N'Schedule update',       N'Your slot has been adjusted.',          0, 'PENDING'),
    ('patient05.seed@mediconnect.local','doctor05.seed@mediconnect.local', CAST('2026-04-10' AS DATE), 'DOCTOR_CONFIRMATION',    'EMAIL',  N'Doctor confirmed',      N'Doctor has accepted your request.',     0, 'SENT'),
    ('patient06.seed@mediconnect.local','doctor06.seed@mediconnect.local', CAST('2026-04-13' AS DATE), 'AI_SMART_REMINDER',      'PUSH',   N'Smart reminder',        N'Based on your history, follow-up soon.',0, 'SENT'),
    ('patient07.seed@mediconnect.local','doctor07.seed@mediconnect.local', CAST('2026-04-14' AS DATE), 'NO_SHOW_ALERT',          'IN_APP', N'No-show alert',         N'Please confirm attendance.',            0, 'FAILED'),
    ('patient08.seed@mediconnect.local','doctor08.seed@mediconnect.local', CAST('2026-04-15' AS DATE), 'PROMOTIONAL',            'ZALO',   N'Health campaign',       N'Join the annual checkup campaign.',     0, 'SENT'),
    ('patient09.seed@mediconnect.local','doctor09.seed@mediconnect.local', CAST('2026-04-16' AS DATE), 'SYSTEM',                 'IN_APP', N'System message',        N'Platform maintenance tonight.',         1, 'SENT'),
    ('patient10.seed@mediconnect.local','doctor10.seed@mediconnect.local', CAST('2026-04-17' AS DATE), 'APPOINTMENT_CANCELLATION','EMAIL', N'Appointment cancelled',  N'Please choose another slot.',           0, 'PENDING')
) v(patient_email, doctor_email, appointment_date, notification_type, channel, title, body, is_read, status)
JOIN dbo.users u ON u.email = v.patient_email
JOIN dbo.users d ON d.email = v.doctor_email
JOIN dbo.appointments a
    ON a.patient_id = u.user_id
   AND a.doctor_id = d.user_id
   AND a.appointment_date = v.appointment_date
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.notifications n
    WHERE n.user_id = u.user_id
      AND n.title = v.title
      AND n.appointment_id = a.appointment_id
);
GO

-- Complaints: 10 records.
INSERT INTO dbo.complaints (
    patient_id, doctor_id, appointment_id, subject, description,
    status, resolved_by, resolution_note, resolved_at
)
SELECT
    pu.user_id,
    du.user_id,
    a.appointment_id,
    v.subject,
    v.description,
    v.status,
    CASE WHEN v.status IN ('RESOLVED','CLOSED') THEN au.user_id ELSE NULL END,
    CASE WHEN v.status IN ('RESOLVED','CLOSED') THEN N'Handled by admin team' ELSE NULL END,
    CASE WHEN v.status IN ('RESOLVED','CLOSED') THEN CURRENT_TIMESTAMP ELSE NULL END
FROM (VALUES
    ('patient01.seed@mediconnect.local','doctor01.seed@mediconnect.local', CAST('2026-04-06' AS DATE), N'Long waiting time',        N'Waited longer than expected.',             'OPEN'),
    ('patient02.seed@mediconnect.local','doctor02.seed@mediconnect.local', CAST('2026-04-07' AS DATE), N'Billing clarification',    N'Need explanation on payment details.',     'IN_REVIEW'),
    ('patient03.seed@mediconnect.local','doctor03.seed@mediconnect.local', CAST('2026-04-08' AS DATE), N'Follow-up delay',          N'Follow-up message arrived late.',          'OPEN'),
    ('patient04.seed@mediconnect.local','doctor04.seed@mediconnect.local', CAST('2026-04-09' AS DATE), N'Appointment delay',        N'Appointment started 25 minutes late.',     'RESOLVED'),
    ('patient05.seed@mediconnect.local','doctor05.seed@mediconnect.local', CAST('2026-04-10' AS DATE), N'Refund request',           N'Requesting refund due to cancellation.',   'IN_REVIEW'),
    ('patient06.seed@mediconnect.local','doctor06.seed@mediconnect.local', CAST('2026-04-13' AS DATE), N'Prescription confusion',   N'Need clearer medication instructions.',    'OPEN'),
    ('patient07.seed@mediconnect.local','doctor07.seed@mediconnect.local', CAST('2026-04-14' AS DATE), N'Notification issue',       N'Did not receive SMS reminder.',            'CLOSED'),
    ('patient08.seed@mediconnect.local','doctor08.seed@mediconnect.local', CAST('2026-04-15' AS DATE), N'Profile information',      N'Doctor profile shown outdated info.',      'IN_REVIEW'),
    ('patient09.seed@mediconnect.local','doctor09.seed@mediconnect.local', CAST('2026-04-16' AS DATE), N'Queue management',         N'Queue handling could be improved.',        'OPEN'),
    ('patient10.seed@mediconnect.local','doctor10.seed@mediconnect.local', CAST('2026-04-17' AS DATE), N'Support response speed',   N'Support response was slower than expected.','RESOLVED')
) v(patient_email, doctor_email, appointment_date, subject, description, status)
JOIN dbo.users pu ON pu.email = v.patient_email
JOIN dbo.users du ON du.email = v.doctor_email
JOIN dbo.users au ON au.email = 'admin.seed@mediconnect.local'
LEFT JOIN dbo.appointments a
    ON a.patient_id = pu.user_id
   AND a.doctor_id = du.user_id
   AND a.appointment_date = v.appointment_date
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.complaints c WHERE c.patient_id = pu.user_id AND c.subject = v.subject
);
GO

-- AI recommendations: 10 records.
INSERT INTO dbo.ai_recommendations (
    patient_id, input_symptoms, recommended_doctors, recommended_slots,
    specialty_suggested, model_version
)
SELECT
    pu.user_id,
    v.input_symptoms,
    v.recommended_doctors,
    v.recommended_slots,
    s.specialty_id,
    N'triage-v1.0'
FROM (VALUES
    ('patient01.seed@mediconnect.local', N'stomach pain after meals', N'doctor01.seed@mediconnect.local', N'2026-04-20 08:00', N'General Medicine'),
    ('patient02.seed@mediconnect.local', N'intermittent chest pain',   N'doctor02.seed@mediconnect.local', N'2026-04-21 08:30', N'Cardiology'),
    ('patient03.seed@mediconnect.local', N'itchy skin rash',           N'doctor03.seed@mediconnect.local', N'2026-04-22 09:00', N'Dermatology'),
    ('patient04.seed@mediconnect.local', N'recurrent headaches',       N'doctor04.seed@mediconnect.local', N'2026-04-23 13:00', N'Neurology'),
    ('patient05.seed@mediconnect.local', N'child fever for 2 days',    N'doctor05.seed@mediconnect.local', N'2026-04-24 08:00', N'Pediatrics'),
    ('patient06.seed@mediconnect.local', N'knee pain and swelling',    N'doctor06.seed@mediconnect.local', N'2026-04-25 13:30', N'Orthopedics'),
    ('patient07.seed@mediconnect.local', N'nasal congestion',          N'doctor07.seed@mediconnect.local', N'2026-04-26 14:00', N'ENT'),
    ('patient08.seed@mediconnect.local', N'prenatal checkup needed',   N'doctor08.seed@mediconnect.local', N'2026-04-27 08:00', N'Gynecology & Obstetrics'),
    ('patient09.seed@mediconnect.local', N'sleep disturbances',        N'doctor09.seed@mediconnect.local', N'2026-04-28 09:30', N'Psychiatry'),
    ('patient10.seed@mediconnect.local', N'upper abdominal bloating',  N'doctor10.seed@mediconnect.local', N'2026-04-29 13:00', N'Gastroenterology')
) v(patient_email, input_symptoms, recommended_doctors, recommended_slots, specialty_name)
JOIN dbo.users pu ON pu.email = v.patient_email
LEFT JOIN dbo.specialties s ON s.specialty_name = v.specialty_name
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ai_recommendations ar
    WHERE ar.patient_id = pu.user_id
      AND ar.input_symptoms = v.input_symptoms
);
GO

-- System logs: 10 records.
INSERT INTO dbo.system_logs (
    user_id, action, entity_type, entity_id, description,
    ip_address, user_agent, severity
)
SELECT
    u.user_id,
    v.action,
    v.entity_type,
    v.entity_id,
    v.description,
    v.ip_address,
    v.user_agent,
    v.severity
FROM (VALUES
    ('admin.seed@mediconnect.local',   N'CREATE_USER',          N'users',                 0, N'Bulk seeded user accounts',          N'127.0.0.1', N'SeedScript/1.0', 'INFO'),
    ('doctor01.seed@mediconnect.local',N'UPDATE_PROFILE',       N'doctor_profiles',       0, N'Doctor profile updated',              N'127.0.0.1', N'SeedScript/1.0', 'INFO'),
    ('doctor02.seed@mediconnect.local',N'CREATE_SCHEDULE',      N'doctor_schedules',      0, N'Doctor schedule created',             N'127.0.0.1', N'SeedScript/1.0', 'INFO'),
    ('patient01.seed@mediconnect.local',N'BOOK_APPOINTMENT',    N'appointments',          0, N'Patient booked appointment',          N'127.0.0.1', N'SeedScript/1.0', 'INFO'),
    ('patient02.seed@mediconnect.local',N'PAY_APPOINTMENT',     N'payments',              0, N'Payment captured',                    N'127.0.0.1', N'SeedScript/1.0', 'INFO'),
    ('patient03.seed@mediconnect.local',N'SUBMIT_REVIEW',       N'reviews',               0, N'Review submitted',                    N'127.0.0.1', N'SeedScript/1.0', 'INFO'),
    ('patient04.seed@mediconnect.local',N'CREATE_COMPLAINT',    N'complaints',            0, N'Complaint opened',                    N'127.0.0.1', N'SeedScript/1.0', 'WARNING'),
    ('admin.seed@mediconnect.local',   N'RESOLVE_COMPLAINT',    N'complaints',            0, N'Complaint resolved by admin',         N'127.0.0.1', N'SeedScript/1.0', 'INFO'),
    ('patient05.seed@mediconnect.local',N'AI_TRIAGE_REQUEST',   N'ai_recommendations',    0, N'AI triage requested by patient',      N'127.0.0.1', N'SeedScript/1.0', 'INFO'),
    ('admin.seed@mediconnect.local',   N'SYSTEM_MAINTENANCE',   N'system',                0, N'Routine maintenance notification',    N'127.0.0.1', N'SeedScript/1.0', 'CRITICAL')
) v(email, action, entity_type, entity_id, description, ip_address, user_agent, severity)
JOIN dbo.users u ON u.email = v.email
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.system_logs sl WHERE sl.action = v.action AND sl.description = v.description
);
GO

-- External calendar integrations: 10 records.
INSERT INTO dbo.external_calendar_integrations (
    user_id, provider, access_token, refresh_token, token_expires_at,
    calendar_id, is_active, last_synced_at
)
SELECT
    u.user_id,
    v.provider,
    v.access_token,
    v.refresh_token,
    DATEADD(DAY, 30, CURRENT_TIMESTAMP),
    v.calendar_id,
    1,
    DATEADD(HOUR, -2, CURRENT_TIMESTAMP)
FROM (VALUES
    ('doctor01.seed@mediconnect.local', N'GOOGLE_CALENDAR',  N'seed_acc_01', N'seed_ref_01', N'cal_doc_01'),
    ('doctor02.seed@mediconnect.local', N'GOOGLE_CALENDAR',  N'seed_acc_02', N'seed_ref_02', N'cal_doc_02'),
    ('doctor03.seed@mediconnect.local', N'GOOGLE_CALENDAR',  N'seed_acc_03', N'seed_ref_03', N'cal_doc_03'),
    ('doctor04.seed@mediconnect.local', N'GOOGLE_CALENDAR',  N'seed_acc_04', N'seed_ref_04', N'cal_doc_04'),
    ('doctor05.seed@mediconnect.local', N'GOOGLE_CALENDAR',  N'seed_acc_05', N'seed_ref_05', N'cal_doc_05'),
    ('doctor06.seed@mediconnect.local', N'OUTLOOK_CALENDAR', N'seed_acc_06', N'seed_ref_06', N'cal_doc_06'),
    ('doctor07.seed@mediconnect.local', N'OUTLOOK_CALENDAR', N'seed_acc_07', N'seed_ref_07', N'cal_doc_07'),
    ('doctor08.seed@mediconnect.local', N'OUTLOOK_CALENDAR', N'seed_acc_08', N'seed_ref_08', N'cal_doc_08'),
    ('doctor09.seed@mediconnect.local', N'OUTLOOK_CALENDAR', N'seed_acc_09', N'seed_ref_09', N'cal_doc_09'),
    ('doctor10.seed@mediconnect.local', N'OUTLOOK_CALENDAR', N'seed_acc_10', N'seed_ref_10', N'cal_doc_10')
) v(email, provider, access_token, refresh_token, calendar_id)
JOIN dbo.users u ON u.email = v.email
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.external_calendar_integrations eci
    WHERE eci.user_id = u.user_id
      AND eci.provider = v.provider
);
GO

-- Promotional campaigns: 10 records.
INSERT INTO dbo.promotional_campaigns (
    title, message, target_role, start_date, end_date,
    created_by, is_sent, sent_at
)
SELECT
    v.title,
    v.message,
    v.target_role,
    v.start_date,
    v.end_date,
    u.user_id,
    v.is_sent,
    CASE WHEN v.is_sent = 1 THEN CURRENT_TIMESTAMP ELSE NULL END
FROM (VALUES
    (N'Heart Health Week',          N'Book a cardiovascular screening package.',       'PATIENT', CAST('2026-04-01' AS DATE), CAST('2026-04-15' AS DATE), 1),
    (N'Skin Care Awareness',        N'Dermatology consultation discount this week.',   'PATIENT', CAST('2026-04-05' AS DATE), CAST('2026-04-20' AS DATE), 1),
    (N'Pediatric Immunization',     N'Vaccination campaign for children.',             'PATIENT', CAST('2026-04-10' AS DATE), CAST('2026-04-30' AS DATE), 1),
    (N'Doctor Productivity Tips',   N'Updated scheduling best practices for doctors.', 'DOCTOR',  CAST('2026-04-01' AS DATE), CAST('2026-04-30' AS DATE), 1),
    (N'New Telehealth Feature',     N'Explore improved video consultation workflow.',  'ALL',     CAST('2026-04-08' AS DATE), CAST('2026-05-08' AS DATE), 0),
    (N'Mental Wellness Month',      N'Mental health check package now available.',     'PATIENT', CAST('2026-05-01' AS DATE), CAST('2026-05-31' AS DATE), 0),
    (N'Staff Onboarding Update',    N'New internal process for support teams.',        'DOCTOR',  CAST('2026-04-12' AS DATE), CAST('2026-05-12' AS DATE), 0),
    (N'Annual Checkup Program',     N'Comprehensive annual checkup with benefits.',    'ALL',     CAST('2026-04-20' AS DATE), CAST('2026-06-20' AS DATE), 1),
    (N'Sleep Health Initiative',    N'Consult psychiatry for sleep-related issues.',   'PATIENT', CAST('2026-04-15' AS DATE), CAST('2026-05-15' AS DATE), 0),
    (N'Digital Records Adoption',   N'Encourage complete digital chart updates.',      'DOCTOR',  CAST('2026-04-18' AS DATE), CAST('2026-05-18' AS DATE), 1)
) v(title, message, target_role, start_date, end_date, is_sent)
JOIN dbo.users u ON u.email = 'admin.seed@mediconnect.local'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.promotional_campaigns pc WHERE pc.title = v.title
);
GO