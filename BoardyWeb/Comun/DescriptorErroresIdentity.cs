using Microsoft.AspNetCore.Identity;

namespace BoardyWeb.Comun
{
    /// <summary>
    /// Mensajes de error de ASP.NET Core Identity en español.
    /// </summary>
    public class DescriptorErroresIdentity : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() =>
            new() { Code = nameof(DefaultError), Description = "Ocurrió un error desconocido." };

        public override IdentityError ConcurrencyFailure() =>
            new() { Code = nameof(ConcurrencyFailure), Description = "Los datos fueron modificados por otro usuario. Volvé a intentarlo." };

        public override IdentityError PasswordMismatch() =>
            new() { Code = nameof(PasswordMismatch), Description = "La contraseña es incorrecta." };

        public override IdentityError InvalidToken() =>
            new() { Code = nameof(InvalidToken), Description = "El enlace no es válido o ya venció." };

        public override IdentityError LoginAlreadyAssociated() =>
            new() { Code = nameof(LoginAlreadyAssociated), Description = "Ya existe un usuario asociado a este inicio de sesión." };

        public override IdentityError InvalidUserName(string? userName) =>
            new() { Code = nameof(InvalidUserName), Description = $"El nombre de usuario '{userName}' no es válido." };

        public override IdentityError InvalidEmail(string? email) =>
            new() { Code = nameof(InvalidEmail), Description = $"El correo electrónico '{email}' no es válido." };

        public override IdentityError DuplicateUserName(string userName) =>
            new() { Code = nameof(DuplicateUserName), Description = $"El nombre de usuario '{userName}' ya está en uso." };

        public override IdentityError DuplicateEmail(string email) =>
            new() { Code = nameof(DuplicateEmail), Description = $"El correo electrónico '{email}' ya está registrado." };

        public override IdentityError InvalidRoleName(string? role) =>
            new() { Code = nameof(InvalidRoleName), Description = $"El nombre de rol '{role}' no es válido." };

        public override IdentityError DuplicateRoleName(string role) =>
            new() { Code = nameof(DuplicateRoleName), Description = $"El rol '{role}' ya existe." };

        public override IdentityError UserAlreadyHasPassword() =>
            new() { Code = nameof(UserAlreadyHasPassword), Description = "El usuario ya tiene una contraseña." };

        public override IdentityError UserLockoutNotEnabled() =>
            new() { Code = nameof(UserLockoutNotEnabled), Description = "El bloqueo de cuenta no está habilitado para este usuario." };

        public override IdentityError UserAlreadyInRole(string role) =>
            new() { Code = nameof(UserAlreadyInRole), Description = $"El usuario ya tiene el rol '{role}'." };

        public override IdentityError UserNotInRole(string role) =>
            new() { Code = nameof(UserNotInRole), Description = $"El usuario no tiene el rol '{role}'." };

        public override IdentityError PasswordTooShort(int length) =>
            new() { Code = nameof(PasswordTooShort), Description = $"La contraseña debe tener al menos {length} caracteres." };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
            new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"La contraseña debe tener al menos {uniqueChars} caracteres distintos." };

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "La contraseña debe tener al menos un símbolo (por ejemplo ! # $ %)." };

        public override IdentityError PasswordRequiresDigit() =>
            new() { Code = nameof(PasswordRequiresDigit), Description = "La contraseña debe tener al menos un número (0-9)." };

        public override IdentityError PasswordRequiresLower() =>
            new() { Code = nameof(PasswordRequiresLower), Description = "La contraseña debe tener al menos una letra minúscula (a-z)." };

        public override IdentityError PasswordRequiresUpper() =>
            new() { Code = nameof(PasswordRequiresUpper), Description = "La contraseña debe tener al menos una letra mayúscula (A-Z)." };

        public override IdentityError RecoveryCodeRedemptionFailed() =>
            new() { Code = nameof(RecoveryCodeRedemptionFailed), Description = "El código de recuperación no es válido." };
    }
}
