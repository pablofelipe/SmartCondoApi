#!/bin/sh

# Método confiável para verificar PostgreSQL usando psql (já incluso na imagem PostgreSQL)
wait_for_db() {
    echo "Waiting for PostgreSQL to accept connections..."
    until PGPASSWORD=$DB_PASSWORD psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -c '\q'; do
        echo "PostgreSQL is unavailable - sleeping"
        sleep 1
    done
    echo "PostgreSQL is ready!"
}

# Chama a função de espera
wait_for_db

# Executa migrações
echo "Applying database migrations..."
dotnet SmartCondoApi.dll migrate || {
    echo "Migration failed!"
    exit 1
}

# Popula o banco de dados
echo "Seeding database..."
dotnet SmartCondoApi.dll seed || {
    echo "Seeding failed!"
    exit 1
}

# Inicia a aplicação
echo "Starting application..."
exec dotnet SmartCondoApi.dll