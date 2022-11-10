CREATE TABLE dbo.ToDo (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [order] INT NULL,
    [title] NVARCHAR(200) NOT NULL,
    [url] NVARCHAR(200) NOT NULL,
    [completed] BIT NOT NULL
);
GO


CREATE PROCEDURE [dbo].[DeleteToDo]
    @Id NVARCHAR(100)
AS
    DECLARE @UID UNIQUEIDENTIFIER = TRY_CAST(@ID AS UNIQUEIDENTIFIER)
    IF @UId IS NOT NULL AND @Id != ''
    BEGIN
        DELETE FROM dbo.ToDo WHERE Id = @UID
    END
    ELSE
    BEGIN
        DELETE FROM dbo.ToDo WHERE @ID = ''
    END

    SELECT [Id], [order], [title], [url], [completed] FROM dbo.ToDo
GO
