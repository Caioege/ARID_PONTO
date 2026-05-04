import 'package:path/path.dart';
import 'package:sqflite/sqflite.dart';

class OfflineDatabase {
  static const _databaseName = 'arid_rastreio_offline.db';
  static const _databaseVersion = 4;

  Database? _database;

  Future<Database> get database async {
    if (_database != null) return _database!;

    final databasePath = await getDatabasesPath();
    final path = join(databasePath, _databaseName);

    _database = await openDatabase(
      path,
      version: _databaseVersion,
      onCreate: _createSchema,
      onUpgrade: _upgradeSchema,
    );

    return _database!;
  }

  Future<void> _createSchema(Database db, int version) async {
    await db.execute('''
      CREATE TABLE offline_config (
        chave TEXT PRIMARY KEY,
        valor TEXT NOT NULL,
        atualizado_em TEXT NOT NULL
      )
    ''');

    await db.execute('''
      CREATE TABLE offline_route_cache (
        rota_id INTEGER PRIMARY KEY,
        codigo TEXT NOT NULL,
        nome TEXT NOT NULL,
        descricao TEXT NOT NULL,
        permite_pausa INTEGER NOT NULL DEFAULT 0,
        quantidade_pausas INTEGER NOT NULL DEFAULT 0,
        permite_iniciar_sem_paciente_acompanhante INTEGER NOT NULL DEFAULT 1,
        permite_iniciar_sem_profissional INTEGER NOT NULL DEFAULT 1,
        unidade_origem_id INTEGER NULL,
        unidade_destino_id INTEGER NULL,
        nome_unidade_origem TEXT NULL,
        nome_unidade_destino TEXT NULL,
        origem_latitude_rota REAL NULL,
        origem_longitude_rota REAL NULL,
        destino_latitude_rota REAL NULL,
        destino_longitude_rota REAL NULL,
        pacientes_json TEXT NULL,
        profissionais_json TEXT NULL,
        pacientes_disponiveis_json TEXT NULL,
        baixado_em TEXT NOT NULL,
        valido_ate TEXT NOT NULL
      )
    ''');

    await db.execute('''
      CREATE TABLE offline_route_vehicle_cache (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        rota_id INTEGER NOT NULL,
        veiculo_id INTEGER NOT NULL,
        nome TEXT NOT NULL,
        placa TEXT NOT NULL,
        modelo TEXT NOT NULL,
        cor TEXT NOT NULL,
        UNIQUE (rota_id, veiculo_id)
      )
    ''');

    await db.execute('''
      CREATE TABLE offline_checklist_cache (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        rota_id INTEGER NOT NULL,
        veiculo_id INTEGER NOT NULL,
        item_id INTEGER NOT NULL,
        descricao TEXT NOT NULL,
        UNIQUE (rota_id, veiculo_id, item_id)
      )
    ''');

    await db.execute('''
      CREATE TABLE offline_route_stop_cache (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        rota_id INTEGER NOT NULL,
        parada_id INTEGER NOT NULL,
        endereco TEXT NOT NULL,
        latitude REAL NULL,
        longitude REAL NULL,
        link TEXT NULL,
        observacao_cadastro TEXT NULL,
        ordem INTEGER NOT NULL DEFAULT 0,
        UNIQUE (rota_id, parada_id)
      )
    ''');

    await db.execute('''
      CREATE TABLE offline_execucao (
        local_execucao_id TEXT PRIMARY KEY,
        rota_id INTEGER NOT NULL,
        veiculo_id INTEGER NOT NULL,
        checklist_execucao_id INTEGER NULL,
        status TEXT NOT NULL,
        data_hora_inicio_local TEXT NOT NULL,
        data_hora_fim_local TEXT NULL,
        latitude_inicio REAL NULL,
        longitude_inicio REAL NULL,
        gps_simulado_inicio INTEGER NOT NULL DEFAULT 0,
        observacao_inicio TEXT NULL,
        observacao_fim TEXT NULL,
        pacientes_presenca_json TEXT NULL,
        profissionais_presenca_json TEXT NULL,
        rota_execucao_id INTEGER NULL,
        sincronizado_em TEXT NULL,
        erro_sincronizacao TEXT NULL,
        criado_em TEXT NOT NULL,
        atualizado_em TEXT NOT NULL
      )
    ''');

    await db.execute('''
      CREATE TABLE offline_evento (
        client_event_id TEXT PRIMARY KEY,
        local_execucao_id TEXT NOT NULL,
        tipo_evento INTEGER NOT NULL,
        parada_rota_id INTEGER NULL,
        unidade_id INTEGER NULL,
        entregue INTEGER NULL,
        observacao TEXT NULL,
        latitude REAL NULL,
        longitude REAL NULL,
        gps_simulado INTEGER NOT NULL DEFAULT 0,
        data_hora_evento TEXT NOT NULL,
        sincronizado_em TEXT NULL,
        FOREIGN KEY (local_execucao_id) REFERENCES offline_execucao(local_execucao_id)
      )
    ''');

    await db.execute('''
      CREATE TABLE offline_localizacao (
        client_event_id TEXT PRIMARY KEY,
        local_execucao_id TEXT NOT NULL,
        latitude REAL NOT NULL,
        longitude REAL NOT NULL,
        data_hora TEXT NOT NULL,
        gps_simulado INTEGER NOT NULL DEFAULT 0,
        precisao_em_metros REAL NULL,
        velocidade_metros_por_segundo REAL NULL,
        direcao_graus REAL NULL,
        altitude_metros REAL NULL,
        fonte_captura INTEGER NULL,
        sincronizado_em TEXT NULL,
        FOREIGN KEY (local_execucao_id) REFERENCES offline_execucao(local_execucao_id)
      )
    ''');

    await db.execute('''
      CREATE TABLE offline_pausa (
        client_event_id TEXT PRIMARY KEY,
        local_execucao_id TEXT NOT NULL,
        motivo TEXT NULL,
        data_hora_inicio TEXT NOT NULL,
        latitude_inicio REAL NULL,
        longitude_inicio REAL NULL,
        gps_simulado_inicio INTEGER NOT NULL DEFAULT 0,
        data_hora_fim TEXT NULL,
        latitude_fim REAL NULL,
        longitude_fim REAL NULL,
        gps_simulado_fim INTEGER NOT NULL DEFAULT 0,
        sincronizado_em TEXT NULL,
        FOREIGN KEY (local_execucao_id) REFERENCES offline_execucao(local_execucao_id)
      )
    ''');

    await db.execute('''
      CREATE TABLE offline_sync_queue (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        local_execucao_id TEXT NOT NULL,
        tipo_registro TEXT NOT NULL,
        client_event_id TEXT NULL,
        payload TEXT NOT NULL,
        criado_em TEXT NOT NULL,
        ultima_tentativa_em TEXT NULL,
        erro TEXT NULL,
        sincronizado_em TEXT NULL
      )
    ''');
  }

  Future<void> _upgradeSchema(
    Database db,
    int oldVersion,
    int newVersion,
  ) async {
    if (oldVersion < 2) {
      await _addColumnIfMissing(
        db,
        'offline_route_cache',
        'origem_latitude_rota',
        'REAL NULL',
      );
      await _addColumnIfMissing(
        db,
        'offline_route_cache',
        'origem_longitude_rota',
        'REAL NULL',
      );
      await _addColumnIfMissing(
        db,
        'offline_route_cache',
        'destino_latitude_rota',
        'REAL NULL',
      );
      await _addColumnIfMissing(
        db,
        'offline_route_cache',
        'destino_longitude_rota',
        'REAL NULL',
      );
      await _addColumnIfMissing(
        db,
        'offline_route_stop_cache',
        'observacao_cadastro',
        'TEXT NULL',
      );
    }
    if (oldVersion < 3) {
      await _addColumnIfMissing(
        db,
        'offline_route_cache',
        'pacientes_json',
        'TEXT NULL',
      );
      await _addColumnIfMissing(
        db,
        'offline_route_cache',
        'profissionais_json',
        'TEXT NULL',
      );
      await _addColumnIfMissing(
        db,
        'offline_route_cache',
        'pacientes_disponiveis_json',
        'TEXT NULL',
      );
      await _addColumnIfMissing(
        db,
        'offline_execucao',
        'pacientes_presenca_json',
        'TEXT NULL',
      );
      await _addColumnIfMissing(
        db,
        'offline_execucao',
        'profissionais_presenca_json',
        'TEXT NULL',
      );
    }
    if (oldVersion < 4) {
      await _addColumnIfMissing(
        db,
        'offline_route_cache',
        'permite_iniciar_sem_paciente_acompanhante',
        'INTEGER NOT NULL DEFAULT 1',
      );
      await _addColumnIfMissing(
        db,
        'offline_route_cache',
        'permite_iniciar_sem_profissional',
        'INTEGER NOT NULL DEFAULT 1',
      );
    }
  }

  Future<void> _addColumnIfMissing(
    Database db,
    String table,
    String column,
    String definition,
  ) async {
    final columns = await db.rawQuery('PRAGMA table_info($table)');
    final exists = columns.any((item) => item['name'] == column);
    if (!exists) {
      await db.execute('ALTER TABLE $table ADD COLUMN $column $definition');
    }
  }
}
