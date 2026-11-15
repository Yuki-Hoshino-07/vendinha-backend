using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Vendinha.Api.Negocio;

namespace Vendinha.Api.Controllers
{
    public class ErroNegocioFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var erro = context.Exception as ErroNegocioException;
            if (erro == null)
            {
                return;
            }

            int status = 400;
            switch (erro.Tipo)
            {
                case TipoErroNegocio.NaoEncontrado:
                    status = 404;
                    break;
                case TipoErroNegocio.Conflito:
                    status = 409;
                    break;
            }

            context.Result = new ObjectResult(new { mensagem = erro.Message }) { StatusCode = status };
            context.ExceptionHandled = true;
        }
    }
}
