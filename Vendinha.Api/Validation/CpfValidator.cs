namespace Vendinha.Api.Validation
{
    public static class CpfValidator
    {
        public static string Normalizar(string cpf)
        {
            return cpf.Trim().Replace(".", "").Replace("-", "");
        }

        public static bool EhValido(string cpf)
        {
            string texto = cpf.Trim();
            if (texto.Length == 14)
            {
                if (texto[3] != '.' || texto[7] != '.' || texto[11] != '-')
                {
                    return false;
                }
            }
            else if (texto.Length != 11)
            {
                return false;
            }

            string numeros = Normalizar(texto);
            if (numeros.Length != 11)
            {
                return false;
            }

            bool todosIguais = true;
            for (int i = 0; i < numeros.Length; i++)
            {
                if (numeros[i] < '0' || numeros[i] > '9')
                {
                    return false;
                }
                if (numeros[i] != numeros[0])
                {
                    todosIguais = false;
                }
            }
            if (todosIguais)
            {
                return false;
            }

            for (int tamanho = 9; tamanho <= 10; tamanho++)
            {
                int soma = 0;
                for (int i = 0; i < tamanho; i++)
                {
                    soma += (numeros[i] - '0') * (tamanho + 1 - i);
                }

                int resto = soma % 11;
                int digito = resto < 2 ? 0 : 11 - resto;
                if (numeros[tamanho] - '0' != digito)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
