using JWT.Contexts;
using JWT.Models;
using JWT.RequestModels;
using JWT.ResponseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace JWT.Services
{
    public interface IAuthService
    {
        Task<LoginResponseModel> LoginAsync(LoginRequestModel model);
        Task<LoginResponseModel> RefreshTokenAsync(string refreshToken);
        Task<bool> RegisterAsync(RegisterRequestModel model);
    }

    public class AuthService(IConfiguration config, DatabaseContext dbContext) : IAuthService
    {
        public async Task<LoginResponseModel> LoginAsync(LoginRequestModel model)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.UserName);
            if (user != null && VerifyPassword(user.PasswordHash, model.Password, user.Salt))
            {
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                dbContext.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    ExpiryDate = DateTime.Now.AddDays(3),
                    UserId = user.UserId
                });
                await dbContext.SaveChangesAsync();

                return new LoginResponseModel
                {
                    Token = token,
                    RefreshToken = refreshToken
                };
            }

            return null;
        }

        public async Task<LoginResponseModel> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await dbContext.RefreshTokens.Include(rt => rt.User)
                .SingleOrDefaultAsync(rt => rt.Token == refreshToken);
            if (storedToken != null && storedToken.ExpiryDate > DateTime.Now)
            {
                var user = storedToken.User;
                var newJwtToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                storedToken.Token = newRefreshToken;
                storedToken.ExpiryDate = DateTime.Now.AddDays(3);
                dbContext.RefreshTokens.Update(storedToken);
                await dbContext.SaveChangesAsync();

                return new LoginResponseModel
                {
                    Token = newJwtToken,
                    RefreshToken = newRefreshToken
                };
            }

            return null;
        }

        public async Task<bool> RegisterAsync(RegisterRequestModel model)
        {
            if (await dbContext.Users.AnyAsync(u => u.Email == model.UserName))
                return false;

            var (hashedPassword, salt) = HashPassword(model.Password);

            var user = new User
            {
                Name = model.UserName,
                Email = model.UserName,
                PasswordHash = hashedPassword,
                Salt = salt
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.Now.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = config["JWT:Issuer"],
                Audience = config["JWT:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private Tuple<string, string> HashPassword(string password)
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));

            string saltBase64 = Convert.ToBase64String(salt);

            return new Tuple<string, string>(hashed, saltBase64);
        }

        private bool VerifyPassword(string storedHash, string password, string storedSalt)
        {
            var salt = Convert.FromBase64String(storedSalt);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));

            return hashed == storedHash;
        }
    }
}