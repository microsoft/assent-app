# Common Libraries

## Summary

This folder contains shared libraries and common infrastructure code used across all components of the Microsoft Assent Approvals Platform. These libraries provide reusable abstractions for data access, messaging, logging, and utility functions, promoting code reuse and consistency.

### Main Function/Purpose

The common libraries provide:

1. **Data Access Abstraction**: Simplified interfaces for Azure Storage Tables, Blobs, and Cosmos DB
2. **Messaging Infrastructure**: Azure Service Bus integration for publishing and consuming messages
3. **Logging and Telemetry**: Centralized logging with OpenTelemetry and Application Insights
4. **Extension Methods**: Common extension methods for collections, strings, and other types
5. **Utility Functions**: Helper classes for common operations

### When/Why Developers Work With This Code

Developers work with these libraries when:

- Adding new data access patterns or storage abstractions
- Implementing new messaging patterns or Service Bus integrations
- Enhancing logging capabilities or adding custom telemetry
- Creating reusable utility functions
- Standardizing common operations across the platform

### Key Technologies

- **.NET 8.0** - Runtime framework
- **Azure SDK for .NET** - Azure service integrations
- **OpenTelemetry** - Observability and distributed tracing
- **Application Insights SDK** - Monitoring and telemetry

## Folder Structure

```
Common/
├── Approvals.Data.Azure.CosmosDb/           # Cosmos DB data access
│   ├── Helpers/
│   │   └── CosmosDbHelper.cs                # Cosmos DB operations wrapper
│   └── Approvals.Data.Azure.CosmosDb.csproj
│
├── Approvals.Data.Azure.Storage/            # Azure Storage (Tables & Blobs) access
│   ├── Helpers/
│   │   ├── TableHelper.cs                   # Table storage operations
│   │   └── BlobStorageHelper.cs             # Blob storage operations
│   └── Approvals.Data.Azure.Storage.csproj
│
├── Approvals.Messaging.Azure.ServiceBus/    # Service Bus messaging
│   ├── Helpers/
│   │   └── ServiceBusHelper.cs              # Service Bus operations
│   └── Approvals.Messaging.Azure.ServiceBus.csproj
│
├── Approvals.LogManager/                    # Centralized logging
│   ├── LogProvider.cs                       # Main logging provider
│   ├── PerformanceLogger.cs                 # Performance metrics logging
│   └── Approvals.LogManager.csproj
│
├── Approvals.Extensions/                    # Extension methods
│   ├── StringExtensions.cs
│   ├── CollectionExtensions.cs
│   └── Approvals.Extensions.csproj
│
└── Approvals.Utilities/                     # Utility classes
    ├── JSONHelper.cs                        # JSON operations
    ├── DateTimeHelper.cs                    # Date/time utilities
    └── Approvals.Utilities.csproj
```

## Components

### Data.Azure.CosmosDb

**Purpose**: Provides a simplified interface for Azure Cosmos DB operations

**Key Classes**:
- `CosmosDbHelper`: Wrapper for Cosmos DB SDK operations
- `ICosmosDbHelper`: Interface for dependency injection

**Capabilities**:
- Create, read, update, delete documents
- Query documents with LINQ or SQL
- Bulk operations for performance
- Automatic retry logic for throttling

**Usage Example**:
```csharp
var document = await cosmosDbHelper.GetItemAsync<ApprovalDocument>(
    database: "history",
    container: "approvals",
    id: documentId,
    partitionKey: tenantId
);
```

### Data.Azure.Storage

**Purpose**: Abstracts Azure Storage Table and Blob operations

**Key Classes**:
- `TableHelper`: Azure Table Storage operations
- `BlobStorageHelper`: Azure Blob Storage operations
- `ITableHelper`, `IBlobStorageHelper`: Interfaces for DI

**Table Storage Capabilities**:
- CRUD operations on table entities
- Batch operations (up to 100 entities)
- Query with filters and pagination
- Partition key and row key management

**Blob Storage Capabilities**:
- Upload, download, delete blobs
- List blobs with prefix filtering
- Manage blob metadata
- Generate SAS tokens for secure access

**Usage Example**:
```csharp
// Table operations
await tableHelper.InsertOrReplace("ApprovalSummary", entity);

// Blob operations
await blobStorageHelper.UploadAsync(containerName, blobName, stream);
```

### Messaging.Azure.ServiceBus

**Purpose**: Simplifies Azure Service Bus messaging operations

**Key Classes**:
- `ServiceBusHelper`: Send and receive messages
- `IServiceBusHelper`: Interface for DI

**Capabilities**:
- Publish messages to topics
- Send messages to queues
- Receive and process messages
- Handle dead letter queues
- Message scheduling and deferred messages

**Usage Example**:
```csharp
await serviceBusHelper.SendMessageAsync(
    topicName: "approvalsmaintopic",
    message: payloadMessage
);
```

### LogManager

**Purpose**: Centralized logging with Application Insights integration

**Key Classes**:
- `LogProvider`: Main logging interface
- `PerformanceLogger`: Performance and timing metrics
- `ILogProvider`, `IPerformanceLogger`: Interfaces for DI

**Logging Capabilities**:
- Structured logging with severity levels (Info, Warning, Error, Critical)
- Correlation vector (Xcv) tracking for distributed tracing
- Custom properties and dimensions
- Exception logging with stack traces
- Performance timing and metrics

**Features**:
- Automatic Application Insights integration
- OpenTelemetry compatibility
- Configurable log levels
- Context-aware logging (user, tenant, document ID)

**Usage Example**:
```csharp
logProvider.LogInformation(
    area: "ApprovalProcessing",
    message: "Processing approval",
    xcv: correlationVector,
    customProperties: new Dictionary<string, string> {
        { "TenantId", tenantId },
        { "DocumentNumber", documentNumber }
    }
);
```

### Extensions

**Purpose**: Common extension methods for framework types

**Extension Categories**:
- **String Extensions**: IsNullOrEmpty, ToTitleCase, Truncate, etc.
- **Collection Extensions**: ForEach, AddRange, RemoveAll, etc.
- **DateTime Extensions**: ToUnixTimestamp, IsBusinessDay, etc.
- **Enum Extensions**: GetDescription, GetAttribute, etc.

**Benefits**:
- Cleaner, more readable code
- Consistent behavior across the platform
- Avoid code duplication

**Usage Example**:
```csharp
if (alias.IsNotNullOrWhiteSpace())
{
    var formatted = alias.ToLowerInvariant().Trim();
}
```

### Utilities

**Purpose**: Helper classes for common operations

**Key Utilities**:
- `JSONHelper`: JSON serialization/deserialization with custom settings
- `DateTimeHelper`: Date/time manipulation and formatting
- `ValidationHelper`: Common validation logic
- `EncryptionHelper`: Encryption/decryption utilities
- `ConfigurationHelper`: Configuration parsing

**Usage Example**:
```csharp
var json = JSONHelper.Serialize(approvalData, prettyPrint: true);
var obj = JSONHelper.Deserialize<ApprovalSummary>(jsonString);
```

## Dependencies

### Internal Dependencies

These libraries have minimal internal dependencies, primarily referencing:
- **Approvals.Contracts**: For data model definitions

### External Dependencies

#### Approvals.Data.Azure.CosmosDb
- **Microsoft.Azure.Cosmos** (^3.x) - Cosmos DB SDK

#### Approvals.Data.Azure.Storage
- **Azure.Data.Tables** (^12.x) - Table Storage SDK
- **Azure.Storage.Blobs** (^12.x) - Blob Storage SDK
- **Azure.Identity** (^1.x) - Managed identity authentication

#### Approvals.Messaging.Azure.ServiceBus
- **Azure.Messaging.ServiceBus** (^7.x) - Service Bus SDK

#### Approvals.LogManager
- **Microsoft.ApplicationInsights** (^2.x) - Application Insights SDK
- **System.Diagnostics.DiagnosticSource** (^7.x) - OpenTelemetry tracing

#### Approvals.Extensions
- No external dependencies (pure .NET)

#### Approvals.Utilities
- **Newtonsoft.Json** (^13.x) - JSON serialization
- **System.Text.Json** (^7.x) - Alternative JSON serialization

## Design Patterns

1. **Dependency Injection**: All helpers expose interfaces for DI
2. **Wrapper Pattern**: Wraps Azure SDK complexity with simpler APIs
3. **Extension Methods**: Add functionality to existing types without modification
4. **Singleton Pattern**: LogProvider and helpers often registered as singletons
5. **Factory Pattern**: Some helpers use factory methods for initialization

## Best Practices

### For Using Common Libraries

1. **Always use interfaces**: Inject `ITableHelper`, not `TableHelper`
2. **Reuse helper instances**: Helpers are typically singletons or scoped
3. **Use extension methods**: Leverage extensions for cleaner code
4. **Log with context**: Always include Xcv and relevant properties
5. **Handle exceptions**: Common libraries may throw - handle appropriately

### For Contributing to Common Libraries

1. **Keep libraries focused**: Each library has a single responsibility
2. **Maintain interfaces**: Don't break existing interfaces
3. **Add tests**: All common code should have unit tests
4. **Document public APIs**: XML comments for all public methods
5. **Avoid dependencies**: Minimize dependencies between common libraries

## Testing

Common libraries have corresponding unit test projects:
- `Approvals.Common.BL.UnitTests` - Tests for business logic helpers
- `Approvals.Common.DL.UnitTests` - Tests for data access helpers

**Test Coverage Expectations**: 80%+ for common libraries (heavily used code)

## Related Documentation

- [APIs README](../APIs/README.md) - How APIs use these libraries
- [Services README](../Services/README.md) - How processors use these libraries
- [High-Level Architecture](../../docs/architecture/HighLevelArchitecture.md) - System architecture
- [Contributing Guidelines](../../CONTRIBUTING.md) - Development guidelines
