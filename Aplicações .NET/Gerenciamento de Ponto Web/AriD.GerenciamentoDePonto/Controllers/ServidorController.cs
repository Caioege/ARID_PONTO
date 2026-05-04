using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class ServidorController : BaseController
    {
        private readonly IServico<Servidor> _servico;
        private readonly IServico<VinculoDeTrabalho> _servicoVinculoDeTrabalho;
        private readonly IServico<TipoDoVinculoDeTrabalho> _servicoTipoDoVinculo;
        private readonly IServico<LotacaoUnidadeOrganizacional> _servicoLotacao;
        private readonly IServico<UnidadeOrganizacional> _servicoUnidadeOrganizacional;
        private readonly IServico<Funcao> _servicoFuncao;
        private readonly IServico<Departamento> _servicoDepartamento;
        private readonly IServico<HorarioDeTrabalho> _servicoHorarioDeTrabalho;
        private readonly IServico<Afastamento> _servicoAfastamento;
        private readonly IServico<JustificativaDeAusencia> _servicoJustificativa;
        private readonly IServico<AnexoServidor> _servicoAnexoServidor;
        private readonly IServico<MotivoDeDemissao> _servicoMotivoDeDemissao;
        private readonly IServicoDeServidor _servicoDeServidor;
        private readonly IServico<ObservacaoServidor> _servicoObservacao;

        public ServidorController(
            IServico<Servidor> servico,
            IServico<VinculoDeTrabalho> servicoVinculoDeTrabalho,
            IServico<TipoDoVinculoDeTrabalho> servicoTipoDoVinculo,
            IServico<LotacaoUnidadeOrganizacional> servicoLotacao,
            IServico<UnidadeOrganizacional> servicoUnidadeOrganizacional,
            IServico<Funcao> servicoFuncao,
            IServico<Departamento> servicoDepartamento,
            IServico<HorarioDeTrabalho> servicoHorarioDeTrabalho,
            IServico<Afastamento> servicoAfastamento,
            IServico<JustificativaDeAusencia> servicoJustificativa,
            IServico<AnexoServidor> servicoAnexoServidor,
            IServico<MotivoDeDemissao> servicoMotivoDeDemissao,
            IServicoDeServidor servicoDeServidor,
            IServico<ObservacaoServidor> servicoObservacao)
        {
            _servico = servico;
            _servicoVinculoDeTrabalho = servicoVinculoDeTrabalho;
            _servicoTipoDoVinculo = servicoTipoDoVinculo;
            _servicoLotacao = servicoLotacao;
            _servicoUnidadeOrganizacional = servicoUnidadeOrganizacional;
            _servicoFuncao = servicoFuncao;
            _servicoDepartamento = servicoDepartamento;
            _servicoHorarioDeTrabalho = servicoHorarioDeTrabalho;
            _servicoAfastamento = servicoAfastamento;
            _servicoJustificativa = servicoJustificativa;
            _servicoAnexoServidor = servicoAnexoServidor;
            _servicoMotivoDeDemissao = servicoMotivoDeDemissao;
            _servicoDeServidor = servicoDeServidor;
            _servicoObservacao = servicoObservacao;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Servidor> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View(listaPaginada);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<Servidor> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View("_TabelaPaginada", listaPaginada);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Adicionar()
        {
            try
            {
                return View(new Servidor() { Pessoa = new() { Endereco = new() } });
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult Alterar(int id)
        {
            try
            {
                var servidor = _servico.Obtenha(id);
                return View(servidor);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult Salvar(Servidor servidor, [FromForm] string MotivoAlteracaoConfiguracaoApp)
        {
            var dadosSessao = this.HttpContext.DadosDaSessao();
            int id = servidor.Id;
            servidor.OrganizacaoId = dadosSessao.OrganizacaoId;
            servidor.Pessoa.OrganizacaoId = servidor.OrganizacaoId;

            ValideExistenciaDeServidorComCpf(servidor.OrganizacaoId, servidor.Id, servidor.Pessoa.Cpf);

            if (servidor.Id == 0)
            {
                servidor.DataDeCadastro = DateTime.Now;
                
                if (servidor.AcessoAoAplicativo)
                {
                    servidor.ListaDeHistoricosDeConfiguracaoApp = new List<HistoricoConfiguracaoAppServidor>
                    {
                        new HistoricoConfiguracaoAppServidor
                        {
                            TipoComprovacaoNova = servidor.TipoComprovacaoPontoApp,
                            TipoComprovacaoAnterior = eTipoComprovacaoPontoApp.Nenhuma,
                            Motivo = "Definição inicial ao criar o servidor.",
                            DataAlteracao = DateTime.Now,
                            UsuarioAlteracaoId = dadosSessao.UsuarioId,
                            OrganizacaoId = dadosSessao.OrganizacaoId
                        }
                    };
                }

                id = _servico.Adicionar(servidor);
            }
            else
            {
                var servidorDB = _servico.Obtenha(servidor.Id);
                if (servidorDB.TipoComprovacaoPontoApp != servidor.TipoComprovacaoPontoApp)
                {
                    if (string.IsNullOrWhiteSpace(MotivoAlteracaoConfiguracaoApp))
                        throw new ApplicationException("É obrigatório informar o motivo da alteração da configuração de liveness do aplicativo.");

                    servidorDB.ListaDeHistoricosDeConfiguracaoApp.Add(new HistoricoConfiguracaoAppServidor
                    {
                        TipoComprovacaoAnterior = servidorDB.TipoComprovacaoPontoApp,
                        TipoComprovacaoNova = servidor.TipoComprovacaoPontoApp,
                        Motivo = MotivoAlteracaoConfiguracaoApp.Trim(),
                        DataAlteracao = DateTime.Now,
                        UsuarioAlteracaoId = dadosSessao.UsuarioId,
                        OrganizacaoId = dadosSessao.OrganizacaoId
                    });
                }
                
                servidorDB.Pessoa.Nome = servidor.Pessoa.Nome;
                servidorDB.Pessoa.NomeSocial = servidor.Pessoa.NomeSocial;
                servidorDB.Pessoa.Cpf = servidor.Pessoa.Cpf;
                servidorDB.Pessoa.Rg = servidor.Pessoa.Rg;
                servidorDB.Pessoa.DataDeNascimento = servidor.Pessoa.DataDeNascimento;
                
                servidorDB.Pessoa.Endereco.Cep = servidor.Pessoa.Endereco.Cep;
                servidorDB.Pessoa.Endereco.Logradouro = servidor.Pessoa.Endereco.Logradouro;
                servidorDB.Pessoa.Endereco.Complemento = servidor.Pessoa.Endereco.Complemento;
                servidorDB.Pessoa.Endereco.Numero = servidor.Pessoa.Endereco.Numero;
                servidorDB.Pessoa.Endereco.Bairro = servidor.Pessoa.Endereco.Bairro;
                servidorDB.Pessoa.Endereco.Cidade = servidor.Pessoa.Endereco.Cidade;
                servidorDB.Pessoa.Endereco.UF = servidor.Pessoa.Endereco.UF;

                servidorDB.AcessoAoAplicativo = servidor.AcessoAoAplicativo;
                servidorDB.RegistroDePontoNoAplicativo = servidor.RegistroDePontoNoAplicativo;
                servidorDB.TipoComprovacaoPontoApp = servidor.TipoComprovacaoPontoApp;
                servidorDB.RegistroManualNoAplicativo = servidor.RegistroManualNoAplicativo;
                servidorDB.RegistroDeAtestadoNoAplicativo = servidor.RegistroDeAtestadoNoAplicativo;
                servidorDB.TelefoneDeContato = servidor.TelefoneDeContato;
                servidorDB.Email = servidor.Email;
                servidorDB.CodigoCRM = servidor.CodigoCRM;
                servidorDB.EspecialidadeMedica = servidor.EspecialidadeMedica;
                servidorDB.HabilitaExportacaoParaFolhaDePagamento = servidor.HabilitaExportacaoParaFolhaDePagamento;
                servidorDB.AlertaManutencaoDePonto = servidor.AlertaManutencaoDePonto;

                _servico.Atualizar(servidorDB);
            }

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpGet]
        public async Task<IActionResult> ModalVinculoDeTrabalho(int id)
        {
            var modelo = id == 0 ?
                    new VinculoDeTrabalho() { Inicio = DateTime.Now } :
                    _servicoVinculoDeTrabalho.Obtenha(id);

            var organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;
            var tipos = _servicoTipoDoVinculo
                .ObtenhaLista(c => c.OrganizacaoId == organizacaoId && c.Ativo)
                .OrderBy(c => c.SiglaComDescricao);
            ViewBag.Tipos = new SelectList(tipos, "Id", "SiglaComDescricao");

            var funcoes = _servicoFuncao
                .ObtenhaLista(c => c.OrganizacaoId == organizacaoId && c.Ativa)
                .OrderBy(c => c.SiglaComDescricao);
            ViewBag.Funcoes = new SelectList(funcoes, "Id", "SiglaComDescricao");

            var departamentos = _servicoDepartamento
                .ObtenhaLista(c => c.OrganizacaoId == organizacaoId && c.Ativo)
                .OrderBy(c => c.SiglaComDescricao);
            ViewBag.Departamentos = new SelectList(departamentos, "Id", "SiglaComDescricao");

            var horarios = _servicoHorarioDeTrabalho
                .ObtenhaLista(c => c.OrganizacaoId == organizacaoId && c.Ativo)
                .OrderBy(c => c.SiglaComDescricao);
            ViewBag.Horarios = new SelectList(horarios, "Id", "SiglaComDescricao");

            var motivosDeDemissao = _servicoMotivoDeDemissao
                .ObtenhaLista(c => c.OrganizacaoId == organizacaoId && c.Ativo)
                .OrderBy(c => c.SiglaComDescricao);
            ViewBag.MotivosDeDemissao = new SelectList(motivosDeDemissao, "Id", "SiglaComDescricao");

            var html = await RenderizarComoString("_Modal", modelo);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult SalvarVinculoDeTrabalho(VinculoDeTrabalho vinculoDeTrabalho)
        {
            int id = vinculoDeTrabalho.Id;
            vinculoDeTrabalho.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            if (vinculoDeTrabalho.Id == 0)
                id = _servicoVinculoDeTrabalho.Adicionar(vinculoDeTrabalho);
            else
                _servicoVinculoDeTrabalho.Atualizar(vinculoDeTrabalho);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpGet]
        public IActionResult DadosEdicaoLotacao(int id)
        {
            var lotacao = id == 0 ?
                    null :
                    _servicoLotacao.Obtenha(id);

            var organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            return Json(new
            {
                sucesso = true,
                id = id,
                unidadeId = lotacao?.UnidadeOrganizacionalId,
                unidades = _servicoUnidadeOrganizacional
                    .ObtenhaLista(c => c.OrganizacaoId == organizacaoId)
                    .OrderBy(c => c.Nome)
                    .Select(c => new CodigoDescricaoDTO(c.Id, c.Nome)),
                entrada = lotacao?.Entrada.ToString("yyyy-MM-dd"),
                saida = lotacao?.Saida?.ToString("yyyy-MM-dd"),
                matriculaEquipamento = lotacao?.MatriculaEquipamento
            });
        }

        [HttpPost]
        public IActionResult SalvarLotacao(LotacaoUnidadeOrganizacional lotacao)
        {
            try
            {
                int id = lotacao.Id;
                lotacao.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                if (lotacao.Id == 0)
                    id = _servicoLotacao.Adicionar(lotacao);
                else
                    _servicoLotacao.Atualizar(lotacao);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                var duplicateEntryText = "duplicate entry";
                if (ex.Message.ToLower().Contains(duplicateEntryText) || (ex.InnerException != null && ex.InnerException.Message.ToLower().Contains(duplicateEntryText)))
                {
                    return Json(new { sucesso = false, mensagem = $"Já existe um outro {HttpContext.NomenclaturaServidor().ToLower()} utilizando esse mesmo ID de equipamento nessa unidade." });
                }

                throw ex;
            }
        }

        [HttpGet]
        public async Task<IActionResult> PartialLotacoes(int vinculoId)
        {
            var vinculo = _servicoVinculoDeTrabalho.Obtenha(vinculoId);
            var html = await RenderizarComoString("_Lotacoes", vinculo.Lotacoes);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult RemoverLotacao(int id)
        {
            _servicoLotacao.Remover(_servicoLotacao.Obtenha(id));
            return Json(new { sucesso = true, mensagem = "A lotação foi removida." });
        }

        [HttpGet]
        public async Task<IActionResult> ModalAfastamento(int id, int servidorId)
        {
            var organizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;

            var modelo = id == 0 ?
                new Afastamento() :
                _servicoAfastamento.Obtenha(id);

            if (id == 0)
                ViewBag.Vinculos = new SelectList(
                    _servicoVinculoDeTrabalho
                        .ObtenhaLista(c => c.ServidorId == servidorId)
                        .OrderBy(c => c.Inicio)
                        .ThenBy(c => c.Matricula)
                        .Select(c => new CodigoDescricaoDTO(c.Id, c.ToString())),
                    "Codigo",
                    "Descricao");

            ViewBag.Justificativas = new SelectList(
                _servicoJustificativa
                .ObtenhaLista(c =>
                    c.OrganizacaoId == organizacaoId && c.Ativa && c.LocalDeUso != eLocalDeUsoDeJustificativaDeAusencia.FolhaDePonto)
                .OrderBy(c => c.SiglaComDescricao),
                "Id", "SiglaComDescricao");

            var html = await RenderizarComoString("_ModalAfastamento", modelo);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult SalvarAfastamento(Afastamento afastamento)
        {
            int id = afastamento.Id;
            afastamento.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            if (afastamento.Id == 0)
                id = _servicoAfastamento.Adicionar(afastamento);
            else
                _servicoAfastamento.Atualizar(afastamento);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpGet]
        public async Task<IActionResult> PartialAfastamentos(int servidorId)
        {
            var servidor = _servico.Obtenha(servidorId);
            var html = await RenderizarComoString("_Afastamentos", servidor);
            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult RemoverAfastamento(int afastamentoId)
        {
            _servicoAfastamento.Remover(_servicoAfastamento.Obtenha(afastamentoId));
            return Json(new { sucesso = true, mensagem = "O afastamento foi removido." });
        }

        [HttpGet]
        public async Task<IActionResult> ModalAnexo(int id, int servidorId)
        {
            var modelo = id == 0 ?
                new AnexoServidor() { ServidorId = servidorId } :
                _servicoAnexoServidor.Obtenha(id);

            var html = await RenderizarComoString("_ModalAnexo", modelo);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult SalvarAnexo(AnexoServidor anexo, IFormFile arquivoUpload)
        {
            if(anexo.Id == 0 && arquivoUpload == null)
                throw new ApplicationException("O arquivo é obrigatório para novos anexos.");

            if (arquivoUpload != null)
            {
                var extensoesPermitidas = new[] { ".pdf", ".doc", ".docx", ".html", ".htm", ".jpg", ".jpeg", ".png", ".gif" };
                var extensao = Path.GetExtension(arquivoUpload.FileName).ToLower();

                if (!extensoesPermitidas.Contains(extensao))
                    throw new Exception("Extensão de arquivo não permitida.");

                var nomeArquivo = $"{anexo.ServidorId}_{Guid.NewGuid()}{extensao}";
                var caminhoPasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "anexos", "servidor");

                if (!Directory.Exists(caminhoPasta))
                    Directory.CreateDirectory(caminhoPasta);

                var caminhoCompleto = Path.Combine(caminhoPasta, nomeArquivo);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    arquivoUpload.CopyTo(stream);
                }

                anexo.CaminhoArquivo = $"anexos/servidor/{nomeArquivo}";
            }

            anexo.OrganizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
            if (anexo.Id == 0)
                _servicoAnexoServidor.Adicionar(anexo);
            else
                _servicoAnexoServidor.Atualizar(anexo);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos." });
        }

        [HttpGet]
        public async Task<IActionResult> PartialAnexo(int servidorId)
        {
            var servidor = _servico.Obtenha(servidorId);
            var html = await RenderizarComoString("_Anexos", servidor);
            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult RemoverAnexo(int anexoId)
        {
            _servicoAnexoServidor.Remover(_servicoAnexoServidor.Obtenha(anexoId));
            return Json(new { sucesso = true, mensagem = "O anexo foi removido." });
        }

        [HttpGet]
        public IActionResult BaixarArquivo(int anexoId)
        {
            var anexo = _servicoAnexoServidor.Obtenha(anexoId);
            if (anexo == null)
                return NotFound("Registro não encontrado.");

            var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", anexo.CaminhoArquivo);

            if (!System.IO.File.Exists(caminhoArquivo))
                return NotFound("Arquivo físico não encontrado no servidor.");

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(caminhoArquivo, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            var fileBytes = System.IO.File.ReadAllBytes(caminhoArquivo);

            return Json(new
            {
                sucesso = true,
                fileName = $"{anexo.Descricao}{Path.GetExtension(anexo.CaminhoArquivo)}",
                base64 = Convert.ToBase64String(fileBytes),
                mimeType = contentType
            });
        }

        [HttpPost]
        public ActionResult Remover(int id)
        {
            var servidor = _servico.Obtenha(id);

            if (servidor.VinculosDeTrabalho.Count > 0)
                throw new ApplicationException($"Não é possível remover o cadastro do {HttpContext.NomenclaturaServidor().ToLower()}, pois existem vínculos de trabalho cadastrados para ele.");

            _servico.Remover(servidor);

            return Json(new { sucesso = true, mensagem = $"O {HttpContext.NomenclaturaServidor().ToLower()} foi removido." });
        }

        [HttpPost]
        public IActionResult ExecutarAcaoEmLote(int acao, string motivo)
        {
            _servicoDeServidor.ExecuteAcaoEmLote(acao, motivo, this.HttpContext.DadosDaSessao());
            return Json(new { sucesso = true, mensagem = $"A ação em lote foi executada." });
        }

        [HttpGet]
        public async Task<IActionResult> ModalObservacao(int id, int servidorId)
        {
            var modelo = id == 0 ?
                new ObservacaoServidor() { ServidorId = servidorId, Ativa = true } :
                _servicoObservacao.Obtenha(id);

            var html = await RenderizarComoString("_ModalObservacao", modelo);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult SalvarObservacao(ObservacaoServidor observacao)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            observacao.OrganizacaoId = dadosDaSessao.OrganizacaoId;
            observacao.UsuarioId = dadosDaSessao.UsuarioId;
            observacao.CadastradaEm = DateTime.Now;

            if (observacao.Id == 0)
                _servicoObservacao.Adicionar(observacao);
            else
            {
                var persistido = _servicoObservacao.Obtenha(observacao.Id);

                persistido.Texto = observacao.Texto;
                persistido.Ativa = observacao.Ativa;

                _servicoObservacao.Atualizar(persistido);
            }

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos." });
        }

        [HttpGet]
        public async Task<IActionResult> PartialObservacao(int servidorId)
        {
            var servidor = _servico.Obtenha(servidorId);
            var html = await RenderizarComoString("_Observacoes", servidor);
            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult RemoverObservacao(int observacaoId)
        {
            _servicoAnexoServidor.Remover(_servicoAnexoServidor.Obtenha(observacaoId));
            return Json(new { sucesso = true, mensagem = "A observação foi removida." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Servidor> listaPaginada)
        {
            var dados = _servico.ObtenhaListaPaginada(
                CarregueFiltrosDePesquisa(listaPaginada), 
                listaPaginada.Pagina, 
                listaPaginada.QuantidadeDeItensPorPagina,
                c => c.Pessoa?.Nome,
                true);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }

        private Expression<Func<Servidor, bool>> CarregueFiltrosDePesquisa(
            ListaPaginada<Servidor> listaPaginada)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();

            Expression<Func<Servidor, bool>> pesquisa = c =>
                (c.OrganizacaoId == dadosDaSessao.OrganizacaoId);

            if (dadosDaSessao.UnidadeOrganizacionais.Any())
            {
                var unidadesIds = dadosDaSessao.UnidadeOrganizacionais;
                pesquisa = ConcatenadorDeExpressao.Concatenar(
                    pesquisa,
                    c => c.VinculosDeTrabalho.Any(v => v.Lotacoes.Any(l => unidadesIds.Contains(l.UnidadeOrganizacionalId))));
            }

            if (dadosDaSessao.DepartamentoId.HasValue)
            {
                var departamentoId = dadosDaSessao.DepartamentoId.Value;
                pesquisa = ConcatenadorDeExpressao.Concatenar(
                    pesquisa,
                    c => c.VinculosDeTrabalho.Any(v => v.DepartamentoId == departamentoId));
            }

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                var pesquisaToLower = listaPaginada.TermoDeBusca.ToLower().Trim();
                var somenteNumeros = ObterSomenteNumeros(listaPaginada.TermoDeBusca, "----");

                pesquisa = ConcatenadorDeExpressao.Concatenar(
                    pesquisa,
                    c => (
                        c.Pessoa.Nome.ToLower().Contains(pesquisaToLower) ||
                        c.Pessoa.Cpf.Replace(".", "").Replace("-", "").Contains(somenteNumeros)) ||
                        c.Pessoa.Rg.Contains(somenteNumeros) ||
                        c.Pessoa.NomeSocial.ToLower().Contains(pesquisaToLower));
            }

            return pesquisa;
        }

        static string ObterSomenteNumeros(string texto, string returnIfNull)
        {
            if (string.IsNullOrEmpty(texto))
                return returnIfNull;

            var retorno = new string(texto.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(retorno))
                return returnIfNull;

            return retorno;
        }

        [HttpPost]
        public IActionResult RestaurarSenhaPadrao(int id)
        {
            try
            {
                var servidor = _servico.Obtenha(id);
                if (servidor == null)
                    return Json(new { sucesso = false, mensagem = "Servidor não encontrado." });

                servidor.SenhaPersonalizadaDeAcesso = null;
                _servico.Atualizar(servidor);

                return Json(new { sucesso = true, mensagem = "A senha foi restaurada para o padrão (data de nascimento)." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        private void ValideExistenciaDeServidorComCpf(
            int organizacaoId,
            int servidorId,
            string cpf)
        {
            if (!string.IsNullOrEmpty(cpf))
            {
                if (_servico.Obtenha(c =>
                                c.OrganizacaoId == organizacaoId &&
                                !string.IsNullOrEmpty(c.Pessoa.Cpf) &&
                                c.Pessoa.Cpf == cpf &&
                                c.Id != servidorId) != null)
                {
                    throw new ApplicationException($"Já existe um outro {HttpContext.NomenclaturaServidor().ToLower()} cadastrado com esse CPF.");
                }
            }
        }
    }
}