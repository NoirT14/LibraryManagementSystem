SET IDENTITY_INSERT LibraryDatabase.dbo.publishers ON;
INSERT INTO LibraryDatabase.dbo.publishers (publisher_id, publisher_name)
VALUES (2, N'NXB Giáo Dục'),
       (4, N'NXB Khoa Học'),
       (1, N'NXB Kim Đồng'),
       (3, N'NXB Trẻ'),
       (5, N'NXB Văn Học');
SET IDENTITY_INSERT LibraryDatabase.dbo.publishers OFF;
