using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Extensao;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using System.Globalization;
using System.Text;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeExportacaoFolhaPagamento : IServicoDeExportacaoFolhaPagamento
    {
        private readonly IRepositorio<LayoutExportacaoFolhaPagamento> _repoLayout;
        private readonly IRepositorio<LayoutExportacaoFolhaPagamentoCampo> _repoCampos;
        private readonly IRepositorio<MapeamentoEventoFolhaPagamento> _repoMap;
        private readonly IRepositorio<LotacaoUnidadeOrganizacional> _repoLotacao;
        private readonly IRepositorio<VinculoDeTrabalho> _repoVinculo;
        private readonly IRepositorio<UnidadeOrganizacional> _repoUnidade;
        private readonly IRepositorio<PontoDoDia> _repoPontoDoDia;
        private readonly IServicoDeFolhaDePonto _servicoFolha;
        private readonly IRepositorio<BonusCalculado> _repoBonus;

        public ServicoDeExportacaoFolhaPagamento(
            IRepositorio<LayoutExportacaoFolhaPagamento> repoLayout,
            IRepositorio<LayoutExportacaoFolhaPagamentoCampo> repoCampos,
            IRepositorio<MapeamentoEventoFolhaPagamento> repoMap,
            IRepositorio<LotacaoUnidadeOrganizacional> repoLotacao,
            IRepositorio<VinculoDeTrabalho> repoVinculo,
            IRepositorio<UnidadeOrganizacional> repoUnidade,
            IServicoDeFolhaDePonto servicoFolha,
            IRepositorio<PontoDoDia> repoPontoDoDia,
            IRepositorio<BonusCalculado> repoBonus)
        {
            _repoLayout = repoLayout;
            _repoCampos = repoCampos;
            _repoMap = repoMap;
            _repoLotacao = repoLotacao;
            _repoVinculo = repoVinculo;
            _repoUnidade = repoUnidade;
            _servicoFolha = servicoFolha;
            _repoPontoDoDia = repoPontoDoDia;
            _repoBonus = repoBonus;
        }

        public List<CodigoDescricaoDTO> ObtenhaLayouts(int organizacaoId)
        {
            GarantaLayoutPadrao(organizacaoId);

            return _repoLayout.ObtenhaLista(l => l.OrganizacaoId == organizacaoId && l.Ativo)
                .OrderBy(l => l.Nome)
                .Select(l => new CodigoDescricaoDTO(l.Id, l.Nome))
                .ToList();
        }

        public LayoutExportacaoFolhaPagamento ObtenhaLayoutCompleto(int organizacaoId, int layoutId)
        {
            GarantaLayoutPadrao(organizacaoId);

            var layout = _repoLayout.Obtenha(l => l.OrganizacaoId == organizacaoId && l.Id == layoutId);
            if (layout == null) throw new ApplicationException("Layout não encontrado.");

            layout.Campos = _repoCampos.ObtenhaLista(c => c.OrganizacaoId == organizacaoId && c.LayoutId == layout.Id && c.Ativo)
                .OrderBy(c => c.Ordem)
                .ToList();

            return layout;
        }

        public int SalvarLayout(int organizacaoId, LayoutExportacaoFolhaPagamento layout, List<LayoutExportacaoFolhaPagamentoCampo> campos)
        {
            if (layout == null) throw new ApplicationException("Layout inválido.");
            if (string.IsNullOrWhiteSpace(layout.Nome)) throw new ApplicationException("Informe o nome do layout.");
            if (string.IsNullOrWhiteSpace(layout.Delimitador)) layout.Delimitador = ";";
            if (layout.Delimitador.Length > 5) throw new ApplicationException("Delimitador inválido.");
            if (campos == null || campos.Count == 0) throw new ApplicationException("Informe ao menos 1 campo no layout.");

            layout.OrganizacaoId = organizacaoId;

            if (layout.Id == 0)
            {
                _repoLayout.Add(layout);
                _repoLayout.Commit();
            }
            else
            {
                _repoLayout.Atualizar(layout);
                _repoLayout.Commit();

                var antigos = _repoCampos.ObtenhaLista(c => c.OrganizacaoId == organizacaoId && c.LayoutId == layout.Id);
                foreach (var a in antigos) _repoCampos.Remover(a);
                _repoCampos.Commit();
            }

            int ordem = 1;
            foreach (var c in campos.OrderBy(x => x.Ordem))
            {
                c.Id = 0;
                c.OrganizacaoId = organizacaoId;
                c.LayoutId = layout.Id;
                c.Ordem = ordem++;
                c.Ativo = true;
                _repoCampos.Add(c);
            }
            _repoCampos.Commit();

            return layout.Id;
        }

        public List<MapeamentoEventoFolhaPagamento> ObtenhaMapeamentos(int organizacaoId)
        {
            return _repoMap.ObtenhaLista(m => m.OrganizacaoId == organizacaoId && m.Ativo)
                .OrderBy(m => m.TipoEvento)
                .ThenBy(m => m.Percentual)
                .ToList();
        }

        public void SalvarMapeamentos(int organizacaoId, List<MapeamentoEventoFolhaPagamento> mapeamentos)
        {
            var antigos = _repoMap.ObtenhaLista(m => m.OrganizacaoId == organizacaoId);
            foreach (var a in antigos) _repoMap.Remover(a);
            _repoMap.Commit();

            foreach (var m in mapeamentos.Where(x => x != null))
            {
                if (string.IsNullOrWhiteSpace(m.Codigo)) continue;

                m.Id = 0;
                m.OrganizacaoId = organizacaoId;
                m.Ativo = true;
                _repoMap.Add(m);
            }

            _repoMap.Commit();
        }

        public ResultadoExportacaoFolhaPagamentoDTO GerarDadosPacote(
            int organizacaoId,
            int unidadeId,
            MesAno mesAno,
            int layoutId,
            eFormatoArquivoExportacao formatoArquivo,
            bool agruparPorMatricula,
            bool somenteServidoresHabilitados)
        {
            var resultado = new ResultadoExportacaoFolhaPagamentoDTO();

            var unidade = _repoUnidade.Obtenha(u => u.OrganizacaoId == organizacaoId && u.Id == unidadeId);
            if (unidade == null) throw new ApplicationException("Unidade inválida.");

            var layout = ObtenhaLayoutCompleto(organizacaoId, layoutId);
            var campos = layout.Campos.Where(c => c.Ativo).OrderBy(c => c.Ordem).ToList();
            if (campos.Count == 0) throw new ApplicationException("O layout não possui campos.");

            var delimiter = layout.Delimitador?.Length > 0 ? layout.Delimitador[0] : ';';
            var diasEsperados = (mesAno.Fim.Date - mesAno.Inicio.Date).Days + 1;

            // 1) vínculos do período/unidade
            var sqlVinculos = @"
                select distinct 
                    v.Id as VinculoId,
                    v.Matricula as Matricula,
                    p.Nome as NomeServidor
                from LotacaoUnidadeOrganizacional l
                inner join VinculoDeTrabalho v on v.Id = l.VinculoDeTrabalhoId
                inner join Servidor s on s.Id = v.ServidorId
                inner join pessoa p on p.Id = s.PessoaId
                where l.OrganizacaoId = @ORG
                  and l.UnidadeOrganizacionalId = @UNID
                  and l.Entrada <= @FIM
                  and (l.Saida is null or l.Saida >= @INI)
                  and v.Inicio <= @FIM
                  and (v.Fim is null or v.Fim >= @INI)
                  and v.Situacao = 0
                  and (@SOHAB = 0 or s.HabilitaExportacaoParaFolhaDePagamento = 1)
                ";

            var vinculos = _repoLotacao.ConsultaDapper<VinculoExportRow>(sqlVinculos, new
            {
                @ORG = organizacaoId,
                @UNID = unidadeId,
                @INI = mesAno.Inicio.Date,
                @FIM = mesAno.Fim.Date,
                @SOHAB = somenteServidoresHabilitados ? 1 : 0
            }) ?? new List<VinculoExportRow>();

            resultado.TotalVinculosConsiderados = vinculos.Count;

            var vinculoIds = vinculos.Select(v => v.VinculoId).Distinct().ToList();
            if (vinculoIds.Count == 0)
            {
                resultado.NomeArquivoExportacao = NomeArquivo(unidade.Nome, mesAno, formatoArquivo);
                resultado.BytesExportacao = Encoding.UTF8.GetBytes(layout.UsarCabecalho
                    ? string.Join(delimiter, campos.Select(c => c.NomeColuna ?? c.Campo.DescricaoDoEnumerador())) + "\n"
                    : "");
                return resultado;
            }

            // 2) (1.3) status agregado por vínculo => decide exportáveis/ignorados sem carregar tudo
            var sqlStatus = @"
                select VinculoDeTrabalhoId,
                       count(distinct Data) as QtdDias,
                       sum(case when PontoFechado = 0 then 1 else 0 end) as QtdAbertos
                from PontoDoDia
                where OrganizacaoId = @ORG
                  and VinculoDeTrabalhoId in @IDS
                  and Data between @INI and @FIM
                group by VinculoDeTrabalhoId;
                ";
            var status = _repoPontoDoDia.ConsultaDapper<StatusFolhaVinculoRow>(sqlStatus, new
            {
                @ORG = organizacaoId,
                @IDS = vinculoIds,
                @INI = mesAno.Inicio.Date,
                @FIM = mesAno.Fim.Date
            }) ?? new List<StatusFolhaVinculoRow>();

            var statusDict = status.ToDictionary(x => x.VinculoDeTrabalhoId);

            var exportaveis = new List<VinculoExportRow>();

            foreach (var v in vinculos)
            {
                if (!statusDict.TryGetValue(v.VinculoId, out var st))
                {
                    resultado.Ignorados.Add(new ExportacaoIgnoradoDTO
                    {
                        VinculoId = v.VinculoId,
                        Matricula = v.Matricula,
                        NomeServidor = v.NomeServidor,
                        Motivo = "Sem apuração salva no período."
                    });
                    continue;
                }

                if (st.QtdDias != diasEsperados)
                {
                    resultado.Ignorados.Add(new ExportacaoIgnoradoDTO
                    {
                        VinculoId = v.VinculoId,
                        Matricula = v.Matricula,
                        NomeServidor = v.NomeServidor,
                        Motivo = $"Período incompleto: {st.QtdDias}/{diasEsperados} dia(s)."
                    });
                    continue;
                }

                if (st.QtdAbertos > 0)
                {
                    resultado.Ignorados.Add(new ExportacaoIgnoradoDTO
                    {
                        VinculoId = v.VinculoId,
                        Matricula = v.Matricula,
                        NomeServidor = v.NomeServidor,
                        Motivo = $"{st.QtdAbertos} dia(s) em aberto."
                    });
                    continue;
                }

                exportaveis.Add(v);
            }

            resultado.TotalExportaveis = exportaveis.Count;
            resultado.TotalIgnorados = resultado.Ignorados.Count;

            var idsExportaveis = exportaveis.Select(x => x.VinculoId).ToList();

            // 3) carrega PontoDoDia só dos exportáveis
            var sqlPontos = @"
                select VinculoDeTrabalhoId, Data, HorasPositivas, HorasNegativas, Abono, BancoDeHorasCredito, BancoDeHorasDebito
                from PontoDoDia
                where OrganizacaoId = @ORG
                  and VinculoDeTrabalhoId in @IDS
                  and Data between @INI and @FIM;
                ";
            var pontos = _repoPontoDoDia.ConsultaDapper<PontoDiaRow>(sqlPontos, new
            {
                @ORG = organizacaoId,
                @IDS = idsExportaveis,
                @INI = mesAno.Inicio.Date,
                @FIM = mesAno.Fim.Date
            }) ?? new List<PontoDiaRow>();

            // 4) HE evoluída (se existir) – aprovado por percentual; senão fallback HorasPositivas
            List<HeAprovadaRow> heAprovadas;
            try
            {
                var sqlHE = @"
                    select p.VinculoDeTrabalhoId as VinculoDeTrabalhoId,
                           p.Data as Data,
                           he.Percentual as Percentual,
                           he.MinutosAprovados as MinutosAprovados
                    from PontoDoDiaHoraExtra he
                    inner join PontoDoDia p on p.Id = he.PontoDoDiaId
                    where p.OrganizacaoId = @ORG
                      and p.VinculoDeTrabalhoId in @IDS
                      and p.Data between @INI and @FIM
                      and he.Status = 2;
                    ";
                heAprovadas = _repoPontoDoDia.ConsultaDapper<HeAprovadaRow>(sqlHE, new
                {
                    @ORG = organizacaoId,
                    @IDS = idsExportaveis,
                    @INI = mesAno.Inicio.Date,
                    @FIM = mesAno.Fim.Date
                }) ?? new List<HeAprovadaRow>();
            }
            catch
            {
                heAprovadas = new List<HeAprovadaRow>();
            }

            var strMes = mesAno.ToString();
            var sqlBonus = @"
                select VinculoDeTrabalhoId, ValorTotal
                from BonusCalculado
                where OrganizacaoId = @ORG
                  and VinculoDeTrabalhoId in @IDS
                  and MesReferencia = @MES;
            ";
            var bonusCalculados = _repoPontoDoDia.ConsultaDapper<BonusRow>(sqlBonus, new
            {
                @ORG = organizacaoId,
                @IDS = idsExportaveis,
                @MES = strMes
            }) ?? new List<BonusRow>();

            // 5) códigos mapeados
            var mapeamentos = ObtenhaMapeamentos(organizacaoId).Where(m => m.Ativo).ToList();
            var mapDict = mapeamentos.ToDictionary(m => (m.TipoEvento, m.Percentual), m => m.Codigo);

            string CodigoOuVazio(eTipoEventoFolhaPagamento tipo, decimal? perc)
            {
                if (perc.HasValue && mapDict.TryGetValue((tipo, perc), out var c1)) return c1;
                if (mapDict.TryGetValue((tipo, null), out var c2)) return c2;
                return ""; // não exporta esse evento
            }

            int Min(TimeSpan? t) => (int)Math.Round((t ?? TimeSpan.Zero).TotalMinutes);

            // 6) gerar arquivo + resumo por colaborador
            var sb = new StringBuilder();
            if (layout.UsarCabecalho)
                sb.AppendLine(string.Join(delimiter, campos.Select(c => EscapeCSV(c.NomeColuna ?? c.Campo.DescricaoDoEnumerador(), delimiter))));

            int exportadosComEventos = 0;

            foreach (var v in exportaveis)
            {
                var pontosV = pontos.Where(p => p.VinculoDeTrabalhoId == v.VinculoId).ToList();

                // resumo por código (minutos)
                var resumoCodigo = new Dictionary<string, int>();
                var semCodigo = new HashSet<string>();

                // eventos mensais (mais comum). Se você quiser diário, mantém o switch agruparPorMatricula
                // Aqui vou manter apenas mensal por simplicidade (como você pediu "mais comum")
                var neg = pontosV.Sum(x => Min(x.HorasNegativas));
                var abono = pontosV.Sum(x => Min(x.Abono));
                var bhc = pontosV.Sum(x => Min(x.BancoDeHorasCredito));
                var bhd = pontosV.Sum(x => Min(x.BancoDeHorasDebito));

                // HE evoluída
                var heV = heAprovadas.Where(x => x.VinculoDeTrabalhoId == v.VinculoId).ToList();

                var eventos = new List<(eTipoEventoFolhaPagamento Tipo, decimal? Perc, int Minutos)>();

                if (heV.Count > 0)
                {
                    foreach (var g in heV.GroupBy(x => x.Percentual))
                    {
                        var mins = g.Sum(x => x.MinutosAprovados);
                        if (mins > 0) eventos.Add((eTipoEventoFolhaPagamento.HoraExtra, g.Key, mins));
                    }
                }
                else
                {
                    var heTotal = pontosV.Sum(x => Min(x.HorasPositivas));
                    if (heTotal > 0) eventos.Add((eTipoEventoFolhaPagamento.HoraExtra, null, heTotal));
                }

                if (neg > 0) eventos.Add((eTipoEventoFolhaPagamento.HorasNegativas, null, neg));
                if (abono > 0) eventos.Add((eTipoEventoFolhaPagamento.Abono, null, abono));
                if (bhc > 0) eventos.Add((eTipoEventoFolhaPagamento.BancoHorasCredito, null, bhc));
                if (bhd > 0) eventos.Add((eTipoEventoFolhaPagamento.BancoHorasDebito, null, bhd));

                var bonusV = bonusCalculados.Where(x => x.VinculoDeTrabalhoId == v.VinculoId).Sum(x => x.ValorTotal);
                if (bonusV > 0) eventos.Add((eTipoEventoFolhaPagamento.Bonus, null, (int)(bonusV * 100)));

                foreach (var ev in eventos)
                {
                    var codigo = CodigoOuVazio(ev.Tipo, ev.Perc);

                    if (string.IsNullOrWhiteSpace(codigo))
                    {
                        semCodigo.Add(ev.Perc.HasValue
                            ? $"{ev.Tipo.DescricaoDoEnumerador()} ({ev.Perc}%)"
                            : ev.Tipo.DescricaoDoEnumerador());
                        continue;
                    }

                    // linha exportação
                    var line = GerarLinhaDelimitada(campos, v, unidade.Nome, mesAno, ev.Tipo, ev.Perc, codigo, ev.Minutos, layout, delimiter);
                    sb.AppendLine(line);

                    // resumo
                    resumoCodigo[codigo] = resumoCodigo.TryGetValue(codigo, out var cur) ? cur + ev.Minutos : ev.Minutos;
                }

                var resumoDto = new ExportacaoResumoColaboradorDTO
                {
                    VinculoId = v.VinculoId,
                    Matricula = v.Matricula,
                    NomeServidor = v.NomeServidor,
                    ResumoPorCodigo = resumoCodigo.OrderBy(k => k.Key)
                                                  .Select(k => new ResumoCodigoDTO { Codigo = k.Key, Minutos = k.Value })
                                                  .ToList(),
                    EventosSemCodigo = semCodigo.OrderBy(x => x).ToList()
                };

                if (resumoDto.ResumoPorCodigo.Count > 0) exportadosComEventos++;

                resultado.ExportadosResumo.Add(resumoDto);
            }

            resultado.TotalExportadosComEventos = exportadosComEventos;

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            if (layout.UsarBOM)
            {
                var bom = Encoding.UTF8.GetPreamble();
                var comBom = new byte[bom.Length + bytes.Length];
                Buffer.BlockCopy(bom, 0, comBom, 0, bom.Length);
                Buffer.BlockCopy(bytes, 0, comBom, bom.Length, bytes.Length);
                bytes = comBom;
            }

            resultado.NomeArquivoExportacao = NomeArquivo(unidade.Nome, mesAno, formatoArquivo);
            resultado.BytesExportacao = bytes;

            return resultado;
        }

        // helpers locais do serviço
        private string NomeArquivo(string unidade, MesAno mesAno, eFormatoArquivoExportacao formato)
        {
            var ext = formato == eFormatoArquivoExportacao.TXT ? "txt" : "csv";
            return $"Exportacao_Folha_{unidade}_{mesAno.ToString().Replace("/", "-")}.{ext}";
        }

        private string GerarLinhaDelimitada(
            List<LayoutExportacaoFolhaPagamentoCampo> campos,
            VinculoExportRow v,
            string unidadeNome,
            MesAno mesAno,
            eTipoEventoFolhaPagamento tipoEvento,
            decimal? percentual,
            string codigoEvento,
            int minutos,
            LayoutExportacaoFolhaPagamento layout,
            char delimiter)
        {
            string valorCampo(eCampoExportacaoFolhaPagamento campo)
            {
                return campo switch
                {
                    eCampoExportacaoFolhaPagamento.Matricula => v.Matricula ?? "",
                    eCampoExportacaoFolhaPagamento.NomeServidor => v.NomeServidor ?? "",
                    eCampoExportacaoFolhaPagamento.Competencia => mesAno.ToString(),
                    eCampoExportacaoFolhaPagamento.CodigoEvento => codigoEvento,
                    eCampoExportacaoFolhaPagamento.Quantidade => FormatarQuantidade(minutos, layout, tipoEvento),
                    eCampoExportacaoFolhaPagamento.Unidade => unidadeNome ?? "",
                    eCampoExportacaoFolhaPagamento.TipoEvento => tipoEvento.DescricaoDoEnumerador(),
                    eCampoExportacaoFolhaPagamento.Percentual => percentual?.ToString(CultureInfo.InvariantCulture) ?? "",
                    _ => ""
                };
            }

            var linha = campos.Select(c =>
            {
                if (!string.IsNullOrWhiteSpace(c.ValorFixo))
                    return EscapeCSV(c.ValorFixo, delimiter);

                return EscapeCSV(valorCampo(c.Campo), delimiter);
            });

            return string.Join(delimiter, linha);
        }

        private static string FormatarQuantidade(int minutos, LayoutExportacaoFolhaPagamento layout, eTipoEventoFolhaPagamento tipoEvento)
        {
            if (tipoEvento == eTipoEventoFolhaPagamento.Bonus)
            {
                decimal valorDec = minutos / 100m;
                var fmtMoeda = "0." + new string('0', Math.Max(0, layout.CasasDecimais));
                return valorDec.ToString(fmtMoeda, CultureInfo.InvariantCulture).Replace(".", ",");
            }

            switch (layout.FormatoQuantidade)
            {
                case eFormatoQuantidadeExportacao.Minutos:
                    return minutos.ToString();

                case eFormatoQuantidadeExportacao.HorasDecimais:
                    var horas = minutos / 60.0m;
                    var fmt = "0." + new string('0', Math.Max(0, layout.CasasDecimais));
                    return horas.ToString(fmt, CultureInfo.InvariantCulture);

                default:
                    var h = minutos / 60;
                    var m = minutos % 60;
                    return $"{h:00}:{m:00}";
            }
        }

        private static string EscapeCSV(string value, char delimiter)
        {
            if (value == null) return "";
            var precisa = value.Contains(delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            if (!precisa) return value;
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        // classes locais (mesmas do seu serviço)
        private sealed class VinculoExportRow
        {
            public int VinculoId { get; set; }
            public string Matricula { get; set; } = "";
            public string NomeServidor { get; set; } = "";
        }

        private sealed class StatusFolhaVinculoRow
        {
            public int VinculoDeTrabalhoId { get; set; }
            public int QtdDias { get; set; }
            public int QtdAbertos { get; set; }
        }

        private sealed class PontoDiaRow
        {
            public int VinculoDeTrabalhoId { get; set; }
            public DateTime Data { get; set; }
            public TimeSpan? HorasPositivas { get; set; }
            public TimeSpan? HorasNegativas { get; set; }
            public TimeSpan? Abono { get; set; }
            public TimeSpan? BancoDeHorasCredito { get; set; }
            public TimeSpan? BancoDeHorasDebito { get; set; }
        }

        private sealed class HeAprovadaRow
        {
            public int VinculoDeTrabalhoId { get; set; }
            public DateTime Data { get; set; }
            public decimal Percentual { get; set; }
            public int MinutosAprovados { get; set; }
        }

        private sealed class BonusRow
        {
            public int VinculoDeTrabalhoId { get; set; }
            public decimal ValorTotal { get; set; }
        }

        private void GarantaLayoutPadrao(int organizacaoId)
        {
            var existe = _repoLayout.Obtenha(l => l.OrganizacaoId == organizacaoId && l.Ativo);
            if (existe != null) return;

            var layout = new LayoutExportacaoFolhaPagamento
            {
                OrganizacaoId = organizacaoId,
                Nome = "Padrão (AriD)",
                Delimitador = ";",
                UsarCabecalho = true,
                FormatoQuantidade = eFormatoQuantidadeExportacao.HHMM,
                CasasDecimais = 2,
                UsarBOM = true,
                Ativo = true
            };

            _repoLayout.Add(layout);
            _repoLayout.Commit();

            var campos = new List<LayoutExportacaoFolhaPagamentoCampo>
            {
                new() { OrganizacaoId = organizacaoId, LayoutId = layout.Id, Ordem = 1, Campo = eCampoExportacaoFolhaPagamento.Matricula, NomeColuna="MATRICULA", Ativo = true },
                new() { OrganizacaoId = organizacaoId, LayoutId = layout.Id, Ordem = 2, Campo = eCampoExportacaoFolhaPagamento.CodigoEvento, NomeColuna="COD_EVENTO", Ativo = true },
                new() { OrganizacaoId = organizacaoId, LayoutId = layout.Id, Ordem = 3, Campo = eCampoExportacaoFolhaPagamento.Quantidade, NomeColuna="QTD", Ativo = true },
                new() { OrganizacaoId = organizacaoId, LayoutId = layout.Id, Ordem = 4, Campo = eCampoExportacaoFolhaPagamento.Competencia, NomeColuna="COMPETENCIA", Ativo = true }
            };

            foreach (var c in campos) _repoCampos.Add(c);
            _repoCampos.Commit();
        }
    }
}
