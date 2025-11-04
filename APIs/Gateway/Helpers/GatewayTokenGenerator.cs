using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace APIs.Gateway.Helpers
{
    public static class GatewayTokenGenerator
    {
        public static string Generate(IConfiguration config, ClaimsPrincipal? user = null)
        {
            var key = Encoding.ASCII.GetBytes(config["Jwt:Key"]!);
            var tokenHandler = new JwtSecurityTokenHandler();

            // Claims do Gateway
            var claims = new List<Claim>
            {
                new Claim("FromGateway", "true")
            };

            // Adiciona claims do usuário se houver
            if (user != null)
            {
                var userClaims = user.Claims
                    .Where(c => c.Type == ClaimTypes.Name || c.Type == ClaimTypes.Role);
                claims.AddRange(userClaims);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}