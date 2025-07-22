SET IDENTITY_INSERT LibraryDatabase.dbo.paper_qualities ON;
INSERT INTO LibraryDatabase.dbo.paper_qualities (paper_quality_id, paper_quality_name)
VALUES (2, N'Giấy tái chế'),
       (1, N'Giấy tốt');
SET IDENTITY_INSERT LibraryDatabase.dbo.paper_qualities OFF;
