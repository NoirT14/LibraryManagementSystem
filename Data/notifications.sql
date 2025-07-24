SET IDENTITY_INSERT LibraryDatabase.dbo.notifications ON;
INSERT INTO LibraryDatabase.dbo.notifications (notification_id, sender_id, sender_type, receiver_id, for_staff, message,
                                               notification_date, notification_type, read_status, handled_status,
                                               handled_by, handled_at, related_table, related_id)
VALUES (9, null, N'System', 4, 0, N'Dear Nam Hong Son,

The following loans are due soon:
- Loan ID: 2, Due on: 2025-07-02

Please return the books on time to avoid fines.

Thank you.', N'2025-07-01 14:10:18.777', N'DueDateReminder', 0, 0, null, null, null, null),
       (10, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $45,000.00 for an overdue loan (Loan ID: 1).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-01 14:20:19.847', N'FineNotice', 1, 0, null, null, N'loans', 1),
       (11, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $10,000.00 for an overdue loan (Loan ID: 3).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-01 14:20:23.903', N'FineNotice', 1, 0, null, null, N'loans', 3),
       (12, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $35,000.00 for an overdue loan (Loan ID: 4).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-01 14:20:27.497', N'FineNotice', 1, 0, null, null, N'loans', 4),
       (13, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $45,000.00 for an overdue loan (Loan ID: 1).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-14 22:15:36.737', N'FineNotice', 0, 0, null, null, N'loans', 1),
       (14, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $55,000.00 for an overdue loan (Loan ID: 2).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-14 22:15:40.067', N'FineNotice', 0, 0, null, null, N'loans', 2),
       (15, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $10,000.00 for an overdue loan (Loan ID: 3).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-14 22:15:43.227', N'FineNotice', 0, 0, null, null, N'loans', 3),
       (16, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $30,000.00 for an overdue loan (Loan ID: 4).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-14 22:15:46.537', N'FineNotice', 0, 0, null, null, N'loans', 4),
       (17, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $45,000.00 for an overdue loan (Loan ID: 1).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-20 20:40:27.297', N'FineNotice', 0, 0, null, null, N'loans', 1),
       (18, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $55,000.00 for an overdue loan (Loan ID: 2).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-20 20:40:30.747', N'FineNotice', 0, 0, null, null, N'loans', 2),
       (19, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $10,000.00 for an overdue loan (Loan ID: 3).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-20 20:40:33.703', N'FineNotice', 0, 0, null, null, N'loans', 3),
       (20, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $30,000.00 for an overdue loan (Loan ID: 4).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-20 20:40:36.883', N'FineNotice', 0, 0, null, null, N'loans', 4),
       (21, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $45,000.00 for an overdue loan (Loan ID: 1).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-21 00:00:46.157', N'FineNotice', 0, 0, null, null, N'loans', 1),
       (22, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $55,000.00 for an overdue loan (Loan ID: 2).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-21 00:00:49.720', N'FineNotice', 0, 0, null, null, N'loans', 2),
       (23, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $10,000.00 for an overdue loan (Loan ID: 3).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-21 00:00:53.723', N'FineNotice', 0, 0, null, null, N'loans', 3),
       (24, null, N'System', 4, 0, N'Dear Nam Hong Son,

You have an outstanding fine of $30,000.00 for an overdue loan (Loan ID: 4).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.', N'2025-07-21 00:00:56.727', N'FineNotice', 0, 0, null, null, N'loans', 4);
SET IDENTITY_INSERT LibraryDatabase.dbo.notifications OFF;
