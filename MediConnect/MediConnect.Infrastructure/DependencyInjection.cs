using MediConnect.Application.Interfaces;
using MediConnect.Infrastructure.Data;
using MediConnect.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MediConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // DbContext
        services.AddDbContext<MediconnectContext>(options =>
            options.UseSqlServer(connectionString));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IComplaintRepository, ComplaintRepository>();
        services.AddScoped<ISystemLogRepository, SystemLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IDoctorSpecialtyRepository, DoctorSpecialtyRepository>();
        services.AddScoped<IAppointmentWaitlistRepository, AppointmentWaitlistRepository>();
        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();

        return services;
    }
}
