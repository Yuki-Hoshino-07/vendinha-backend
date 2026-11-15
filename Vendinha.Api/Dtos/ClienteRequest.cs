using System.ComponentModel.DataAnnotations;
using Vendinha.Api.Validation;

namespace Vendinha.Api.Dtos;

public class ClienteRequest : IValidatableObject
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(150, ErrorMessage = "O nome deve ter até 150 caracteres.")]
    public string NomeCompleto { get; set; } = "";

    [Required(ErrorMessage = "Informe o CPF.")]
    public string Cpf { get; set; } = "";

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    public DateOnly? DataNascimento { get; set; }

    [StringLength(254, ErrorMessage = "O e-mail deve ter até 254 caracteres.")]
    public string? Email { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Cpf) && !CpfValidator.EhValido(Cpf))
            yield return new ValidationResult("CPF inválido.", new[] { nameof(Cpf) });

        if (DataNascimento == DateOnly.MinValue || DataNascimento > DateOnly.FromDateTime(DateTime.Today))
            yield return new ValidationResult("Informe uma data de nascimento válida, que não esteja no futuro.",
                new[] { nameof(DataNascimento) });

        if (!string.IsNullOrWhiteSpace(Email) && !new EmailAddressAttribute().IsValid(Email.Trim()))
            yield return new ValidationResult("E-mail inválido.", new[] { nameof(Email) });
    }
}
