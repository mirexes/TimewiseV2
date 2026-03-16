# Настройка инфраструктуры ServiceDesk

Используй этот скилл для создания/обновления инфраструктурных компонентов проекта.

## Структура проектов (.csproj)

### ServiceDesk.Core.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```
Никаких NuGet-зависимостей — чистый домен.

### ServiceDesk.Infrastructure.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.*" />
    <PackageReference Include="WebPush" Version="*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\ServiceDesk.Core\ServiceDesk.Core.csproj" />
  </ItemGroup>
</Project>
```

### ServiceDesk.Application.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\ServiceDesk.Core\ServiceDesk.Core.csproj" />
    <ProjectReference Include="..\ServiceDesk.Infrastructure\ServiceDesk.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

### ServiceDesk.Web.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Serilog.AspNetCore" Version="*" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\ServiceDesk.Application\ServiceDesk.Application.csproj" />
    <ProjectReference Include="..\ServiceDesk.Infrastructure\ServiceDesk.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

## AppDbContext — все DbSet'ы
```csharp
public DbSet<Ticket> Tickets => Set<Ticket>();
public DbSet<Equipment> Equipment => Set<Equipment>();
public DbSet<ServicePoint> ServicePoints => Set<ServicePoint>();
public DbSet<Client> Clients => Set<Client>();
public DbSet<SparePart> SpareParts => Set<SparePart>();
public DbSet<AppUser> Users => Set<AppUser>();
public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
public DbSet<Notification> Notifications => Set<Notification>();
public DbSet<ContactPerson> ContactPersons => Set<ContactPerson>();
public DbSet<SparePartStock> SparePartStocks => Set<SparePartStock>();
public DbSet<SparePartPriceHistory> SparePartPriceHistory => Set<SparePartPriceHistory>();
public DbSet<TicketSparePart> TicketSpareParts => Set<TicketSparePart>();
public DbSet<ChatAttachment> ChatAttachments => Set<ChatAttachment>();
```

## Program.cs — порядок регистрации
```csharp
builder.Services.AddInfrastructure(builder.Configuration);  // DbContext, Repositories, SMS, Push
builder.Services.AddApplication();                           // Services

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
```

## Middleware pipeline
```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Dashboard}/{id?}");
```

## appsettings.json — необходимые секции
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=servicedesk;User=root;Password=..."
  },
  "SmsSettings": {
    "ApiKey": "",
    "Provider": "SmsRu"
  },
  "VapidSettings": {
    "Subject": "",
    "PublicKey": "",
    "PrivateKey": ""
  },
  "Serilog": { ... }
}
```

## Docker Compose
```yaml
services:
  web:
    build: .
    ports: ["5000:8080"]
    depends_on: [db]
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=servicedesk;User=root;Password=secret
  db:
    image: mysql:8
    environment:
      MYSQL_ROOT_PASSWORD: secret
      MYSQL_DATABASE: servicedesk
    volumes: [mysql_data:/var/lib/mysql]
volumes:
  mysql_data:
```

## Команды EF Core миграций
```bash
dotnet ef migrations add {MigrationName} --project src/ServiceDesk.Infrastructure --startup-project src/ServiceDesk.Web
dotnet ef database update --project src/ServiceDesk.Infrastructure --startup-project src/ServiceDesk.Web
```

$ARGUMENTS
