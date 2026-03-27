using MediConnect.Application.Interfaces;
using MediConnect.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MediConnect.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<ISpecialtyService, SpecialtyService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPortalService, PortalService>();
        services.AddScoped<IPatientPortalService, PatientPortalService>();
        services.AddScoped<IAdminPortalService, AdminPortalService>();
        services.AddScoped<IDoctorPortalService, DoctorPortalService>();

        // Payment Services
        services.AddScoped<IVnPayService, VnPayService>();
        services.AddScoped<IMomoService, MomoService>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();

        // RAG Services
        services.AddScoped<IRagService, RagService>();

        return services;
    }
}
