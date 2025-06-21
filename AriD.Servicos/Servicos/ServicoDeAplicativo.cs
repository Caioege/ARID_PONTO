using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Extensao;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeAplicativo : IServicoDeAplicativo
    {
        private readonly IRepositorio<Servidor> _repositorioServidor;
        private readonly IRepositorio<RegistroAplicativo> _repositorioRegistroApp;
        private readonly IRepositorio<RegistroDePonto> _repositorioRegistroDePonto;
        private readonly IRepositorio<VinculoDeTrabalho> _repositorioVinculo;

        public ServicoDeAplicativo(
            IRepositorio<Servidor> repositorioServidor,
            IRepositorio<RegistroAplicativo> repositorioRegistroApp,
            IRepositorio<RegistroDePonto> repositorioRegistroDePonto,
            IRepositorio<VinculoDeTrabalho> repositorioVinculo)
        {
            _repositorioServidor = repositorioServidor;
            _repositorioRegistroApp = repositorioRegistroApp;
            _repositorioRegistroDePonto = repositorioRegistroDePonto;
            _repositorioVinculo = repositorioVinculo;
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
                        s.RegistroDeAtestadoNoAplicativo
                    from pessoa p
                    inner join organizacao o
	                    on o.Id = p.OrganizacaoId
                    inner join servidor s
	                    on s.PessoaId = p.Id
                    where
	                    replace(replace(p.Cpf, '.', ''), '-', '') = @USUARIO
                        and DATE_FORMAT(p.DataDeNascimento, '%d%m%Y') = @SENHA
                        and s.AcessoAoAplicativo = true
                    limit 1";

                return _repositorioServidor.ConsultaDapper<AutenticacaoAppDTO>(queryAcesso, new
                {
                    @USUARIO = ObterSomenteNumeros(credenciais.Usuario, "---"),
                    @SENHA = ObterSomenteNumeros(credenciais.Senha, "---")
                }).FirstOrDefault();
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

        public void ReceptarRegistro(PostRegistroDePontoDTO registro)
        {
            try
            {
                var dataAgora = DateTime.Now;
                var vinculo = _repositorioVinculo.Obtenha(registro.VinculoDeTrabalhoId);

                string anexoPontoNome = null;
                if (registro.Imagem != null)
                {
                    if (!registro.Imagem.ContentType.Contains("image"))
                        throw new ApplicationException("O comprovante enviado não é uma imagem.");

                    var extensaoArquivo = Path.GetExtension(registro.Imagem.FileName);

                    using (var ms = new MemoryStream())
                    {
                        registro.Imagem.CopyTo(ms);
                        var arquivo = ms.ToArray();

                        var pastaBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "registrosapp", $"{vinculo.OrganizacaoId}");

                        if (!Path.Exists(pastaBase))
                            Directory.CreateDirectory(pastaBase);

                        anexoPontoNome = $"{vinculo.Id}_{dataAgora.Ticks}_{Guid.NewGuid()}.{extensaoArquivo}";
                        var caminho = Path.Combine(pastaBase, anexoPontoNome);

                        using (FileStream fs = new FileStream(caminho, FileMode.OpenOrCreate, FileAccess.Write))
                            fs.Write(arquivo, 0, arquivo.Length);
                    }
                }

                var registroAplicativo = new RegistroAplicativo
                {
                    OrganizacaoId = vinculo.OrganizacaoId,
                    Situacao = registro.Manual ? eSituacaoRegistroAplicativo.AguardandoAvaliacao : eSituacaoRegistroAplicativo.Aprovado,
                    Manual = registro.Manual,
                    Observacao = registro.Observacao,
                    DataHora = registro.Manual && registro.DataHora.HasValue ? registro.DataHora.Value : dataAgora,
                    VinculoDeTrabalhoId = registro.VinculoDeTrabalhoId,
                    Latitude = registro.Latitude,
                    Longitude = registro.Longitude,
                    AnexoPonto = anexoPontoNome,
                    DataInicialAtestado = registro.DataInicialAtestado,
                    DataFinalAtestado = registro.DataFinalAtestado,
                    JustificativaDeAusenciaId = registro.JustificativaDeAusenciaId
                };

                if (registro.DataHora > dataAgora)
                    throw new ApplicationException("O registro não pode ser para uma hora futura.");

                _repositorioRegistroApp.Add(registroAplicativo);

                if (!registro.Manual)
                {
                    /* Se for manual vai para a avaliação do administrador. */
                    var registroDePonto = new RegistroDePonto
                    {
                        OrganizacaoId = registroAplicativo.OrganizacaoId,
                        DataHoraRegistro = registroAplicativo.DataHora,
                        DataHoraRecebimento = DateTime.Now,
                        TipoRegistro = eTipoDeRegistroEquipamento.Aplicativo,
                        RegistroAplicativoId = registroAplicativo.Id
                    };

                    _repositorioRegistroDePonto.Add(registroDePonto);
                }

                _repositorioRegistroApp.Commit();
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
    }
}