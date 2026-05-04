using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoChatRota : IServicoChatRota
    {
        private const int OrigemSistema = 1;
        private const int OrigemAplicativo = 2;
        private const int StatusExecucaoFinalizada = 3;

        private readonly IRepositorio<RotaExecucao> _repositorio;

        public ServicoChatRota(IRepositorio<RotaExecucao> repositorio)
        {
            _repositorio = repositorio;
        }

        public RotaChatResumoDTO ObterChat(int organizacaoId, int rotaExecucaoId)
        {
            var execucao = ObterExecucaoResumo(organizacaoId, rotaExecucaoId);
            if (execucao == null)
                throw new ApplicationException("Execução de rota não encontrada.");

            MarcarComoLidasNoSistema(organizacaoId, rotaExecucaoId);

            return new RotaChatResumoDTO
            {
                RotaExecucaoId = execucao.RotaExecucaoId,
                RotaId = execucao.RotaId,
                RotaDescricao = execucao.RotaDescricao,
                Finalizada = execucao.Finalizada,
                Mensagens = ObterMensagens(organizacaoId, rotaExecucaoId)
            };
        }

        public RotaChatResumoDTO ObterChatAplicativo(int rotaExecucaoId, int servidorId)
        {
            var execucao = ObterExecucaoParaAplicativo(rotaExecucaoId, servidorId);
            if (execucao == null)
                throw new ApplicationException("Execução de rota não encontrada para o usuário informado.");

            MarcarComoLidasNoAplicativo(execucao.OrganizacaoId, rotaExecucaoId);

            return new RotaChatResumoDTO
            {
                RotaExecucaoId = execucao.RotaExecucaoId,
                RotaId = execucao.RotaId,
                RotaDescricao = execucao.RotaDescricao,
                Finalizada = execucao.Finalizada,
                Mensagens = ObterMensagens(execucao.OrganizacaoId, rotaExecucaoId)
            };
        }

        public RotaChatMensagemDTO EnviarMensagemSistema(int organizacaoId, int rotaExecucaoId, int usuarioId, string usuarioNome, string mensagem)
        {
            var execucao = ObterExecucaoResumo(organizacaoId, rotaExecucaoId);
            if (execucao == null)
                throw new ApplicationException("Execução de rota não encontrada.");

            if (execucao.Finalizada)
                throw new ApplicationException("Esta rota já foi finalizada. O chat está disponível somente para consulta.");

            var texto = NormalizarMensagem(mensagem);

            var sql = @"
                INSERT INTO rotaexecucaochat
                    (OrganizacaoId, RotaExecucaoId, RotaId, Origem, UsuarioId, RemetenteNome, Mensagem, DataHoraEnvio, LidaNoSistema, LidaNoAplicativo)
                VALUES
                    (@OrganizacaoId, @RotaExecucaoId, @RotaId, @Origem, @UsuarioId, @RemetenteNome, @Mensagem, @DataHoraEnvio, 1, 0);
                SELECT LAST_INSERT_ID();";

            var id = _repositorio.ConsultaDapper<int>(sql, new
            {
                OrganizacaoId = organizacaoId,
                RotaExecucaoId = rotaExecucaoId,
                execucao.RotaId,
                Origem = OrigemSistema,
                UsuarioId = usuarioId,
                RemetenteNome = usuarioNome,
                Mensagem = texto,
                DataHoraEnvio = DateTime.Now
            }).First();

            return ObterMensagemPorId(organizacaoId, id);
        }

        public RotaChatMensagemDTO EnviarMensagemAplicativo(int rotaExecucaoId, int servidorId, string mensagem)
        {
            var execucao = ObterExecucaoParaAplicativo(rotaExecucaoId, servidorId);
            if (execucao == null)
                throw new ApplicationException("Execução de rota não encontrada para o usuário informado.");

            if (execucao.Finalizada)
                throw new ApplicationException("Esta rota já foi finalizada. O chat está disponível somente para consulta.");

            var texto = NormalizarMensagem(mensagem);

            var sql = @"
                INSERT INTO rotaexecucaochat
                    (OrganizacaoId, RotaExecucaoId, RotaId, Origem, ServidorId, RemetenteNome, Mensagem, DataHoraEnvio, LidaNoSistema, LidaNoAplicativo)
                VALUES
                    (@OrganizacaoId, @RotaExecucaoId, @RotaId, @Origem, @ServidorId, @RemetenteNome, @Mensagem, @DataHoraEnvio, 0, 1);
                SELECT LAST_INSERT_ID();";

            var id = _repositorio.ConsultaDapper<int>(sql, new
            {
                execucao.OrganizacaoId,
                RotaExecucaoId = rotaExecucaoId,
                execucao.RotaId,
                Origem = OrigemAplicativo,
                ServidorId = servidorId,
                RemetenteNome = execucao.RemetenteNome,
                Mensagem = texto,
                DataHoraEnvio = DateTime.Now
            }).First();

            return ObterMensagemPorId(execucao.OrganizacaoId, id);
        }

        public RotaChatPushDestinoDTO? ObterDestinoPushAplicativo(int organizacaoId, int rotaExecucaoId)
        {
            var sql = @"
                SELECT
                    re.Id as RotaExecucaoId,
                    re.RotaId,
                    r.Descricao as RotaDescricao,
                    p.Nome as MotoristaNome,
                    s.PushToken
                FROM rotaexecucao re
                INNER JOIN rota r ON r.Id = re.RotaId
                INNER JOIN motorista m ON m.Id = re.MotoristaId
                INNER JOIN servidor s ON s.Id = m.ServidorId
                INNER JOIN pessoa p ON p.Id = s.PessoaId
                WHERE re.OrganizacaoId = @OrganizacaoId
                  AND re.Id = @RotaExecucaoId
                  AND COALESCE(s.PushToken, '') <> ''
                LIMIT 1";

            return _repositorio.ConsultaDapper<RotaChatPushDestinoDTO>(sql, new { OrganizacaoId = organizacaoId, RotaExecucaoId = rotaExecucaoId }).FirstOrDefault();
        }

        public List<RotaChatNaoLidasDTO> ObterNaoLidasSistema(int organizacaoId, List<int> rotaExecucaoIds)
        {
            var ids = rotaExecucaoIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<RotaChatNaoLidasDTO>();

            var sql = @"
                SELECT
                    RotaExecucaoId,
                    COUNT(1) as Quantidade
                FROM rotaexecucaochat
                WHERE OrganizacaoId = @OrganizacaoId
                  AND RotaExecucaoId IN @RotaExecucaoIds
                  AND Origem = @OrigemAplicativo
                  AND LidaNoSistema = 0
                GROUP BY RotaExecucaoId";

            return _repositorio.ConsultaDapper<RotaChatNaoLidasDTO>(sql, new
            {
                OrganizacaoId = organizacaoId,
                RotaExecucaoIds = ids,
                OrigemAplicativo
            }).ToList();
        }

        public int ObterNaoLidasAplicativo(int rotaExecucaoId, int servidorId)
        {
            var execucao = ObterExecucaoParaAplicativo(rotaExecucaoId, servidorId);
            if (execucao == null)
                throw new ApplicationException("Execução de rota não encontrada para o usuário informado.");

            var sql = @"
                SELECT COUNT(1)
                FROM rotaexecucaochat
                WHERE OrganizacaoId = @OrganizacaoId
                  AND RotaExecucaoId = @RotaExecucaoId
                  AND Origem = @OrigemSistema
                  AND LidaNoAplicativo = 0";

            return _repositorio.ConsultaDapper<int>(sql, new
            {
                execucao.OrganizacaoId,
                RotaExecucaoId = rotaExecucaoId,
                OrigemSistema
            }).FirstOrDefault();
        }

        private RotaChatExecucaoResumoRowDTO? ObterExecucaoResumo(int organizacaoId, int rotaExecucaoId)
        {
            var sql = @"
                SELECT
                    re.Id as RotaExecucaoId,
                    re.OrganizacaoId,
                    re.RotaId,
                    r.Descricao as RotaDescricao,
                    CASE WHEN re.Status = @StatusFinalizada OR re.DataHoraFim IS NOT NULL THEN 1 ELSE 0 END as Finalizada
                FROM rotaexecucao re
                INNER JOIN rota r ON r.Id = re.RotaId
                WHERE re.OrganizacaoId = @OrganizacaoId
                  AND re.Id = @RotaExecucaoId
                LIMIT 1";

            return _repositorio.ConsultaDapper<RotaChatExecucaoResumoRowDTO>(sql, new
            {
                OrganizacaoId = organizacaoId,
                RotaExecucaoId = rotaExecucaoId,
                StatusFinalizada = StatusExecucaoFinalizada
            }).FirstOrDefault();
        }

        private RotaChatExecucaoAppRowDTO? ObterExecucaoParaAplicativo(int rotaExecucaoId, int servidorId)
        {
            var sql = @"
                SELECT
                    re.Id as RotaExecucaoId,
                    re.OrganizacaoId,
                    re.RotaId,
                    r.Descricao as RotaDescricao,
                    p.Nome as RemetenteNome,
                    CASE WHEN re.Status = @StatusFinalizada OR re.DataHoraFim IS NOT NULL THEN 1 ELSE 0 END as Finalizada
                FROM rotaexecucao re
                INNER JOIN rota r ON r.Id = re.RotaId
                INNER JOIN servidor s ON s.Id = @ServidorId
                INNER JOIN pessoa p ON p.Id = s.PessoaId
                WHERE re.Id = @RotaExecucaoId
                  AND (
                      EXISTS (
                          SELECT 1
                          FROM motorista m
                          WHERE m.Id = re.MotoristaId
                            AND m.ServidorId = @ServidorId
                      )
                      OR EXISTS (
                          SELECT 1
                          FROM rotaprofissional rp
                          WHERE rp.RotaId = re.RotaId
                            AND rp.ServidorId = @ServidorId
                      )
                  )
                LIMIT 1";

            return _repositorio.ConsultaDapper<RotaChatExecucaoAppRowDTO>(sql, new
            {
                RotaExecucaoId = rotaExecucaoId,
                ServidorId = servidorId,
                StatusFinalizada = StatusExecucaoFinalizada
            }).FirstOrDefault();
        }

        private List<RotaChatMensagemDTO> ObterMensagens(int organizacaoId, int rotaExecucaoId)
        {
            var sql = @"
                SELECT
                    c.Id,
                    c.RotaExecucaoId,
                    c.RotaId,
                    r.Descricao as RotaDescricao,
                    c.Origem,
                    CASE WHEN c.Origem = @OrigemSistema THEN 'Sistema' ELSE 'Aplicativo' END as OrigemDescricao,
                    c.UsuarioId,
                    c.ServidorId,
                    c.RemetenteNome,
                    c.Mensagem,
                    c.DataHoraEnvio,
                    c.LidaNoSistema,
                    c.LidaNoAplicativo
                FROM rotaexecucaochat c
                INNER JOIN rota r ON r.Id = c.RotaId
                WHERE c.OrganizacaoId = @OrganizacaoId
                  AND c.RotaExecucaoId = @RotaExecucaoId
                ORDER BY c.DataHoraEnvio, c.Id";

            return _repositorio.ConsultaDapper<RotaChatMensagemDTO>(sql, new
            {
                OrganizacaoId = organizacaoId,
                RotaExecucaoId = rotaExecucaoId,
                OrigemSistema
            }).ToList();
        }

        private RotaChatMensagemDTO ObterMensagemPorId(int organizacaoId, int id)
        {
            var sql = @"
                SELECT
                    c.Id,
                    c.RotaExecucaoId,
                    c.RotaId,
                    r.Descricao as RotaDescricao,
                    c.Origem,
                    CASE WHEN c.Origem = @OrigemSistema THEN 'Sistema' ELSE 'Aplicativo' END as OrigemDescricao,
                    c.UsuarioId,
                    c.ServidorId,
                    c.RemetenteNome,
                    c.Mensagem,
                    c.DataHoraEnvio,
                    c.LidaNoSistema,
                    c.LidaNoAplicativo
                FROM rotaexecucaochat c
                INNER JOIN rota r ON r.Id = c.RotaId
                WHERE c.OrganizacaoId = @OrganizacaoId
                  AND c.Id = @Id
                LIMIT 1";

            return _repositorio.ConsultaDapper<RotaChatMensagemDTO>(sql, new { OrganizacaoId = organizacaoId, Id = id, OrigemSistema }).First();
        }

        private void MarcarComoLidasNoSistema(int organizacaoId, int rotaExecucaoId)
        {
            var sql = @"
                UPDATE rotaexecucaochat
                SET LidaNoSistema = 1, DataHoraLeituraSistema = COALESCE(DataHoraLeituraSistema, @Agora)
                WHERE OrganizacaoId = @OrganizacaoId
                  AND RotaExecucaoId = @RotaExecucaoId
                  AND Origem = @OrigemAplicativo
                  AND LidaNoSistema = 0";

            _repositorio.ExecutarComando(sql, new { OrganizacaoId = organizacaoId, RotaExecucaoId = rotaExecucaoId, OrigemAplicativo, Agora = DateTime.Now });
        }

        private void MarcarComoLidasNoAplicativo(int organizacaoId, int rotaExecucaoId)
        {
            var sql = @"
                UPDATE rotaexecucaochat
                SET LidaNoAplicativo = 1, DataHoraLeituraAplicativo = COALESCE(DataHoraLeituraAplicativo, @Agora)
                WHERE OrganizacaoId = @OrganizacaoId
                  AND RotaExecucaoId = @RotaExecucaoId
                  AND Origem = @OrigemSistema
                  AND LidaNoAplicativo = 0";

            _repositorio.ExecutarComando(sql, new { OrganizacaoId = organizacaoId, RotaExecucaoId = rotaExecucaoId, OrigemSistema, Agora = DateTime.Now });
        }

        private static string NormalizarMensagem(string mensagem)
        {
            var texto = (mensagem ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(texto))
                throw new ApplicationException("Informe uma mensagem para enviar.");

            if (texto.Length > 1000)
                throw new ApplicationException("A mensagem deve ter no máximo 1000 caracteres.");

            return texto;
        }
    }

    internal class RotaChatExecucaoResumoRowDTO
    {
        public int RotaExecucaoId { get; set; }
        public int OrganizacaoId { get; set; }
        public int RotaId { get; set; }
        public string RotaDescricao { get; set; } = string.Empty;
        public bool Finalizada { get; set; }
    }

    internal class RotaChatExecucaoAppRowDTO : RotaChatExecucaoResumoRowDTO
    {
        public string RemetenteNome { get; set; } = string.Empty;
    }
}
