IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [HotelId] uniqueidentifier NULL,
        [EntityType] nvarchar(450) NOT NULL,
        [EntityId] nvarchar(450) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [DetailsJson] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [CancellationPolicies] (
        [Id] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_CancellationPolicies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [Companies] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Gstin] nvarchar(15) NOT NULL,
        [StateCode] nvarchar(max) NOT NULL,
        [BillingAddress] nvarchar(max) NOT NULL,
        [CreditLimit] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [Folios] (
        [Id] uniqueidentifier NOT NULL,
        [ReservationId] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [OwnerCompanyId] uniqueidentifier NULL,
        [Status] int NOT NULL,
        [Balance] decimal(18,2) NOT NULL,
        [OpenedAtUtc] datetime2 NOT NULL,
        [ClosedAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Folios] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [FolioTransfers] (
        [Id] uniqueidentifier NOT NULL,
        [FromFolioId] uniqueidentifier NOT NULL,
        [ToFolioId] uniqueidentifier NOT NULL,
        [SourceReversalTransactionId] uniqueidentifier NOT NULL,
        [DestinationTransactionId] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [TransferredByUserId] uniqueidentifier NOT NULL,
        [TransferredAtUtc] datetime2 NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_FolioTransfers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [GstRules] (
        [Id] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NULL,
        [ChargeCategory] int NOT NULL,
        [HsnSac] nvarchar(20) NOT NULL,
        [MinAmount] decimal(18,2) NULL,
        [MaxAmount] decimal(18,2) NULL,
        [CgstRate] decimal(18,2) NOT NULL,
        [SgstRate] decimal(18,2) NOT NULL,
        [IgstRate] decimal(18,2) NOT NULL,
        [EffectiveFrom] date NOT NULL,
        [EffectiveTo] date NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_GstRules] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [Guests] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [IdProofType] nvarchar(max) NOT NULL,
        [IdProofNumber] nvarchar(max) NOT NULL,
        [AddressLine1] nvarchar(max) NOT NULL,
        [City] nvarchar(max) NOT NULL,
        [StateCode] nvarchar(max) NOT NULL,
        [Pincode] nvarchar(max) NOT NULL,
        [Gstin] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Guests] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [HotelGroups] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_HotelGroups] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [InventoryHolds] (
        [Id] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [RoomTypeId] uniqueidentifier NOT NULL,
        [RatePlanId] uniqueidentifier NOT NULL,
        [CheckInDate] date NOT NULL,
        [CheckOutDate] date NOT NULL,
        [RoomsRequested] int NOT NULL,
        [Adults] int NOT NULL,
        [Children] int NOT NULL,
        [Status] int NOT NULL,
        [Source] int NOT NULL,
        [ExpiresAtUtc] datetime2 NOT NULL,
        [IdempotencyKey] nvarchar(100) NOT NULL,
        [GuestId] uniqueidentifier NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [ReservationId] uniqueidentifier NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_InventoryHolds] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [Id] uniqueidentifier NOT NULL,
        [ReservationId] uniqueidentifier NOT NULL,
        [FolioId] uniqueidentifier NOT NULL,
        [InvoiceNumber] nvarchar(30) NOT NULL,
        [InvoiceDate] date NOT NULL,
        [SupplierGstin] nvarchar(max) NOT NULL,
        [SupplierStateCode] nvarchar(max) NOT NULL,
        [BilledPartyName] nvarchar(max) NULL,
        [BilledPartyGstin] nvarchar(max) NULL,
        [BilledPartyStateCode] nvarchar(max) NOT NULL,
        [PlaceOfSupplyStateCode] nvarchar(max) NOT NULL,
        [IsInterState] bit NOT NULL,
        [TotalTaxableValue] decimal(18,2) NOT NULL,
        [TotalCgst] decimal(18,2) NOT NULL,
        [TotalSgst] decimal(18,2) NOT NULL,
        [TotalIgst] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [NightAuditRuns] (
        [Id] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [BusinessDate] date NOT NULL,
        [Status] int NOT NULL,
        [StartedAtUtc] datetime2 NOT NULL,
        [CompletedAtUtc] datetime2 NULL,
        [TotalRoomRevenuePosted] decimal(18,2) NOT NULL,
        [TotalTaxPosted] decimal(18,2) NOT NULL,
        [StaysProcessed] int NOT NULL,
        [NoShowCount] int NOT NULL,
        [ExceptionCount] int NOT NULL,
        [TriggeredByUserId] uniqueidentifier NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_NightAuditRuns] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] uniqueidentifier NOT NULL,
        [FolioId] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Method] int NOT NULL,
        [Status] int NOT NULL,
        [GatewayReference] nvarchar(max) NULL,
        [IdempotencyKey] nvarchar(450) NULL,
        [RecordedByUserId] uniqueidentifier NULL,
        [FolioTransactionId] uniqueidentifier NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [PosCharges] (
        [Id] uniqueidentifier NOT NULL,
        [PosOutletId] uniqueidentifier NOT NULL,
        [PosReferenceNumber] nvarchar(max) NOT NULL,
        [IdempotencyKey] nvarchar(150) NOT NULL,
        [RoomId] uniqueidentifier NOT NULL,
        [ReservationId] uniqueidentifier NOT NULL,
        [FolioId] uniqueidentifier NOT NULL,
        [FolioTransactionId] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_PosCharges] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [PosOutlets] (
        [Id] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [ApiKeyHash] nvarchar(max) NOT NULL,
        [DefaultCreditLimit] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_PosOutlets] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [TokenHash] nvarchar(200) NOT NULL,
        [ExpiresAtUtc] datetime2 NOT NULL,
        [RevokedAtUtc] datetime2 NULL,
        [ReplacedByTokenHash] nvarchar(max) NULL,
        [CreatedByIp] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [Reservations] (
        [Id] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [ReservationNumber] nvarchar(30) NOT NULL,
        [GuestId] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [TravelAgentId] uniqueidentifier NULL,
        [RoomTypeId] uniqueidentifier NOT NULL,
        [RatePlanId] uniqueidentifier NOT NULL,
        [CheckInDate] date NOT NULL,
        [CheckOutDate] date NOT NULL,
        [RoomsBooked] int NOT NULL,
        [Adults] int NOT NULL,
        [Children] int NOT NULL,
        [Status] int NOT NULL,
        [Source] int NOT NULL,
        [InventoryHoldId] uniqueidentifier NULL,
        [IdempotencyKey] nvarchar(100) NOT NULL,
        [BusinessDateCreated] date NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [BillRoomChargeToCompany] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Reservations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [TravelAgents] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Gstin] nvarchar(max) NOT NULL,
        [StateCode] nvarchar(max) NOT NULL,
        [CommissionPercent] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_TravelAgents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [UserHotelAccesses] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_UserHotelAccesses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [CancellationPolicyRules] (
        [Id] uniqueidentifier NOT NULL,
        [CancellationPolicyId] uniqueidentifier NOT NULL,
        [HoursBeforeCheckInMin] int NULL,
        [HoursBeforeCheckInMax] int NULL,
        [PenaltyType] int NOT NULL,
        [PenaltyValue] decimal(18,2) NULL,
        [AppliesToNoShow] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_CancellationPolicyRules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CancellationPolicyRules_CancellationPolicies_CancellationPolicyId] FOREIGN KEY ([CancellationPolicyId]) REFERENCES [CancellationPolicies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [RatePlans] (
        [Id] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [MealPlan] int NOT NULL,
        [Scope] int NOT NULL,
        [CancellationPolicyId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_RatePlans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RatePlans_CancellationPolicies_CancellationPolicyId] FOREIGN KEY ([CancellationPolicyId]) REFERENCES [CancellationPolicies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [FolioTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [FolioId] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [GstAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [ReversalOfTransactionId] uniqueidentifier NULL,
        [BusinessDate] date NOT NULL,
        [PostedByUserId] uniqueidentifier NULL,
        [SourceReference] nvarchar(max) NULL,
        [UniquePostingKey] nvarchar(450) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_FolioTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FolioTransactions_Folios_FolioId] FOREIGN KEY ([FolioId]) REFERENCES [Folios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [Hotels] (
        [Id] uniqueidentifier NOT NULL,
        [HotelGroupId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [AddressLine1] nvarchar(max) NOT NULL,
        [AddressLine2] nvarchar(max) NOT NULL,
        [City] nvarchar(max) NOT NULL,
        [Pincode] nvarchar(max) NOT NULL,
        [StateCode] nvarchar(2) NOT NULL,
        [StateName] nvarchar(max) NOT NULL,
        [Gstin] nvarchar(15) NOT NULL,
        [TimeZoneId] nvarchar(64) NOT NULL,
        [BusinessDate] date NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Hotels] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Hotels_HotelGroups_HotelGroupId] FOREIGN KEY ([HotelGroupId]) REFERENCES [HotelGroups] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [InvoiceLines] (
        [Id] uniqueidentifier NOT NULL,
        [InvoiceId] uniqueidentifier NOT NULL,
        [FolioTransactionId] uniqueidentifier NULL,
        [Description] nvarchar(max) NOT NULL,
        [HsnSac] nvarchar(max) NOT NULL,
        [TaxableValue] decimal(18,2) NOT NULL,
        [CgstRate] decimal(18,2) NOT NULL,
        [CgstAmount] decimal(18,2) NOT NULL,
        [SgstRate] decimal(18,2) NOT NULL,
        [SgstAmount] decimal(18,2) NOT NULL,
        [IgstRate] decimal(18,2) NOT NULL,
        [IgstAmount] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_InvoiceLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InvoiceLines_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [NightAuditExceptions] (
        [Id] uniqueidentifier NOT NULL,
        [NightAuditRunId] uniqueidentifier NOT NULL,
        [ReservationId] uniqueidentifier NULL,
        [ExceptionType] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_NightAuditExceptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NightAuditExceptions_NightAuditRuns_NightAuditRunId] FOREIGN KEY ([NightAuditRunId]) REFERENCES [NightAuditRuns] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [Cancellations] (
        [Id] uniqueidentifier NOT NULL,
        [ReservationId] uniqueidentifier NOT NULL,
        [TriggerType] int NOT NULL,
        [CancelledAtUtc] datetime2 NOT NULL,
        [HotelBusinessDateAtCancellation] date NOT NULL,
        [CancelledByUserId] uniqueidentifier NULL,
        [AppliedPolicyRuleId] uniqueidentifier NOT NULL,
        [HoursBeforeCheckIn] int NOT NULL,
        [StayGrossAmount] decimal(18,2) NOT NULL,
        [PenaltyAmount] decimal(18,2) NOT NULL,
        [PenaltyGstAmount] decimal(18,2) NOT NULL,
        [RefundDueAmount] decimal(18,2) NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Cancellations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cancellations_Reservations_ReservationId] FOREIGN KEY ([ReservationId]) REFERENCES [Reservations] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [ReservationNightRates] (
        [Id] uniqueidentifier NOT NULL,
        [ReservationId] uniqueidentifier NOT NULL,
        [StayDate] date NOT NULL,
        [RoomRate] decimal(18,2) NOT NULL,
        [MealPlan] int NOT NULL,
        [InclusionsDescription] nvarchar(max) NOT NULL,
        [GstRuleId] uniqueidentifier NOT NULL,
        [CgstRate] decimal(18,2) NOT NULL,
        [SgstRate] decimal(18,2) NOT NULL,
        [IgstRate] decimal(18,2) NOT NULL,
        [CurrencyCode] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_ReservationNightRates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReservationNightRates_Reservations_ReservationId] FOREIGN KEY ([ReservationId]) REFERENCES [Reservations] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [ReservationPolicySnapshots] (
        [Id] uniqueidentifier NOT NULL,
        [ReservationId] uniqueidentifier NOT NULL,
        [SourceCancellationPolicyId] uniqueidentifier NOT NULL,
        [PolicyName] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_ReservationPolicySnapshots] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReservationPolicySnapshots_Reservations_ReservationId] FOREIGN KEY ([ReservationId]) REFERENCES [Reservations] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [ReservationRoomAssignments] (
        [Id] uniqueidentifier NOT NULL,
        [ReservationId] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NOT NULL,
        [AssignedAtUtc] datetime2 NOT NULL,
        [UnassignedAtUtc] datetime2 NULL,
        [MoveReason] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_ReservationRoomAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReservationRoomAssignments_Reservations_ReservationId] FOREIGN KEY ([ReservationId]) REFERENCES [Reservations] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [AgentRateContracts] (
        [Id] uniqueidentifier NOT NULL,
        [TravelAgentId] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [RatePlanId] uniqueidentifier NOT NULL,
        [ContractStart] date NOT NULL,
        [ContractEnd] date NOT NULL,
        [DiscountPercent] decimal(18,2) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_AgentRateContracts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AgentRateContracts_RatePlans_RatePlanId] FOREIGN KEY ([RatePlanId]) REFERENCES [RatePlans] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AgentRateContracts_TravelAgents_TravelAgentId] FOREIGN KEY ([TravelAgentId]) REFERENCES [TravelAgents] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [CorporateRateContracts] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [RatePlanId] uniqueidentifier NOT NULL,
        [ContractStart] date NOT NULL,
        [ContractEnd] date NOT NULL,
        [DiscountPercent] decimal(18,2) NULL,
        [IsActive] bit NOT NULL,
        [BillToCompanyByDefault] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_CorporateRateContracts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CorporateRateContracts_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CorporateRateContracts_RatePlans_RatePlanId] FOREIGN KEY ([RatePlanId]) REFERENCES [RatePlans] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [RatePlanPrices] (
        [Id] uniqueidentifier NOT NULL,
        [RatePlanId] uniqueidentifier NOT NULL,
        [RoomTypeId] uniqueidentifier NOT NULL,
        [Occupancy] int NOT NULL,
        [DayOfWeek] int NULL,
        [EffectiveFrom] date NOT NULL,
        [EffectiveTo] date NOT NULL,
        [Rate] decimal(18,2) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_RatePlanPrices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RatePlanPrices_RatePlans_RatePlanId] FOREIGN KEY ([RatePlanId]) REFERENCES [RatePlans] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [FolioTaxLines] (
        [Id] uniqueidentifier NOT NULL,
        [FolioTransactionId] uniqueidentifier NOT NULL,
        [GstRuleId] uniqueidentifier NOT NULL,
        [HsnSac] nvarchar(max) NOT NULL,
        [TaxableValue] decimal(18,2) NOT NULL,
        [CgstRate] decimal(18,2) NOT NULL,
        [CgstAmount] decimal(18,2) NOT NULL,
        [SgstRate] decimal(18,2) NOT NULL,
        [SgstAmount] decimal(18,2) NOT NULL,
        [IgstRate] decimal(18,2) NOT NULL,
        [IgstAmount] decimal(18,2) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_FolioTaxLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FolioTaxLines_FolioTransactions_FolioTransactionId] FOREIGN KEY ([FolioTransactionId]) REFERENCES [FolioTransactions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [RoomTypes] (
        [Id] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [BaseOccupancy] int NOT NULL,
        [MaxOccupancy] int NOT NULL,
        [MaxChildren] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_RoomTypes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoomTypes_Hotels_HotelId] FOREIGN KEY ([HotelId]) REFERENCES [Hotels] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [Refunds] (
        [Id] uniqueidentifier NOT NULL,
        [CancellationId] uniqueidentifier NOT NULL,
        [ReservationId] uniqueidentifier NOT NULL,
        [OriginalPaymentId] uniqueidentifier NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [GatewayReference] nvarchar(max) NULL,
        [FailureReason] nvarchar(max) NULL,
        [RequestedAtUtc] datetime2 NOT NULL,
        [ApprovedAtUtc] datetime2 NULL,
        [SentToGatewayAtUtc] datetime2 NULL,
        [CompletedAtUtc] datetime2 NULL,
        [ApprovedByUserId] uniqueidentifier NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Refunds] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Refunds_Cancellations_CancellationId] FOREIGN KEY ([CancellationId]) REFERENCES [Cancellations] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [ReservationPolicySnapshotRules] (
        [Id] uniqueidentifier NOT NULL,
        [ReservationPolicySnapshotId] uniqueidentifier NOT NULL,
        [HoursBeforeCheckInMin] int NULL,
        [HoursBeforeCheckInMax] int NULL,
        [PenaltyType] int NOT NULL,
        [PenaltyValue] decimal(18,2) NULL,
        [AppliesToNoShow] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_ReservationPolicySnapshotRules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReservationPolicySnapshotRules_ReservationPolicySnapshots_ReservationPolicySnapshotId] FOREIGN KEY ([ReservationPolicySnapshotId]) REFERENCES [ReservationPolicySnapshots] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [Rooms] (
        [Id] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [RoomTypeId] uniqueidentifier NOT NULL,
        [RoomNumber] nvarchar(20) NOT NULL,
        [Floor] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Rooms_Hotels_HotelId] FOREIGN KEY ([HotelId]) REFERENCES [Hotels] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Rooms_RoomTypes_RoomTypeId] FOREIGN KEY ([RoomTypeId]) REFERENCES [RoomTypes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [HousekeepingTasks] (
        [Id] uniqueidentifier NOT NULL,
        [HotelId] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NOT NULL,
        [TaskType] int NOT NULL,
        [Status] int NOT NULL,
        [AssignedToUserId] uniqueidentifier NULL,
        [Notes] nvarchar(max) NOT NULL,
        [StartedAtUtc] datetime2 NULL,
        [CompletedAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_HousekeepingTasks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HousekeepingTasks_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [RoomOutOfServicePeriods] (
        [Id] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [RequestedByUserId] uniqueidentifier NULL,
        [ApprovedByUserId] uniqueidentifier NULL,
        [ApprovedAtUtc] datetime2 NULL,
        [ReturnedToServiceAtUtc] datetime2 NULL,
        [ReturnedByUserId] uniqueidentifier NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_RoomOutOfServicePeriods] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoomOutOfServicePeriods_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE TABLE [RoomStatusHistories] (
        [Id] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NOT NULL,
        [FromStatus] int NOT NULL,
        [ToStatus] int NOT NULL,
        [Reason] nvarchar(max) NULL,
        [ChangedByUserId] uniqueidentifier NULL,
        [ChangedAtUtc] datetime2 NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_RoomStatusHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoomStatusHistories_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AgentRateContracts_RatePlanId] ON [AgentRateContracts] ([RatePlanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AgentRateContracts_TravelAgentId_HotelId_IsActive] ON [AgentRateContracts] ([TravelAgentId], [HotelId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_CreatedAtUtc] ON [AuditLogs] ([CreatedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [AuditLogs] ([EntityType], [EntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CancellationPolicies_HotelId] ON [CancellationPolicies] ([HotelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CancellationPolicyRules_CancellationPolicyId_SortOrder] ON [CancellationPolicyRules] ([CancellationPolicyId], [SortOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Cancellations_ReservationId] ON [Cancellations] ([ReservationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Companies_Gstin] ON [Companies] ([Gstin]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CorporateRateContracts_CompanyId_HotelId_IsActive] ON [CorporateRateContracts] ([CompanyId], [HotelId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CorporateRateContracts_RatePlanId] ON [CorporateRateContracts] ([RatePlanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Folios_ReservationId_Status] ON [Folios] ([ReservationId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Folios_ReservationId_Type] ON [Folios] ([ReservationId], [Type]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FolioTaxLines_FolioTransactionId] ON [FolioTaxLines] ([FolioTransactionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FolioTransactions_FolioId_BusinessDate] ON [FolioTransactions] ([FolioId], [BusinessDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_FolioTransactions_UniquePostingKey] ON [FolioTransactions] ([UniquePostingKey]) WHERE [UniquePostingKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FolioTransfers_FromFolioId] ON [FolioTransfers] ([FromFolioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FolioTransfers_ToFolioId] ON [FolioTransfers] ([ToFolioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GstRules_ChargeCategory_EffectiveFrom_EffectiveTo] ON [GstRules] ([ChargeCategory], [EffectiveFrom], [EffectiveTo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Guests_Email] ON [Guests] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Guests_Phone] ON [Guests] ([Phone]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HotelGroups_Name] ON [HotelGroups] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Hotels_Code] ON [Hotels] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Hotels_HotelGroupId] ON [Hotels] ([HotelGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HousekeepingTasks_HotelId_Status] ON [HousekeepingTasks] ([HotelId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HousekeepingTasks_RoomId] ON [HousekeepingTasks] ([RoomId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InventoryHolds_HotelId_RoomTypeId_Status_CheckInDate_CheckOutDate] ON [InventoryHolds] ([HotelId], [RoomTypeId], [Status], [CheckInDate], [CheckOutDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryHolds_IdempotencyKey] ON [InventoryHolds] ([IdempotencyKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InventoryHolds_Status_ExpiresAtUtc] ON [InventoryHolds] ([Status], [ExpiresAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InvoiceLines_InvoiceId] ON [InvoiceLines] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Invoices_InvoiceNumber] ON [Invoices] ([InvoiceNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_ReservationId] ON [Invoices] ([ReservationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NightAuditExceptions_NightAuditRunId] ON [NightAuditExceptions] ([NightAuditRunId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NightAuditRuns_HotelId_BusinessDate] ON [NightAuditRuns] ([HotelId], [BusinessDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_FolioId] ON [Payments] ([FolioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Payments_IdempotencyKey] ON [Payments] ([IdempotencyKey]) WHERE [IdempotencyKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PosCharges_IdempotencyKey] ON [PosCharges] ([IdempotencyKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PosOutlets_HotelId_Code] ON [PosOutlets] ([HotelId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RatePlanPrices_RatePlanId_RoomTypeId_Occupancy_EffectiveFrom_EffectiveTo] ON [RatePlanPrices] ([RatePlanId], [RoomTypeId], [Occupancy], [EffectiveFrom], [EffectiveTo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RatePlans_CancellationPolicyId] ON [RatePlans] ([CancellationPolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RatePlans_HotelId_Code] ON [RatePlans] ([HotelId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Refunds_CancellationId] ON [Refunds] ([CancellationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Refunds_Status] ON [Refunds] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReservationNightRates_ReservationId_StayDate] ON [ReservationNightRates] ([ReservationId], [StayDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReservationPolicySnapshotRules_ReservationPolicySnapshotId] ON [ReservationPolicySnapshotRules] ([ReservationPolicySnapshotId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReservationPolicySnapshots_ReservationId] ON [ReservationPolicySnapshots] ([ReservationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReservationRoomAssignments_ReservationId] ON [ReservationRoomAssignments] ([ReservationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReservationRoomAssignments_RoomId_UnassignedAtUtc] ON [ReservationRoomAssignments] ([RoomId], [UnassignedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reservations_HotelId_CheckInDate] ON [Reservations] ([HotelId], [CheckInDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reservations_HotelId_CheckOutDate] ON [Reservations] ([HotelId], [CheckOutDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reservations_HotelId_RoomTypeId_Status_CheckInDate_CheckOutDate] ON [Reservations] ([HotelId], [RoomTypeId], [Status], [CheckInDate], [CheckOutDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Reservations_IdempotencyKey] ON [Reservations] ([IdempotencyKey]) WHERE [IdempotencyKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Reservations_ReservationNumber] ON [Reservations] ([ReservationNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RoomOutOfServicePeriods_RoomId_StartDate_EndDate] ON [RoomOutOfServicePeriods] ([RoomId], [StartDate], [EndDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Rooms_HotelId_RoomNumber] ON [Rooms] ([HotelId], [RoomNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Rooms_HotelId_RoomTypeId_Status] ON [Rooms] ([HotelId], [RoomTypeId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Rooms_RoomTypeId] ON [Rooms] ([RoomTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RoomStatusHistories_RoomId_ChangedAtUtc] ON [RoomStatusHistories] ([RoomId], [ChangedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RoomTypes_HotelId_Code] ON [RoomTypes] ([HotelId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserHotelAccesses_UserId_HotelId] ON [UserHotelAccesses] ([UserId], [HotelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729080952_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260729080952_InitialCreate', N'10.0.10');
END;

COMMIT;
GO

