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
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
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
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
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
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] uniqueidentifier NULL,
        [Action] nvarchar(50) NOT NULL,
        [EntityName] nvarchar(200) NOT NULL,
        [EntityId] nvarchar(100) NOT NULL,
        [ChangesJson] nvarchar(max) NULL,
        [IpAddress] nvarchar(max) NULL,
        [OccurredAt] datetimeoffset(0) NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [BirdBuildings] (
        [Id] uniqueidentifier NOT NULL,
        [OwnerUserId] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Province] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [Latitude] decimal(10,7) NULL,
        [Longitude] decimal(10,7) NULL,
        [StartedOn] date NULL,
        [FloorCount] int NULL,
        [RoomCount] int NULL,
        [AreaSquareMeters] decimal(12,2) NULL,
        [Notes] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_BirdBuildings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [Buyers] (
        [Id] uniqueidentifier NOT NULL,
        [OwnerUserId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [TaxId] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Buyers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [ExpenseCategories] (
        [Id] uniqueidentifier NOT NULL,
        [OwnerUserId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ExpenseCategories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [FinancialTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [OwnerUserId] uniqueidentifier NOT NULL,
        [BuildingId] uniqueidentifier NULL,
        [HarvestRoundId] uniqueidentifier NULL,
        [SaleId] uniqueidentifier NULL,
        [MaintenanceJobId] uniqueidentifier NULL,
        [ExpenseCategoryId] uniqueidentifier NULL,
        [Type] int NOT NULL,
        [TransactionDate] date NOT NULL,
        [PaidOn] date NULL,
        [Description] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [ReferenceNumber] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_FinancialTransactions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [MaintenanceJobs] (
        [Id] uniqueidentifier NOT NULL,
        [BuildingId] uniqueidentifier NOT NULL,
        [AssetId] uniqueidentifier NULL,
        [Type] int NOT NULL,
        [Priority] int NOT NULL,
        [Status] int NOT NULL,
        [Issue] nvarchar(max) NOT NULL,
        [ReportedAt] datetimeoffset NOT NULL,
        [StartedAt] datetimeoffset NULL,
        [CompletedAt] datetimeoffset NULL,
        [AssignedUserId] uniqueidentifier NULL,
        [PartsCost] decimal(18,2) NOT NULL,
        [LaborCost] decimal(18,2) NOT NULL,
        [DowntimeHours] decimal(18,2) NOT NULL,
        [AcceptanceResult] nvarchar(max) NULL,
        [NextInspectionOn] date NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_MaintenanceJobs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [MasterOptions] (
        [Id] uniqueidentifier NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_MasterOptions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [QualityStandards] (
        [Id] uniqueidentifier NOT NULL,
        [OwnerUserId] uniqueidentifier NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Version] int NOT NULL,
        [EffectiveFrom] date NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_QualityStandards] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
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
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
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
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
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
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
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
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
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
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [Assets] (
        [Id] uniqueidentifier NOT NULL,
        [BuildingId] uniqueidentifier NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NULL,
        [InstalledOn] date NULL,
        [NextInspectionOn] date NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Assets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Assets_BirdBuildings_BuildingId] FOREIGN KEY ([BuildingId]) REFERENCES [BirdBuildings] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [BuildingUsers] (
        [Id] uniqueidentifier NOT NULL,
        [BuildingId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_BuildingUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BuildingUsers_BirdBuildings_BuildingId] FOREIGN KEY ([BuildingId]) REFERENCES [BirdBuildings] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [DailyLogs] (
        [Id] uniqueidentifier NOT NULL,
        [BuildingId] uniqueidentifier NOT NULL,
        [LogDate] date NOT NULL,
        [Type] int NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Details] nvarchar(max) NULL,
        [HarvestRoundId] uniqueidentifier NULL,
        [FinancialTransactionId] uniqueidentifier NULL,
        [MaintenanceJobId] uniqueidentifier NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_DailyLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DailyLogs_BirdBuildings_BuildingId] FOREIGN KEY ([BuildingId]) REFERENCES [BirdBuildings] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [HarvestRounds] (
        [Id] uniqueidentifier NOT NULL,
        [BuildingId] uniqueidentifier NOT NULL,
        [RoundNumber] nvarchar(50) NOT NULL,
        [HarvestArea] nvarchar(max) NULL,
        [StartedOn] date NOT NULL,
        [HarvestedOn] date NOT NULL,
        [TotalWeightKg] decimal(14,3) NOT NULL,
        [NestCount] int NOT NULL,
        [SampleCount] int NOT NULL,
        [CollectorUserId] uniqueidentifier NULL,
        [InspectorUserId] uniqueidentifier NULL,
        [LaborCost] decimal(18,2) NOT NULL,
        [EnvironmentNotes] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_HarvestRounds] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HarvestRounds_BirdBuildings_BuildingId] FOREIGN KEY ([BuildingId]) REFERENCES [BirdBuildings] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [Sales] (
        [Id] uniqueidentifier NOT NULL,
        [OwnerUserId] uniqueidentifier NOT NULL,
        [DocumentNumber] nvarchar(50) NOT NULL,
        [SaleDate] date NOT NULL,
        [BuyerId] uniqueidentifier NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [ReturnAmount] decimal(18,2) NOT NULL,
        [NetAmount] decimal(18,2) NOT NULL,
        [PaidAmount] decimal(18,2) NOT NULL,
        [PaymentMethod] nvarchar(max) NULL,
        [PaymentStatus] int NOT NULL,
        [DueDate] date NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Sales] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sales_Buyers_BuyerId] FOREIGN KEY ([BuyerId]) REFERENCES [Buyers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [QualityBands] (
        [Id] uniqueidentifier NOT NULL,
        [QualityStandardId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [MinimumScore] decimal(18,2) NOT NULL,
        [MaximumScore] decimal(18,2) NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_QualityBands] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QualityBands_QualityStandards_QualityStandardId] FOREIGN KEY ([QualityStandardId]) REFERENCES [QualityStandards] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [QualityCriteria] (
        [Id] uniqueidentifier NOT NULL,
        [QualityStandardId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [MaxScore] decimal(18,2) NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_QualityCriteria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QualityCriteria_QualityStandards_QualityStandardId] FOREIGN KEY ([QualityStandardId]) REFERENCES [QualityStandards] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [StoredFiles] (
        [Id] uniqueidentifier NOT NULL,
        [BuildingId] uniqueidentifier NOT NULL,
        [DailyLogId] uniqueidentifier NULL,
        [HarvestRoundId] uniqueidentifier NULL,
        [HarvestSampleId] uniqueidentifier NULL,
        [MaintenanceJobId] uniqueidentifier NULL,
        [SaleId] uniqueidentifier NULL,
        [Kind] int NOT NULL,
        [StorageKey] nvarchar(max) NOT NULL,
        [OriginalFileName] nvarchar(max) NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [SizeBytes] bigint NOT NULL,
        [Sha256] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_StoredFiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StoredFiles_DailyLogs_DailyLogId] FOREIGN KEY ([DailyLogId]) REFERENCES [DailyLogs] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [HarvestItems] (
        [Id] uniqueidentifier NOT NULL,
        [HarvestRoundId] uniqueidentifier NOT NULL,
        [NestTypeId] uniqueidentifier NOT NULL,
        [GradeId] uniqueidentifier NULL,
        [ColorId] uniqueidentifier NULL,
        [NestSourceId] uniqueidentifier NULL,
        [WeightKg] decimal(14,3) NOT NULL,
        [RemainingWeightKg] decimal(14,3) NOT NULL,
        [NestCount] int NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_HarvestItems] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_HarvestItems_Weight] CHECK ([WeightKg] >= 0 AND [RemainingWeightKg] >= 0 AND [RemainingWeightKg] <= [WeightKg]),
        CONSTRAINT [FK_HarvestItems_HarvestRounds_HarvestRoundId] FOREIGN KEY ([HarvestRoundId]) REFERENCES [HarvestRounds] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [HarvestSamples] (
        [Id] uniqueidentifier NOT NULL,
        [HarvestRoundId] uniqueidentifier NOT NULL,
        [SampleNumber] nvarchar(max) NOT NULL,
        [WeightGrams] decimal(10,3) NULL,
        [NestTypeId] uniqueidentifier NULL,
        [GradeId] uniqueidentifier NULL,
        [ColorId] uniqueidentifier NULL,
        [NestSourceId] uniqueidentifier NULL,
        [BellyConditionId] uniqueidentifier NULL,
        [FeatherLevelId] uniqueidentifier NULL,
        [CleanlinessLevelId] uniqueidentifier NULL,
        [InspectorNotes] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_HarvestSamples] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HarvestSamples_HarvestRounds_HarvestRoundId] FOREIGN KEY ([HarvestRoundId]) REFERENCES [HarvestRounds] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [QualityScores] (
        [Id] uniqueidentifier NOT NULL,
        [HarvestRoundId] uniqueidentifier NOT NULL,
        [QualityStandardId] uniqueidentifier NOT NULL,
        [TotalScore] decimal(18,2) NOT NULL,
        [ResultLabel] nvarchar(max) NOT NULL,
        [ConfirmedByUserId] uniqueidentifier NOT NULL,
        [ConfirmedAt] datetimeoffset NOT NULL,
        [Explanation] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_QualityScores] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QualityScores_HarvestRounds_HarvestRoundId] FOREIGN KEY ([HarvestRoundId]) REFERENCES [HarvestRounds] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [SaleItems] (
        [Id] uniqueidentifier NOT NULL,
        [SaleId] uniqueidentifier NOT NULL,
        [HarvestItemId] uniqueidentifier NOT NULL,
        [WeightKg] decimal(14,3) NOT NULL,
        [PricePerKg] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [NetAmount] decimal(18,2) NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SaleItems] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_SaleItems_Weight] CHECK ([WeightKg] > 0),
        CONSTRAINT [FK_SaleItems_HarvestItems_HarvestItemId] FOREIGN KEY ([HarvestItemId]) REFERENCES [HarvestItems] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SaleItems_Sales_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sales] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE TABLE [SampleCharacteristics] (
        [Id] uniqueidentifier NOT NULL,
        [HarvestSampleId] uniqueidentifier NOT NULL,
        [CharacteristicId] uniqueidentifier NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SampleCharacteristics] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SampleCharacteristics_HarvestSamples_HarvestSampleId] FOREIGN KEY ([HarvestSampleId]) REFERENCES [HarvestSamples] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_BuildingId] ON [Assets] ([BuildingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_OccurredAt] ON [AuditLogs] ([OccurredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BirdBuildings_OwnerUserId_Code] ON [BirdBuildings] ([OwnerUserId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BuildingUsers_BuildingId_UserId] ON [BuildingUsers] ([BuildingId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DailyLogs_BuildingId] ON [DailyLogs] ([BuildingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HarvestItems_HarvestRoundId] ON [HarvestItems] ([HarvestRoundId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HarvestRounds_BuildingId_RoundNumber] ON [HarvestRounds] ([BuildingId], [RoundNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HarvestSamples_HarvestRoundId] ON [HarvestSamples] ([HarvestRoundId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MasterOptions_Category_Code] ON [MasterOptions] ([Category], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_QualityBands_QualityStandardId] ON [QualityBands] ([QualityStandardId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_QualityCriteria_QualityStandardId] ON [QualityCriteria] ([QualityStandardId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_QualityScores_HarvestRoundId] ON [QualityScores] ([HarvestRoundId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_QualityStandards_OwnerUserId_Name_Version] ON [QualityStandards] ([OwnerUserId], [Name], [Version]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SaleItems_HarvestItemId] ON [SaleItems] ([HarvestItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SaleItems_SaleId] ON [SaleItems] ([SaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sales_BuyerId] ON [Sales] ([BuyerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Sales_OwnerUserId_DocumentNumber] ON [Sales] ([OwnerUserId], [DocumentNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SampleCharacteristics_HarvestSampleId] ON [SampleCharacteristics] ([HarvestSampleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StoredFiles_DailyLogId] ON [StoredFiles] ([DailyLogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907032243_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907032243_InitialCreate', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907033427_FixIdentityIndexLengths'
)
BEGIN
    ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [PK_AspNetUserTokens];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907033427_FixIdentityIndexLengths'
)
BEGIN
    ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [PK_AspNetUserLogins];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907033427_FixIdentityIndexLengths'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserTokens]') AND [c].[name] = N'Name');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [AspNetUserTokens] ALTER COLUMN [Name] nvarchar(128) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907033427_FixIdentityIndexLengths'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserTokens]') AND [c].[name] = N'LoginProvider');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [AspNetUserTokens] ALTER COLUMN [LoginProvider] nvarchar(128) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907033427_FixIdentityIndexLengths'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserLogins]') AND [c].[name] = N'ProviderKey');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [AspNetUserLogins] ALTER COLUMN [ProviderKey] nvarchar(128) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907033427_FixIdentityIndexLengths'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserLogins]') AND [c].[name] = N'LoginProvider');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [AspNetUserLogins] ALTER COLUMN [LoginProvider] nvarchar(128) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907033427_FixIdentityIndexLengths'
)
BEGIN
    ALTER TABLE [AspNetUserLogins] ADD CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907033427_FixIdentityIndexLengths'
)
BEGIN
    ALTER TABLE [AspNetUserTokens] ADD CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907033427_FixIdentityIndexLengths'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907033427_FixIdentityIndexLengths', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907035802_AddThaiAddressReferenceData'
)
BEGIN
    CREATE TABLE [ThaiProvinces] (
        [Code] smallint NOT NULL,
        [NameTh] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ThaiProvinces] PRIMARY KEY ([Code])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907035802_AddThaiAddressReferenceData'
)
BEGIN
    CREATE TABLE [ThaiDistricts] (
        [Code] int NOT NULL,
        [ProvinceCode] smallint NOT NULL,
        [NameTh] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ThaiDistricts] PRIMARY KEY ([Code]),
        CONSTRAINT [FK_ThaiDistricts_ThaiProvinces_ProvinceCode] FOREIGN KEY ([ProvinceCode]) REFERENCES [ThaiProvinces] ([Code]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907035802_AddThaiAddressReferenceData'
)
BEGIN
    CREATE TABLE [ThaiSubdistricts] (
        [Code] int NOT NULL,
        [DistrictCode] int NOT NULL,
        [NameTh] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ThaiSubdistricts] PRIMARY KEY ([Code]),
        CONSTRAINT [FK_ThaiSubdistricts_ThaiDistricts_DistrictCode] FOREIGN KEY ([DistrictCode]) REFERENCES [ThaiDistricts] ([Code]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907035802_AddThaiAddressReferenceData'
)
BEGIN
    CREATE TABLE [ThaiSubdistrictPostalCodes] (
        [SubdistrictCode] int NOT NULL,
        [PostalCode] char(5) NOT NULL,
        [IsPrimary] bit NOT NULL,
        CONSTRAINT [PK_ThaiSubdistrictPostalCodes] PRIMARY KEY ([SubdistrictCode], [PostalCode]),
        CONSTRAINT [FK_ThaiSubdistrictPostalCodes_ThaiSubdistricts_SubdistrictCode] FOREIGN KEY ([SubdistrictCode]) REFERENCES [ThaiSubdistricts] ([Code]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907035802_AddThaiAddressReferenceData'
)
BEGIN
    CREATE INDEX [IX_ThaiDistricts_ProvinceCode_NameTh] ON [ThaiDistricts] ([ProvinceCode], [NameTh]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907035802_AddThaiAddressReferenceData'
)
BEGIN
    CREATE INDEX [IX_ThaiProvinces_NameTh] ON [ThaiProvinces] ([NameTh]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907035802_AddThaiAddressReferenceData'
)
BEGIN
    CREATE INDEX [IX_ThaiSubdistrictPostalCodes_PostalCode] ON [ThaiSubdistrictPostalCodes] ([PostalCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907035802_AddThaiAddressReferenceData'
)
BEGIN
    CREATE INDEX [IX_ThaiSubdistricts_DistrictCode_NameTh] ON [ThaiSubdistricts] ([DistrictCode], [NameTh]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907035802_AddThaiAddressReferenceData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907035802_AddThaiAddressReferenceData', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    DROP INDEX [IX_BirdBuildings_OwnerUserId_Code] ON [BirdBuildings];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD [AccountId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE TABLE [AccountInvitations] (
        [Id] uniqueidentifier NOT NULL,
        [AccountId] uniqueidentifier NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [RoleName] nvarchar(50) NOT NULL,
        [TokenHash] nvarchar(128) NOT NULL,
        [ExpiresAt] datetimeoffset NOT NULL,
        [AcceptedAt] datetimeoffset NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_AccountInvitations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE TABLE [Accounts] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [BusinessType] nvarchar(100) NULL,
        [TaxId] nvarchar(20) NULL,
        [AddressLine] nvarchar(500) NULL,
        [ProvinceCode] smallint NULL,
        [DistrictCode] int NULL,
        [SubdistrictCode] int NULL,
        [PostalCode] char(5) NULL,
        [TimeZoneId] nvarchar(100) NOT NULL,
        [CurrencyCode] char(3) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Accounts_ThaiDistricts_DistrictCode] FOREIGN KEY ([DistrictCode]) REFERENCES [ThaiDistricts] ([Code]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Accounts_ThaiProvinces_ProvinceCode] FOREIGN KEY ([ProvinceCode]) REFERENCES [ThaiProvinces] ([Code]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Accounts_ThaiSubdistricts_SubdistrictCode] FOREIGN KEY ([SubdistrictCode]) REFERENCES [ThaiSubdistricts] ([Code]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE TABLE [EmailVerificationLogs] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RequestedAt] datetimeoffset NOT NULL,
        [ConfirmedAt] datetimeoffset NULL,
        [IpAddress] nvarchar(64) NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_EmailVerificationLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE TABLE [UserConsents] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ConsentType] nvarchar(50) NOT NULL,
        [Version] nvarchar(30) NOT NULL,
        [AcceptedAt] datetimeoffset NOT NULL,
        [IpAddress] nvarchar(64) NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserConsents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE TABLE [AccountUsers] (
        [Id] uniqueidentifier NOT NULL,
        [AccountId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RoleName] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset(0) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_AccountUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AccountUsers_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BirdBuildings_AccountId_Code] ON [BirdBuildings] ([AccountId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE INDEX [IX_AccountInvitations_AccountId_Email] ON [AccountInvitations] ([AccountId], [Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE INDEX [IX_Accounts_DistrictCode] ON [Accounts] ([DistrictCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE INDEX [IX_Accounts_ProvinceCode] ON [Accounts] ([ProvinceCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE INDEX [IX_Accounts_SubdistrictCode] ON [Accounts] ([SubdistrictCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AccountUsers_AccountId_UserId] ON [AccountUsers] ([AccountId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE INDEX [IX_EmailVerificationLogs_UserId_RequestedAt] ON [EmailVerificationLogs] ([UserId], [RequestedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserConsents_UserId_ConsentType_Version] ON [UserConsents] ([UserId], [ConsentType], [Version]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD CONSTRAINT [FK_BirdBuildings_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041323_AddAccountOnboarding'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907041323_AddAccountOnboarding', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041410_AddAccountForeignKeys'
)
BEGIN
    CREATE INDEX [IX_AccountUsers_UserId] ON [AccountUsers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041410_AddAccountForeignKeys'
)
BEGIN
    ALTER TABLE [AccountInvitations] ADD CONSTRAINT [FK_AccountInvitations_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041410_AddAccountForeignKeys'
)
BEGIN
    ALTER TABLE [AccountUsers] ADD CONSTRAINT [FK_AccountUsers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041410_AddAccountForeignKeys'
)
BEGIN
    ALTER TABLE [EmailVerificationLogs] ADD CONSTRAINT [FK_EmailVerificationLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041410_AddAccountForeignKeys'
)
BEGIN
    ALTER TABLE [UserConsents] ADD CONSTRAINT [FK_UserConsents_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041410_AddAccountForeignKeys'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907041410_AddAccountForeignKeys', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041713_SetFinancialAndQualityPrecision'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QualityScores]') AND [c].[name] = N'TotalScore');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [QualityScores] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [QualityScores] ALTER COLUMN [TotalScore] decimal(9,3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041713_SetFinancialAndQualityPrecision'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QualityCriteria]') AND [c].[name] = N'MaxScore');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [QualityCriteria] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [QualityCriteria] ALTER COLUMN [MaxScore] decimal(9,3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041713_SetFinancialAndQualityPrecision'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QualityBands]') AND [c].[name] = N'MinimumScore');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [QualityBands] DROP CONSTRAINT ' + @var6 + ';');
    ALTER TABLE [QualityBands] ALTER COLUMN [MinimumScore] decimal(9,3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041713_SetFinancialAndQualityPrecision'
)
BEGIN
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QualityBands]') AND [c].[name] = N'MaximumScore');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [QualityBands] DROP CONSTRAINT ' + @var7 + ';');
    ALTER TABLE [QualityBands] ALTER COLUMN [MaximumScore] decimal(9,3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041713_SetFinancialAndQualityPrecision'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MaintenanceJobs]') AND [c].[name] = N'DowntimeHours');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [MaintenanceJobs] DROP CONSTRAINT ' + @var8 + ';');
    ALTER TABLE [MaintenanceJobs] ALTER COLUMN [DowntimeHours] decimal(10,2) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907041713_SetFinancialAndQualityPrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907041713_SetFinancialAndQualityPrecision', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD [DistrictCode] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD [PostalCode] char(5) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD [ProvinceCode] smallint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD [SubdistrictCode] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    CREATE INDEX [IX_BirdBuildings_DistrictCode] ON [BirdBuildings] ([DistrictCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    CREATE INDEX [IX_BirdBuildings_ProvinceCode] ON [BirdBuildings] ([ProvinceCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    CREATE INDEX [IX_BirdBuildings_SubdistrictCode] ON [BirdBuildings] ([SubdistrictCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD CONSTRAINT [FK_BirdBuildings_ThaiDistricts_DistrictCode] FOREIGN KEY ([DistrictCode]) REFERENCES [ThaiDistricts] ([Code]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD CONSTRAINT [FK_BirdBuildings_ThaiProvinces_ProvinceCode] FOREIGN KEY ([ProvinceCode]) REFERENCES [ThaiProvinces] ([Code]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD CONSTRAINT [FK_BirdBuildings_ThaiSubdistricts_SubdistrictCode] FOREIGN KEY ([SubdistrictCode]) REFERENCES [ThaiSubdistricts] ([Code]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907075520_AddBuildingAddressLocation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907075520_AddBuildingAddressLocation', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907080038_BackfillBuildingAddressFromAccount'
)
BEGIN
    UPDATE building
    SET ProvinceCode = COALESCE(building.ProvinceCode, accountRow.ProvinceCode),
        DistrictCode = COALESCE(building.DistrictCode, accountRow.DistrictCode),
        SubdistrictCode = COALESCE(building.SubdistrictCode, accountRow.SubdistrictCode),
        PostalCode = COALESCE(building.PostalCode, accountRow.PostalCode)
    FROM BirdBuildings AS building
    INNER JOIN Accounts AS accountRow ON accountRow.Id = building.AccountId
    WHERE building.ProvinceCode IS NULL
       OR building.DistrictCode IS NULL
       OR building.SubdistrictCode IS NULL
       OR building.PostalCode IS NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907080038_BackfillBuildingAddressFromAccount'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907080038_BackfillBuildingAddressFromAccount', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907080748_AddBuildingDimensionsAndBudget'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD [ConstructionBudget] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907080748_AddBuildingDimensionsAndBudget'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD [DepthMeters] decimal(10,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907080748_AddBuildingDimensionsAndBudget'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD [WidthMeters] decimal(10,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907080748_AddBuildingDimensionsAndBudget'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907080748_AddBuildingDimensionsAndBudget', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907081513_AddBuildingBuiltOrPurchasedYear'
)
BEGIN
    ALTER TABLE [BirdBuildings] ADD [BuiltOrPurchasedYear] smallint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907081513_AddBuildingBuiltOrPurchasedYear'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907081513_AddBuildingBuiltOrPurchasedYear', N'10.0.9');
END;

COMMIT;
GO

