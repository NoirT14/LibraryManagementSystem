SET IDENTITY_INSERT LibraryDatabase.dbo.book_volumes ON;
INSERT INTO LibraryDatabase.dbo.book_volumes (volume_id, book_id, volume_number, volume_title, description)
VALUES (1, 1, 1, N'Mắt Biếc', null),
       (2, 2, 1, N'Harry Potter 1', null),
       (3, 3, 1, N'Harry Potter 2', null),
       (4, 4, 1, N'Những Người Khốn Khổ', null),
       (5, 5, 1, N'Lịch Sử Việt Nam', null),
       (6, 6, 1, N'Toán Cao Cấp', null),
       (7, 7, 1, N'Tin Học Đại Cương', null),
       (8, 8, 1, N'Kinh Tế Vi Mô', null),
       (9, 9, 1, N'Nhà Giả Kim', null),
       (10, 10, 1, N'Đắc Nhân Tâm', null),
       (11, 11, 1, N'Không Gia Đình', null),
       (12, 12, 1, N'Truyện Kiều', null),
       (13, 13, 1, N'Dế Mèn Phiêu Lưu Ký', null),
       (14, 14, 1, N'Tự Truyện Hồ Chí Minh', null),
       (15, 15, 1, N'Newton', null),
       (16, 16, 1, N'Cây Cam Ngọt', null),
       (17, 17, 1, N'Chí Phèo', null),
       (18, 18, 1, N'Số Đỏ', null),
       (19, 19, 1, N'Lập Trình C++', null),
       (20, 20, 1, N'Python cho Người mới bắt đầu', null);
SET IDENTITY_INSERT LibraryDatabase.dbo.book_volumes OFF;
