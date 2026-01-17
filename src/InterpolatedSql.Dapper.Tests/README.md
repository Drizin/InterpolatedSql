# How to setup a MSSQL for tests

1. Download https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorks2019.bak
1. Find your DATA FOLDER  
   ```sql
    -- Common locations are
    -- C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\
    -- or /var/opt/mssql/data/
    SELECT TOP 1 
    substring(f.physical_name, 1, len(f.physical_name)-charindex('\', reverse(f.physical_name))+1) 
    from sys.master_files f INNER JOIN sys.databases db ON f.database_id=db.database_id
    order by len ( substring(f.physical_name, 1, len(f.physical_name)-charindex('\', reverse(f.physical_name))+1)  )
   -- Replace folders below with the correct locations.
   ```
1. Restore the bak you downloaded:
   ```sql
   RESTORE DATABASE [AdventureWorks2019] FROM DISK = N'C:\Users\youruser\Downloads\AdventureWorks2019.bak' WITH FILE=1,
   MOVE 'AdventureWorks2019' TO 'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\AdventureWorks2019.mdf',
   MOVE 'AdventureWorks2019_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\AdventureWorks2019.ldf'
   GO
   ```
   or
   ```sql
   -- or for Linux based installations:
   RESTORE DATABASE [AdventureWorks2019] FROM DISK = N'/downloads/AdventureWorks2019.bak' WITH FILE=1,
   MOVE 'AdventureWorks2019' TO '/var/opt/mssql/data/AdventureWorks2019.mdf',
   MOVE 'AdventureWorks2019_log' TO '/var/opt/mssql/data/AdventureWorks2019.ldf'
   GO
   ```
   <!--
   RESTORE FILELISTONLY FROM DISK = N'/downloads/AdventureWorks2019.bak'
   -->
1. Adjust Connection string in `TestSettings.json`  
   e.g. remove `\SQLEXPRESS`, and/or change `(local)` by hostname  
   e.g. replace "Integrated Security=True" by "User Id=username;Password=password"
1. Create extra objects for our tests:
   ```sql
   USE [AdventureWorks2019]
   GO

   CREATE PROCEDURE [sp_TestOutput]
       @Input1 [int], 
       @Output1 [int] OUTPUT
   AS
   BEGIN
   	SET @Output1 = 2
   END;
   GO
   ```

# How to setup a MySQL for tests
1. Download MariaDB for Windows https://dlm.mariadb.com/browse/mariadb_server/200/1374/winx64-packages/
1. Create database and some objects:
    ```sql
    CREATE DATABASE DapperQueryBuilderTests;

    USE DapperQueryBuilderTests;

    CREATE TABLE authors
    (author_id INT AUTO_INCREMENT PRIMARY KEY,
    name_last VARCHAR(50),
    name_first VARCHAR(50),
    country VARCHAR(50) );

    INSERT INTO authors
    (name_last, name_first, country)
    VALUES('Kafka', 'Franz', 'Czech Republic');

    INSERT INTO authors
    (name_last, name_first, country)
    VALUES('de Assis', 'Machado', 'Brazil');


    CREATE TABLE books (
    isbn CHAR(20) PRIMARY KEY, 
    title VARCHAR(50),
    author_id INT,
    publisher_id INT,
    year_pub CHAR(4),
    description TEXT );

    INSERT INTO books
    (title, author_id, isbn, year_pub)
    VALUES('The Castle', '1', '0805211063', '1998');

    INSERT INTO books
    (title, author_id, isbn, year_pub)
    VALUES('The Trial', '1', '0805210407', '1995'),
    ('The Metamorphosis', '1', '0553213695', '1995'),
    ('America', '1', '0805210644', '1995');
    ```