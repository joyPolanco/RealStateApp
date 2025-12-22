# Entity Summary - Real State App

## Folder Structure

```
RealStateApp.Core.Domain/
├── Common/
│   └── Enums/
│       ├── AppRoles.cs (ADMIN, DEVELOPER, CLIENT, AGENT)
│       ├── PropertyStatus.cs (Available, Sold)
│       └── OfferStatus.cs (Pending, Accepted, Rejected)
├── Entities/
│   ├── PropertyType.cs
│   ├── SaleType.cs
│   ├── Improvement.cs
│   ├── Property.cs
│   ├── PropertyImprovement.cs (junction table)
│   ├── PropertyPhoto.cs
│   ├── Offer.cs
│   ├── Message.cs
│   └── FavoriteProperty.cs
└── Interfaces/

RealStateApp.Infraestructure.Identity/
└── Entities/
    └── AppUser.cs (Inherits from IdentityUser)
```

## Domain Entities (Core.Domain/Entities)

### 1. PropertyType
- **Id**: int
- **Name**: string (required)
- **Description**: string (required)
- **Properties**: ICollection<Property>

### 2. SaleType
- **Id**: int
- **Name**: string (required)
- **Description**: string (required)
- **Properties**: ICollection<Property>

### 3. Improvement
- **Id**: int
- **Name**: string (required)
- **Description**: string (required)
- **PropertyImprovements**: ICollection<PropertyImprovement> (N:M relationship with Property)

### 4. Property
- **Id**: int
- **Code**: string (required) - Unique 6-digit code
- **Price**: decimal (in Dominican pesos)
- **SizeInMeters**: double
- **Bedrooms**: int
- **Bathrooms**: int
- **Description**: string (required)
- **Status**: PropertyStatus (Available by default)
- **PropertyTypeId**: int (FK)
- **SaleTypeId**: int (FK)
- **AgentId**: string (FK to AppUser)
- **Relationships**:
  - PropertyType
  - SaleType
  - Photos: ICollection<PropertyPhoto>
  - PropertyImprovements: ICollection<PropertyImprovement>
  - Offers: ICollection<Offer>
  - Messages: ICollection<Message>
  - FavoriteProperties: ICollection<FavoriteProperty>

### 5. PropertyImprovement (N:M Junction Table)
- **PropertyId**: int (FK)
- **ImprovementId**: int (FK)
- **Navigation**:
  - Property
  - Improvement

### 6. PropertyPhoto
- **Id**: int
- **ImageUrl**: string (required)
- **PropertyId**: int (FK)
- **Property**: Property

### 7. Offer
- **Id**: int
- **Amount**: decimal
- **Date**: DateTime
- **Status**: OfferStatus (Pending by default)
- **PropertyId**: int (FK)
- **ClientId**: string (FK to AppUser)
- **Property**: Property

### 8. Message (Chat)
- **Id**: int
- **Content**: string (required)
- **Date**: DateTime
- **PropertyId**: int (FK)
- **SenderUserId**: string (FK to AppUser)
- **ReceiverUserId**: string (FK to AppUser)
- **Property**: Property

### 9. FavoriteProperty
- **Id**: int
- **ClientId**: string (FK to AppUser)
- **PropertyId**: int (FK)
- **Property**: Property

## Identity Entity (Infrastructure.Identity/Entities)

### AppUser (Inherits from IdentityUser)

**Common properties for all roles:**
- **FirstName**: string (required)
- **LastName**: string (required)
- **IsActive**: bool (true by default)

**Specific for Client and Agent:**
- **Photo**: string? (URL or path)
- **PhoneNumber**: string?

**Specific for Administrator and Developer:**
- **IdCard**: string? (Cedula)

**Inherited from IdentityUser:**
- Id: string
- UserName: string
- Email: string
- PasswordHash: string
- etc.

## Enumerations (Core.Domain/Common/Enums)

### AppRoles
- ADMIN
- DEVELOPER
- CLIENT
- AGENT

### PropertyStatus
- Available
- Sold

### OfferStatus
- Pending
- Accepted
- Rejected

## Important Relationships

1. **Property ↔ PropertyType**: N:1
2. **Property ↔ SaleType**: N:1
3. **Property ↔ Improvement**: N:M (through PropertyImprovement)
4. **Property ↔ AppUser (Agent)**: N:1
5. **Property ↔ PropertyPhoto**: 1:N (1 to 4 photos)
6. **Property ↔ Offer**: 1:N
7. **Property ↔ Message**: 1:N
8. **Property ↔ FavoriteProperty**: 1:N
9. **AppUser (Client) ↔ Offer**: 1:N
10. **AppUser (Client) ↔ FavoriteProperty**: 1:N
11. **AppUser ↔ Message**: N:M (as sender and receiver)

## Important Notes

1. ✅ **Correct separation**: Domain entities in `Core.Domain/Entities`, Identity entities in `Infrastructure.Identity/Entities`
2. ✅ **Centralized enums**: In `Core.Domain/Common/Enums`
3. ✅ **Extended AppUser**: Includes all necessary properties for the 4 roles
4. ✅ **N:M relationships**: Explicit `PropertyImprovement` junction table
5. ✅ **Unique codes**: Property has a unique 6-digit code
6. ✅ **States**: Enums for property and offer status
7. ✅ **Nullable references**: Use of `required` and `?` as appropriate
8. ✅ **All in English**: Entities, properties, and file names in English

## Next Steps

1. Configure relationships in `DbContext` using Fluent API
2. Create Entity Framework migrations
3. Implement generic repositories
4. Create ViewModels and DTOs
5. Implement AutoMapper profiles
6. Develop application services
