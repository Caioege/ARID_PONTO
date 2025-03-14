using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoEscolar.WebGrid
{
    public class ListaPaginada<T>
    {
        public ListaPaginada()
        {
            Pagina = 1;
            QuantidadeDeItensPorPagina = 10;
            Itens = Enumerable.Empty<T>().ToList();
            Adicional = "{}";
            GridId = "grid";
            Action = "TabelaPaginada";
            ExecutarScripts = string.Empty;
        }

        public string ExecutarScripts { get; private set; }

        public bool DesabilitarTotalizador { get; set; }

        public string GridId { get; set; }

        public string DirecaoDaOrdenacao { get; set; }

        public string TermoDeBusca { get; set; }

        public string Ordenacao { get; set; }

        public int Pagina { get; set; }

        public bool PossuiProximo { get { return ((Pagina * QuantidadeDeItensPorPagina) < TotalDeItens); } }

        public bool PossuiAnterior { get { return Pagina > 1; } }

        public IList<T> Itens { get; set; }

        public int ProximaPagina { get { return Pagina + 1; } }

        public int PaginaAnterior { get { return Pagina - 1; } }

        public int TotalDeItens { get; set; }

        public int QuantidadeDeItensPorPagina { get; set; }

        public int TotalDePaginas 
        { 
            get 
            { 
                return (TotalDeItens % QuantidadeDeItensPorPagina > 0) ? (TotalDeItens / QuantidadeDeItensPorPagina) + 1 : (TotalDeItens / QuantidadeDeItensPorPagina); 
            } 
        }

        public string Controller { get; set; }

        public string Action { get; set; } = "TabelaPaginada";

        public bool Ordenamento { get { return (!string.IsNullOrWhiteSpace(DirecaoDaOrdenacao) && DirecaoDaOrdenacao == "DESC") ? false : true; } }

        public string Adicional { get; set; }

        public void Parametros(Controller controllerContext, IList<T> entities, int totalRecords)
        {
            Itens = entities;
            TotalDeItens = totalRecords;
            controllerContext.ViewBag.SearchTerm = TermoDeBusca;
            controllerContext.ViewBag.QuantidadeDeItens = QuantidadeDeItensPorPagina;
        }

        public void Parametros(Controller controllerContext, IList<T> entities, int totalRecords, string action, string controller)
        {
            Itens = entities;
            TotalDeItens = totalRecords;
            Action = action;
            Controller = controller;
            controllerContext.ViewBag.SearchTerm = TermoDeBusca;
        }

        public void Parametros(Controller controllerContext, IList<T> entities, int totalRecords, string action)
        {
            Itens = entities;
            TotalDeItens = totalRecords;
            Action = action;
            controllerContext.ViewBag.SearchTerm = TermoDeBusca;
        }

        public void AdicioneScriptsParaExecutar(string script)
        {
            if (!string.IsNullOrWhiteSpace(script))
            {
                ExecutarScripts += script;
            }
        }
    }
}
