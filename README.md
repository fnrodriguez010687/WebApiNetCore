# API REST para Gestión de Gateways y Dispositivos Periféricos

API desarrollada con ASP.NET Core 6 (C#) y SQL Server para registrar usuarios, autenticarse con JWT y administrar gateways junto con sus dispositivos periféricos.

## Requisitos

- .NET SDK 6
- SQL Server (Express local o instancia compatible)
- Una conexión local o remota a SQL Server
- EF Core CLI: `dotnet tool install --global dotnet-ef` (si aún no está instalado)

## Configuración de SQL Server

No se debe guardar secretos (cadena de conexión con credenciales ni clave JWT) en `appsettings.json` ni subirlos al repositorio.

El ejemplo de desarrollo usa autenticación de Windows:

```text
Server=localhost\SQLEXPRESS; Database=dotnet-GatewayDevice; Trusted_Connection=true;
```

Para desarrollo local, configure la cadena de conexión mediante una variable de entorno:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS; Database=dotnet-GatewayDevice; Trusted_Connection=true;"
$env:AppSettings__Token = "CLAVE_JWT_LARGA_Y_SECRETA"
```

También puede usar User Secrets:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS; Database=dotnet-GatewayDevice; Trusted_Connection=true;"
dotnet user-secrets set "AppSettings:Token" "CLAVE_JWT_LARGA_Y_SECRETA"
```

La base `dotnet-GatewayDevice` debe existir o debe crearse previamente. Ejemplo con `sqlcmd`:

```powershell
sqlcmd -S "localhost\SQLEXPRESS" -Q "CREATE DATABASE [dotnet-GatewayDevice];"
```

La clave JWT debe tener longitud suficiente para HMAC-SHA512. El valor de ejemplo del repositorio no debe usarse fuera de un entorno local controlado.

## Ejecutar

```powershell
dotnet restore
dotnet build
dotnet run
```

Este proyecto no aplica migraciones automáticamente al iniciar. Hay que crear y aplicar el esquema con EF Core antes de usar la API.

Por defecto, el perfil `WebApiNetCore` usa `http://localhost:5098` y abre Swagger en `/swagger`. La URL HTTPS o IIS Express puede variar según `Properties/launchSettings.json`.

Los endpoints de Gateway y PeripheralDevice requieren el encabezado:

```text
Authorization: Bearer <token>
```

Swagger no está configurado con esquema Bearer; el token se obtiene en `POST /Auth/Login` y se envía de forma manual.

## Migraciones

El repositorio no incluye carpeta `Migrations`. Crear el esquema inicial:

```powershell
dotnet ef migrations add InitialCreate
```

Aplicar migraciones:

```powershell
dotnet ef database update
```

Crear una migración posterior:

```powershell
dotnet ef migrations add NombreDeLaMigracion
```

## Modelo de usuario

| Campo | Tipo | Reglas |
|---|---|---|
| `id` | entero | Generado por SQL Server |
| `userName` | texto | Obligatorio, único (comparación sin distinción de mayúsculas) |
| `passwordHash` | binario | Generado al registrar; no se expone en respuestas |
| `passwordSalt` | binario | Generado al registrar; no se expone en respuestas |

El registro recibe `username` y `password`. El login devuelve un JWT con vigencia de un día y claims de identificador y nombre.

## Modelo de gateway

| Campo | Tipo | Reglas |
|---|---|---|
| `serialNumber` | texto | Clave primaria; GUID generado en el servidor |
| `human_ReadAble_Name` | texto | Nombre visible del gateway |
| `ipv4_Address` | texto | Obligatorio; debe coincidir con una IPv4 válida |
| `user` | relación | El gateway pertenece al usuario autenticado |
| `devices` | lista | Dispositivos periféricos asociados |

La IPv4 se valida con expresión regular en el servicio. El alta usa una comprobación más estricta (octetos 0–255). La actualización usa un patrón más permisivo.

`GET /Gateway` no carga `Devices`. `GET /Gateway/{serialnumber}` sí incluye los dispositivos del gateway.

## Modelo de dispositivo periférico

| Campo | Tipo | Reglas |
|---|---|---|
| `uId` | entero | Generado por SQL Server |
| `vendor` | texto | Obligatorio en alta y actualización |
| `dateCreated` | fecha y hora | Asignado en el servidor al crear |
| `gatewayId` | texto | Número de serie del gateway dueño |
| `status` | entero | Obligatorio: `1` o `2` |

Estados:

| Valor | Nombre |
|---:|---|
| `1` | online |
| `2` | offline |

Las respuestas de dispositivo incluyen `status` como enum (`online` / `offline`) y el `gatewayId` asociado.

## Endpoints

| Método | Ruta | Resultado |
|---|---|---|
| `POST` | `/Auth/Register` | Crea un usuario |
| `POST` | `/Auth/Login` | Devuelve un JWT |
| `GET` | `/Gateway` | Lista gateways del usuario autenticado |
| `GET` | `/Gateway/{serialnumber}` | Gateway por número de serie, con dispositivos |
| `POST` | `/Gateway` | Crea un gateway |
| `PUT` | `/Gateway` | Actualiza nombre e IPv4 |
| `DELETE` | `/Gateway/{serialnumber}` | Elimina un gateway |
| `GET` | `/PeripheralDevice/{uid}` | Dispositivo por identificador |
| `GET` | `/PeripheralDevice/getall/{serialnumber}` | Dispositivos de un gateway |
| `POST` | `/PeripheralDevice` | Crea un dispositivo |
| `PUT` | `/PeripheralDevice` | Actualiza un dispositivo |
| `DELETE` | `/PeripheralDevice/{uid}` | Elimina un dispositivo |

Ejemplo de autenticación:

```json
POST /Auth/Register
{
  "username": "ana",
  "password": "ClaveSegura"
}

POST /Auth/Login
{
  "username": "ana",
  "password": "ClaveSegura"
}
```

Ejemplo de creación de gateway:

```json
POST /Gateway
{
  "human_ReadAble_Name": "Gateway oficina",
  "ipv4_Address": "192.168.1.10"
}
```

Ejemplo de creación de dispositivo:

```json
POST /PeripheralDevice
{
  "vendor": "Cisco",
  "gatewayId": "serial-del-gateway",
  "status": "1"
}
```

El cuerpo de las respuestas de negocio usa `ServiceResponse<T>`:

```json
{
  "data": {},
  "success": true,
  "message": null
}
```

Códigos HTTP principales:

- `200 OK`: operación procesada (también se usa cuando `success` es `false` en varios métodos de alta y consulta).
- `400 Bad Request`: registro o login rechazados (`usuario existente`, `usuario no encontrado`, `contraseña incorrecta`).
- `401 Unauthorized`: falta de JWT o token inválido en Gateway y PeripheralDevice.
- `404 Not Found`: actualización o eliminación cuando `data` es nulo.

## Arquitectura y patrones

El proyecto usa una separación por carpetas dentro de un único proyecto, manteniendo una estructura simple:

- `Models`: entidades `User`, `Gateway`, `PeripheralDevice`, enum `StatusDevice` y envoltorio `ServiceResponse<T>`.
- `Dtos`: contratos de entrada y salida para usuario, gateway y dispositivo.
- `Data`: `DataContext` (EF Core) e `IAuthRepository` / `AuthRepository`.
- `Services`: `IGatewayService`, `IPeripheralDeviceService` y sus implementaciones.
- `Controllers`: endpoints REST con `[ApiController]`; Gateway y PeripheralDevice van con `[Authorize]`.

### Servicio de aplicación y repositorio

Los controladores no acceden al `DbContext`. El CRUD de gateways y dispositivos pasa por servicios inyectados. La autenticación usa un repositorio (`IAuthRepository`) que genera el hash de contraseña y el JWT.

AutoMapper convierte entidades a DTOs (`AutoMapperProfile`). El usuario autenticado se obtiene de `IHttpContextAccessor` mediante el claim `NameIdentifier`.

### SOLID

- Responsabilidad única: controladores HTTP, servicios de dominio y repositorio de autenticación tienen responsabilidades separadas.
- Abierto/cerrado: nuevos campos o reglas pueden incorporarse en servicios y DTOs sin cambiar la forma de mapear controladores.
- Sustitución de Liskov e inversión de dependencias: controladores dependen de `IGatewayService`, `IPeripheralDeviceService` e `IAuthRepository`.
- Segregación de interfaces: cada servicio expone las operaciones de su agregado, no una interfaz general.

No hay Unit of Work explícito: EF Core administra el seguimiento y `SaveChangesAsync()` dentro de cada servicio.

## Validación y errores

La API valida IPv4 en el servicio de gateways. Si la dirección no cumple el patrón, responde con `success: false` y el mensaje `IPv4 Wrong`.

Alta de dispositivo:

- El gateway debe existir y pertenecer al usuario autenticado.
- Si no existe, el mensaje es `Gateway not found.`.
- `status` se convierte a entero y se asigna al enum `StatusDevice`.

Autenticación:

- Usuario duplicado: `User already exists.`
- Login: `User not found.` o `Wrong password.`

Los estados de dispositivo se representan como números explícitos (`1`, `2`) para que el cliente no dependa solo del nombre:

- No se documentan valores fuera de ese rango.
- Un valor no definido se persiste según la conversión a entero del enum.

Limitaciones actuales de validación:

- No hay tope de dispositivos por gateway.
- `Get` de recursos inexistentes suele devolver `200` con `data: null`.
- `UpdateGateway` y `UpdateDevice` pueden lanzar excepción si el identificador no existe (acceso a navegación nula).
- `DeleteDevice` localiza por `uId` sin filtrar primero por usuario.
- `UpdateDevice` puede reasignar el dispositivo a otro `gatewayId` sin comprobar que el gateway destino pertenezca al mismo usuario.

## Pruebas

El repositorio no incluye proyecto de pruebas.

Ejecutar todas las pruebas (cuando existan):

```powershell
dotnet test
```

Casos recomendados para una primera batería:

Pruebas unitarias:

- Rechazo de IPv4 inválida en alta y actualización.
- Rechazo de alta de dispositivo cuando el gateway no pertenece al usuario.
- Rechazo de usuario duplicado en registro.

Pruebas de integración:

- `POST /Auth/Register` y `POST /Auth/Login` devuelven `200` y un token.
- Acceso a `/Gateway` sin token devuelve `401`.
- `POST /Gateway` con IPv4 inválida no persiste el registro.
- Identificador de gateway inexistente en actualización o borrado.

Las integraciones deberían usar `WebApplicationFactory` y SQL Server local (o un contenedor) con una base separada, por ejemplo `dotnet-GatewayDevice-tests`.

## Decisiones técnicas

- Controladores REST por el tamaño del proyecto y el uso de `[Authorize]` por recurso.
- Un único proyecto con capas lógicas (`Models`, `Dtos`, `Data`, `Services`, `Controllers`).
- SQL Server mediante EF Core.
- JWT Bearer para proteger gateways y dispositivos; registro y login públicos.
- Contraseñas con HMAC-SHA512, hash y salt; nunca se devuelve el hash.
- Número de serie del gateway generado en servidor (GUID).
- DTOs separados de entidades para no exponer `User` ni credenciales.
- Envoltorio `ServiceResponse<T>` con `data`, `success` y `message`.
- Swagger habilitado solo en Development.

## Mejoras con más tiempo

- Limitar a 10 dispositivos periféricos por gateway.
- Unificar la validación de IPv4 (misma regla en alta y actualización, o `IPAddress.TryParse`).
- Devolver `400`/`404` reales cuando `success` es `false`, en lugar de `200`.
- Comprobar pertenencia del usuario en todas las operaciones de dispositivo (evitar IDOR).
- Configurar Swagger con esquema Bearer.
- Mover cadena de conexión y clave JWT a User Secrets o variables de entorno.
- Validar issuer y audience del token; usar una clave JWT de longitud adecuada.
- Añadir migraciones al repositorio y, si aplica, aplicarlas de forma controlada al iniciar.
- Eliminar `WeatherForecast` de la plantilla.
- Definir un formato consistente de error (por ejemplo RFC 7807 `ProblemDetails`).
- Incorporar logging estructurado y trazas OpenTelemetry.
- Añadir Docker Compose para API y SQL Server.
- Crear pruebas unitarias e integración y automatizarlas en CI/CD.

## Uso de inteligencia artificial

La IA se utilizó como asistente de análisis y documentación, no como reemplazo de validación manual del código existente.

### Solicitudes realizadas

- Analizar la estructura, endpoints, autenticación y reglas de la API.
- Contrastar controladores, servicios, DTOs, `DataContext` y configuración.
- Redactar este README siguiendo la estructura de `Gestor.Tareas.Api.Rest/Readme.md`.

### Cambios aceptados

- Documentar el stack real: ASP.NET Core 6, SQL Server, JWT, AutoMapper y controladores.
- Describir el modelo de usuario, gateway y dispositivo tal como está implementado.
- Incluir limitaciones conocidas (IPv4 inconsistente, códigos HTTP, autorización de dispositivos, ausencia de migraciones y pruebas).
- Mantener las mismas secciones del README de referencia: requisitos, ejecución, migraciones, modelo, endpoints, arquitectura, validación, pruebas, decisiones, mejoras y uso de IA.

### Correcciones realizadas en la documentación

- Se evitó copiar PostgreSQL, Minimal API y migraciones automáticas del proyecto de tareas; este repositorio usa SQL Server, controladores y no trae migraciones.
- Se documentó Swagger en `http://localhost:5098/swagger` según `launchSettings.json`.
- Se describió `ServiceResponse<T>` y el uso de JWT, que no existen en el README de referencia.
- Se listaron riesgos de autorización y nulos observados en `GatewayService` y `PeripheralDeviceService`.

No se aceptaron descripciones de IA sin revisar el código del repositorio. El README refleja el comportamiento actual, no un diseño deseado.
