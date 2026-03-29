using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class AppointmentConfirmModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;

    public AppointmentConfirmModel(IAppointmentService appointmentService, IDoctorService doctorService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
    }

    [BindProperty]
    [Required]
    public int DoctorUserId { get; set; }

    [BindProperty]
    [Required]
    public int SlotId { get; set; }

    [BindProperty]
    public int? SpecialtyId { get; set; }

    [BindProperty]
    [Required]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [BindProperty]
    public string? Reason { get; set; }

    public DoctorListDto? Doctor { get; set; }
    public SelectList SlotList { get; set; } = default!;
    public SelectList SpecialtyList { get; set; } = default!;
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int doctorUserId)
    {
        DoctorUserId = doctorUserId;
        await LoadData();
        if (Doctor == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadData();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Lấy thông tin bệnh nhân
        var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var doctor = Doctor;
        if (doctor == null)
        {
            ErrorMessage = "Không tìm thấy thông tin bác sĩ.";
            return Page();
        }

        // Tạo payment request
        var paymentRequest = new MediConnect.Application.DTOs.CreatePaymentRequestDto
        {
            PatientId = patientId,
            Amount = doctor.ConsultationFee,
            Currency = "VND",
            PaymentMethod = "VNPAY", // hoặc lấy từ lựa chọn của user nếu có
            OrderInfo = $"doctorUserId={DoctorUserId};slotId={SlotId};specialtyId={SpecialtyId};appointmentDate={AppointmentDate:yyyy-MM-dd};reason={Reason}",
            ClientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
        };

        var paymentService = HttpContext.RequestServices.GetService(typeof(MediConnect.Application.Interfaces.IPaymentGatewayService)) as MediConnect.Application.Interfaces.IPaymentGatewayService;
        if (paymentService == null)
        {
            ErrorMessage = "Không thể khởi tạo dịch vụ thanh toán.";
            return Page();
        }

        var paymentResult = await paymentService.CreatePaymentUrlAsync(paymentRequest);
        if (!paymentResult.Success || string.IsNullOrEmpty(paymentResult.PaymentUrl))
        {
            ErrorMessage = paymentResult.ErrorMessage ?? "Không thể khởi tạo thanh toán.";
            return Page();
        }

        // Redirect sang payment gateway
        return Redirect(paymentResult.PaymentUrl);
    }

    private async Task LoadData()
    {
        var doctors = await _doctorService.SearchDoctorsAsync(new DoctorSearchFilterDto());
        Doctor = doctors.FirstOrDefault(d => d.UserId == DoctorUserId);

        var slots = await _appointmentService.GetAvailableSlotsAsync();
        SlotList = new SelectList(slots, "SlotId", "Display");

        var specialties = await _doctorService.GetActiveSpecialtiesAsync();
        SpecialtyList = new SelectList(specialties, "SpecialtyId", "SpecialtyName");
    }
}
