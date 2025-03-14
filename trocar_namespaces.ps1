$oldNamespace = "AriD.GerenciamentoDePonto"
$newNamespace = "AriD.GerenciamentoEscolar"

Get-ChildItem -Path . -Recurse -Include *.cs,*.cshtml,*.json,*.xml,*.config | ForEach-Object {
    (Get-Content $_.FullName) -replace $oldNamespace, $newNamespace | Set-Content $_.FullName
}

Write-Host "Namespaces substituídos com sucesso!"