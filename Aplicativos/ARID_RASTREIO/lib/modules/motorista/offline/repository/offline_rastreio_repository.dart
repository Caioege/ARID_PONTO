import 'dart:convert';

import 'package:arid_rastreio/core/storage/offline_database.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/veiculo_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/checklist/store/checklist_item_store.dart';
import 'package:arid_rastreio/modules/motorista/offline/dto/pacote_offline_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/parada_rota_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/rota_execucao_dto.dart';
import 'package:sqflite/sqflite.dart';

class OfflineRastreioRepository {
  static const _configModoOffline = 'modo_offline_habilitado';
  static const _configUltimoDownload = 'ultimo_download_pacote';
  static const _configValidoAte = 'pacote_valido_ate';
  static const _configIdentificadorDispositivo = 'identificador_dispositivo';

  final OfflineDatabase _database;

  OfflineRastreioRepository(this._database);

  Future<void> salvarPacote(PacoteOfflineDTO pacote) async {
    final db = await _database.database;
    final baixadoEm = pacote.dataHoraGeracao.toIso8601String();
    final validoAte = pacote.validoAte.toIso8601String();

    await db.transaction((txn) async {
      await _limparCache(txn);

      for (final rota in pacote.rotas) {
        await txn.insert('offline_route_cache', {
          'rota_id': rota.id,
          'codigo': rota.codigo,
          'nome': rota.nome,
          'descricao': rota.descricao,
          'permite_pausa': rota.permitePausa ? 1 : 0,
          'quantidade_pausas': rota.quantidadePausas,
          'unidade_origem_id': rota.unidadeOrigemId,
          'unidade_destino_id': rota.unidadeDestinoId,
          'nome_unidade_origem': rota.nomeUnidadeOrigem,
          'nome_unidade_destino': rota.nomeUnidadeDestino,
          'origem_latitude_rota': rota.origemLatitudeRota,
          'origem_longitude_rota': rota.origemLongitudeRota,
          'destino_latitude_rota': rota.destinoLatitudeRota,
          'destino_longitude_rota': rota.destinoLongitudeRota,
          'baixado_em': baixadoEm,
          'valido_ate': validoAte,
        });

        for (final veiculo in rota.veiculos) {
          await txn.insert('offline_route_vehicle_cache', {
            'rota_id': rota.id,
            'veiculo_id': veiculo.id,
            'nome': veiculo.nome,
            'placa': veiculo.placa,
            'modelo': veiculo.modelo,
            'cor': veiculo.cor,
          });

          for (final item in veiculo.checklist) {
            await txn.insert('offline_checklist_cache', {
              'rota_id': rota.id,
              'veiculo_id': veiculo.id,
              'item_id': item.id,
              'descricao': item.descricao,
            });
          }
        }

        for (var i = 0; i < rota.paradas.length; i++) {
          final parada = rota.paradas[i];
          await txn.insert('offline_route_stop_cache', {
            'rota_id': rota.id,
            'parada_id': parada.id,
            'endereco': parada.endereco,
            'latitude': parada.latitude,
            'longitude': parada.longitude,
            'link': parada.link,
            'observacao_cadastro': parada.observacaoCadastro,
            'ordem': i,
          });
        }
      }

      await _salvarConfig(txn, _configModoOffline, 'true');
      await _salvarConfig(txn, _configUltimoDownload, baixadoEm);
      await _salvarConfig(txn, _configValidoAte, validoAte);
    });
  }

  Future<void> habilitarModoOffline(bool habilitado) async {
    final db = await _database.database;
    await db.transaction((txn) async {
      await _salvarConfig(txn, _configModoOffline, habilitado.toString());
    });
  }

  Future<bool> modoOfflineHabilitado() async {
    final valor = await _obterConfig(_configModoOffline);
    return valor?.toLowerCase() == 'true';
  }

  Future<bool> pacoteValido() async {
    final resumo = await obterResumoPacote();
    if (!resumo.modoOfflineHabilitado) return false;
    if (resumo.quantidadeRotas == 0 || resumo.validoAte == null) return false;

    return !DateTime.now().isAfter(resumo.validoAte!);
  }

  Future<List<RotaChecklistDTO>> listarRotasCache() async {
    if (!await pacoteValido()) return [];

    final db = await _database.database;
    final rows = await db.query(
      'offline_route_cache',
      orderBy: 'descricao ASC, rota_id ASC',
    );

    return rows
        .map(
          (row) => RotaChecklistDTO(
            id: row['rota_id'] as int,
            codigo: row['codigo'] as String,
            nome: row['nome'] as String,
            descricao: row['descricao'] as String,
          ),
        )
        .toList();
  }

  Future<List<VeiculoChecklistDTO>> listarVeiculosCache(int rotaId) async {
    if (!await pacoteValido()) return [];

    final db = await _database.database;
    final veiculos = await db.query(
      'offline_route_vehicle_cache',
      where: 'rota_id = ?',
      whereArgs: [rotaId],
      orderBy: 'nome ASC, veiculo_id ASC',
    );

    final retorno = <VeiculoChecklistDTO>[];
    for (final veiculo in veiculos) {
      final veiculoId = veiculo['veiculo_id'] as int;
      final itens = await db.query(
        'offline_checklist_cache',
        where: 'rota_id = ? AND veiculo_id = ?',
        whereArgs: [rotaId, veiculoId],
        orderBy: 'item_id ASC',
      );

      retorno.add(
        VeiculoChecklistDTO(
          id: veiculoId,
          rotaId: rotaId,
          nome: veiculo['nome'] as String,
          placa: veiculo['placa'] as String,
          modelo: veiculo['modelo'] as String,
          cor: veiculo['cor'] as String,
          checklist: itens
              .map(
                (item) => ChecklistItemStore(
                  id: item['item_id'] as int,
                  descricao: item['descricao'] as String,
                ),
              )
              .toList(),
        ),
      );
    }

    return retorno;
  }

  Future<int> salvarChecklistLocal({
    required int rotaId,
    required int veiculoId,
    required List<int> itensMarcados,
  }) async {
    if (!await pacoteValido()) {
      throw Exception('Pacote offline vencido ou indisponível.');
    }

    final localChecklistId = -DateTime.now().millisecondsSinceEpoch;
    await _enfileirar(
      localExecucaoId: 'checklist_${localChecklistId.abs()}',
      tipoRegistro: 'checklist',
      clientEventId: 'checklist_${localChecklistId.abs()}',
      payload: {
        'rotaId': rotaId,
        'veiculoId': veiculoId,
        'itens': itensMarcados,
        'localChecklistId': localChecklistId,
      },
    );

    return localChecklistId;
  }

  Future<RotaExecucaoDTO> iniciarExecucaoLocal({
    required int rotaId,
    required int veiculoId,
    int? checklistExecucaoId,
    double? latitudeInicio,
    double? longitudeInicio,
    bool gpsSimulado = false,
  }) async {
    if (!await pacoteValido()) {
      throw Exception('Pacote offline vencido ou indisponível.');
    }

    final db = await _database.database;
    final rotaRows = await db.query(
      'offline_route_cache',
      where: 'rota_id = ?',
      whereArgs: [rotaId],
      limit: 1,
    );
    if (rotaRows.isEmpty) {
      throw Exception('Rota não encontrada no pacote offline.');
    }

    final rota = rotaRows.first;
    final agora = DateTime.now();
    final localExecucaoId = 'exec_${agora.microsecondsSinceEpoch}';
    final execucaoIdLocal = -agora.millisecondsSinceEpoch;

    await db.insert('offline_execucao', {
      'local_execucao_id': localExecucaoId,
      'rota_id': rotaId,
      'veiculo_id': veiculoId,
      'checklist_execucao_id': checklistExecucaoId,
      'status': 'em_andamento',
      'data_hora_inicio_local': agora.toIso8601String(),
      'latitude_inicio': latitudeInicio,
      'longitude_inicio': longitudeInicio,
      'gps_simulado_inicio': gpsSimulado ? 1 : 0,
      'criado_em': agora.toIso8601String(),
      'atualizado_em': agora.toIso8601String(),
    });

    await _enfileirar(
      localExecucaoId: localExecucaoId,
      tipoRegistro: 'inicio_rota',
      clientEventId: '${localExecucaoId}_inicio',
      payload: {
        'localExecucaoId': localExecucaoId,
        'rotaId': rotaId,
        'veiculoId': veiculoId,
        'checklistExecucaoId': checklistExecucaoId,
        'dataHoraInicioLocal': agora.toIso8601String(),
        'latitudeInicio': latitudeInicio,
        'longitudeInicio': longitudeInicio,
        'gpsSimuladoInicio': gpsSimulado,
      },
    );

    return RotaExecucaoDTO(
      id: execucaoIdLocal,
      rotaId: rotaId,
      descricao: rota['descricao'] as String,
      emAndamento: true,
      permitePausa: (rota['permite_pausa'] as int) == 1,
      quantidadePausas: rota['quantidade_pausas'] as int,
      quantidadePausasRealizadas: 0,
      estaPausada: false,
      nomeUnidadeOrigem: rota['nome_unidade_origem'] as String?,
      nomeUnidadeDestino: rota['nome_unidade_destino'] as String?,
      origemLatitudeRota: rota['origem_latitude_rota'] as double?,
      origemLongitudeRota: rota['origem_longitude_rota'] as double?,
      destinoLatitudeRota: rota['destino_latitude_rota'] as double?,
      destinoLongitudeRota: rota['destino_longitude_rota'] as double?,
      veiculoId: veiculoId,
      checklistExecucaoId: checklistExecucaoId,
      execucaoOffline: true,
      localExecucaoId: localExecucaoId,
      paradas: await listarParadasCache(rotaId),
    );
  }

  Future<List<ParadaRotaDTO>> listarParadasCache(int rotaId) async {
    final db = await _database.database;
    final rows = await db.query(
      'offline_route_stop_cache',
      where: 'rota_id = ?',
      whereArgs: [rotaId],
      orderBy: 'ordem ASC, parada_id ASC',
    );

    return rows
        .map(
          (row) => ParadaRotaDTO(
            id: row['parada_id'] as int,
            endereco: row['endereco'] as String,
            latitude: row['latitude'] as double?,
            longitude: row['longitude'] as double?,
            link: row['link'] as String?,
            observacaoCadastro: row['observacao_cadastro'] as String?,
          ),
        )
        .toList();
  }

  Future<void> registrarEventoParadaLocal({
    required String localExecucaoId,
    required int paradaId,
    required bool? entregue,
    required String? observacao,
    double? latitude,
    double? longitude,
  }) async {
    final clientEventId =
        '${localExecucaoId}_parada_${DateTime.now().microsecondsSinceEpoch}';
    final dataHora = DateTime.now();
    final db = await _database.database;

    await db.insert('offline_evento', {
      'client_event_id': clientEventId,
      'local_execucao_id': localExecucaoId,
      'tipo_evento': _tipoEventoPorParada(paradaId),
      'parada_rota_id': paradaId > 0 ? paradaId : null,
      'entregue': entregue == null ? null : (entregue ? 1 : 0),
      'observacao': observacao,
      'latitude': latitude,
      'longitude': longitude,
      'data_hora_evento': dataHora.toIso8601String(),
    });

    await _enfileirar(
      localExecucaoId: localExecucaoId,
      tipoRegistro: 'evento',
      clientEventId: clientEventId,
      payload: {
        'clientEventId': clientEventId,
        'localExecucaoId': localExecucaoId,
        'tipoEvento': _tipoEventoPorParada(paradaId),
        'paradaRotaId': paradaId > 0 ? paradaId : null,
        'entregue': entregue,
        'observacao': observacao,
        'latitude': latitude,
        'longitude': longitude,
        'dataHoraEvento': dataHora.toIso8601String(),
      },
    );
  }

  Future<void> encerrarExecucaoLocal({
    required String localExecucaoId,
    required String? observacao,
  }) async {
    final agora = DateTime.now();
    final db = await _database.database;
    await db.update(
      'offline_execucao',
      {
        'status': 'finalizada',
        'data_hora_fim_local': agora.toIso8601String(),
        'observacao_fim': observacao,
        'atualizado_em': agora.toIso8601String(),
      },
      where: 'local_execucao_id = ?',
      whereArgs: [localExecucaoId],
    );

    await _enfileirar(
      localExecucaoId: localExecucaoId,
      tipoRegistro: 'fim_rota',
      clientEventId: '${localExecucaoId}_fim',
      payload: {
        'localExecucaoId': localExecucaoId,
        'observacaoFim': observacao,
        'dataHoraFimLocal': agora.toIso8601String(),
      },
    );
  }

  Future<void> iniciarPausaLocal({
    required String localExecucaoId,
    required String motivo,
    double? latitude,
    double? longitude,
  }) async {
    final agora = DateTime.now();
    final clientEventId =
        '${localExecucaoId}_pausa_${agora.microsecondsSinceEpoch}';
    final db = await _database.database;

    await db.insert('offline_pausa', {
      'client_event_id': clientEventId,
      'local_execucao_id': localExecucaoId,
      'motivo': motivo,
      'data_hora_inicio': agora.toIso8601String(),
      'latitude_inicio': latitude,
      'longitude_inicio': longitude,
    });

    await db.update(
      'offline_execucao',
      {'status': 'pausada', 'atualizado_em': agora.toIso8601String()},
      where: 'local_execucao_id = ?',
      whereArgs: [localExecucaoId],
    );

    await _enfileirar(
      localExecucaoId: localExecucaoId,
      tipoRegistro: 'pausa_inicio',
      clientEventId: clientEventId,
      payload: {
        'clientEventId': clientEventId,
        'localExecucaoId': localExecucaoId,
        'motivo': motivo,
        'dataHoraInicio': agora.toIso8601String(),
        'latitudeInicio': latitude,
        'longitudeInicio': longitude,
      },
    );
  }

  Future<void> finalizarPausaLocal({
    required String localExecucaoId,
    double? latitude,
    double? longitude,
  }) async {
    final agora = DateTime.now();
    final db = await _database.database;
    final pausas = await db.query(
      'offline_pausa',
      where: 'local_execucao_id = ? AND data_hora_fim IS NULL',
      whereArgs: [localExecucaoId],
      orderBy: 'data_hora_inicio DESC',
      limit: 1,
    );

    if (pausas.isNotEmpty) {
      final clientEventId = pausas.first['client_event_id'] as String;
      await db.update(
        'offline_pausa',
        {
          'data_hora_fim': agora.toIso8601String(),
          'latitude_fim': latitude,
          'longitude_fim': longitude,
          'sincronizado_em': null,
        },
        where: 'client_event_id = ?',
        whereArgs: [clientEventId],
      );

      await _enfileirar(
        localExecucaoId: localExecucaoId,
        tipoRegistro: 'pausa_fim',
        clientEventId: '${clientEventId}_fim',
        payload: {
          'clientEventId': clientEventId,
          'localExecucaoId': localExecucaoId,
          'dataHoraFim': agora.toIso8601String(),
          'latitudeFim': latitude,
          'longitudeFim': longitude,
        },
      );
    }

    await db.update(
      'offline_execucao',
      {'status': 'em_andamento', 'atualizado_em': agora.toIso8601String()},
      where: 'local_execucao_id = ?',
      whereArgs: [localExecucaoId],
    );
  }

  Future<void> registrarLocalizacaoLocal({
    required String localExecucaoId,
    required double latitude,
    required double longitude,
    required DateTime dataHora,
    bool? gpsSimulado,
    double? precisaoEmMetros,
    double? velocidadeMetrosPorSegundo,
    double? direcaoGraus,
    double? altitudeMetros,
    int? fonteCaptura,
  }) async {
    final clientEventId =
        '${localExecucaoId}_loc_${dataHora.microsecondsSinceEpoch}';
    final db = await _database.database;

    await db.insert('offline_localizacao', {
      'client_event_id': clientEventId,
      'local_execucao_id': localExecucaoId,
      'latitude': latitude,
      'longitude': longitude,
      'data_hora': dataHora.toIso8601String(),
      'gps_simulado': gpsSimulado == true ? 1 : 0,
      'precisao_em_metros': precisaoEmMetros,
      'velocidade_metros_por_segundo': velocidadeMetrosPorSegundo,
      'direcao_graus': direcaoGraus,
      'altitude_metros': altitudeMetros,
      'fonte_captura': fonteCaptura,
    }, conflictAlgorithm: ConflictAlgorithm.ignore);

    await _enfileirar(
      localExecucaoId: localExecucaoId,
      tipoRegistro: 'localizacao',
      clientEventId: clientEventId,
      payload: {
        'clientEventId': clientEventId,
        'localExecucaoId': localExecucaoId,
        'latitude': latitude,
        'longitude': longitude,
        'dataHora': dataHora.toIso8601String(),
        'gpsSimulado': gpsSimulado ?? false,
        'precisaoEmMetros': precisaoEmMetros,
        'velocidadeMetrosPorSegundo': velocidadeMetrosPorSegundo,
        'direcaoGraus': direcaoGraus,
        'altitudeMetros': altitudeMetros,
        'fonteCaptura': fonteCaptura,
      },
    );
  }

  Future<ResumoPacoteOffline> obterResumoPacote() async {
    final db = await _database.database;
    final ultimoDownload = await _obterConfig(_configUltimoDownload);
    final validoAte = await _obterConfig(_configValidoAte);
    final quantidade =
        Sqflite.firstIntValue(
          await db.rawQuery('SELECT COUNT(1) FROM offline_route_cache'),
        ) ??
        0;

    return ResumoPacoteOffline(
      modoOfflineHabilitado: await modoOfflineHabilitado(),
      ultimoDownload: ultimoDownload != null
          ? DateTime.tryParse(ultimoDownload)
          : null,
      validoAte: validoAte != null ? DateTime.tryParse(validoAte) : null,
      quantidadeRotas: quantidade,
      quantidadePendencias: await contarPendencias(),
    );
  }

  Future<int> contarPendencias() async {
    final db = await _database.database;
    final execucoes =
        Sqflite.firstIntValue(
          await db.rawQuery(
            'SELECT COUNT(1) FROM offline_execucao WHERE sincronizado_em IS NULL',
          ),
        ) ??
        0;
    final fila =
        Sqflite.firstIntValue(
          await db.rawQuery(
            'SELECT COUNT(1) FROM offline_sync_queue WHERE sincronizado_em IS NULL',
          ),
        ) ??
        0;

    return execucoes + fila;
  }

  Future<List<Map<String, dynamic>>> montarLotesSincronizacao() async {
    final db = await _database.database;
    final execucoes = await db.rawQuery('''
      SELECT DISTINCT e.*
      FROM offline_execucao e
      LEFT JOIN offline_sync_queue q
        ON q.local_execucao_id = e.local_execucao_id
       AND q.sincronizado_em IS NULL
      WHERE e.sincronizado_em IS NULL OR q.id IS NOT NULL
      ORDER BY e.data_hora_inicio_local ASC
    ''');

    final identificadorDispositivo = await obterIdentificadorDispositivo();
    final lotes = <Map<String, dynamic>>[];

    for (final execucao in execucoes) {
      final localExecucaoId = execucao['local_execucao_id'] as String;
      final checklistLocalId = execucao['checklist_execucao_id'] as int?;

      final eventos = await db.query(
        'offline_evento',
        where: 'local_execucao_id = ? AND sincronizado_em IS NULL',
        whereArgs: [localExecucaoId],
        orderBy: 'data_hora_evento ASC',
      );
      final localizacoes = await db.query(
        'offline_localizacao',
        where: 'local_execucao_id = ? AND sincronizado_em IS NULL',
        whereArgs: [localExecucaoId],
        orderBy: 'data_hora ASC',
      );
      final pausas = await db.query(
        'offline_pausa',
        where: 'local_execucao_id = ? AND sincronizado_em IS NULL',
        whereArgs: [localExecucaoId],
        orderBy: 'data_hora_inicio ASC',
      );

      lotes.add({
        'localExecucaoId': localExecucaoId,
        'identificadorDispositivo': identificadorDispositivo,
        'rotaId': execucao['rota_id'],
        'veiculoId': execucao['veiculo_id'],
        'checklistExecucaoId': checklistLocalId != null && checklistLocalId > 0
            ? checklistLocalId
            : null,
        'checklistLocalId': checklistLocalId,
        'itensChecklist': await _obterItensChecklistLocal(checklistLocalId),
        'dataHoraInicioLocal': execucao['data_hora_inicio_local'],
        'dataHoraFimLocal': execucao['data_hora_fim_local'],
        'latitudeInicio': _valorTexto(execucao['latitude_inicio']),
        'longitudeInicio': _valorTexto(execucao['longitude_inicio']),
        'gpsSimuladoInicio': _inteiroComoBool(execucao['gps_simulado_inicio']),
        'observacaoInicio': execucao['observacao_inicio'],
        'observacaoFim': execucao['observacao_fim'],
        'eventos': eventos.map(_eventoParaPayload).toList(),
        'localizacoes': localizacoes.map(_localizacaoParaPayload).toList(),
        'pausas': pausas.map(_pausaParaPayload).toList(),
      });
    }

    return lotes;
  }

  Future<String> obterIdentificadorDispositivo() async {
    final atual = await _obterConfig(_configIdentificadorDispositivo);
    if (atual != null && atual.isNotEmpty) return atual;

    final novo = 'app_${DateTime.now().microsecondsSinceEpoch}';
    final db = await _database.database;
    await db.transaction((txn) async {
      await _salvarConfig(txn, _configIdentificadorDispositivo, novo);
    });
    return novo;
  }

  Future<void> marcarExecucaoSincronizada({
    required String localExecucaoId,
    required int rotaExecucaoId,
    required DateTime sincronizadoEm,
    int? checklistLocalId,
  }) async {
    final db = await _database.database;
    final data = sincronizadoEm.toIso8601String();

    await db.transaction((txn) async {
      await txn.update(
        'offline_execucao',
        {
          'rota_execucao_id': rotaExecucaoId,
          'sincronizado_em': data,
          'erro_sincronizacao': null,
          'atualizado_em': data,
        },
        where: 'local_execucao_id = ?',
        whereArgs: [localExecucaoId],
      );

      for (final tabela in [
        'offline_evento',
        'offline_localizacao',
        'offline_pausa',
      ]) {
        await txn.update(
          tabela,
          {'sincronizado_em': data},
          where: 'local_execucao_id = ? AND sincronizado_em IS NULL',
          whereArgs: [localExecucaoId],
        );
      }

      await txn.update(
        'offline_sync_queue',
        {'sincronizado_em': data, 'erro': null},
        where: 'local_execucao_id = ? AND sincronizado_em IS NULL',
        whereArgs: [localExecucaoId],
      );

      if (checklistLocalId != null && checklistLocalId < 0) {
        await txn.update(
          'offline_sync_queue',
          {'sincronizado_em': data, 'erro': null},
          where: 'client_event_id = ? AND sincronizado_em IS NULL',
          whereArgs: ['checklist_${checklistLocalId.abs()}'],
        );
      }
    });
  }

  Future<void> marcarErroSincronizacao({
    required String localExecucaoId,
    required String erro,
  }) async {
    final db = await _database.database;
    final agora = DateTime.now().toIso8601String();

    await db.transaction((txn) async {
      await txn.update(
        'offline_execucao',
        {'erro_sincronizacao': erro, 'atualizado_em': agora},
        where: 'local_execucao_id = ?',
        whereArgs: [localExecucaoId],
      );

      await txn.update(
        'offline_sync_queue',
        {'erro': erro, 'ultima_tentativa_em': agora},
        where: 'local_execucao_id = ? AND sincronizado_em IS NULL',
        whereArgs: [localExecucaoId],
      );
    });
  }

  Future<void> _limparCache(Transaction txn) async {
    await txn.delete('offline_route_stop_cache');
    await txn.delete('offline_checklist_cache');
    await txn.delete('offline_route_vehicle_cache');
    await txn.delete('offline_route_cache');
  }

  Future<void> _salvarConfig(
    Transaction txn,
    String chave,
    String valor,
  ) async {
    await txn.insert('offline_config', {
      'chave': chave,
      'valor': valor,
      'atualizado_em': DateTime.now().toIso8601String(),
    }, conflictAlgorithm: ConflictAlgorithm.replace);
  }

  Future<String?> _obterConfig(String chave) async {
    final db = await _database.database;
    final rows = await db.query(
      'offline_config',
      columns: ['valor'],
      where: 'chave = ?',
      whereArgs: [chave],
      limit: 1,
    );

    if (rows.isEmpty) return null;
    return rows.first['valor'] as String?;
  }

  Future<void> _enfileirar({
    required String localExecucaoId,
    required String tipoRegistro,
    required String clientEventId,
    required Map<String, dynamic> payload,
  }) async {
    final db = await _database.database;
    await db.insert('offline_sync_queue', {
      'local_execucao_id': localExecucaoId,
      'tipo_registro': tipoRegistro,
      'client_event_id': clientEventId,
      'payload': jsonEncode(payload),
      'criado_em': DateTime.now().toIso8601String(),
    });
  }

  int _tipoEventoPorParada(int paradaId) {
    if (paradaId == -1) return 2;
    if (paradaId == -2) return 4;
    return 3;
  }

  Future<List<int>> _obterItensChecklistLocal(int? checklistLocalId) async {
    if (checklistLocalId == null) return [];

    final db = await _database.database;
    final rows = await db.query(
      'offline_sync_queue',
      where: 'tipo_registro = ? AND sincronizado_em IS NULL',
      whereArgs: ['checklist'],
      orderBy: 'id DESC',
    );

    for (final row in rows) {
      final payload = jsonDecode(row['payload'] as String);
      if (payload is! Map<String, dynamic>) continue;
      if (payload['localChecklistId'] != checklistLocalId) continue;

      final itens = payload['itens'];
      if (itens is! List) return [];
      return itens.whereType<int>().toList();
    }

    return [];
  }

  Map<String, dynamic> _eventoParaPayload(Map<String, dynamic> row) {
    return {
      'registradoOffline': true,
      'dataHoraRegistroLocal': row['data_hora_evento'],
      'identificadorDispositivo': null,
      'localExecucaoId': row['local_execucao_id'],
      'clientEventId': row['client_event_id'],
      'tipoEvento': row['tipo_evento'],
      'paradaRotaId': row['parada_rota_id'],
      'unidadeId': row['unidade_id'],
      'entregue': _inteiroNuloComoBool(row['entregue']),
      'observacao': row['observacao'],
      'latitude': _valorTexto(row['latitude']),
      'longitude': _valorTexto(row['longitude']),
      'gpsSimulado': _inteiroComoBool(row['gps_simulado']),
      'dataHoraEvento': row['data_hora_evento'],
    };
  }

  Map<String, dynamic> _localizacaoParaPayload(Map<String, dynamic> row) {
    return {
      'registradoOffline': true,
      'dataHoraRegistroLocal': row['data_hora'],
      'identificadorDispositivo': null,
      'localExecucaoId': row['local_execucao_id'],
      'clientEventId': row['client_event_id'],
      'latitude': _valorTexto(row['latitude']),
      'longitude': _valorTexto(row['longitude']),
      'dataHora': row['data_hora'],
      'gpsSimulado': _inteiroComoBool(row['gps_simulado']),
      'precisaoEmMetros': row['precisao_em_metros'],
      'velocidadeMetrosPorSegundo': row['velocidade_metros_por_segundo'],
      'direcaoGraus': row['direcao_graus'],
      'altitudeMetros': row['altitude_metros'],
      'fonteCaptura': row['fonte_captura'],
    };
  }

  Map<String, dynamic> _pausaParaPayload(Map<String, dynamic> row) {
    return {
      'registradoOffline': true,
      'dataHoraRegistroLocal': row['data_hora_inicio'],
      'identificadorDispositivo': null,
      'localExecucaoId': row['local_execucao_id'],
      'clientEventId': row['client_event_id'],
      'motivo': row['motivo'],
      'dataHoraInicio': row['data_hora_inicio'],
      'latitudeInicio': _valorTexto(row['latitude_inicio']),
      'longitudeInicio': _valorTexto(row['longitude_inicio']),
      'gpsSimuladoInicio': _inteiroComoBool(row['gps_simulado_inicio']),
      'dataHoraFim': row['data_hora_fim'],
      'latitudeFim': _valorTexto(row['latitude_fim']),
      'longitudeFim': _valorTexto(row['longitude_fim']),
      'gpsSimuladoFim': _inteiroComoBool(row['gps_simulado_fim']),
    };
  }

  bool _inteiroComoBool(Object? valor) {
    if (valor is bool) return valor;
    if (valor is int) return valor == 1;
    return false;
  }

  bool? _inteiroNuloComoBool(Object? valor) {
    if (valor == null) return null;
    return _inteiroComoBool(valor);
  }

  String? _valorTexto(Object? valor) {
    if (valor == null) return null;
    return valor.toString();
  }
}

class ResumoPacoteOffline {
  final bool modoOfflineHabilitado;
  final DateTime? ultimoDownload;
  final DateTime? validoAte;
  final int quantidadeRotas;
  final int quantidadePendencias;

  const ResumoPacoteOffline({
    required this.modoOfflineHabilitado,
    required this.ultimoDownload,
    required this.validoAte,
    required this.quantidadeRotas,
    required this.quantidadePendencias,
  });
}
