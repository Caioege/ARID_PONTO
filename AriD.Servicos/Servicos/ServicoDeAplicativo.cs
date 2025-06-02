using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Extensao;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeAplicativo : IServicoDeAplicativo
    {
        private readonly IRepositorio<Servidor> _repositorioServidor;

        public ServicoDeAplicativo(
            IRepositorio<Servidor> repositorioServidor)
        {
            _repositorioServidor = repositorioServidor;
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
	                            h.Id = @HORARIOID
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