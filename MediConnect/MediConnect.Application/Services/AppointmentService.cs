using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using MediConnect.Domain.Entities;

namespace MediConnect.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        ITimeSlotRepository timeSlotRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _timeSlotRepository = timeSlotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AppointmentListDto>> GetAllAppointmentsAsync()
    {
        var appointments = await _appointmentRepository.GetAllAsync();
        return MapToListDto(appointments);
    }

    public async Task<List<AppointmentListDto>> GetAppointmentsByPatientAsync(int patientId)
    {
        var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);
        return MapToListDto(appointments);
    }

    public async Task<List<AppointmentListDto>> GetAppointmentsByDoctorAsync(int doctorId)
    {
        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
        return MapToListDto(appointments);
    }

    public async Task<AppointmentDetailDto?> GetAppointmentDetailAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null) return null;

        return new AppointmentDetailDto
        {
            AppointmentId = appointment.AppointmentId,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient.FullName,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.Doctor.FullName,
            SpecialtyName = appointment.Specialty?.SpecialtyName,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Reason = appointment.Reason,
            Status = appointment.Status,
            Notes = appointment.Notes,
            CancelReason = appointment.CancelReason,
            ConfirmedAt = appointment.ConfirmedAt,
            CancelledAt = appointment.CancelledAt,
            CreatedAt = appointment.CreatedAt
        };
    }

    public async Task<AppointmentResultDto> CreateAppointmentAsync(CreateAppointmentDto dto)
    {
        var slot = await _timeSlotRepository.GetByIdAsync(dto.SlotId);
        if (slot == null || !slot.IsAvailable || slot.BookedCount >= slot.MaxCapacity)
        {
            return new AppointmentResultDto
            {
                Success = false,
                ErrorMessage = "Khung giờ này không còn trống."
            };
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                SlotId = dto.SlotId,
                SpecialtyId = dto.SpecialtyId,
                AppointmentDate = DateOnly.FromDateTime(dto.AppointmentDate),
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                Reason = dto.Reason,
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _appointmentRepository.CreateAsync(appointment);

            slot.BookedCount++;
            if (slot.BookedCount >= slot.MaxCapacity)
            {
                slot.IsAvailable = false;
            }
            await _timeSlotRepository.UpdateAsync(slot);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return new AppointmentResultDto
            {
                Success = true,
                AppointmentId = appointment.AppointmentId
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<AppointmentResultDto> CancelAppointmentAsync(CancelAppointmentDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId);
        if (appointment == null)
        {
            return new AppointmentResultDto
            {
                Success = false,
                ErrorMessage = "Không tìm thấy lịch hẹn."
            };
        }

        if (!AppointmentStatus.IsCancellable(appointment.Status))
        {
            return new AppointmentResultDto
            {
                Success = false,
                ErrorMessage = "Không thể hủy lịch hẹn ở trạng thái hiện tại."
            };
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            appointment.Status = AppointmentStatus.CancelledByPatient;
            appointment.CancelReason = dto.CancelReason;
            appointment.CancelledAt = DateTime.Now;
            appointment.UpdatedAt = DateTime.Now;
            await _appointmentRepository.UpdateAsync(appointment);

            if (appointment.SlotId > 0)
            {
                var slot = await _timeSlotRepository.GetByIdAsync(appointment.SlotId);
                if (slot != null)
                {
                    slot.BookedCount = Math.Max(0, slot.BookedCount - 1);
                    if (slot.BookedCount < slot.MaxCapacity)
                    {
                        slot.IsAvailable = true;
                    }
                    await _timeSlotRepository.UpdateAsync(slot);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return new AppointmentResultDto
            {
                Success = true,
                AppointmentId = appointment.AppointmentId
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<AppointmentResultDto> ConfirmAppointmentAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            return new AppointmentResultDto
            {
                Success = false,
                ErrorMessage = "Không tìm thấy lịch hẹn."
            };
        }

        if (appointment.Status != AppointmentStatus.Pending)
        {
            return new AppointmentResultDto
            {
                Success = false,
                ErrorMessage = "Chỉ có thể xác nhận lịch hẹn đang chờ."
            };
        }

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.ConfirmedAt = DateTime.Now;
        appointment.UpdatedAt = DateTime.Now;
        await _appointmentRepository.UpdateAsync(appointment);
        await _unitOfWork.SaveChangesAsync();

        return new AppointmentResultDto
        {
            Success = true,
            AppointmentId = appointment.AppointmentId
        };
    }

    public async Task<List<TimeSlotDto>> GetAvailableSlotsAsync()
    {
        var slots = await _timeSlotRepository.GetAvailableAsync();
        return slots.Select(s => new TimeSlotDto
        {
            SlotId = s.SlotId,
            SlotDate = s.SlotDate,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsAvailable = s.IsAvailable,
            Display = $"{s.SlotDate:dd/MM} | {s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}"
        }).ToList();
    }

    private static List<AppointmentListDto> MapToListDto(List<Appointment> appointments)
    {
        return appointments.Select(a => new AppointmentListDto
        {
            AppointmentId = a.AppointmentId,
            PatientName = a.Patient.FullName,
            DoctorName = a.Doctor.FullName,
            SpecialtyName = a.Specialty?.SpecialtyName,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Reason = a.Reason,
            Status = a.Status,
            CreatedAt = a.CreatedAt
        }).ToList();
    }
}
