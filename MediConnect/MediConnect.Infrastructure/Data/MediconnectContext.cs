using System;
using System.Collections.Generic;
using MediConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Data;

public partial class MediconnectContext : DbContext
{
    public MediconnectContext(DbContextOptions<MediconnectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiRecommendation> AiRecommendations { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentWaitlist> AppointmentWaitlists { get; set; }

    public virtual DbSet<Complaint> Complaints { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DoctorProfile> DoctorProfiles { get; set; }

    public virtual DbSet<DoctorSchedule> DoctorSchedules { get; set; }

    public virtual DbSet<DoctorSpecialty> DoctorSpecialties { get; set; }

    public virtual DbSet<ExternalCalendarIntegration> ExternalCalendarIntegrations { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PromotionalCampaign> PromotionalCampaigns { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Specialty> Specialties { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    public virtual DbSet<TimeSlot> TimeSlots { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiRecommendation>(entity =>
        {
            entity.HasKey(e => e.RecommendationId);

            entity.ToTable("ai_recommendations");

            entity.HasIndex(e => e.PatientId, "IX_airec_patient");

            entity.HasIndex(e => e.SpecialtySuggested, "IX_airec_specialty");

            entity.Property(e => e.RecommendationId).HasColumnName("recommendation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.InputSymptoms).HasColumnName("input_symptoms");
            entity.Property(e => e.ModelVersion)
                .HasMaxLength(50)
                .HasColumnName("model_version");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.RecommendedDoctors).HasColumnName("recommended_doctors");
            entity.Property(e => e.RecommendedSlots).HasColumnName("recommended_slots");
            entity.Property(e => e.SpecialtySuggested).HasColumnName("specialty_suggested");

            entity.HasOne(d => d.Patient).WithMany(p => p.AiRecommendations)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_airec_patient");

            entity.HasOne(d => d.SpecialtySuggestedNavigation).WithMany(p => p.AiRecommendations)
                .HasForeignKey(d => d.SpecialtySuggested)
                .HasConstraintName("FK_airec_specialty");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");

            entity.HasIndex(e => e.AppointmentDate, "IX_appt_date");

            entity.HasIndex(e => e.DoctorId, "IX_appt_doctor");

            entity.HasIndex(e => e.PatientId, "IX_appt_patient");

            entity.HasIndex(e => e.SlotId, "IX_appt_slot");

            entity.HasIndex(e => e.Status, "IX_appt_status");

            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.AppointmentDate).HasColumnName("appointment_date");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledAt)
                .HasColumnType("datetime")
                .HasColumnName("cancelled_at");
            entity.Property(e => e.ConfirmedAt)
                .HasColumnType("datetime")
                .HasColumnName("confirmed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.NoShowRiskScore)
                .HasColumnType("decimal(5, 4)")
                .HasColumnName("no_show_risk_score");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.SlotId).HasColumnName("slot_id");
            entity.Property(e => e.SpecialtyId).HasColumnName("specialty_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("PENDING")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Doctor).WithMany(p => p.AppointmentDoctors)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_appt_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.AppointmentPatients)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_appt_patient");

            entity.HasOne(d => d.Slot).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.SlotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_appt_slot");

            entity.HasOne(d => d.Specialty).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.SpecialtyId)
                .HasConstraintName("FK_appt_specialty");
        });

        modelBuilder.Entity<AppointmentWaitlist>(entity =>
        {
            entity.HasKey(e => e.WaitlistId);

            entity.ToTable("appointment_waitlists");

            entity.HasIndex(e => e.DoctorId, "IX_wl_doctor");

            entity.HasIndex(e => e.PatientId, "IX_wl_patient");

            entity.Property(e => e.WaitlistId).HasColumnName("waitlist_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.NotifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("notified_at");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.PreferredDate).HasColumnName("preferred_date");
            entity.Property(e => e.PreferredTime).HasColumnName("preferred_time");
            entity.Property(e => e.SpecialtyId).HasColumnName("specialty_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("WAITING")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Doctor).WithMany(p => p.AppointmentWaitlistDoctors)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_wl_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.AppointmentWaitlistPatients)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_wl_patient");

            entity.HasOne(d => d.Specialty).WithMany(p => p.AppointmentWaitlists)
                .HasForeignKey(d => d.SpecialtyId)
                .HasConstraintName("FK_wl_specialty");
        });

        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.ToTable("complaints");

            entity.HasIndex(e => e.AppointmentId, "IX_comp_appointment");

            entity.HasIndex(e => e.DoctorId, "IX_comp_doctor");

            entity.HasIndex(e => e.PatientId, "IX_comp_patient");

            entity.Property(e => e.ComplaintId).HasColumnName("complaint_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.ResolutionNote).HasColumnName("resolution_note");
            entity.Property(e => e.ResolvedAt)
                .HasColumnType("datetime")
                .HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy).HasColumnName("resolved_by");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("OPEN")
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(300)
                .HasColumnName("subject");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_comp_appointment");

            entity.HasOne(d => d.Doctor).WithMany(p => p.ComplaintDoctors)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_comp_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.ComplaintPatients)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_comp_patient");

            entity.HasOne(d => d.ResolvedByNavigation).WithMany(p => p.ComplaintResolvedByNavigations)
                .HasForeignKey(d => d.ResolvedBy)
                .HasConstraintName("FK_comp_resolved_by");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("departments");

            entity.HasIndex(e => e.DepartmentName, "UQ_department_name").IsUnique();

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(150)
                .HasColumnName("department_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .HasColumnName("location");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<DoctorProfile>(entity =>
        {
            entity.ToTable("doctor_profiles");

            entity.HasIndex(e => e.ApprovalStatus, "IX_dp_approval_status");

            entity.HasIndex(e => e.DepartmentId, "IX_dp_department_id");

            entity.HasIndex(e => e.LicenseNumber, "UQ_dp_license_number").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_dp_user_id").IsUnique();

            entity.Property(e => e.DoctorProfileId).HasColumnName("doctor_profile_id");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(20)
                .HasDefaultValue("PENDING")
                .HasColumnName("approval_status");
            entity.Property(e => e.ApprovedAt)
                .HasColumnType("datetime")
                .HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.AverageRating)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("average_rating");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.ConsultationFee)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("consultation_fee");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Education).HasColumnName("education");
            entity.Property(e => e.InsuranceAccepted)
                .HasMaxLength(500)
                .HasColumnName("insurance_accepted");
            entity.Property(e => e.LicenseNumber)
                .HasMaxLength(100)
                .HasColumnName("license_number");
            entity.Property(e => e.Location)
                .HasMaxLength(300)
                .HasColumnName("location");
            entity.Property(e => e.TotalReviews).HasColumnName("total_reviews");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.YearsOfExperience).HasColumnName("years_of_experience");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.DoctorProfileApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_dp_approved_by");

            entity.HasOne(d => d.Department).WithMany(p => p.DoctorProfiles)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_dp_department");

            entity.HasOne(d => d.User).WithOne(p => p.DoctorProfileUser)
                .HasForeignKey<DoctorProfile>(d => d.UserId)
                .HasConstraintName("FK_dp_user");
        });

        modelBuilder.Entity<DoctorSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId);

            entity.ToTable("doctor_schedules");

            entity.HasIndex(e => e.DayOfWeek, "IX_sched_day_of_week");

            entity.HasIndex(e => e.UserId, "IX_sched_user");

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaxPatientsPerSlot)
                .HasDefaultValue(1)
                .HasColumnName("max_patients_per_slot");
            entity.Property(e => e.SlotDurationMins)
                .HasDefaultValue(30)
                .HasColumnName("slot_duration_mins");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.DoctorSchedules)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_sched_user");
        });

        modelBuilder.Entity<DoctorSpecialty>(entity =>
        {
            entity.ToTable("doctor_specialties");

            entity.HasIndex(e => e.SpecialtyId, "IX_ds_specialty_id");

            entity.HasIndex(e => new { e.UserId, e.SpecialtyId }, "UQ_doctor_specialty").IsUnique();

            entity.Property(e => e.DoctorSpecialtyId).HasColumnName("doctor_specialty_id");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.SpecialtyId).HasColumnName("specialty_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Specialty).WithMany(p => p.DoctorSpecialties)
                .HasForeignKey(d => d.SpecialtyId)
                .HasConstraintName("FK_ds_specialty");

            entity.HasOne(d => d.User).WithMany(p => p.DoctorSpecialties)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ds_user");
        });

        modelBuilder.Entity<ExternalCalendarIntegration>(entity =>
        {
            entity.HasKey(e => e.IntegrationId).HasName("PK_eci");

            entity.ToTable("external_calendar_integrations");

            entity.HasIndex(e => new { e.UserId, e.Provider }, "UQ_eci_user_provider").IsUnique();

            entity.Property(e => e.IntegrationId).HasColumnName("integration_id");
            entity.Property(e => e.AccessToken).HasColumnName("access_token");
            entity.Property(e => e.CalendarId)
                .HasMaxLength(200)
                .HasColumnName("calendar_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastSyncedAt)
                .HasColumnType("datetime")
                .HasColumnName("last_synced_at");
            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .HasDefaultValue("GOOGLE_CALENDAR")
                .HasColumnName("provider");
            entity.Property(e => e.RefreshToken).HasColumnName("refresh_token");
            entity.Property(e => e.TokenExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("token_expires_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.ExternalCalendarIntegrations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_eci_user");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId);

            entity.ToTable("medical_records");

            entity.HasIndex(e => e.AppointmentId, "IX_mr_appointment");

            entity.HasIndex(e => e.DoctorId, "IX_mr_doctor");

            entity.HasIndex(e => e.PatientId, "IX_mr_patient");

            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Diagnosis).HasColumnName("diagnosis");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Prescription).HasColumnName("prescription");
            entity.Property(e => e.RecordDate).HasColumnName("record_date");
            entity.Property(e => e.Symptoms).HasColumnName("symptoms");
            entity.Property(e => e.TreatmentPlan).HasColumnName("treatment_plan");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Appointment).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_mr_appointment");

            entity.HasOne(d => d.Doctor).WithMany(p => p.MedicalRecordDoctors)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_mr_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.MedicalRecordPatients)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_mr_patient");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasIndex(e => e.AppointmentId, "IX_notif_appointment");

            entity.HasIndex(e => e.Status, "IX_notif_status");

            entity.HasIndex(e => e.UserId, "IX_notif_user");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.Channel)
                .HasMaxLength(20)
                .HasColumnName("channel");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(40)
                .HasColumnName("notification_type");
            entity.Property(e => e.ReadAt)
                .HasColumnType("datetime")
                .HasColumnName("read_at");
            entity.Property(e => e.SentAt)
                .HasColumnType("datetime")
                .HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("PENDING")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(300)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_notif_appointment");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_notif_user");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK_prt");

            entity.ToTable("password_reset_tokens");

            entity.HasIndex(e => e.Token, "IX_prt_token");

            entity.HasIndex(e => e.UserId, "IX_prt_user_id");

            entity.Property(e => e.TokenId).HasColumnName("token_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.IsUsed).HasColumnName("is_used");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_prt_user");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");

            entity.HasIndex(e => e.AppointmentId, "IX_pay_appointment");

            entity.HasIndex(e => e.PatientId, "IX_pay_patient");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND")
                .HasColumnName("currency");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("PENDING")
                .HasColumnName("payment_status");
            entity.Property(e => e.RefundAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("refund_amount");
            entity.Property(e => e.RefundedAt)
                .HasColumnType("datetime")
                .HasColumnName("refunded_at");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(200)
                .HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Payments)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false)
                .HasConstraintName("FK_pay_appointment");

            entity.HasOne(d => d.Patient).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_pay_patient");
        });

        modelBuilder.Entity<PromotionalCampaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId);

            entity.ToTable("promotional_campaigns");

            entity.HasIndex(e => e.CreatedBy, "IX_promo_created_by");

            entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsSent).HasColumnName("is_sent");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.SentAt)
                .HasColumnType("datetime")
                .HasColumnName("sent_at");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.TargetRole)
                .HasMaxLength(20)
                .HasDefaultValue("PATIENT")
                .HasColumnName("target_role");
            entity.Property(e => e.Title)
                .HasMaxLength(300)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PromotionalCampaigns)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_promo_created_by");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");

            entity.HasIndex(e => e.DoctorId, "IX_rev_doctor");

            entity.HasIndex(e => e.PatientId, "IX_rev_patient");

            entity.HasIndex(e => e.AppointmentId, "UQ_appt_review").IsUnique();

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.IsVisible)
                .HasDefaultValue(true)
                .HasColumnName("is_visible");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.SentimentLabel)
                .HasMaxLength(20)
                .HasColumnName("sentiment_label");
            entity.Property(e => e.SentimentScore)
                .HasColumnType("decimal(5, 4)")
                .HasColumnName("sentiment_score");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Appointment).WithOne(p => p.Review)
                .HasForeignKey<Review>(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rev_appointment");

            entity.HasOne(d => d.Doctor).WithMany(p => p.ReviewDoctors)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rev_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.ReviewPatients)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rev_patient");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "UQ_role_name").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.ToTable("specialties");

            entity.HasIndex(e => e.SpecialtyName, "UQ_specialty_name").IsUnique();

            entity.Property(e => e.SpecialtyId).HasColumnName("specialty_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IconUrl)
                .HasMaxLength(500)
                .HasColumnName("icon_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.SpecialtyName)
                .HasMaxLength(150)
                .HasColumnName("specialty_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.ToTable("system_logs");

            entity.HasIndex(e => e.Action, "IX_slog_action");

            entity.HasIndex(e => e.CreatedAt, "IX_slog_created_at");

            entity.HasIndex(e => e.Severity, "IX_slog_severity");

            entity.HasIndex(e => e.UserId, "IX_slog_user");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.Action)
                .HasMaxLength(200)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(100)
                .HasColumnName("entity_type");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.Severity)
                .HasMaxLength(20)
                .HasDefaultValue("INFO")
                .HasColumnName("severity");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.SystemLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_slog_user");
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId);

            entity.ToTable("time_slots");

            entity.HasIndex(e => new { e.UserId, e.SlotDate }, "IX_ts_doctor_date");

            entity.HasIndex(e => e.ScheduleId, "IX_ts_schedule");

            entity.Property(e => e.SlotId).HasColumnName("slot_id");
            entity.Property(e => e.BookedCount).HasColumnName("booked_count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true)
                .HasColumnName("is_available");
            entity.Property(e => e.MaxCapacity)
                .HasDefaultValue(1)
                .HasColumnName("max_capacity");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.SlotDate).HasColumnName("slot_date");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Schedule).WithMany(p => p.TimeSlots)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ts_schedule");

            entity.HasOne(d => d.User).WithMany(p => p.TimeSlots)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ts_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasIndex(e => e.RoleId, "IX_users_role_id");

            entity.HasIndex(e => e.Email, "UQ_users_email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(300)
                .HasColumnName("address");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.LastLoginAt)
                .HasColumnType("datetime")
                .HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_users_role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
