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
cd ..

echo "[3/4] Enviando arquivos para o servidor VPS via SCP..."
echo "*(Isso pode demorar alguns minutos. Se pedir senha, digite a senha da VPS)*"
# Usamos -o StrictHostKeyChecking=no para evitar que pergunte sobre chave pela primeira vez
scp -o StrictHostKeyChecking=no -r ./NuvPizza.API/publish/* root@69.62.102.68:/root/nuvpizza

echo "[4/4] Reiniciando serviço na Hostinger..."
echo "*(Se pedir senha, informe a senha da VPS novamente)*"
ssh root@69.62.102.68 "systemctl restart nuvpizza && systemctl status nuvpizza --no-pager"

echo "=========================================="
echo "✅ Deploy finalizado com sucesso!!"
echo "=========================================="
