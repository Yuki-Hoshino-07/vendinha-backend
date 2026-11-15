using System.Text.RegularExpressions;

namespace Vendinha.Api.Validation;

public static class CpfValidator
{
    public static string Normalizar(string cpf)
    {
        return cpf.Trim().Replace(".", "").Replace("-", "");
    }

    public static bool EhValido(string cpf)
    {
        if (!Regex.IsMatch(cpf.Trim(), @"\A([0-9]{11}|[0-9]{3}\.[0-9]{3}\.[0-9]{3}-[0-9]{2})\z"))
            return false;

        string numeros = Normalizar(cpf);
        if (numeros.All(numero => numero == numeros[0]))
            return false;

        for (int tamanho = 9; tamanho <= 10; tamanho++)
        {
            int soma = 0;
            for (int i = 0; i < tamanho; i++)
                soma += (numeros[i] - '0') * (tamanho + 1 - i);

            int resto = soma % 11;
            int digito = resto < 2 ? 0 : 11 - resto;
            if (numeros[tamanho] - '0' != digito)
                return false;
        }

        return true;
    }
}
