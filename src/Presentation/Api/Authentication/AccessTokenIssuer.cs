using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Api.Authentication;

public sealed class AccessTokenIssuer
{
    public const string Issuer = "ExpenseWise.Api";
    public const string Audience = "ExpenseWise.UI";
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(20);

    private readonly SigningCredentials _signingCredentials;

    public AccessTokenIssuer(IConfiguration configuration)
    {
        _signingCredentials = new SigningCredentials(
            GetSigningKey(configuration),
            SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTime ExpiresAtUtc) Issue(Guid userKey)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.Add(TokenLifetime);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, userKey.ToString())],
            notBefore: now,
            expires: expiresAt,
            signingCredentials: _signingCredentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public static SymmetricSecurityKey GetSigningKey(IConfiguration configuration)
    {
        var signingKey = configuration["Authentication:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey) || System.Text.Encoding.UTF8.GetByteCount(signingKey) < 32)
        {
            throw new InvalidOperationException(
                "Authentication:SigningKey must be configured with at least 32 UTF-8 bytes.");
        }

        return new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(signingKey));
    }
}
