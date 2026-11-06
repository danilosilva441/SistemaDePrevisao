using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PrevisaoFaturamento.Application.Identidade;
using PrevisaoFaturamento.Domain.Identidade;

namespace PrevisaoFaturamento.Infrastructure.Identidade;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, int ExpiraEmSegundos) GerarToken(Usuario usuario)
    {
        var jwtKey = ObterChaveJwt();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiracaoHoras = int.TryParse(_configuration["Jwt:ExpiracaoHoras"], out var horas) ? horas : 8;
        var expiracao = TimeSpan.FromHours(expiracaoHoras);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Name, usuario.Nome),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "PrevisaoFaturamento.Api",
            audience: _configuration["Jwt:Audience"] ?? "PrevisaoFaturamento.Web",
            claims: claims,
            expires: DateTime.UtcNow.Add(expiracao),
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        return (token, (int)expiracao.TotalSeconds);
    }

    private string ObterChaveJwt()
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
        {
            throw new InvalidOperationException(
                "A chave 'Jwt:Key' não está configurada ou possui menos de 32 caracteres. Configure via user-secrets.");
        }

        return jwtKey;
    }
}