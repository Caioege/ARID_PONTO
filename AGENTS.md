# AGENTS.md

## Dev environment tips
- Install dependencies with `npm install` before running scaffolds.
- Use `npm run dev` for the interactive TypeScript session that powers local experimentation.
- Run `npm run build` to refresh the CommonJS bundle in `dist/` before shipping changes.
- Store generated artefacts in `.context/` so reruns stay deterministic.
- Always clean up temporary compilation or log files (e.g., `build_errors.txt`, `build_log`, etc.) once their analysis is finished at the end of an implementation.

## Testing instructions
- Execute `npm run test` to run the Jest suite.
- Append `-- --watch` while iterating on a failing spec.
- Trigger `npm run build && npm run test` before opening a PR to mimic CI.
- Add or update tests alongside any generator or CLI changes.

## PR instructions
- Follow Conventional Commits (for example, `feat(scaffolding): add doc links`).
- Cross-link new scaffolds in `docs/README.md` and `agents/README.md` so future agents can find them.
- Attach sample CLI output or generated markdown when behaviour shifts.
- Confirm the built artefacts in `dist/` match the new source changes.

## Documentation instructions
- Toda e qualquer nova funcionalidade ou melhoria de funcionamento deve ser documentada em Markdown dentro de `Documentação de Melhorias/`. O contexto da IA (`.context/`) só deve ser atualizado com essas funcionalidades se estritamente necessário (ex: informações relevantes para o agente utilizar no desenvolvimento).
- O documento deve obrigatoriamente conter seções como: "o que é", "como usar", "por que", e "guias de uso".
- Mantenha a documentação atualizada sempre que houver mudanças de comportamento.
- Agentes de IA devem buscar consultar a pasta `Documentação de Melhorias/` se precisarem entender regras de negócios ou o funcionamento de uma funcionalidade recém-criada ou existente.

## Repository map
- `Documentação de Melhorias/` — contém a documentação (MD) explicativa de todas as novas funcionalidades e melhorias.
- `ScriptsSQL/` — pasta centralizadora contendo todos os scripts de criação, migração e alteração de banco de dados (.sql).
- `AriD.BibliotecaDeClasses/` — explain what lives here and when agents should edit it.
- `AriD.GerenciamentoDePonto/` — explain what lives here and when agents should edit it.
- `AriD.GerenciamentoDePonto.sln/` — explain what lives here and when agents should edit it.
- `AriD.Servicos/` — explain what lives here and when agents should edit it.

## AI Context References
- Documentation index: `.context/docs/README.md`
- Agent playbooks: `.context/agents/README.md`
- Contributor guide: `CONTRIBUTING.md`
