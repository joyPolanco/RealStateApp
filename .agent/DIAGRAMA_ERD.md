# Diagrama de Relaciones de Entidades (ERD)

## Modelo de Datos - Real State App

```
┌─────────────────────────┐
│      AppUser            │
│  (Identity.Entities)    │
├─────────────────────────┤
│ Id (PK) string          │
│ UserName                │
│ Email                   │
│ Nombre (required)       │
│ Apellido (required)     │
│ EstaActivo              │
│ Foto (nullable)         │
│ Telefono (nullable)     │
│ Cedula (nullable)       │
└─────────────┬───────────┘
              │
              │ AgenteId (FK)
              │
┌─────────────▼───────────┐         ┌──────────────────┐
│      Propiedad          │────────→│  TipoPropiedad   │
├─────────────────────────┤ N:1     ├──────────────────┤
│ Id (PK)                 │         │ Id (PK)          │
│ Codigo (unique, 6 dig.) │         │ Nombre           │
│ Precio (decimal DOP)    │         │ Descripcion      │
│ TamanioMetros           │         └──────────────────┘
│ CantidadHabitaciones    │
│ CantidadBanos           │         ┌──────────────────┐
│ Descripcion             │────────→│   TipoVenta      │
│ Estado (enum)           │ N:1     ├──────────────────┤
│ TipoPropiedadId (FK)    │         │ Id (PK)          │
│ TipoVentaId (FK)        │         │ Nombre           │
│ AgenteId (FK)           │         │ Descripcion      │
└─────────────┬───────────┘         └──────────────────┘
              │
       ┌──────┼──────┬──────────┬────────────┐
       │      │      │          │            │
       │      │      │          │            │
   ┌───▼──┐ ┌▼────┐┌▼──────┐ ┌▼────────┐ ┌─▼──────────────┐
   │ Foto │ │Mejora││Oferta│ │Mensaje  │ │PropiedadFavorita│
   │Prop. │ │(N:M) ││       │ │         │ │                │
   └──────┘ └┬─────┘└───────┘ └─────────┘ └────────────────┘
            │
            │
   ┌────────▼─────────┐        ┌──────────────┐
   │ PropiedadMejora  │───────→│   Mejora     │
   │ (Join Table)     │ N:1    ├──────────────┤
   ├──────────────────┤        │ Id (PK)      │
   │ PropiedadId (FK) │        │ Nombre       │
   │ MejoraId (FK)    │        │ Descripcion  │
   └──────────────────┘        └──────────────┘

┌─────────────────────┐
│  FotoPropiedad      │
├─────────────────────┤
│ Id (PK)             │
│ UrlImagen           │
│ PropiedadId (FK)    │
└─────────────────────┘
        │
        └──→ 1:N con Propiedad (1 a 4 fotos)

┌─────────────────────┐
│     Oferta          │
├─────────────────────┤
│ Id (PK)             │
│ Monto (decimal)     │
│ Fecha (DateTime)    │
│ Estado (enum)       │
│ PropiedadId (FK)    │
│ ClienteId (FK)      │
└─────────────────────┘
        │
        └──→ ClienteId → AppUser (rol Cliente)

┌─────────────────────┐
│     Mensaje         │
│     (Chat)          │
├─────────────────────┤
│ Id (PK)             │
│ Contenido           │
│ Fecha (DateTime)    │
│ PropiedadId (FK)    │
│ UsuarioEmisorId (FK)│
│ UsuarioReceptorId(FK)│
└─────────────────────┘
        │
        ├──→ UsuarioEmisorId → AppUser
        └──→ UsuarioReceptorId → AppUser

┌─────────────────────┐
│ PropiedadFavorita   │
├─────────────────────┤
│ Id (PK)             │
│ ClienteId (FK)      │
│ PropiedadId (FK)    │
└─────────────────────┘
        │
        └──→ ClienteId → AppUser (rol Cliente)
```

## Enumeraciones

### EstadoPropiedad
```csharp
public enum EstadoPropiedad
{
    Disponible,  // Estado inicial por defecto
    Vendida      // Cuando se acepta una oferta
}
```

### EstadoOferta
```csharp
public enum EstadoOferta
{
    Pendiente,   // Estado inicial
    Aceptada,    // Oferta aceptada por el agente
    Rechazada    // Oferta rechazada por el agente
}
```

### AppRoles
```csharp
public enum AppRoles
{
    ADMIN,       // Administrador del sistema
    DEVELOPER,   // Desarrollador (acceso API)
    CLIENT,      // Cliente (puede hacer ofertas, favoritos)
    AGENT        // Agente inmobiliario (gestiona propiedades)
}
```

## Reglas de Negocio Importantes

### Propiedad
- El **Codigo** debe ser único y de 6 dígitos
- Una propiedad puede tener de **1 a 4 fotos**
- El **Precio** debe estar en pesos dominicanos (DOP)
- Solo las propiedades con estado **Disponible** se muestran en el home público
- Cuando se acepta una oferta, la propiedad pasa a estado **Vendida**

### Oferta
- Se crea en estado **Pendiente**
- Un cliente **no puede** crear una nueva oferta si:
  - Ya tiene una oferta **Pendiente** para esa propiedad
  - Ya existe una oferta **Aceptada** (de cualquier cliente) para esa propiedad
- Cuando un agente **acepta** una oferta:
  - Todas las demás ofertas pendientes se **rechazan automáticamente**
  - La propiedad pasa a estado **Vendida**

### Mensaje (Chat)
- Cada mensaje está asociado a una **propiedad específica**
- Los clientes pueden iniciar conversaciones con el agente de la propiedad
- El agente puede responder a múltiples clientes por cada propiedad

### AppUser
- **Cliente/Agente**: Necesitan Foto y Teléfono
- **Administrador/Desarrollador**: Necesitan Cédula
- Los **clientes** se registran y deben activar su cuenta por email
- Los **agentes** se registran pero un administrador debe activarlos
- Los **administradores** y **desarrolladores** se crean activos por defecto

### Activación de Usuarios
- **Cliente**: Se crea inactivo, se envía email de confirmación
- **Agente**: Se crea inactivo, el administrador lo activa manualmente
- **Admin/Developer**: Se crean activos por defecto

## Integridad Referencial

### Cascadas
- Al eliminar un **TipoPropiedad** → Se eliminan todas sus **Propiedades**
- Al eliminar un **TipoVenta** → Se eliminan todas sus **Propiedades**
- Al eliminar un **Agente** → Se eliminan todas sus **Propiedades**
- Al eliminar una **Propiedad** → Se eliminan:
  - Todas sus **FotoPropiedad**
  - Todas sus **Ofertas**
  - Todas sus **PropiedadMejora**
  - Todos sus **Mensajes**
  - Todas sus **PropiedadFavorita**

### Restricciones
- Al eliminar una **Mejora** → Solo se elimina si no tiene propiedades asociadas
- Un **Cliente** no puede ser eliminado si tiene ofertas activas
