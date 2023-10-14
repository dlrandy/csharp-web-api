
https://setapp.com/how-to/install-sql-server
docker run -d --name sql_server_test -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=reallyStrongPwd123' -p 1433:1433 mcr.microsoft.com/mssql/server:2019-latest

dotnet ef migrations add Initial

dotnet ef database update Initial

``` sql
USE master;
GO

ALTER DATABASE MyBGList SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
DROP DATABASE MyBGList;
GO

```
 dotnet ef migrations add FirstUpgrade

 dotnet ef database update FirstUpgrade


 创建distributed mssql
 ```sql
 dotnet sql-cache create 'Server=localhost;Database=MyBGList; User Id=MyBGList;Password=MyS3cretP4$$; Integrated Security=False;MultipleActiveResultSets=True; TrustServerCertificate=True' dbo AppCache
 ```




