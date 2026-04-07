ALTER TABLE organizacao ADD COLUMN RecebeNotificacaoConectividade BOOLEAN NOT NULL DEFAULT 0;
ALTER TABLE organizacao ADD COLUMN EmailNotificacaoConectividade VARCHAR(255) NULL;

ALTER TABLE unidadeorganizacional ADD COLUMN RecebeNotificacaoConectividade BOOLEAN NOT NULL DEFAULT 0;
ALTER TABLE unidadeorganizacional ADD COLUMN EmailNotificacaoConectividade VARCHAR(255) NULL;

-- Atualização das colunas relativas aos metadados do aplicativo (Plataforma, Push Token e Último Acesso)
ALTER TABLE Servidor 
ADD COLUMN PushToken VARCHAR(255) NULL,
ADD COLUMN PlataformaDispositivo VARCHAR(20) NULL,
ADD COLUMN UltimoAcessoApp DATETIME NULL;
