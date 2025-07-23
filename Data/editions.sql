SET IDENTITY_INSERT LibraryDatabase.dbo.editions ON;
INSERT INTO LibraryDatabase.dbo.editions (edition_id, edition_name)
VALUES (3, N'Bản đặc biệt'),
       (1, N'Tái bản lần 1'),
       (2, N'Tái bản lần 2');
SET IDENTITY_INSERT LibraryDatabase.dbo.editions OFF;
