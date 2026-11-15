using System.ComponentModel.DataAnnotations;

namespace Vendinha.Api.Dtos;

public class DividaRequest : IValidatableObject
{
    [Required(ErrorMessage = "Informe o valor da dívida.")]
    public decimal? Valor { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Valor.HasValue && (Valor.Value < 0.01m || Valor.Value > 999999999.99m))
            yield return new ValidationResult("O valor deve estar entre 0,01 e 999.999.999,99.", new[] { nameof(Valor) });

        if (Valor.HasValue && decimal.Round(Valor.Value, 2) != Valor.Value)
            yield return new ValidationResult("O valor deve ter no máximo duas casas decimais.", new[] { nameof(Valor) });
    }
}
