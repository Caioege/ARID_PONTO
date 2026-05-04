using AriD.Servicos.Servicos.Interfaces;

namespace AriD.GerenciamentoDePonto.Helpers
{
    public class UsuarioAtual : IUsuarioAtual
    {
        private readonly IHttpContextAccessor _http;

        public UsuarioAtual(IHttpContextAccessor http)
        {
            _http = http;
        }

        public int? UsuarioId
        {
            get
            {
                var ctx = _http.HttpContext;
                if (ctx == null) return null;

                var s = ctx.DadosDaSessao();
                return s?.UsuarioId;
            }
        }

        public string Nome
        {
            get
            {
                var ctx = _http.HttpContext;
                if (ctx == null) return "Sistema";

                var s = ctx.DadosDaSessao();
                return string.IsNullOrWhiteSpace(s?.UsuarioNome) ? "Sistema" : s.UsuarioNome;
            }
        }
    }
}
