using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using MediConnect.Domain.Entities;

namespace MediConnect.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUserRepository userRepository, IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user == null || !user.IsActive)
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Email hoặc mật khẩu không đúng."
            };
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Email hoặc mật khẩu không đúng."
            };
        }

        user.LastLoginAt = DateTime.Now;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResultDto
        {
            Success = true,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.Role?.RoleName
        };
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Email này đã được sử dụng."
            };
        }

        var patientRole = await _roleRepository.GetByNameAsync(RoleNames.Patient);
        if (patientRole == null)
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Lỗi hệ thống: Không tìm thấy vai trò bệnh nhân."
            };
        }

        var user = new User
        {
            RoleId = patientRole.RoleId,
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth.HasValue
                ? DateOnly.FromDateTime(dto.DateOfBirth.Value)
                : null,
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _userRepository.CreateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResultDto
        {
            Success = true,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = RoleNames.Patient
        };
    }

    public async Task<AuthResultDto> LoginOrRegisterGoogleAsync(string email, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Không lấy được email từ Google."
            };
        }

        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            var patientRole = await _roleRepository.GetByNameAsync(RoleNames.Patient);
            if (patientRole == null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "Lỗi hệ thống: Không tìm thấy vai trò bệnh nhân."
                };
            }

            user = new User
            {
                RoleId = patientRole.RoleId,
                FullName = string.IsNullOrWhiteSpace(fullName) ? email : fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                LastLoginAt = DateTime.Now
            };

            await _userRepository.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            user = await _userRepository.GetByEmailAsync(email);
        }
        else
        {
            if (!user.IsActive)
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên."
                };
            }

            if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(fullName))
            {
                user.FullName = fullName;
            }

            user.IsVerified = true;
            user.LastLoginAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        if (user == null)
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Không thể hoàn tất đăng nhập Google."
            };
        }

        return new AuthResultDto
        {
            Success = true,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.Role?.RoleName ?? RoleNames.Patient
        };
    }
}
