using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Extensao;
using AriD.Servicos.Helpers;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeAplicativo : IServicoDeAplicativo
    {
        private readonly IRepositorio<Servidor> _repositorioServidor;
        private readonly IRepositorio<RegistroAplicativo> _repositorioRegistroApp;
        private readonly IRepositorio<RegistroDePonto> _repositorioRegistroDePonto;
        private readonly IRepositorio<VinculoDeTrabalho> _repositorioVinculo;

        private readonly IWhatsappService _whatsappService;
        private readonly IEmailService _emailService;

        public ServicoDeAplicativo(
            IRepositorio<Servidor> repositorioServidor,
            IRepositorio<RegistroAplicativo> repositorioRegistroApp,
            IRepositorio<RegistroDePonto> repositorioRegistroDePonto,
            IRepositorio<VinculoDeTrabalho> repositorioVinculo,
            IWhatsappService whatsappService,
            IEmailService emailService)
        {
            _repositorioServidor = repositorioServidor;
            _repositorioRegistroApp = repositorioRegistroApp;
            _repositorioRegistroDePonto = repositorioRegistroDePonto;
            _repositorioVinculo = repositorioVinculo;
            _whatsappService = whatsappService;
            _emailService = emailService;
        }

        public AutenticacaoAppDTO AutenticarUsuario(CredenciaisDTO credenciais)
        {
            try
            {
                var queryAcesso =
                    @"select
	                    s.Id as ServidorId,
                        upper(p.Nome) as ServidorNome,
                        o.Id as OrganizacaoId,
                        o.Nome as OrganizacaoNome,
                        p.Cpf as Cpf,
                        s.RegistroDePontoNoAplicativo,
                        s.RegistroManualNoAplicativo,
                        s.RegistroDeAtestadoNoAplicativo,
                        s.TipoComprovacaoPontoApp as TipoComprovacaoApp
                    from pessoa p
                    inner join organizacao o
	                    on o.Id = p.OrganizacaoId
                    inner join servidor s
	                    on s.PessoaId = p.Id
                    where
	                    replace(replace(p.Cpf, '.', ''), '-', '') = @USUARIO
                        and IF(s.SenhaPersonalizadaDeAcesso IS NULL, DATE_FORMAT(p.DataDeNascimento, '%d%m%Y') = @SENHA, s.SenhaPersonalizadaDeAcesso = @SENHACRIPTOGRAFADA) = true
                        and s.AcessoAoAplicativo = true
                    limit 1";

                var result = _repositorioServidor.ConsultaDapper<AutenticacaoAppDTO>(queryAcesso, new
                {
                    @USUARIO = ObterSomenteNumeros(credenciais.Usuario, "---"),
                    @SENHA = ObterSomenteNumeros(credenciais.Senha, "---"),
                    @SENHACRIPTOGRAFADA = Criptografia.CriptografarSenha(credenciais.Senha)
                }).FirstOrDefault();

                if (result != null)
                {
                    var caminhoFoto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "organizacao", $"{result.OrganizacaoId}", $"{result.ServidorId}.png");
                    if (File.Exists(caminhoFoto))
                    {
                        var bytes = File.ReadAllBytes(caminhoFoto);
                        result.FotoBase64 = Convert.ToBase64String(bytes);
                    }
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TelaHorarioDeTrabalhoAppDTO> ObtenhaHorariosDoServidor(int servidorId)
        {
            try
            {
                var query =
                    @"select
	                    distinct h.Id as HorarioId,
	                    h.Descricao as HorarioDescricao
                    from vinculodetrabalho v
                    inner join horariodetrabalho h
	                    on h.Id = v.HorarioDeTrabalhoId
                    where
	                    v.ServidorId = @SERVIDORID
                    order by 2";

                var horarios = _repositorioServidor.ConsultaDapper<TelaHorarioDeTrabalhoAppDTO>(query, new { @SERVIDORID = servidorId });

                foreach (var horario in horarios)
                {
                    var horariosDia = _repositorioServidor
                        .ConsultaDapper<HorarioDeTrabalhoDia>(
                            @"select
	                            *
                            from horariodetrabalhodia h
                            where
	                            h.HorarioDeTrabalhoId = @HORARIOID
                            order by h.DiaDaSemana", new { @HORARIOID = horario.HorarioId });

                    foreach (eDiaDaSemana diaDaSemana in Enum.GetValues(typeof(eDiaDaSemana)))
                    {
                        var horarioDia = horariosDia
                            .FirstOrDefault(c => c.DiaDaSemana == diaDaSemana);

                        horario.Dias.Add(new ItemHorarioAppDTO
                        {
                            Entrada1 = horarioDia?.Entrada1,
                            Saida1 = horarioDia?.Saida1,
                            Entrada2 = horarioDia?.Entrada2,
                            Saida2 = horarioDia?.Saida2,
                            Entrada3 = horarioDia?.Entrada3,
                            Saida3 = horarioDia?.Saida3,
                            Entrada4 = horarioDia?.Entrada4,
                            Saida4 = horarioDia?.Saida4,
                            Entrada5 = horarioDia?.Entrada5,
                            Saida5 = horarioDia?.Saida5,
                            CargaHoraria = horarioDia?.CalculeCargaHorariaTotal(false),
                            DiaDaSemana = diaDaSemana.DescricaoDoEnumerador(),
                            Utiliza5Periodos = false
                        });
                    }
                }

                return horarios;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<EventoAppDTO> ObtenhaListaDeEventos(int organizacaoId)
        {
            try
            {
                var query =
                    @"select
	                    Descricao,
                        Data,
                        Tipo as TipoEvento
                    from eventoanual
                    where
	                    OrganizacaoId = @ORGANIZACAOID
                    order by Data";

                return _repositorioServidor.ConsultaDapper<EventoAppDTO>(query, new
                {
                    @ORGANIZACAOID = organizacaoId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<CodigoDescricaoDTO> ObtenhaListaDeVinculos(int servidorId)
        {
            try
            {
                var query =
                    @"select
	                    v.Id as Codigo,
                        concat(v.Matricula, ' - ', f.Descricao) as Descricao
                    from vinculodetrabalho v
                    inner join funcao f
	                    on f.Id = v.FuncaoId
                    where
	                    ServidorId = @SERVIDORID
                    order by Inicio desc";

                return _repositorioServidor.ConsultaDapper<CodigoDescricaoDTO>(query, new
                {
                    @SERVIDORID = servidorId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<CodigoDescricaoDTO> ObtenhaListaDeLotacoes(int vinculoId)
        {
            try
            {
                var query =
                    @"select
	                    u.Id as Codigo,
                        u.Nome as Descricao
                    from lotacaounidadeorganizacional l
                    inner join unidadeorganizacional u
	                    on u.Id = l.UnidadeOrganizacionalId
                    where
	                    l.VinculoDeTrabalhoId = @VINCULOID
                    order by 2";

                return _repositorioServidor.ConsultaDapper<CodigoDescricaoDTO>(query, new
                {
                    @VINCULOID = vinculoId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<KeyValuePair<DateTime, string>> ObtenhaUltimosRegistrosDoServidor(int servidorId)
        {
            try
            {
                var query =
                    @"select
	                        itens.*
                        from
                        ((select
	                        app.DataHora as 'Key',
                            if(app.Manual = true, 'Manual', 'Aplicativo') as 'Value'
                        from registroaplicativo app
                        inner join vinculodetrabalho vin
	                        on vin.Id = app.VinculoDeTrabalhoId
                        where
	                        vin.ServidorId = @SERVIDORID
                            and app.JustificativaDeAusenciaId is null
                        order by app.DataHora desc
                        limit 5) union 
                        (select
	                        reg.DataHoraRegistro as 'Key',
                            'Equipamento de Ponto' as 'Value'
                        from registrodeponto reg
                        inner join equipamentodeponto eq
	                        on eq.Id = reg.EquipamentoDePontoId
                        inner join lotacaounidadeorganizacional lot
	                        on lot.MatriculaEquipamento = reg.UsuarioEquipamentoId
                            and lot.UnidadeOrganizacionalId = eq.UnidadeOrganizacionalId
                        inner join vinculodetrabalho vin
	                        on vin.Id = lot.VinculoDeTrabalhoId
                        where
	                        vin.ServidorId = @SERVIDORID
                            and reg.RegistroAplicativoId is null
                        order by reg.DataHoraRegistro desc
                        limit 5)) as itens
                        order by itens.Key desc
                        limit 5";

                return _repositorioServidor.ConsultaDapper<KeyValuePair<DateTime, string>>(query, new { @SERVIDORID = servidorId });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<CodigoDescricaoDTO> ObtenhaListaDeJustificativas(int organizacaoId)
        {
            try
            {
                var query =
                    @"select
	                    Id as 'Codigo',
                        Descricao as 'Descricao'
                    from justificativadeausencia
                    where
	                    OrganizacaoId = @ORGANIZACAOID
                        and Ativa = true
                        and LocalDeUso <> @SOMENTEAFASTAMENTO
                    order by 2";

                return _repositorioServidor.ConsultaDapper<CodigoDescricaoDTO>(query, 
                    new { @ORGANIZACAOID = organizacaoId, @SOMENTEAFASTAMENTO = eLocalDeUsoDeJustificativaDeAusencia.Afastamento });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ReceptarRegistro(PostRegistroDePontoDTO registro, bool fromApp)
        {
            try
            {
                var dataAgora = DateTime.Now;
                var vinculo = _repositorioVinculo.Obtenha(registro.VinculoDeTrabalhoId);

                var configuracaoAplicativo = eTipoComprovacaoPontoApp.Nenhuma;
                if (registro.ImagemDesafio != null) configuracaoAplicativo = eTipoComprovacaoPontoApp.LivenessFacial;
                else if (registro.Imagem != null) configuracaoAplicativo = eTipoComprovacaoPontoApp.ApenasSelfie;

                bool ehPontoComum = !registro.Manual && !registro.JustificativaDeAusenciaId.HasValue;

                if (ehPontoComum && vinculo.Servidor.TipoComprovacaoPontoApp != eTipoComprovacaoPontoApp.Nenhuma && vinculo.Servidor.TipoComprovacaoPontoApp != configuracaoAplicativo)
                    throw new ApplicationException("Suas configurações de segurança foram alteradas. Por favor, saia do aplicativo e entre novamente para sincronizar os dados e depois refaça o registro de ponto.");

                string anexoPontoNome = null;
                string anexoLivenessNome = null;

                if (registro.ImagemDesafio != null)
                {
                    var extensao = Path.GetExtension(registro.ImagemDesafio.FileName);
                    using (var ms = new MemoryStream())
                    {
                        registro.ImagemDesafio.CopyTo(ms);
                        var arquivo = ms.ToArray();

                        var pastaLiveness = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "registrosapp", $"{vinculo.OrganizacaoId}", "liveness");
                        if (!Path.Exists(pastaLiveness)) Directory.CreateDirectory(pastaLiveness);

                        var nomeUnico = $"{vinculo.Id}_LIVENESS_{dataAgora.Ticks}_{Guid.NewGuid()}.{extensao}";
                        var caminho = Path.Combine(pastaLiveness, nomeUnico);

                        using (FileStream fs = new FileStream(caminho, FileMode.OpenOrCreate, FileAccess.Write))
                            fs.Write(arquivo, 0, arquivo.Length);

                        anexoLivenessNome = nomeUnico;
                        anexoPontoNome = $"liveness/{nomeUnico}";
                    }
                }
                else if (vinculo.Servidor.TipoComprovacaoPontoApp != eTipoComprovacaoPontoApp.Nenhuma)
                {
                    if (registro.Imagem != null)
                    {
                        if (registro.Imagem.ContentType == null || !registro.Imagem.ContentType.Contains("image") && fromApp)
                            throw new ApplicationException("O comprovante enviado não é uma imagem.");

                        var extensaoArquivo = Path.GetExtension(registro.Imagem.FileName);
                        using (var ms = new MemoryStream())
                        {
                            registro.Imagem.CopyTo(ms);
                            var arquivo = ms.ToArray();

                            var pastaBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "registrosapp", $"{vinculo.OrganizacaoId}");
                            if (!Path.Exists(pastaBase)) Directory.CreateDirectory(pastaBase);

                            anexoPontoNome = $"{vinculo.Id}_{dataAgora.Ticks}_{Guid.NewGuid()}.{extensaoArquivo}";
                            var caminho = Path.Combine(pastaBase, anexoPontoNome);

                            using (FileStream fs = new FileStream(caminho, FileMode.OpenOrCreate, FileAccess.Write))
                                fs.Write(arquivo, 0, arquivo.Length);
                        }
                    }
                }

                string comprovanteAtestadoNome = null;
                if (registro.AnexoAtestado != null)
                {
                    var extensaoArquivoAnexoAtestado = Path.GetExtension(registro.AnexoAtestado.FileName);

                    using (var ms = new MemoryStream())
                    {
                        registro.AnexoAtestado.CopyTo(ms);
                        var arquivo = ms.ToArray();

                        var pastaBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "anexos", "atestados", $"{vinculo.OrganizacaoId}");

                        if (!Path.Exists(pastaBase))
                            Directory.CreateDirectory(pastaBase);

                        comprovanteAtestadoNome = $"{vinculo.Id}_ATESTADO_{dataAgora.Ticks}_{Guid.NewGuid()}{extensaoArquivoAnexoAtestado}";
                        var caminho = Path.Combine(pastaBase, comprovanteAtestadoNome);

                        using (FileStream fs = new FileStream(caminho, FileMode.OpenOrCreate, FileAccess.Write))
                            fs.Write(arquivo, 0, arquivo.Length);
                    }
                }

                bool foraDaCerca = false;

                if (!registro.Manual)
                {
                    try
                    {
                        double regLat = 0;
                        double regLon = 0;

                        bool coordenadasRegistroValidas =
                            !string.IsNullOrWhiteSpace(registro.Latitude) &&
                            !string.IsNullOrWhiteSpace(registro.Longitude) &&
                            double.TryParse(registro.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out regLat) &&
                            double.TryParse(registro.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out regLon);

                        if (coordenadasRegistroValidas && vinculo.Lotacoes != null && vinculo.Lotacoes.Any())
                        {
                            bool dentroDeAlgumaUnidade = false;

                            foreach (var lotacao in vinculo.Lotacoes)
                            {
                                var unidade = lotacao.UnidadeOrganizacional;
                                if (!unidade.RaioDaCercaVirtualEmMetros.HasValue || string.IsNullOrEmpty(unidade.Longitude) || string.IsNullOrEmpty(unidade.Latitude))
                                {
                                    dentroDeAlgumaUnidade = true;
                                    break;
                                }

                                double uniLat = 0;
                                double uniLon = 0;

                                bool unidadeValida =
                                    !string.IsNullOrWhiteSpace(unidade.Latitude) &&
                                    !string.IsNullOrWhiteSpace(unidade.Longitude) &&
                                    unidade.RaioDaCercaVirtualEmMetros.HasValue &&
                                    double.TryParse(unidade.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out uniLat) &&
                                    double.TryParse(unidade.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out uniLon);

                                if (unidadeValida)
                                {
                                    var distancia = CalculeDistanciaEmMetros(regLat, regLon, uniLat, uniLon);

                                    if (distancia <= unidade.RaioDaCercaVirtualEmMetros.Value)
                                    {
                                        dentroDeAlgumaUnidade = true;
                                        break;
                                    }
                                }
                            }

                            if (!dentroDeAlgumaUnidade)
                            {
                                foraDaCerca = true;
                            }
                        }
                    }
                    catch { }
                }

                var motivoAuditoria = string.Empty;
                if (registro.IsMockLocation)
                    motivoAuditoria += "[ALERTA] GPS Simulado detectado; ";

                bool vivacidadeObrigatoria = vinculo.Servidor.TipoComprovacaoPontoApp == eTipoComprovacaoPontoApp.LivenessFacial;
                bool livenessFalhou = vivacidadeObrigatoria && !registro.LivenessSuccess;

                if (livenessFalhou)
                    motivoAuditoria += "[ALERTA] Falha no teste de vivacidade (Liveness); ";

                if (foraDaCerca)
                    motivoAuditoria += "[AVISO] Registro fora da cerca virtual; ";

                if (registro.Manual)
                    motivoAuditoria += "[AVISO] Registro manual; ";

                var registroAplicativo = new RegistroAplicativo
                {
                    OrganizacaoId = vinculo.OrganizacaoId,
                    Situacao = (registro.Manual || foraDaCerca || registro.IsMockLocation || livenessFalhou) ? eSituacaoRegistroAplicativo.AguardandoAvaliacao : eSituacaoRegistroAplicativo.Aprovado,
                    Manual = registro.Manual,
                    Observacao = registro.Observacao,
                    DataHora = registro.Manual && registro.DataHora.HasValue ? registro.DataHora.Value : dataAgora,
                    VinculoDeTrabalhoId = registro.VinculoDeTrabalhoId,
                    Latitude = registro.Latitude,
                    Longitude = registro.Longitude,
                    AnexoPonto = anexoPontoNome,
                    DataInicialAtestado = registro.DataInicialAtestado,
                    DataFinalAtestado = registro.DataFinalAtestado,
                    JustificativaDeAusenciaId = registro.JustificativaDeAusenciaId,
                    ForaDaCerca = foraDaCerca,
                    MockGPS = registro.IsMockLocation,
                    LivenessSuccess = registro.LivenessSuccess,
                    AnexoLiveness = anexoLivenessNome,
                    MotivoAuditoria = string.IsNullOrWhiteSpace(motivoAuditoria) ? null : motivoAuditoria.Trim(),
                    ComprovanteAtestado = comprovanteAtestadoNome
                };

                if (registro.DataHora > dataAgora)
                    throw new ApplicationException("O registro não pode ser para uma hora futura.");

                RegistroDePonto registroDePonto = null;
                if (!registro.Manual && registroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                {
                    /* Se for manual vai para a avaliação do administrador. */
                    registroDePonto = new RegistroDePonto
                    {
                        OrganizacaoId = registroAplicativo.OrganizacaoId,
                        DataHoraRegistro = registroAplicativo.DataHora,
                        DataHoraRecebimento = DateTime.Now,
                        TipoRegistro = eTipoDeRegistroEquipamento.Aplicativo,
                        RegistroAplicativo = registroAplicativo
                    };

                    _repositorioRegistroDePonto.Add(registroDePonto);
                }
                else
                {
                    _repositorioRegistroApp.Add(registroAplicativo);
                }

                _repositorioRegistroApp.Commit();

                if (!registro.Manual && registroDePonto != null)
                {
                    _whatsappService.EnviarComprovantePontoAsync(vinculo.Servidor, registroDePonto.Id, dataAgora);
                    _emailService.EnviarComprovantePontoAsync(vinculo.Servidor, registroDePonto.Id, dataAgora);
                }
            }
            catch (Exception)
            {
                throw;
            }
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

        private double CalculeDistanciaEmMetros(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3;
            var lat1Rad = lat1 * (Math.PI / 180);
            var lat2Rad = lat2 * (Math.PI / 180);
            var deltaLat = (lat2 - lat1) * (Math.PI / 180);
            var deltaLon = (lon2 - lon1) * (Math.PI / 180);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        public void RegistrarToken(RegistrarTokenDTO registrarToken)
        {
            var servidor = _repositorioServidor.Obtenha(registrarToken.ServidorId);
            if (servidor == null) return;

            servidor.PushToken = registrarToken.Token;
            servidor.PlataformaDispositivo = registrarToken.Plataforma;
            servidor.UltimoAcessoApp = DateTime.Now;

            _repositorioServidor.Atualizar(servidor);
            _repositorioServidor.Commit();
        }

        public void AlterarSenha(int servidorId, string senhaAtual, string novaSenha)
        {
            var servidor = _repositorioServidor.Obtenha(servidorId);
            if (servidor == null) throw new ApplicationException("Servidor não encontrado.");

            bool senhaValida = false;
            
            if (string.IsNullOrEmpty(servidor.SenhaPersonalizadaDeAcesso))
            {
                // Verifica contra a senha padrão (Data de Nascimento ddMMyyyy)
                var senhaPadrao = servidor.Pessoa.DataDeNascimento.ToString("ddMMyyyy");
                if (ObterSomenteNumeros(senhaAtual, "") == senhaPadrao)
                    senhaValida = true;
            }
            else
            {
                // Verifica contra a senha personalizada criptografada
                if (servidor.SenhaPersonalizadaDeAcesso == Criptografia.CriptografarSenha(senhaAtual))
                    senhaValida = true;
            }

            if (!senhaValida)
                throw new ApplicationException("A senha atual informada está incorreta.");

            if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 4)
                throw new ApplicationException("A nova senha deve ter pelo menos 4 caracteres.");

            servidor.SenhaPersonalizadaDeAcesso = Criptografia.CriptografarSenha(novaSenha);
            _repositorioServidor.Atualizar(servidor);
            _repositorioServidor.Commit();
        }
    }
}