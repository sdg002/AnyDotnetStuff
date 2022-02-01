# Overview


# How to install Docker?
to be done

# Run MS SQL server using Docker?

## Where to find information about MS SQL Server docker container?
https://hub.docker.com/_/microsoft-mssql-server?tab=description


## How to pull the MS SQL server image
```
docker pull mcr.microsoft.com/mssql/server
```

## How to verify that image was downloaded?
```
docker images
```
Expected output
```
REPOSITORY                       TAG       IMAGE ID       CREATED      SIZE
mcr.microsoft.com/mssql/server   latest    d78e982c2f2b   9 days ago   1.48GB
```

# How to run the SQL Server image?
```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=yourStrong(!)Password" -e "MSSQL_PID=Express" -p 1433:1433 -d mcr.microsoft.com/mssql/server
```
## Verify running list of containers

```
docker ps
```

Expected output

```
CONTAINER ID   IMAGE                            COMMAND                  CREATED              STATUS              PORTS                    NAMES
7dd76f25dc17   mcr.microsoft.com/mssql/server   "/opt/mssql/bin/perm…"   About a minute ago   Up About a minute   0.0.0.0:1433->1433/tcp   magical_perlman
```

# Understanding the arguments for DOCKER RUN
## -e,--env
Sets the environment variables
```
-e "name1=value2"
```

## -p, --publish
Publish the container's port to the host. Example:
```
-p 1433:1433
```
This will map the port 1433 of the container to 1433 of the host


## -d, --detach
This command starts the container, prints its id, and then returns to the shell prompt. Thus, we can continue with other tasks while the container continues to run in the background

## -v, --volume
<<TO BE DONE>>
Talk about volumes, ls 

# How to connect to the SQL Server instance inside the new container?
## List all databases using SQLCMD
I have stored the password in the environment variable *password*
```
SQLCMD -S localhost -U sa -P %password% -d master -Q "select name  from sys.databases"
```

Expected output:
```
name
--------------------------------------------------------------------------------------------------------------------------------
master
tempdb
model
msdb
```

## Using Sql Server Management Studio

![SSMS connection dialog!](images/ssms_connection.png "SSMS connection dialog")


# How to stop a running container?

```
docker stop [id of the container]
```
The id of the container is obtained by running `docker ps`


# How to launch MSSQL server and also mount a volume?
```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=%password%" -e "MSSQL_PID=Express" -p 1433:1433   -v c:\truetemp\dock001:/cooldata mcr.microsoft.com/mssql/server
```

# How to create a new database using Sql Server Management Studio?
In this step we are creating a new database such that the MDF and LDF files are created in the mounted volume **/cooldata**. This ensures persistence of the data. 
```
CREATE DATABASE [MyDatabase] 
    ON (NAME = N'MyDatabase', FILENAME = N'/cooldata/MyDatabase.mdf', SIZE = 1024MB, FILEGROWTH = 256MB)
LOG ON (NAME = N'MyDatabase_log', FILENAME = N'/cooldata/MyDatabase_log.ldf', SIZE = 512MB, FILEGROWTH = 125MB)
GO
```
I went ahead and created a simple **Employees** table in the database **MyDatabase**

![New database!](/images/ssms_create_mydatabase.png "New database")

You should also see the new MDF and LDF files on your host folder *c:\truetemp\dock001*

# How to re-attach the same database across container sessions?

## Step 1 - Select the Attach option from the context menu
![Attach database!](/images/ssms_attach_database_right_click_menu.png "Attach database")

## Step 2 - Select the mounted path
This is the folder we have mounted via the **-v** command line option
![Attach database!](/images/ssms_attach_database.png "Attach database")

## Step 3 - Run some query
![Attach database and run query!](/images/ssms_attach_database_run_select_query.png "Attach database and run some query")

# Simplifying the re-attachment to your MDF files by specifying a default directory?
The steps described above are easily repeatable. We could slightly simplify the steps by instructing MSSQL that it should use the mounted folder as the default data directory. We would still need to use the "Attach Database" context menu. However, this folder selection will be initialized with the folder containing our data files
```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=%password%" -e "MSSQL_PID=Express" -e "MSSQL_DATA_DIR=/cooldata" -p 1433:1433   -v c:\truetemp\dock001:/cooldata mcr.microsoft.com/mssql/server
```

# Microsoft documentation

## Overview
https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment?view=sql-server-ver15&pivots=cs1-bash

## Provide a default data directory
https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-configure?view=sql-server-ver15&pivots=cs1-powershell#changefilelocation


## How to pull a Docker image with latest SQL?
https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver15&pivots=cs1-powershell#pullandrun2019




---

# You were here

Mounting a volume
## What worked?
```
docker run -it   -v c:/truetemp/dock001:/cooldata mcr.microsoft.com/mssql/server
```

## How to list the mounted folder?
```
docker run -it   -v c:/truetemp/dock001:/cooldata mcr.microsoft.com/mssql/server  ls -l
```
and
```
docker run -it   -v c:/truetemp/dock001:/cooldata mcr.microsoft.com/mssql/server  ls
```

## What did not work?
```
run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=%password%" -e "MSSQL_PID=Express" -p 1433:1433   -v c:\truetemp\dock001:c:\cooldata mcr.microsoft.com/mssql/server
```


# What was I doing?
How to run an interactive commands?
Generate some files and get them into the mounted folder?
Use a mounted folder for attaching a SQL server database

