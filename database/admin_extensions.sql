USE CloneEbayDB;
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dbo.User', N'moderationStatus') IS NULL
        EXEC(N'ALTER TABLE dbo.[User] ADD moderationStatus nvarchar(20) NOT NULL CONSTRAINT DF_User_ModerationStatus DEFAULT N''Active''');
    IF COL_LENGTH(N'dbo.User', N'moderationReason') IS NULL EXEC(N'ALTER TABLE dbo.[User] ADD moderationReason nvarchar(500) NULL');
    IF COL_LENGTH(N'dbo.User', N'moderatedBy') IS NULL EXEC(N'ALTER TABLE dbo.[User] ADD moderatedBy int NULL');
    IF COL_LENGTH(N'dbo.User', N'moderatedAtUtc') IS NULL EXEC(N'ALTER TABLE dbo.[User] ADD moderatedAtUtc datetime2(0) NULL');

    IF COL_LENGTH(N'dbo.Product', N'moderationStatus') IS NULL
        EXEC(N'ALTER TABLE dbo.Product ADD moderationStatus nvarchar(20) NOT NULL CONSTRAINT DF_Product_ModerationStatus DEFAULT N''Active''');
    IF COL_LENGTH(N'dbo.Product', N'moderationReason') IS NULL EXEC(N'ALTER TABLE dbo.Product ADD moderationReason nvarchar(500) NULL');
    IF COL_LENGTH(N'dbo.Product', N'moderatedBy') IS NULL EXEC(N'ALTER TABLE dbo.Product ADD moderatedBy int NULL');
    IF COL_LENGTH(N'dbo.Product', N'moderatedAtUtc') IS NULL EXEC(N'ALTER TABLE dbo.Product ADD moderatedAtUtc datetime2(0) NULL');

    IF COL_LENGTH(N'dbo.Review', N'moderationStatus') IS NULL
        EXEC(N'ALTER TABLE dbo.Review ADD moderationStatus nvarchar(20) NOT NULL CONSTRAINT DF_Review_ModerationStatus DEFAULT N''Visible''');
    IF COL_LENGTH(N'dbo.Review', N'moderationReason') IS NULL EXEC(N'ALTER TABLE dbo.Review ADD moderationReason nvarchar(500) NULL');
    IF COL_LENGTH(N'dbo.Review', N'moderatedBy') IS NULL EXEC(N'ALTER TABLE dbo.Review ADD moderatedBy int NULL');
    IF COL_LENGTH(N'dbo.Review', N'moderatedAtUtc') IS NULL EXEC(N'ALTER TABLE dbo.Review ADD moderatedAtUtc datetime2(0) NULL');

    IF COL_LENGTH(N'dbo.Dispute', N'assignedTo') IS NULL EXEC(N'ALTER TABLE dbo.Dispute ADD assignedTo int NULL');
    IF COL_LENGTH(N'dbo.Dispute', N'assignedAtUtc') IS NULL EXEC(N'ALTER TABLE dbo.Dispute ADD assignedAtUtc datetime2(0) NULL');
    IF COL_LENGTH(N'dbo.Dispute', N'reviewStartedAtUtc') IS NULL EXEC(N'ALTER TABLE dbo.Dispute ADD reviewStartedAtUtc datetime2(0) NULL');
    IF COL_LENGTH(N'dbo.Dispute', N'resolvedBy') IS NULL EXEC(N'ALTER TABLE dbo.Dispute ADD resolvedBy int NULL');
    IF COL_LENGTH(N'dbo.Dispute', N'resolvedAtUtc') IS NULL EXEC(N'ALTER TABLE dbo.Dispute ADD resolvedAtUtc datetime2(0) NULL');

    IF OBJECT_ID(N'dbo.AdminAuditLog', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.AdminAuditLog
        (
            id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AdminAuditLog PRIMARY KEY,
            adminUserId int NOT NULL,
            action nvarchar(50) NOT NULL,
            resourceType nvarchar(50) NOT NULL,
            resourceId int NOT NULL,
            reason nvarchar(500) NULL,
            createdAtUtc datetime2(0) NOT NULL CONSTRAINT DF_AdminAuditLog_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_AdminAuditLog_User_AdminUserId FOREIGN KEY (adminUserId) REFERENCES dbo.[User](id)
        );
    END;

    IF OBJECT_ID(N'CK_User_ModerationStatus', N'C') IS NULL
        EXEC(N'ALTER TABLE dbo.[User] ADD CONSTRAINT CK_User_ModerationStatus CHECK (moderationStatus IN (N''Pending'', N''Active'', N''Banned''))');
    IF OBJECT_ID(N'CK_Product_ModerationStatus', N'C') IS NULL
        EXEC(N'ALTER TABLE dbo.Product ADD CONSTRAINT CK_Product_ModerationStatus CHECK (moderationStatus IN (N''Active'', N''Hidden''))');
    IF OBJECT_ID(N'CK_Review_ModerationStatus', N'C') IS NULL
        EXEC(N'ALTER TABLE dbo.Review ADD CONSTRAINT CK_Review_ModerationStatus CHECK (moderationStatus IN (N''Visible'', N''Hidden''))');
    IF OBJECT_ID(N'CK_Dispute_AdminWorkflowStatus', N'C') IS NULL
        EXEC(N'ALTER TABLE dbo.Dispute ADD CONSTRAINT CK_Dispute_AdminWorkflowStatus CHECK (status IS NULL OR status IN (N''Open'', N''Assigned'', N''InReview'', N''Resolved'', N''Rejected''))');

    IF OBJECT_ID(N'FK_User_User_ModeratedBy', N'F') IS NULL
        EXEC(N'ALTER TABLE dbo.[User] ADD CONSTRAINT FK_User_User_ModeratedBy FOREIGN KEY (moderatedBy) REFERENCES dbo.[User](id)');
    IF OBJECT_ID(N'FK_Product_User_ModeratedBy', N'F') IS NULL
        EXEC(N'ALTER TABLE dbo.Product ADD CONSTRAINT FK_Product_User_ModeratedBy FOREIGN KEY (moderatedBy) REFERENCES dbo.[User](id)');
    IF OBJECT_ID(N'FK_Review_User_ModeratedBy', N'F') IS NULL
        EXEC(N'ALTER TABLE dbo.Review ADD CONSTRAINT FK_Review_User_ModeratedBy FOREIGN KEY (moderatedBy) REFERENCES dbo.[User](id)');
    IF OBJECT_ID(N'FK_Dispute_User_AssignedTo', N'F') IS NULL
        EXEC(N'ALTER TABLE dbo.Dispute ADD CONSTRAINT FK_Dispute_User_AssignedTo FOREIGN KEY (assignedTo) REFERENCES dbo.[User](id)');
    IF OBJECT_ID(N'FK_Dispute_User_ResolvedBy', N'F') IS NULL
        EXEC(N'ALTER TABLE dbo.Dispute ADD CONSTRAINT FK_Dispute_User_ResolvedBy FOREIGN KEY (resolvedBy) REFERENCES dbo.[User](id)');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.User') AND name=N'IX_User_ModerationStatus') EXEC(N'CREATE INDEX IX_User_ModerationStatus ON dbo.[User](moderationStatus)');
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Product') AND name=N'IX_Product_ModerationStatus') EXEC(N'CREATE INDEX IX_Product_ModerationStatus ON dbo.Product(moderationStatus)');
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Review') AND name=N'IX_Review_ModerationStatus') EXEC(N'CREATE INDEX IX_Review_ModerationStatus ON dbo.Review(moderationStatus)');
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.AdminAuditLog') AND name=N'IX_AdminAuditLog_CreatedAtUtc') EXEC(N'CREATE INDEX IX_AdminAuditLog_CreatedAtUtc ON dbo.AdminAuditLog(createdAtUtc DESC)');
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.AdminAuditLog') AND name=N'IX_AdminAuditLog_Resource') EXEC(N'CREATE INDEX IX_AdminAuditLog_Resource ON dbo.AdminAuditLog(resourceType, resourceId)');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
