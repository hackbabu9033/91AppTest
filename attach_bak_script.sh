sleep 15s

/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P '>Lf5=!yP=yY(8-Re' -Q "CREATE DATABASE FileSystemMock ON (FILENAME = '/var/opt/mssql/data/FileSystemMock.mdf') FOR ATTACH_REBUILD_LOG"

RUN \
rm -f /var/opt/mssql/data/FileSystemMock.mdf