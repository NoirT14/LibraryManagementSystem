SET IDENTITY_INSERT LibraryDatabase.dbo.authors ON;
INSERT INTO LibraryDatabase.dbo.authors (author_id, author_name, bio)
VALUES (1, N'Nguyễn Nhật Ánh', N'Nhà văn nổi tiếng với truyện thiếu nhi.'),
       (2, N'Hồ Chí Minh', N'Lãnh tụ cách mạng.'),
       (3, N'J.K. Rowling', N'Tác giả bộ Harry Potter.'),
       (4, N'Isaac Newton', N'Nhà vật lý vĩ đại.'),
       (5, N'Nguyễn Văn A', null),
       (6, N'Nguyễn Văn B', null),
       (7, N'Nguyễn Văn C', null),
       (8, N'Nguyễn Văn D', null);
SET IDENTITY_INSERT LibraryDatabase.dbo.authors OFF;
