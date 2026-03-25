# Contract (Data Models and DTOs)

## Summary

This folder contains the shared data contracts, data transfer objects (DTOs), models, and interfaces used across all components of the Microsoft Assent Approvals Platform. The Contract project defines the "language" that all components use to communicate, ensuring consistency and type safety.

### Main Function/Purpose

The Contract project serves as the central definition for:

1. **Data Transfer Objects (DTOs)**: Models for API requests and responses
2. **Entity Models**: Representations of data stored in Azure Storage and Cosmos DB
3. **Interface Definitions**: Contracts for dependency injection and abstraction
4. **Enumerations**: Shared enum types for status codes, operation types, etc.
5. **Constants**: Shared constant values used across the platform
6. **Validators**: FluentValidation validators for input validation

### When/Why Developers Work With This Code

Developers work with the Contract project when:

- Adding new API endpoints (define request/response DTOs)
- Modifying data models (update entity classes)
- Adding new validation rules (create or update validators)
- Defining new service interfaces (add to interface definitions)
- Adding new enum types or constant values
- Ensuring data consistency across components

### Key Technologies

- **.NET 8.0** - Runtime framework
- **FluentValidation** - Model validation library
- **System.ComponentModel.DataAnnotations** - Attribute-based validation
- **Newtonsoft.Json** - JSON serialization attributes

## Folder Structure

```
Contract/
├── Approvals.Contracts/
│   ├── DataContracts/               # DTOs and data models
│   │   ├── ApprovalSummaryRow.cs    # Approval summary entity
│   │   ├── ApprovalDetailsEntity.cs # Approval details entity
│   │   ├── ApprovalRequest.cs       # Payload request model
│   │   ├── ApprovalResponse.cs      # API response model
│   │   ├── TenantInfo.cs            # Tenant configuration model
│   │   ├── UserDelegation.cs        # Delegation settings model
│   │   └── ... (many more)
│   │
│   ├── Validators/                  # FluentValidation validators
│   │   ├── ApprovalRequestValidator.cs
│   │   ├── ActionDetailValidator.cs
│   │   ├── ApproversValidator.cs
│   │   └── ... (more validators)
│   │
│   ├── Interfaces/                  # Service and provider interfaces
│   │   ├── IApprovalSummaryProvider.cs
│   │   ├── IApprovalDetailProvider.cs
│   │   ├── ITableHelper.cs
│   │   └── ... (many interfaces)
│   │
│   ├── Constants.cs                 # Shared constants
│   ├── Enums.cs                     # Enum definitions
│   ├── Extensions.cs                # Extension methods
│   └── Approvals.Contracts.csproj
```

## Components

### DataContracts

**Purpose**: Define the shape of data across the platform

**Key Categories**:

1. **API Request/Response Models**:
   - `ApprovalRequest`: Payload from tenant systems
   - `ApprovalSummaryResponse`: Summary list response
   - `ApprovalDetailsResponse`: Detailed approval response
   - `DocumentActionRequest`: Action request (approve/reject)

2. **Storage Entity Models**:
   - `ApprovalSummaryRow`: Entity for ApprovalSummary table
   - `ApprovalDetailsEntity`: Entity for ApprovalDetails table
   - `ApprovalTenantInfo`: Tenant configuration entity
   - `UserDelegationSetting`: Delegation configuration entity

3. **Business Models**:
   - `Approver`: Approver information
   - `ApprovalHierarchy`: Multi-level approval structure
   - `AdditionalData`: Custom attributes and metadata
   - `NotificationDetail`: Notification settings

4. **Configuration Models**:
   - `TenantInfo`: Tenant-specific configuration
   - `FlightingSetting`: Feature flag configuration
   - `ApplicationConfiguration`: App-wide settings

**Naming Convention**: 
- DTOs: `{EntityName}Request`, `{EntityName}Response`
- Entities: `{EntityName}Row`, `{EntityName}Entity`
- Business Models: `{EntityName}` (no suffix)

### Validators

**Purpose**: Validate input data using FluentValidation

**Key Validators**:
- `ApprovalRequestValidator`: Validates incoming approval payloads
- `ActionDetailValidator`: Validates approval action requests
- `ApproversValidator`: Validates approver list structure
- `ApprovalIdentifierValidator`: Validates document identifiers
- `ApprovalHierarchyValidator`: Validates approval hierarchy structure

**Validation Rules**:
- Required field validation
- Format validation (email, date, etc.)
- Range validation (min/max values)
- Business rule validation (e.g., approver can't be submitter)

**Usage in APIs**:
```csharp
var validator = new ApprovalRequestValidator();
var result = await validator.ValidateAsync(request);
if (!result.IsValid)
{
    return BadRequest(result.Errors);
}
```

### Interfaces

**Purpose**: Define contracts for dependency injection

**Key Interface Categories**:

1. **Data Providers**:
   - `IApprovalSummaryProvider`: Approval summary operations
   - `IApprovalDetailProvider`: Approval details operations
   - `IApprovalHistoryProvider`: History and audit queries
   - `IApprovalTenantInfoProvider`: Tenant configuration access

2. **Helper Interfaces**:
   - `ITableHelper`: Azure Table Storage operations
   - `IBlobStorageHelper`: Blob Storage operations
   - `ICosmosDbHelper`: Cosmos DB operations
   - `IServiceBusHelper`: Service Bus messaging

3. **Business Logic Interfaces**:
   - `ISummaryHelper`: Summary business logic
   - `IDocumentActionHelper`: Action processing logic
   - `IDelegationHelper`: Delegation management
   - `IAuthenticationHelper`: Authentication logic

4. **Factory Interfaces**:
   - `ITenantFactory`: Create tenant-specific instances
   - `IAuditFactory`: Create audit loggers
   - `IHistoryStorageFactory`: Create history storage providers

**Benefits of Interfaces**:
- Dependency injection and loose coupling
- Testability with mocking
- Ability to swap implementations
- Clear contracts between layers

### Enums

**Purpose**: Define enumeration types for type-safe constants

**Key Enums**:

- `ApprovalStatus`: Pending, Approved, Rejected, Cancelled
- `OperationType`: Create, Update, Delete, OutOfSyncDelete
- `ActionType`: Approve, Reject, Reassign, TakeAction
- `ApprovalTenantStatus`: Active, Inactive, Suspended
- `NotificationType`: ActionableEmail, PushNotification, None
- `ApprovalHierarchyLevel`: Level1, Level2, Level3
- `ConfigurationKey`: Keys for app configuration settings

**Usage**:
```csharp
if (approval.Status == ApprovalStatus.Pending)
{
    // Process approval
}
```

### Constants

**Purpose**: Define shared constant values

**Key Constants**:

- **Table Names**: `ApprovalSummary`, `ApprovalDetails`, `ApprovalTenantInfo`
- **Configuration Keys**: `StorageAccountName`, `CosmosDbEndPoint`, `ServiceBusNamespace`
- **Service Bus Topics**: `approvalsmaintopic`, `approvalsnotificationtopic`
- **HTTP Headers**: Custom header names for correlation tracking
- **Default Values**: Timeouts, retry counts, page sizes

**Usage**:
```csharp
var summaries = await tableHelper.QueryAsync(Constants.ApprovalSummaryTable, filter);
```

## Design Patterns

1. **Data Transfer Object (DTO)**: Separate models for API and storage layers
2. **Entity Pattern**: Storage entities represent table rows
3. **Interface Segregation**: Small, focused interfaces
4. **Validation Pattern**: FluentValidation for complex validation logic
5. **Constants Pattern**: Centralized constant definitions

## Dependencies

### External Dependencies

- **FluentValidation** (^11.x) - Validation framework
- **System.ComponentModel.DataAnnotations** - Attribute-based validation
- **Newtonsoft.Json** (^13.x) - JSON attributes for serialization
- **Azure.Data.Tables** (^12.x) - For ITableEntity interface

**Why These Dependencies?**

- **FluentValidation**: Provides expressive, testable validation rules
- **Newtonsoft.Json**: Flexible JSON serialization with attributes
- **Azure.Data.Tables**: Required for table entity types

### Internal Dependencies

**None** - The Contract project has no dependencies on other projects in the solution. This is intentional to avoid circular dependencies. All other projects reference the Contract project.

## Best Practices

### For Using Contracts

1. **Don't modify existing properties**: Breaking changes impact all components
2. **Add new properties carefully**: Consider backward compatibility
3. **Use validators**: Always validate input using provided validators
4. **Reference interfaces**: Depend on interfaces, not concrete implementations
5. **Use enums for type safety**: Prefer enums over magic strings

### For Adding New Contracts

1. **Follow naming conventions**: Request/Response suffixes, clear names
2. **Add XML documentation**: Document all public classes and properties
3. **Create validators**: Add FluentValidation validators for complex models
4. **Consider versioning**: Plan for API versioning if adding new DTOs
5. **Keep it clean**: No business logic in Contract project

### For Validation

1. **Use FluentValidation for complex rules**: Not just attributes
2. **Provide meaningful error messages**: Help developers understand failures
3. **Validate at API boundary**: Validate before entering business logic
4. **Don't duplicate validation**: Centralize rules in validators
5. **Test validators**: Write unit tests for validation rules

## Example: Adding a New DTO

```csharp
// 1. Create the DTO in DataContracts/
public class FeedbackRequest
{
    public string DocumentNumber { get; set; }
    public string Rating { get; set; }
    public string Comments { get; set; }
    public string UserAlias { get; set; }
}

// 2. Create a validator in Validators/
public class FeedbackRequestValidator : AbstractValidator<FeedbackRequest>
{
    public FeedbackRequestValidator()
    {
        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .WithMessage("DocumentNumber is required");
        
        RuleFor(x => x.Rating)
            .Must(r => new[] { "1", "2", "3", "4", "5" }.Contains(r))
            .WithMessage("Rating must be between 1 and 5");
        
        RuleFor(x => x.Comments)
            .MaximumLength(1000)
            .WithMessage("Comments cannot exceed 1000 characters");
    }
}

// 3. Use in API controller
[HttpPost]
public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackRequest request)
{
    var validator = new FeedbackRequestValidator();
    var validationResult = await validator.ValidateAsync(request);
    
    if (!validationResult.IsValid)
    {
        return BadRequest(validationResult.Errors);
    }
    
    // Process feedback...
}
```

## Testing

The Contract project itself doesn't require extensive testing (it's mostly data models), but:

1. **Validators should be tested**: Ensure validation rules work correctly
2. **Serialization should be tested**: Verify JSON round-tripping
3. **Interface compatibility**: Ensure implementations satisfy interfaces

## Related Documentation

- [APIs README](../APIs/README.md) - How APIs use contracts
- [Services README](../Services/README.md) - How processors use contracts
- [Common README](../Common/README.md) - How common libraries use contracts
- [High-Level Architecture](../../docs/architecture/HighLevelArchitecture.md) - Data model overview
- [Contributing Guidelines](../../CONTRIBUTING.md) - Development guidelines
