SET IDENTITY_INSERT LibraryDatabase.dbo.roles ON;
INSERT INTO LibraryDatabase.dbo.roles (role_id, role_name)VALUES (1, N'Admin');

INSERT INTO LibraryDatabase.dbo.roles (role_id, role_name) VALUES (2, N'Staff');
INSERT INTO LibraryDatabase.dbo.roles (role_id, role_name) VALUES (3, N'User');
SET IDENTITY_INSERT LibraryDatabase.dbo.roles OFF;