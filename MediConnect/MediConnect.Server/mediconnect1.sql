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