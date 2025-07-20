-- Auth Service Seed Data for Development
-- This script inserts sample data for development and testing

-- Insert default admin user (password: Admin@123)
INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, Role, IsActive) VALUES
(
    '00000000-0000-0000-0000-000000000001',
    'admin',
    'admin@libraryapp.com',
    '$2a$11$k6xQrC5zOqKj8LGqU5qKneCLALtZ9XzKEWcJ7SdJhX2vQ4eP2wqLK', -- Admin@123
    'System',
    'Administrator',
    'Administrator',
    true
),
(
    '00000000-0000-0000-0000-000000000002',
    'librarian1',
    'librarian1@libraryapp.com',
    '$2a$11$k6xQrC5zOqKj8LGqU5qKneCLALtZ9XzKEWcJ7SdJhX2vQ4eP2wqLK', -- Admin@123
    'Sarah',
    'Johnson',
    'Librarian',
    true
),
(
    '00000000-0000-0000-0000-000000000003',
    'librarian2',
    'librarian2@libraryapp.com',
    '$2a$11$k6xQrC5zOqKj8LGqU5qKneCLALtZ9XzKEWcJ7SdJhX2vQ4eP2wqLK', -- Admin@123
    'Michael',
    'Chen',
    'Librarian',
    true
),
(
    '00000000-0000-0000-0000-000000000004',
    'member1',
    'member1@example.com',
    '$2a$11$8Ec2PqQ7TLKfJKjU7qKfJeCLALtZ9XzKEWcJ7SdJhX2vQ4eP2wqLK', -- Member@123
    'John',
    'Doe',
    'Member',
    true
),
(
    '00000000-0000-0000-0000-000000000005',
    'member2',
    'member2@example.com',
    '$2a$11$8Ec2PqQ7TLKfJKjU7qKfJeCLALtZ9XzKEWcJ7SdJhX2vQ4eP2wqLK', -- Member@123
    'Jane',
    'Smith',
    'Member',
    true
),
(
    '00000000-0000-0000-0000-000000000006',
    'member3',
    'member3@example.com',
    '$2a$11$8Ec2PqQ7TLKfJKjU7qKfJeCLALtZ9XzKEWcJ7SdJhX2vQ4eP2wqLK', -- Member@123
    'Emily',
    'Davis',
    'Member',
    true
);

-- Insert sample audit logs
INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId, IpAddress, UserAgent) VALUES
(
    '00000000-0000-0000-0000-000000000001',
    'UserCreated',
    'User',
    '00000000-0000-0000-0000-000000000001',
    '127.0.0.1',
    'System Initialization'
),
(
    '00000000-0000-0000-0000-000000000002',
    'UserCreated',
    'User',
    '00000000-0000-0000-0000-000000000002',
    '127.0.0.1',
    'System Initialization'
),
(
    '00000000-0000-0000-0000-000000000003',
    'UserCreated',
    'User',
    '00000000-0000-0000-0000-000000000003',
    '127.0.0.1',
    'System Initialization'
);

-- Update LastLoginAt for some users to simulate activity
UPDATE Users 
SET LastLoginAt = NOW() - INTERVAL '1 day'
WHERE Username IN ('admin', 'librarian1');

UPDATE Users 
SET LastLoginAt = NOW() - INTERVAL '3 days'
WHERE Username = 'member1';

UPDATE Users 
SET LastLoginAt = NOW() - INTERVAL '1 week'
WHERE Username = 'member2';