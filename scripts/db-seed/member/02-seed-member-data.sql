-- Member Service Seed Data for Development
-- This script inserts sample member data for development and testing

-- Insert sample members (matching the auth service users)
INSERT INTO Members (Id, FirstName, LastName, Email, PhoneNumber, DateOfBirth, Address, City, State, PostalCode, Country, MembershipType, MembershipStatus, JoinDate, ExpiryDate, MaxBooksAllowed, CurrentBooksCount, LibraryCardNumber) VALUES
(
    '00000000-0000-0000-0000-000000000004', -- member1 from auth
    'John',
    'Doe',
    'member1@example.com',
    '+1-555-0101',
    '1985-03-15',
    '123 Main Street, Apt 4B',
    'New York',
    'NY',
    '10001',
    'USA',
    'Standard',
    'Active',
    '2023-01-15',
    '2024-01-15',
    5,
    2,
    'LIB0000001'
),
(
    '00000000-0000-0000-0000-000000000005', -- member2 from auth
    'Jane',
    'Smith',
    'member2@example.com',
    '+1-555-0102',
    '1990-07-22',
    '456 Oak Avenue',
    'Los Angeles',
    'CA',
    '90210',
    'USA',
    'Premium',
    'Active',
    '2023-02-01',
    '2024-02-01',
    10,
    0,
    'LIB0000002'
),
(
    '00000000-0000-0000-0000-000000000006', -- member3 from auth
    'Emily',
    'Davis',
    'member3@example.com',
    '+1-555-0103',
    '1995-11-08',
    '789 Pine Street',
    'Chicago',
    'IL',
    '60601',
    'USA',
    'Student',
    'Active',
    '2023-09-01',
    '2024-09-01',
    7,
    1,
    'LIB0000003'
),
(
    'a0000000-0000-0000-0000-000000000001', -- Additional member for testing
    'Michael',
    'Johnson',
    'michael.johnson@example.com',
    '+1-555-0104',
    '1978-05-12',
    '321 Elm Street',
    'Boston',
    'MA',
    '02101',
    'USA',
    'Senior',
    'Active',
    '2022-06-01',
    '2024-06-01',
    8,
    0,
    'LIB0000004'
),
(
    'a0000000-0000-0000-0000-000000000002', -- Additional member for testing
    'Sarah',
    'Wilson',
    'sarah.wilson@example.com',
    '+1-555-0105',
    '1988-12-03',
    '654 Maple Drive',
    'Seattle',
    'WA',
    '98101',
    'USA',
    'Premium',
    'Active',
    '2023-03-15',
    '2024-03-15',
    10,
    3,
    'LIB0000005'
),
(
    'a0000000-0000-0000-0000-000000000003', -- Suspended member for testing
    'Robert',
    'Brown',
    'robert.brown@example.com',
    '+1-555-0106',
    '1992-08-27',
    '987 Cedar Lane',
    'Denver',
    'CO',
    '80201',
    'USA',
    'Standard',
    'Suspended',
    '2023-01-01',
    '2024-01-01',
    5,
    2,
    'LIB0000006'
);

-- Insert member preferences
INSERT INTO MemberPreferences (MemberId, PreferredGenres, NotificationEmail, NotificationSms, AutoRenewBooks, ReceiveRecommendations, PreferredLanguage) VALUES
(
    '00000000-0000-0000-0000-000000000004', -- John Doe
    ARRAY['Fiction', 'Science Fiction', 'Fantasy'],
    true,
    false,
    true,
    true,
    'English'
),
(
    '00000000-0000-0000-0000-000000000005', -- Jane Smith
    ARRAY['Romance', 'Biography', 'History'],
    true,
    true,
    false,
    true,
    'English'
),
(
    '00000000-0000-0000-0000-000000000006', -- Emily Davis
    ARRAY['Technology', 'Business', 'Non-Fiction'],
    true,
    false,
    true,
    false,
    'English'
),
(
    'a0000000-0000-0000-0000-000000000001', -- Michael Johnson
    ARRAY['History', 'Biography', 'Fiction'],
    false,
    false,
    false,
    true,
    'English'
),
(
    'a0000000-0000-0000-0000-000000000002', -- Sarah Wilson
    ARRAY['Mystery', 'Fiction', 'Romance'],
    true,
    true,
    true,
    true,
    'English'
),
(
    'a0000000-0000-0000-0000-000000000003', -- Robert Brown
    ARRAY['Science Fiction', 'Technology'],
    true,
    false,
    false,
    false,
    'English'
);

-- Insert member activity logs
INSERT INTO MemberActivityLogs (MemberId, ActivityType, Description, RelatedEntityId, RelatedEntityType, ActivityDate) VALUES
(
    '00000000-0000-0000-0000-000000000004',
    'BookBorrowed',
    'Borrowed "The Lord of the Rings: The Fellowship of the Ring"',
    '20000000-0000-0000-0000-000000000004',
    'Book',
    NOW() - INTERVAL '5 days'
),
(
    '00000000-0000-0000-0000-000000000005',
    'BookBorrowed',
    'Borrowed "To Kill a Mockingbird"',
    '20000000-0000-0000-0000-000000000002',
    'Book',
    NOW() - INTERVAL '10 days'
),
(
    '00000000-0000-0000-0000-000000000005',
    'BookReturned',
    'Returned "To Kill a Mockingbird"',
    '20000000-0000-0000-0000-000000000002',
    'Book',
    NOW() - INTERVAL '2 days'
),
(
    '00000000-0000-0000-0000-000000000006',
    'BookBorrowed',
    'Borrowed "Sapiens: A Brief History of Humankind"',
    '20000000-0000-0000-0000-000000000010',
    'Book',
    NOW() - INTERVAL '7 days'
),
(
    '00000000-0000-0000-0000-000000000004',
    'MembershipRenewed',
    'Renewed membership for another year',
    '00000000-0000-0000-0000-000000000004',
    'Member',
    NOW() - INTERVAL '30 days'
),
(
    'a0000000-0000-0000-0000-000000000002',
    'BookBorrowed',
    'Borrowed multiple books',
    NULL,
    'Book',
    NOW() - INTERVAL '15 days'
);

-- Insert member fees
INSERT INTO MemberFees (MemberId, FeeType, Amount, Description, IsPaid, DueDate, PaymentMethod) VALUES
(
    '00000000-0000-0000-0000-000000000004',
    'MembershipFee',
    25.00,
    'Annual membership fee for 2024',
    true,
    '2024-01-15',
    'Card'
),
(
    '00000000-0000-0000-0000-000000000005',
    'MembershipFee',
    50.00,
    'Premium membership fee for 2024',
    true,
    '2024-02-01',
    'Online'
),
(
    '00000000-0000-0000-0000-000000000006',
    'MembershipFee',
    15.00,
    'Student membership fee for 2024',
    true,
    '2024-09-01',
    'Cash'
),
(
    'a0000000-0000-0000-0000-000000000003',
    'LateFee',
    5.00,
    'Late return fee for overdue book',
    false,
    NOW() + INTERVAL '7 days',
    NULL
),
(
    'a0000000-0000-0000-0000-000000000001',
    'MembershipFee',
    20.00,
    'Senior membership fee for 2024',
    true,
    '2024-06-01',
    'Card'
);

-- Update outstanding fees based on unpaid fees
UPDATE Members 
SET OutstandingFees = (
    SELECT COALESCE(SUM(Amount), 0) 
    FROM MemberFees 
    WHERE MemberFees.MemberId = Members.Id AND IsPaid = false
);

-- Insert member notifications
INSERT INTO MemberNotifications (MemberId, Type, Subject, Message, IsSent, SentAt) VALUES
(
    '00000000-0000-0000-0000-000000000004',
    'DueReminder',
    'Book Due Soon',
    'Your book "The Lord of the Rings: The Fellowship of the Ring" is due in 3 days. Please return or renew it to avoid late fees.',
    true,
    NOW() - INTERVAL '1 day'
),
(
    '00000000-0000-0000-0000-000000000005',
    'ReservationReady',
    'Reserved Book Available',
    'Your reserved book "The Lord of the Rings: The Fellowship of the Ring" is now available for pickup.',
    true,
    NOW() - INTERVAL '2 hours'
),
(
    '00000000-0000-0000-0000-000000000006',
    'MembershipExpiry',
    'Membership Expiring Soon',
    'Your library membership will expire in 30 days. Please renew to continue enjoying our services.',
    false,
    NULL
),
(
    'a0000000-0000-0000-0000-000000000003',
    'OverdueNotice',
    'Overdue Books',
    'You have overdue books. Please return them immediately to avoid additional fees.',
    true,
    NOW() - INTERVAL '3 days'
);

-- Mark some notifications as read
UPDATE MemberNotifications 
SET IsRead = true, ReadAt = NOW() - INTERVAL '1 hour'
WHERE Type = 'DueReminder';