using System.ComponentModel.DataAnnotations;

namespace Vendinha.Api.Dtos;

public class DividaRequest : IValidatableObject
{
    [Required(ErrorMessage = "Informe o valor da dívida.")]
    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "O valor deve estar entre 0,01 e 999.999.999,99.")]
    public decimal? Valor { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Valor.HasValue && decimal.Round(Valor.Value, 2) != Valor.Value)
            yield return new ValidationResult("O valor deve ter no máximo duas casas decimais.", new[] { nameof(Valor) });
    }
}
