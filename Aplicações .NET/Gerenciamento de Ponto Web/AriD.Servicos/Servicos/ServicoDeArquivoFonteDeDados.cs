using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeArquivoFonteDeDados : IServicoDeArquivoFonteDeDados
    {
		private readonly IRepositorio<EquipamentoDePonto> _repositorioEquipamentoDePonto;
		private readonly IRepositorio<RegistroDePonto> _repositorioRegistroDePonto;

        public ServicoDeArquivoFonteDeDados(
			IRepositorio<EquipamentoDePonto> repositorioEquipamentoDePonto, 
			IRepositorio<RegistroDePonto> repositorioRegistroDePonto)
        {
            _repositorioEquipamentoDePonto = repositorioEquipamentoDePonto;
            _repositorioRegistroDePonto = repositorioRegistroDePonto;
        }

        public int ImportarArquivoAFD(
			int equipamentoId,
            int ultimoNsrInformado,
            SessaoDTO sessaoDTO,
            Stream arquivoStream)
        {
			try
			{
                int qtdImportado = 0;
                var agora = DateTime.Now;
				var equipamento = _repositorioEquipamentoDePonto.Obtenha(equipamentoId);
                using var reader = new StreamReader(arquivoStream);

                while (!reader.EndOfStream)
                {
                    var linha = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(linha)) continue;

                    var tipoRegistro = linha.Substring(0, 1);

                    if (tipoRegistro == "3")
                    {
                        var nsrStr = linha.Substring(1, 9);
                        if (!int.TryParse(nsrStr, out int nsr)) continue;

                        if (nsr <= ultimoNsrInformado) continue;

                        var numeroRelogio = linha.Substring(10, 17);
                        var data = linha.Substring(27, 8);
                        var hora = linha.Substring(35, 4);
                        var pis = linha.Substring(39, 12);

                        if (DateTime.TryParseExact(data + hora, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out var dataHora))
                        {
                            _repositorioRegistroDePonto.Add(new RegistroDePonto
                            {
                                OrganizacaoId = sessaoDTO.OrganizacaoId,
                                UsuarioImportacaoId = sessaoDTO.UsuarioId,
                                DataImportacao = agora,
                                UsuarioEquipamentoId = pis,
                                DataHoraRegistro = dataHora,
                                DataHoraRecebimento = agora,
                                EquipamentoDePontoId = equipamentoId,
                                TipoRegistro = eTipoDeRegistroEquipamento.Biometria
                            });

                            qtdImportado++;

                            if (nsr > ultimoNsrInformado)
                                ultimoNsrInformado = nsr;
                        }
                    }
                }

                equipamento.UltimoNSRLido = ultimoNsrInformado;
                _repositorioEquipamentoDePonto.Atualizar(equipamento);

                _repositorioRegistroDePonto.Commit();

                return qtdImportado;
            }
			catch (Exception)
			{
				throw;
			}
        }
    }
}