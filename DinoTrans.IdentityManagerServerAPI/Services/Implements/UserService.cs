using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class UserService : IUserService
    {
        // Inject các thành phần cần thiết từ DI container
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocationRepository _locationRepository;
        private readonly ICompanyRepository _companyRepository;

        public UserService(IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IUnitOfWork unitOfWork,
            ILocationRepository locationRepository,
            ICompanyRepository companyRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _locationRepository = locationRepository;
            _companyRepository = companyRepository;
        }

        // Phương thức tạo tài khoản người dùng do admin của công ty
        public async Task<GeneralResponse> CreateAccount(UserDTO userDTO)
        {
            if (userDTO is null) return new GeneralResponse(false, "Model is empty");
            try
            {
                _unitOfWork.BeginTransaction();
                // Kiểm tra xem công ty đã tồn tại trong cơ sở dữ liệu chưa
                var existingCompany = _companyRepository
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CompanyName == userDTO.CompanyName && c.Email == userDTO.CompanyEmail);

                if (existingCompany != null)
                {
                    _unitOfWork.Rollback();
                    return new GeneralResponse(false, "Company already registered");
                }
                // Thêm công ty mới
                var newCompany = new Company
                {
                    CompanyName = userDTO.CompanyName,
                    Email = userDTO.CompanyEmail,
                    PhoneNumber = userDTO.CompanyPhoneNumber,
                    Role = userDTO.CompanyRole,
                    Address = userDTO.CompanyAddress,
                    IsAdminConfirm = false
                };
                _companyRepository.Add(newCompany);
                _companyRepository.SaveChange();

                // Thêm địa điểm mới
                var newLocation = new Location
                {
                    LocationName = LocationName.HeadOffice,
                    LocationAddress = userDTO.CompanyAddress,
                    CompanyId = newCompany.Id,
                };
                _locationRepository.Add(newLocation);
                _locationRepository.SaveChange();

                // Thêm người dùng mới
                var newUser = new ApplicationUser()
                {
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Email = userDTO.Email,
                    PasswordHash = userDTO.Password,
                    UserName = userDTO.Email,
                    PhoneNumber = userDTO.PhoneNumber,
                    Address = userDTO.Address,
                    LocationId = newLocation.Id
                };

                // Kiểm tra xem người dùng đã đăng ký trước đó chưa
                var user = await _userManager.FindByEmailAsync(newUser.Email);
                if (user is not null) return new GeneralResponse(false, "User registered already");

                var createUser = await _userManager.CreateAsync(newUser!, userDTO.Password);
                if (!createUser.Succeeded)
                {
                    _unitOfWork.Rollback();
                    var allErrors = "";
                    foreach (var item in createUser.Errors)
                    {
                        allErrors += item.Description.ToString() + " ";
                    }
                    return new GeneralResponse(false, allErrors);
                }

                // Thêm vai trò cho người dùng
                var findRole = await _roleManager.FindByNameAsync(userDTO.Role);
                if (findRole is null)
                {
                    _unitOfWork.Rollback();
                    return new GeneralResponse(false, "Role name doesn't exist");
                }
                if (findRole.Name != Role.CompanyAdministrator)
                {
                    return new GeneralResponse(false, "Forbidden");
                }
                await _userManager.AddToRoleAsync(newUser, userDTO.Role);

                _unitOfWork.SaveChanges();
                _unitOfWork.Commit();
                return new GeneralResponse(true, "Account Created");
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return new GeneralResponse(false, ex.Message);
            }
        }

        // Phương thức đăng nhập
        public async Task<LoginResponse> LoginAccount(LoginDTO loginDTO)
        {
            if (loginDTO == null)
                return new LoginResponse(false, null!, "Login container is empty");

            var getUser = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (getUser is null)
                return new LoginResponse(false, null!, "User not found");

            bool checkUserPasswords = await _userManager.CheckPasswordAsync(getUser, loginDTO.Password);
            if (!checkUserPasswords)
                return new LoginResponse(false, null!, "Invalid password");

            // Lấy ID công ty từ địa điểm của người dùng
            var companyId = _locationRepository
                .AsNoTracking()
                .Where(l => l.Id == getUser.LocationId)
                .Select(l => l.CompanyId)
                .FirstOrDefault();

            if (companyId == default)
                return new LoginResponse(false, null, "Can't find your company");

            // Kiểm tra xem công ty đã được admin xác nhận chưa
            var isAdminConfirm = _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == companyId)
                .Select(c => c.IsAdminConfirm)
                .FirstOrDefault();

            if ((bool)!isAdminConfirm)
                return new LoginResponse(false, null, "Your company hasn't been confirmed by admin");

            // Lấy vai trò của người dùng
            var getUserRole = await _userManager.GetRolesAsync(getUser);

            // Tạo token và trả về kết quả đăng nhập thành công
            var userSession = new UserSession(getUser.Id.ToString(), getUser.FirstName + " " + getUser.LastName, getUser.Email, getUserRole.First());
            string token = GenerateToken(userSession);
            return new LoginResponse(true, token!, "Login completed");
        }

        // Phương thức sinh token JWT
        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: userClaims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
