using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AriD.GerenciamentoDePonto.Helpers
{
    public static class SessionExtensions
    {
        const string session_key = "session_info";

        public static bool EstaAutenticado(this Controller controllerContext)
        {
            return controllerContext?.HttpContext.EstaAutenticado() ?? false;
        }

        public static bool EstaAutenticado(this HttpContext httpContext)
        {
            return ObtenhaSessao(httpContext) != null;
        }

        public static SessaoDTO DadosDaSessao(this Controller controllerContext)
        {
            return ObtenhaSessao(controllerContext);
        }

        public static SessaoDTO DadosDaSessao(this HttpContext httpContext)
        {
            return ObtenhaSessao(httpContext);
        }

        public static void Autenticar(this Controller controllerContext, SessaoDTO sessaoDTO)
        {
            controllerContext.HttpContext.Session
                .Set(session_key,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sessaoDTO)));
        }

        private static SessaoDTO ObtenhaSessao(Controller controllerContext)
        {
            return ObtenhaSessao(controllerContext?.HttpContext);
        }

        private static SessaoDTO ObtenhaSessao(HttpContext httpContext)
        {
            var dados = httpContext?.Session.GetString(session_key);
            if (dados == null)
                return null;

            return JsonConvert.DeserializeObject<SessaoDTO>(dados);
        }

        public static bool PossuiPermissao<T>(this HttpContext httpContext, T permissao)
            where T : Enum
        {
            var sessao = DadosDaSessao(httpContext);
            return sessao.Permissoes.PossuiPermissao(permissao, sessao.UsuarioAdministradorAutenticado);
        }

        public static bool PossuiPermissao<T>(
            this List<KeyValuePair<string, int>> permissoes, 
            T permissao, 
            bool usuarioAdm) 
                where T : Enum
        {
            return usuarioAdm || permissoes.Any(d => d.Key == typeof(T).FullName && d.Value == (int)(object)permissao);
        }
    }
}