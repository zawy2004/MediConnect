using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    department_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.department_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "specialties",
                columns: table => new
                {
                    specialty_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    specialty_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    icon_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_specialties", x => x.specialty_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_verified = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    last_login_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_users_role",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "ai_recommendations",
                columns: table => new
                {
                    recommendation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    input_symptoms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    recommended_doctors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    recommended_slots = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    specialty_suggested = table.Column<int>(type: "int", nullable: true),
                    model_version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_recommendations", x => x.recommendation_id);
                    table.ForeignKey(
                        name: "FK_airec_patient",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_airec_specialty",
                        column: x => x.specialty_suggested,
                        principalTable: "specialties",
                        principalColumn: "specialty_id");
                });

            migrationBuilder.CreateTable(
                name: "appointment_waitlists",
                columns: table => new
                {
                    waitlist_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: false),
                    specialty_id = table.Column<int>(type: "int", nullable: true),
                    preferred_date = table.Column<DateOnly>(type: "date", nullable: true),
                    preferred_time = table.Column<TimeOnly>(type: "time", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "WAITING"),
                    notified_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_waitlists", x => x.waitlist_id);
                    table.ForeignKey(
                        name: "FK_wl_doctor",
                        column: x => x.doctor_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_wl_patient",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_wl_specialty",
                        column: x => x.specialty_id,
                        principalTable: "specialties",
                        principalColumn: "specialty_id");
                });

            migrationBuilder.CreateTable(
                name: "doctor_profiles",
                columns: table => new
                {
                    doctor_profile_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: true),
                    license_number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    years_of_experience = table.Column<int>(type: "int", nullable: false),
                    education = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consultation_fee = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    insurance_accepted = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    approval_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    average_rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    total_reviews = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_profiles", x => x.doctor_profile_id);
                    table.ForeignKey(
                        name: "FK_dp_approved_by",
                        column: x => x.approved_by,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_dp_department",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "department_id");
                    table.ForeignKey(
                        name: "FK_dp_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "doctor_schedules",
                columns: table => new
                {
                    schedule_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    day_of_week = table.Column<byte>(type: "tinyint", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    slot_duration_mins = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    max_patients_per_slot = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: true),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_schedules", x => x.schedule_id);
                    table.ForeignKey(
                        name: "FK_sched_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "doctor_specialties",
                columns: table => new
                {
                    doctor_specialty_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    specialty_id = table.Column<int>(type: "int", nullable: false),
                    is_primary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_specialties", x => x.doctor_specialty_id);
                    table.ForeignKey(
                        name: "FK_ds_specialty",
                        column: x => x.specialty_id,
                        principalTable: "specialties",
                        principalColumn: "specialty_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ds_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "external_calendar_integrations",
                columns: table => new
                {
                    integration_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "GOOGLE_CALENDAR"),
                    access_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    refresh_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    calendar_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    last_synced_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eci", x => x.integration_id);
                    table.ForeignKey(
                        name: "FK_eci_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    token_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    is_used = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prt", x => x.token_id);
                    table.ForeignKey(
                        name: "FK_prt_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promotional_campaigns",
                columns: table => new
                {
                    campaign_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    target_role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PATIENT"),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    is_sent = table.Column<bool>(type: "bit", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotional_campaigns", x => x.campaign_id);
                    table.ForeignKey(
                        name: "FK_promo_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "system_logs",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    entity_id = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "INFO"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_logs", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_slog_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "time_slots",
                columns: table => new
                {
                    slot_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    schedule_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    slot_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    max_capacity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    booked_count = table.Column<int>(type: "int", nullable: false),
                    is_available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_slots", x => x.slot_id);
                    table.ForeignKey(
                        name: "FK_ts_schedule",
                        column: x => x.schedule_id,
                        principalTable: "doctor_schedules",
                        principalColumn: "schedule_id");
                    table.ForeignKey(
                        name: "FK_ts_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    appointment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: false),
                    slot_id = table.Column<int>(type: "int", nullable: false),
                    specialty_id = table.Column<int>(type: "int", nullable: true),
                    appointment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "PENDING"),
                    confirmed_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    cancel_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    no_show_risk_score = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.appointment_id);
                    table.ForeignKey(
                        name: "FK_appt_doctor",
                        column: x => x.doctor_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_appt_patient",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_appt_slot",
                        column: x => x.slot_id,
                        principalTable: "time_slots",
                        principalColumn: "slot_id");
                    table.ForeignKey(
                        name: "FK_appt_specialty",
                        column: x => x.specialty_id,
                        principalTable: "specialties",
                        principalColumn: "specialty_id");
                });

            migrationBuilder.CreateTable(
                name: "complaints",
                columns: table => new
                {
                    complaint_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: true),
                    appointment_id = table.Column<int>(type: "int", nullable: true),
                    subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "OPEN"),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "OTHER"),
                    priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "NORMAL"),
                    escalation_level = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    assigned_to_admin_id = table.Column<int>(type: "int", nullable: true),
                    follow_up_reminder_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    resolved_by = table.Column<int>(type: "int", nullable: true),
                    resolution_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_complaints", x => x.complaint_id);
                    table.ForeignKey(
                        name: "FK_comp_appointment",
                        column: x => x.appointment_id,
                        principalTable: "appointments",
                        principalColumn: "appointment_id");
                    table.ForeignKey(
                        name: "FK_comp_assigned_admin",
                        column: x => x.assigned_to_admin_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_comp_doctor",
                        column: x => x.doctor_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_comp_patient",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_comp_resolved_by",
                        column: x => x.resolved_by,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "medical_records",
                columns: table => new
                {
                    record_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: false),
                    appointment_id = table.Column<int>(type: "int", nullable: true),
                    diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    symptoms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    treatment_plan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    prescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    record_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_records", x => x.record_id);
                    table.ForeignKey(
                        name: "FK_mr_appointment",
                        column: x => x.appointment_id,
                        principalTable: "appointments",
                        principalColumn: "appointment_id");
                    table.ForeignKey(
                        name: "FK_mr_doctor",
                        column: x => x.doctor_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_mr_patient",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    appointment_id = table.Column<int>(type: "int", nullable: true),
                    notification_type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    read_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_notif_appointment",
                        column: x => x.appointment_id,
                        principalTable: "appointments",
                        principalColumn: "appointment_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_notif_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    appointment_id = table.Column<int>(type: "int", nullable: false),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    payment_method = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    payment_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    transaction_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    paid_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    refunded_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    refund_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_pay_appointment",
                        column: x => x.appointment_id,
                        principalTable: "appointments",
                        principalColumn: "appointment_id");
                    table.ForeignKey(
                        name: "FK_pay_patient",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    appointment_id = table.Column<int>(type: "int", nullable: false),
                    patient_id = table.Column<int>(type: "int", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<byte>(type: "tinyint", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sentiment_score = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    sentiment_label = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_visible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_rev_appointment",
                        column: x => x.appointment_id,
                        principalTable: "appointments",
                        principalColumn: "appointment_id");
                    table.ForeignKey(
                        name: "FK_rev_doctor",
                        column: x => x.doctor_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_rev_patient",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_airec_patient",
                table: "ai_recommendations",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_airec_specialty",
                table: "ai_recommendations",
                column: "specialty_suggested");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_waitlists_specialty_id",
                table: "appointment_waitlists",
                column: "specialty_id");

            migrationBuilder.CreateIndex(
                name: "IX_wl_doctor",
                table: "appointment_waitlists",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_wl_patient",
                table: "appointment_waitlists",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_specialty_id",
                table: "appointments",
                column: "specialty_id");

            migrationBuilder.CreateIndex(
                name: "IX_appt_date",
                table: "appointments",
                column: "appointment_date");

            migrationBuilder.CreateIndex(
                name: "IX_appt_doctor",
                table: "appointments",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_appt_patient",
                table: "appointments",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_appt_slot",
                table: "appointments",
                column: "slot_id");

            migrationBuilder.CreateIndex(
                name: "IX_appt_status",
                table: "appointments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_comp_appointment",
                table: "complaints",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_comp_doctor",
                table: "complaints",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_comp_patient",
                table: "complaints",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_complaints_assigned_to_admin_id",
                table: "complaints",
                column: "assigned_to_admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_complaints_resolved_by",
                table: "complaints",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "UQ_department_name",
                table: "departments",
                column: "department_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_doctor_profiles_approved_by",
                table: "doctor_profiles",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_dp_approval_status",
                table: "doctor_profiles",
                column: "approval_status");

            migrationBuilder.CreateIndex(
                name: "IX_dp_department_id",
                table: "doctor_profiles",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "UQ_dp_license_number",
                table: "doctor_profiles",
                column: "license_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_dp_user_id",
                table: "doctor_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sched_day_of_week",
                table: "doctor_schedules",
                column: "day_of_week");

            migrationBuilder.CreateIndex(
                name: "IX_sched_user",
                table: "doctor_schedules",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ds_specialty_id",
                table: "doctor_specialties",
                column: "specialty_id");

            migrationBuilder.CreateIndex(
                name: "UQ_doctor_specialty",
                table: "doctor_specialties",
                columns: new[] { "user_id", "specialty_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_eci_user_provider",
                table: "external_calendar_integrations",
                columns: new[] { "user_id", "provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mr_appointment",
                table: "medical_records",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_mr_doctor",
                table: "medical_records",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_mr_patient",
                table: "medical_records",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_notif_appointment",
                table: "notifications",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_notif_status",
                table: "notifications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_notif_user",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_prt_token",
                table: "password_reset_tokens",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "IX_prt_user_id",
                table: "password_reset_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_pay_appointment",
                table: "payments",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_pay_patient",
                table: "payments",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_promo_created_by",
                table: "promotional_campaigns",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_rev_doctor",
                table: "reviews",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_rev_patient",
                table: "reviews",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "UQ_appt_review",
                table: "reviews",
                column: "appointment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_role_name",
                table: "roles",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_specialty_name",
                table: "specialties",
                column: "specialty_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_slog_action",
                table: "system_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "IX_slog_created_at",
                table: "system_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_slog_severity",
                table: "system_logs",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "IX_slog_user",
                table: "system_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ts_doctor_date",
                table: "time_slots",
                columns: new[] { "user_id", "slot_date" });

            migrationBuilder.CreateIndex(
                name: "IX_ts_schedule",
                table: "time_slots",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "UQ_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_recommendations");

            migrationBuilder.DropTable(
                name: "appointment_waitlists");

            migrationBuilder.DropTable(
                name: "complaints");

            migrationBuilder.DropTable(
                name: "doctor_profiles");

            migrationBuilder.DropTable(
                name: "doctor_specialties");

            migrationBuilder.DropTable(
                name: "external_calendar_integrations");

            migrationBuilder.DropTable(
                name: "medical_records");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "promotional_campaigns");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "system_logs");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "appointments");

            migrationBuilder.DropTable(
                name: "time_slots");

            migrationBuilder.DropTable(
                name: "specialties");

            migrationBuilder.DropTable(
                name: "doctor_schedules");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
