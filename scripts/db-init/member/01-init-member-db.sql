-- Member Service Database Initialization Script
-- This script sets up the initial schema for the Member Service

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create members table
CREATE TABLE IF NOT EXISTS Members (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PhoneNumber VARCHAR(20),
    DateOfBirth DATE,
    Address TEXT,
    City VARCHAR(100),
    State VARCHAR(100),
    PostalCode VARCHAR(20),
    Country VARCHAR(100) DEFAULT 'USA',
    MembershipType VARCHAR(50) NOT NULL DEFAULT 'Standard', -- Standard, Premium, Student, Senior
    MembershipStatus VARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Suspended, Expired, Cancelled
    JoinDate DATE NOT NULL DEFAULT CURRENT_DATE,
    ExpiryDate DATE,
    MaxBooksAllowed INTEGER NOT NULL DEFAULT 5,
    CurrentBooksCount INTEGER NOT NULL DEFAULT 0,
    TotalLateFees DECIMAL(10,2) DEFAULT 0.00,
    OutstandingFees DECIMAL(10,2) DEFAULT 0.00,
    LibraryCardNumber VARCHAR(50) UNIQUE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for members
CREATE INDEX IF NOT EXISTS idx_members_email ON Members(Email);
CREATE INDEX IF NOT EXISTS idx_members_library_card_number ON Members(LibraryCardNumber);
CREATE INDEX IF NOT EXISTS idx_members_membership_status ON Members(MembershipStatus);
CREATE INDEX IF NOT EXISTS idx_members_membership_type ON Members(MembershipType);
CREATE INDEX IF NOT EXISTS idx_members_join_date ON Members(JoinDate);
CREATE INDEX IF NOT EXISTS idx_members_expiry_date ON Members(ExpiryDate);

-- Create member preferences table
CREATE TABLE IF NOT EXISTS MemberPreferences (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    MemberId UUID NOT NULL REFERENCES Members(Id) ON DELETE CASCADE,
    PreferredGenres TEXT[], -- Array of preferred book genres
    NotificationEmail BOOLEAN NOT NULL DEFAULT true,
    NotificationSms BOOLEAN NOT NULL DEFAULT false,
    AutoRenewBooks BOOLEAN NOT NULL DEFAULT false,
    ReceiveRecommendations BOOLEAN NOT NULL DEFAULT true,
    PreferredLanguage VARCHAR(50) DEFAULT 'English',
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create index for preferences
CREATE INDEX IF NOT EXISTS idx_member_preferences_member_id ON MemberPreferences(MemberId);

-- Create member activity log table
CREATE TABLE IF NOT EXISTS MemberActivityLogs (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    MemberId UUID NOT NULL REFERENCES Members(Id) ON DELETE CASCADE,
    ActivityType VARCHAR(100) NOT NULL, -- BookBorrowed, BookReturned, FeesPaid, MembershipRenewed, etc.
    Description TEXT,
    RelatedEntityId UUID, -- Could be BookId, TransactionId, etc.
    RelatedEntityType VARCHAR(100), -- Book, Transaction, etc.
    ActivityDate TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for activity logs
CREATE INDEX IF NOT EXISTS idx_member_activity_logs_member_id ON MemberActivityLogs(MemberId);
CREATE INDEX IF NOT EXISTS idx_member_activity_logs_activity_type ON MemberActivityLogs(ActivityType);
CREATE INDEX IF NOT EXISTS idx_member_activity_logs_activity_date ON MemberActivityLogs(ActivityDate);

-- Create member fees table
CREATE TABLE IF NOT EXISTS MemberFees (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    MemberId UUID NOT NULL REFERENCES Members(Id) ON DELETE CASCADE,
    FeeType VARCHAR(100) NOT NULL, -- LateFee, MembershipFee, DamageFee, etc.
    Amount DECIMAL(10,2) NOT NULL,
    Description TEXT,
    IsPaid BOOLEAN NOT NULL DEFAULT false,
    DueDate DATE,
    PaidDate DATE,
    PaymentMethod VARCHAR(50), -- Cash, Card, Online, etc.
    TransactionId VARCHAR(100),
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for fees
CREATE INDEX IF NOT EXISTS idx_member_fees_member_id ON MemberFees(MemberId);
CREATE INDEX IF NOT EXISTS idx_member_fees_fee_type ON MemberFees(FeeType);
CREATE INDEX IF NOT EXISTS idx_member_fees_is_paid ON MemberFees(IsPaid);
CREATE INDEX IF NOT EXISTS idx_member_fees_due_date ON MemberFees(DueDate);

-- Create member notifications table
CREATE TABLE IF NOT EXISTS MemberNotifications (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    MemberId UUID NOT NULL REFERENCES Members(MemberId) ON DELETE CASCADE,
    Type VARCHAR(100) NOT NULL, -- DueReminder, OverdueNotice, ReservationReady, etc.
    Subject VARCHAR(255) NOT NULL,
    Message TEXT NOT NULL,
    IsSent BOOLEAN NOT NULL DEFAULT false,
    SentAt TIMESTAMP WITH TIME ZONE,
    IsRead BOOLEAN NOT NULL DEFAULT false,
    ReadAt TIMESTAMP WITH TIME ZONE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for notifications
CREATE INDEX IF NOT EXISTS idx_member_notifications_member_id ON MemberNotifications(MemberId);
CREATE INDEX IF NOT EXISTS idx_member_notifications_type ON MemberNotifications(Type);
CREATE INDEX IF NOT EXISTS idx_member_notifications_is_sent ON MemberNotifications(IsSent);
CREATE INDEX IF NOT EXISTS idx_member_notifications_is_read ON MemberNotifications(IsRead);

-- Create member statistics view
CREATE OR REPLACE VIEW MemberStatistics AS
SELECT 
    m.Id,
    m.FirstName,
    m.LastName,
    m.Email,
    m.MembershipType,
    m.MembershipStatus,
    m.CurrentBooksCount,
    m.MaxBooksAllowed,
    m.OutstandingFees,
    COUNT(CASE WHEN mal.ActivityType = 'BookBorrowed' THEN 1 END) as TotalBooksBorrowed,
    COUNT(CASE WHEN mal.ActivityType = 'BookReturned' THEN 1 END) as TotalBooksReturned,
    MAX(mal.ActivityDate) as LastActivity
FROM Members m
LEFT JOIN MemberActivityLogs mal ON m.Id = mal.MemberId
GROUP BY m.Id, m.FirstName, m.LastName, m.Email, m.MembershipType, 
         m.MembershipStatus, m.CurrentBooksCount, m.MaxBooksAllowed, m.OutstandingFees;

-- Create function to generate library card number
CREATE OR REPLACE FUNCTION generate_library_card_number()
RETURNS TEXT AS $$
DECLARE
    card_number TEXT;
    is_unique BOOLEAN := FALSE;
BEGIN
    WHILE NOT is_unique LOOP
        -- Generate a 10-digit card number with LIB prefix
        card_number := 'LIB' || LPAD(FLOOR(RANDOM() * 10000000)::TEXT, 7, '0');
        
        -- Check if this number is already used
        SELECT NOT EXISTS(SELECT 1 FROM Members WHERE LibraryCardNumber = card_number) INTO is_unique;
    END LOOP;
    
    RETURN card_number;
END;
$$ LANGUAGE plpgsql;

-- Create function to update the UpdatedAt timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.UpdatedAt = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers to automatically update UpdatedAt
CREATE TRIGGER update_members_updated_at
    BEFORE UPDATE ON Members
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_member_preferences_updated_at
    BEFORE UPDATE ON MemberPreferences
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_member_fees_updated_at
    BEFORE UPDATE ON MemberFees
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Create trigger to auto-generate library card number
CREATE OR REPLACE FUNCTION assign_library_card_number()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.LibraryCardNumber IS NULL THEN
        NEW.LibraryCardNumber := generate_library_card_number();
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER assign_library_card_number_trigger
    BEFORE INSERT ON Members
    FOR EACH ROW
    EXECUTE FUNCTION assign_library_card_number();

-- Grant permissions to the member user (handle both production and dev users)
DO $$
BEGIN
    -- Grant permissions to member_user (production)
    IF EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'member_user') THEN
        GRANT USAGE ON SCHEMA public TO member_user;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO member_user;
        GRANT SELECT ON MemberStatistics TO member_user;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO member_user;
        RAISE NOTICE 'Permissions granted to member_user';
    END IF;
    
    -- Grant permissions to member_dev_user (development)  
    IF EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'member_dev_user') THEN
        GRANT USAGE ON SCHEMA public TO member_dev_user;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO member_dev_user;
        GRANT SELECT ON MemberStatistics TO member_dev_user;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO member_dev_user;
        RAISE NOTICE 'Permissions granted to member_dev_user';
    END IF;
END
$$;