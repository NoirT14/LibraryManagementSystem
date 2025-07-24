SET IDENTITY_INSERT LibraryDatabase.dbo.reservations ON;
INSERT INTO LibraryDatabase.dbo.reservations (reservation_id, user_id, variant_id, reservation_date, expiration_date,
                                              reservation_status, fulfilled_copy_id, processed_by)
VALUES (2, 14, 2, N'2025-07-21 03:05:34.977', N'2025-08-05 10:00:00.000', N'Cancelled', null, 10),
       (7, 14, 2, N'2025-07-21 03:44:22.380', N'2025-07-28 03:44:22.380', N'Pending', null, null),
       (8, 15, 2, N'2025-07-21 03:44:40.513', N'2025-07-28 03:44:40.513', N'Pending', null, null);
SET IDENTITY_INSERT LibraryDatabase.dbo.reservations OFF;
