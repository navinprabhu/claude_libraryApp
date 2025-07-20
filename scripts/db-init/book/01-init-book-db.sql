-- Book Service Database Initialization Script
-- This script sets up the initial schema for the Book Service

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create books table
CREATE TABLE IF NOT EXISTS Books (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Title VARCHAR(500) NOT NULL,
    Author VARCHAR(300) NOT NULL,
    ISBN VARCHAR(20) UNIQUE,
    Publisher VARCHAR(200),
    PublishedDate DATE,
    Genre VARCHAR(100),
    Description TEXT,
    Language VARCHAR(50) DEFAULT 'English',
    PageCount INTEGER,
    Status INTEGER NOT NULL DEFAULT 0, -- 0: Available, 1: Borrowed, 2: Reserved, 3: Maintenance
    TotalCopies INTEGER NOT NULL DEFAULT 1,
    AvailableCopies INTEGER NOT NULL DEFAULT 1,
    Location VARCHAR(100), -- Shelf location
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for books
CREATE INDEX IF NOT EXISTS idx_books_title ON Books(Title);
CREATE INDEX IF NOT EXISTS idx_books_author ON Books(Author);
CREATE INDEX IF NOT EXISTS idx_books_isbn ON Books(ISBN);
CREATE INDEX IF NOT EXISTS idx_books_genre ON Books(Genre);
CREATE INDEX IF NOT EXISTS idx_books_status ON Books(Status);

-- Create borrowing records table
CREATE TABLE IF NOT EXISTS BorrowingRecords (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    BookId UUID NOT NULL REFERENCES Books(Id) ON DELETE CASCADE,
    MemberId UUID NOT NULL, -- Reference to Member service
    BorrowedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    DueDate TIMESTAMP WITH TIME ZONE NOT NULL,
    ReturnedAt TIMESTAMP WITH TIME ZONE,
    IsReturned BOOLEAN NOT NULL DEFAULT false,
    LateFee DECIMAL(10,2) DEFAULT 0.00,
    Notes TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for borrowing records
CREATE INDEX IF NOT EXISTS idx_borrowing_records_book_id ON BorrowingRecords(BookId);
CREATE INDEX IF NOT EXISTS idx_borrowing_records_member_id ON BorrowingRecords(MemberId);
CREATE INDEX IF NOT EXISTS idx_borrowing_records_borrowed_at ON BorrowingRecords(BorrowedAt);
CREATE INDEX IF NOT EXISTS idx_borrowing_records_due_date ON BorrowingRecords(DueDate);
CREATE INDEX IF NOT EXISTS idx_borrowing_records_is_returned ON BorrowingRecords(IsReturned);

-- Create book reservations table
CREATE TABLE IF NOT EXISTS BookReservations (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    BookId UUID NOT NULL REFERENCES Books(Id) ON DELETE CASCADE,
    MemberId UUID NOT NULL, -- Reference to Member service
    ReservedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT true,
    NotificationSent BOOLEAN NOT NULL DEFAULT false,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for reservations
CREATE INDEX IF NOT EXISTS idx_book_reservations_book_id ON BookReservations(BookId);
CREATE INDEX IF NOT EXISTS idx_book_reservations_member_id ON BookReservations(MemberId);
CREATE INDEX IF NOT EXISTS idx_book_reservations_expires_at ON BookReservations(ExpiresAt);
CREATE INDEX IF NOT EXISTS idx_book_reservations_is_active ON BookReservations(IsActive);

-- Create book categories table
CREATE TABLE IF NOT EXISTS BookCategories (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Name VARCHAR(100) NOT NULL UNIQUE,
    Description TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create book category mapping table
CREATE TABLE IF NOT EXISTS BookCategoryMappings (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    BookId UUID NOT NULL REFERENCES Books(Id) ON DELETE CASCADE,
    CategoryId UUID NOT NULL REFERENCES BookCategories(Id) ON DELETE CASCADE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(BookId, CategoryId)
);

-- Create indexes for category mappings
CREATE INDEX IF NOT EXISTS idx_book_category_mappings_book_id ON BookCategoryMappings(BookId);
CREATE INDEX IF NOT EXISTS idx_book_category_mappings_category_id ON BookCategoryMappings(CategoryId);

-- Create book reviews table
CREATE TABLE IF NOT EXISTS BookReviews (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    BookId UUID NOT NULL REFERENCES Books(Id) ON DELETE CASCADE,
    MemberId UUID NOT NULL, -- Reference to Member service
    Rating INTEGER NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    ReviewText TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(BookId, MemberId) -- One review per member per book
);

-- Create indexes for reviews
CREATE INDEX IF NOT EXISTS idx_book_reviews_book_id ON BookReviews(BookId);
CREATE INDEX IF NOT EXISTS idx_book_reviews_member_id ON BookReviews(MemberId);
CREATE INDEX IF NOT EXISTS idx_book_reviews_rating ON BookReviews(Rating);

-- Create function to update the UpdatedAt timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.UpdatedAt = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers to automatically update UpdatedAt
CREATE TRIGGER update_books_updated_at
    BEFORE UPDATE ON Books
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_borrowing_records_updated_at
    BEFORE UPDATE ON BorrowingRecords
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_book_reservations_updated_at
    BEFORE UPDATE ON BookReservations
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_book_reviews_updated_at
    BEFORE UPDATE ON BookReviews
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Grant permissions to the book user
GRANT USAGE ON SCHEMA public TO book_user;
GRANT USAGE ON SCHEMA public TO book_dev_user;

GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO book_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO book_dev_user;

GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO book_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO book_dev_user;