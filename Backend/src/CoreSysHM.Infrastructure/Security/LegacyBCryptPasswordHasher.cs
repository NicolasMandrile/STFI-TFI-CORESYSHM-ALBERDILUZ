using Microsoft.AspNetCore.Identity;
using CoreSysHM.Domain.Entities.Auth;

namespace CoreSysHM.Infrastructure.Security;

/// <summary>
/// Permite que los usuarios sembrados con BCrypt (antes de migrar a ASP.NET Core Identity) sigan
/// logueándose sin resetear contraseña. Verifica hashes BCrypt "a mano" y devuelve
/// SuccessRehashNeeded para que Identity re-hashee automáticamente al formato nativo
/// (PasswordHasher&lt;TUser&gt;) en el próximo login exitoso -- es el gancho oficial que expone
/// UserManager.CheckPasswordAsync/SignInManager.CheckPasswordSignInAsync para este escenario,
/// no hace falta tocar AuthService para que el re-hash ocurra.
/// </summary>
public class LegacyBCryptPasswordHasher : IPasswordHasher<ApplicationUser>
{
    private readonly PasswordHasher<ApplicationUser> _default = new();

    public string HashPassword(ApplicationUser user, string password) =>
        _default.HashPassword(user, password);

    public PasswordVerificationResult VerifyHashedPassword(
        ApplicationUser user, string hashedPassword, string providedPassword)
    {
        if (IsBCryptHash(hashedPassword))
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword)
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Failed;
        }

        return _default.VerifyHashedPassword(user, hashedPassword, providedPassword);
    }

    private static bool IsBCryptHash(string hash) =>
        hash.Length >= 4 && (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));
}
