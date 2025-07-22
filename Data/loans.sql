SET IDENTITY_INSERT LibraryDatabase.dbo.loans ON;
INSERT INTO LibraryDatabase.dbo.loans (loan_id, user_id, copy_id, borrow_date, due_date, return_date, loan_status,
                                       fine_amount, extended, reservation_id)
VALUES (1, 4, 1, N'2025-06-16 07:17:39.203', N'2025-06-22 07:17:39.203', null, N'Overdue', 45000.00, 0, null),
       (2, 4, 2, N'2025-06-28 00:00:00.000', N'2025-07-02 00:00:00.000', null, N'Overdue', 55000.00, 0, null),
       (3, 4, 3, N'2025-06-20 00:00:00.000', N'2025-06-29 00:00:00.000', null, N'Overdue', 10000.00, 0, null),
       (4, 4, 4, N'2025-06-15 00:00:00.000', N'2025-06-25 00:00:00.000', null, N'Overdue', 30000.00, 0, null),
       (5, 3, 1, N'2025-06-30 02:38:24.040', N'2025-08-14 00:00:00.000', null, N'Borrowed', 0.00, 1, null);
SET IDENTITY_INSERT LibraryDatabase.dbo.loans OFF;
