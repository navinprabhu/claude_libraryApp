-- Auth Service Database Initialization Script
-- This script sets up the initial schema and permissions for the Auth Service

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create users table
CREATE TABLE IF NOT EXISTS Users (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Username VARCHAR(100) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(500) NOT NULL,
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    Role VARCHAR(50) NOT NULL DEFAULT 'Member',
    IsActive BOOLEAN NOT NULL DEFAULT true,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    LastLoginAt TIMESTAMP WITH TIME ZONE
);

-- Create index for faster lookups
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_role ON Users(Role);

-- Create refresh tokens table for JWT management
CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Token VARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    IsRevoked BOOLEAN NOT NULL DEFAULT false,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    RevokedAt TIMESTAMP WITH TIME ZONE
);

-- Create index for token lookups
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON RefreshTokens(UserId);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token ON RefreshTokens(Token);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires_at ON RefreshTokens(ExpiresAt);

-- Create user sessions table for tracking active sessions
CREATE TABLE IF NOT EXISTS UserSessions (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    SessionToken VARCHAR(500) NOT NULL UNIQUE,
    IpAddress INET,
    UserAgent TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT true
);

-- Create index for session management
CREATE INDEX IF NOT EXISTS idx_user_sessions_user_id ON UserSessions(UserId);
CREATE INDEX IF NOT EXISTS idx_user_sessions_token ON UserSessions(SessionToken);
CREATE INDEX IF NOT EXISTS idx_user_sessions_expires_at ON UserSessions(ExpiresAt);

-- Create audit log table for security tracking
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    UserId UUID REFERENCES Users(Id),
    Action VARCHAR(100) NOT NULL,
    EntityType VARCHAR(100),
    EntityId VARCHAR(100),
    OldValues JSONB,
    NewValues JSONB,
    IpAddress INET,
    UserAgent TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create index for audit log queries
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON AuditLogs(UserId);
CREATE INDEX IF NOT EXISTS idx_audit_logs_action ON AuditLogs(Action);
CREATE INDEX IF NOT EXISTS idx_audit_logs_created_at ON AuditLogs(CreatedAt);

-- Create function to update the UpdatedAt timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.UpdatedAt = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create trigger to automatically update UpdatedAt
CREATE TRIGGER update_users_updated_at
    BEFORE UPDATE ON Users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Grant permissions to the auth user (handle both production and dev users)
DO $$
BEGIN
    -- Grant permissions to auth_user (production)
    IF EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'auth_user') THEN
        GRANT USAGE ON SCHEMA public TO auth_user;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO auth_user;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO auth_user;
        RAISE NOTICE 'Permissions granted to auth_user';
    END IF;
    
    -- Grant permissions to auth_dev_user (development)  
    IF EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'auth_dev_user') THEN
        GRANT USAGE ON SCHEMA public TO auth_dev_user;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO auth_dev_user;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO auth_dev_user;
        RAISE NOTICE 'Permissions granted to auth_dev_user';
    END IF;
END
$$;