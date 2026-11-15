namespace Vendinha.Api.Negocio
{
    public enum TipoErroNegocio
    {
        Validacao,
        NaoEncontrado,
        Conflito
    }

    public class ErroNegocioException : Exception
    {
        public TipoErroNegocio Tipo { get; }

        public ErroNegocioException(TipoErroNegocio tipo, string mensagem) : base(mensagem)
        {
            Tipo = tipo;
        }
    }
}
