CREATE DATABASE [MyDatabase] 
    ON (NAME = N'MyDatabase', FILENAME = N'/cooldata/MyDatabase.mdf', SIZE = 1024MB, FILEGROWTH = 256MB)
LOG ON (NAME = N'MyDatabase_log', FILENAME = N'/cooldata/MyDatabase_log.ldf', SIZE = 512MB, FILEGROWTH = 125MB)
GO