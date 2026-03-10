#!/bin/bash

# Para o caso de algum comando falhar, o script para de executar
set -e

echo "=========================================="
echo "🚀 Iniciando Deploy da API NuvPizza..."
echo "=========================================="

echo "[1/4] Limpando pasta de publish antiga..."
rm -rf ./NuvPizza.API/publish

echo "[2/4] Gerando nova versão (Build/Publish)..."
cd NuvPizza.API
dotnet publish -c Release -o ./publish
# Remove o banco de dados local da pasta publish para não sobrescrever o bd de produção!
rm -f ./publish/*.db ./publish/*.db-shm ./publish/*.db-wal
cd ..

echo "[3/4] Enviando arquivos para o servidor VPS com Rsync (Sincronização Diferencial)..."
echo "*(Isso pode demorar alguns minutos na primeira vez. Se pedir senha, digite a senha da VPS)*"
# Usamos rsync com --delete para limpar arquivos órfãos (como dlls antigas que mudaram de nome)
# Excluímos explicitamente bancos de dados, chaves sensíveis e arquivos gerados em prod (imagens, logs)
rsync -avz --delete \
      --exclude 'app.db*' \
      --exclude 'credentials.json' \
      --exclude 'tokens/' \
      --exclude 'wwwroot/images/' \
      --exclude 'logs/' \
      -e "ssh -o StrictHostKeyChecking=no" \
      ./NuvPizza.API/publish/ root@69.62.102.68:/root/nuvpizza

echo "[4/4] Reiniciando serviço na Hostinger..."
echo "*(Se pedir senha, informe a senha da VPS novamente)*"
ssh root@69.62.102.68 "systemctl restart nuvpizza && systemctl status nuvpizza --no-pager"

echo "=========================================="
echo "✅ Deploy finalizado com sucesso!!"
echo "=========================================="
