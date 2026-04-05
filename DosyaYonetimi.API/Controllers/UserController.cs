using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using DosyaYonetimi.API.DTOs; 
using DosyaYonetimi.API.Models; 
using Microsoft.AspNetCore.Authorization;

namespace DosyaYonetimi.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize] 
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        ResultDto result = new ResultDto();

        public UserController(UserManager<AppUser> userManager, IMapper mapper, RoleManager<AppRole> roleManager, IConfiguration configuration, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _configuration = configuration;
            _signInManager = signInManager;
        }

        //GÖRÜNTÜLEME

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public List<UserDto> List()
        {
            var users = _userManager.Users.ToList();
            var userDtos = _mapper.Map<List<UserDto>>(users);
            return userDtos;
        }

        [HttpGet("{id}")]
        public UserDto GetById(string id)
        {
            var user = _userManager.Users.Where(s => s.Id == id).SingleOrDefault();
            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        // KAYIT  

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultDto> Add(RegisterDto dto)
        {
            
            var newUser = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                StorageQuota = 50,
                UsedStorage = 0,
                PhotoUrl = "profil.jpg"
            };

            var identityResult = await _userManager.CreateAsync(newUser, dto.Password);

            if (!identityResult.Succeeded)
            {
                result.Status = false;
                foreach (var item in identityResult.Errors)
                {
                    result.Message += "<p>" + item.Description + "</p>";
                }
                return result;
            }

            var user = await _userManager.FindByNameAsync(dto.UserName);

            
            string roleName = dto.UserName.ToLower() == "admin" ? "Admin" : "Uye";

            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var role = new AppRole { Name = roleName };
                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(user, roleName);

            result.Status = true;
            result.Message = $"{roleName} Başarıyla Eklendi!";
            return result;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultDto> SignIn(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);

            if (user is null)
            {
                result.Status = false;
                result.Message = "Üye Bulunamadı!";
                return result;
            }
            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!isPasswordCorrect)
            {
                result.Status = false;
                result.Message = "Kullanıcı Adı veya Parola Geçersiz!";
                return result;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("JWTID", Guid.NewGuid().ToString()),
                new Claim("UserPhoto", user.PhotoUrl ?? "profil.jpg"),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenerateJWT(authClaims);

            result.Status = true;
            result.Message = token;
            return result;
        }

        //GÜNCELLEME

        [HttpPut]
        public async Task<ResultDto> Update(RegisterDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                result.Status = false;
                result.Message = "Kullanıcı Bulunamadı!";
                return result;
            }

            user.PhoneNumber = dto.PhoneNumber;
            user.FullName = dto.FullName;
            user.Email = dto.Email;

            await _userManager.UpdateAsync(user);
            result.Status = true;
            result.Message = "Kullanıcı Güncellendi";

            return result;
        }

        //KOTA İŞLEMLERİ
        [HttpPut]
        [Authorize(Roles = "Admin")] 
        public async Task<ResultDto> UpdateQuota(UpdateQuotaDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                result.Status = false;
                result.Message = "Kullanıcı Bulunamadı!";
                return result;
            }

            
            if (dto.NewStorageQuota < user.UsedStorage)
            {
                result.Status = false;
                result.Message = $"Hata: Kullanıcının zaten {user.UsedStorage} MB dolu alanı var. Kotayı bu değerin altına düşüremezsiniz!";
                return result;
            }

            user.StorageQuota = dto.NewStorageQuota;
            await _userManager.UpdateAsync(user);

            result.Status = true;
            result.Message = $"{user.UserName} adlı kullanıcının yeni kotası {dto.NewStorageQuota} MB olarak güncellenmiştir.";
            return result;
        }

       
        private string GenerateJWT(List<Claim> claims)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["AccessTokenExpiration"] ?? "60"));
            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var tokenObject = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: accessTokenExpiration,
                    claims: claims,
                    signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256)
                );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);
            return token;
        }
    }
}