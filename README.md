# Book Store

## A study project, the purpose is to learn the structure of a microservice application.

### Detail structure of the newest version showed as:

![Architecture](/resource/bookstore_architecture.png)

### Note: Not the last version (till I'm able to work as Full-snack dev :">)

## Database configuration

When running from the service project folders, both services load the local
`Settings/appsettings.json` file at the repository root. This file is not tracked;
keep credentials out of source control.

Configure two entries under `ConnectionStrings`:

- `ProductConnection`: SQL Server connection for `BookStore.Products`.
- `AuthenticationConnection`: SQL Server connection for `BookStore.Authentication`.

Use the same server, credentials and connection options when splitting the old
`DefaultConnection`; change only the database name for Authentication. Each
service requires its own non-empty connection string.

To create an Authentication migration from PowerShell at the repository root:

```powershell
Push-Location src\Authentication
dotnet ef migrations add InitDb
Pop-Location
```

Creating a migration does not create the database. After reviewing the generated
migration, run `dotnet ef database update` from the same service folder to apply
it. Use an EF CLI version matching that service's EF Core packages.
