#!/usr/bin/env bash
# .env.example'dan rastgele, güçlü şifrelerle .env dosyası oluşturur.
set -euo pipefail

cd "$(dirname "$0")/.."

if [ -f .env ]; then
  echo ".env zaten var, değiştirilmedi. Yeniden oluşturmak için önce silin."
  exit 1
fi

# 16 rastgele harf/rakam + SQL Server ve Identity şifre kurallarını garanti eden sonek
generate_password() {
  local random
  random=$(LC_ALL=C tr -dc 'A-Za-z0-9' < /dev/urandom | head -c 16)
  echo "${random}Aa1!"
}

sa_password=$(generate_password)
admin_password=$(generate_password)
user_password=$(generate_password)

sed -e "s|^MSSQL_SA_PASSWORD=.*|MSSQL_SA_PASSWORD=${sa_password}|" \
    -e "s|^SeedUsers__AdminPassword=.*|SeedUsers__AdminPassword=${admin_password}|" \
    -e "s|^SeedUsers__UserPassword=.*|SeedUsers__UserPassword=${user_password}|" \
    .env.example > .env

chmod 600 .env

echo ".env oluşturuldu. Giriş bilgileri:"
echo "  admin     / ${admin_password}"
echo "  kullanici / ${user_password}"
echo "(Şifreleri daha sonra .env dosyasında görebilirsiniz.)"
