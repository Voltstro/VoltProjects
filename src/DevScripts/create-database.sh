psql -v ON_ERROR_STOP=1 --username postgres --dbname voltprojects <<-EOSQL
        CREATE ROLE voltprojects NOSUPERUSER NOCREATEDB NOCREATEROLE INHERIT LOGIN NOREPLICATION NOBYPASSRLS PASSWORD 'Testing123';
        GRANT ALL ON SCHEMA public TO voltprojects;
EOSQL
