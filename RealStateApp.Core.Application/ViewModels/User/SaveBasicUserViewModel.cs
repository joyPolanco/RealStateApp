using System.ComponentModel.DataAnnotations;

namespace RealStateApp.Core.Application.ViewModels.User
{
    public class SaveBasicUserViewModel
    {
        public string? Id { get; set; }

        // --- Datos de acceso ---
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [Display(Name = "Nombre de usuario")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "Debe ser un correo válido.")]
        [Display(Name = "Correo electrónico")]
        public required string Email { get; set; }

        public bool IsVerified { get; set; }

        // --- Datos personales ---
        [Required(ErrorMessage = "El nombre es requerido.")]
        [Display(Name = "Nombre")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido.")]
        [Display(Name = "Apellido")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "La cédula/DNI es requerida.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "La cédula/DNI debe tener exactamente 11 dígitos.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "La cédula/DNI debe contener solo números.")]
        [Display(Name = "Cédula / DNI")]
        public string Dni { get; set; } = string.Empty;

        // --- Contraseña ---
        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':"",.<>/?]).+$",
            ErrorMessage = "La contraseña debe contener una mayúscula, un número y un carácter especial."
        )]
        [Display(Name = "Contraseña")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public required string ConfirmPassword { get; set; }

        // --- Otros ---
        [Display(Name = "Rol")]
        public string? Role { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; }
    }
}
