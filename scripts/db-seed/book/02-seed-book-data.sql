-- Book Service Seed Data for Development
-- This script inserts sample books and categories for development and testing

-- Insert book categories first
INSERT INTO BookCategories (Id, Name, Description) VALUES
('10000000-0000-0000-0000-000000000001', 'Fiction', 'Fictional literature including novels, short stories, and novellas'),
('10000000-0000-0000-0000-000000000002', 'Non-Fiction', 'Factual books including biographies, history, and educational content'),
('10000000-0000-0000-0000-000000000003', 'Science Fiction', 'Speculative fiction dealing with futuristic concepts'),
('10000000-0000-0000-0000-000000000004', 'Fantasy', 'Fiction involving magical or supernatural elements'),
('10000000-0000-0000-0000-000000000005', 'Mystery', 'Fiction dealing with puzzling crimes or unexplained events'),
('10000000-0000-0000-0000-000000000006', 'Romance', 'Fiction focusing on romantic relationships'),
('10000000-0000-0000-0000-000000000007', 'Biography', 'Life stories of real people'),
('10000000-0000-0000-0000-000000000008', 'History', 'Books about historical events and periods'),
('10000000-0000-0000-0000-000000000009', 'Technology', 'Books about technology, programming, and computing'),
('10000000-0000-0000-0000-000000000010', 'Business', 'Books about business, management, and entrepreneurship');

-- Insert sample books
INSERT INTO Books (Id, Title, Author, ISBN, Publisher, PublishedDate, Genre, Description, Language, PageCount, Status, TotalCopies, AvailableCopies, Location) VALUES
(
    '20000000-0000-0000-0000-000000000001',
    'The Great Gatsby',
    'F. Scott Fitzgerald',
    '9780743273565',
    'Scribner',
    '1925-04-10',
    'Fiction',
    'A classic American novel about the Jazz Age and the American Dream.',
    'English',
    180,
    0, -- Available
    3,
    3,
    'A-01-001'
),
(
    '20000000-0000-0000-0000-000000000002',
    'To Kill a Mockingbird',
    'Harper Lee',
    '9780060935467',
    'J.B. Lippincott & Co.',
    '1960-07-11',
    'Fiction',
    'A gripping tale of racial injustice and childhood innocence.',
    'English',
    324,
    0, -- Available
    2,
    1,
    'A-01-002'
),
(
    '20000000-0000-0000-0000-000000000003',
    'Dune',
    'Frank Herbert',
    '9780441172719',
    'Chilton Books',
    '1965-08-01',
    'Science Fiction',
    'Epic science fiction novel set in a distant future.',
    'English',
    688,
    0, -- Available
    2,
    2,
    'B-02-001'
),
(
    '20000000-0000-0000-0000-000000000004',
    'The Lord of the Rings: The Fellowship of the Ring',
    'J.R.R. Tolkien',
    '9780544003415',
    'George Allen & Unwin',
    '1954-07-29',
    'Fantasy',
    'The first volume of the epic fantasy trilogy.',
    'English',
    432,
    1, -- Borrowed
    2,
    1,
    'B-02-015'
),
(
    '20000000-0000-0000-0000-000000000005',
    'The Catcher in the Rye',
    'J.D. Salinger',
    '9780316769488',
    'Little, Brown and Company',
    '1951-07-16',
    'Fiction',
    'Coming-of-age story of Holden Caulfield.',
    'English',
    234,
    0, -- Available
    1,
    1,
    'A-01-015'
),
(
    '20000000-0000-0000-0000-000000000006',
    'Pride and Prejudice',
    'Jane Austen',
    '9780141439518',
    'T. Egerton',
    '1813-01-28',
    'Romance',
    'Classic romance novel about Elizabeth Bennet and Mr. Darcy.',
    'English',
    279,
    0, -- Available
    2,
    2,
    'A-01-020'
),
(
    '20000000-0000-0000-0000-000000000007',
    'The Hitchhiker''s Guide to the Galaxy',
    'Douglas Adams',
    '9780345391803',
    'Pan Books',
    '1979-10-12',
    'Science Fiction',
    'Comedic science fiction series following Arthur Dent.',
    'English',
    224,
    0, -- Available
    1,
    1,
    'B-02-005'
),
(
    '20000000-0000-0000-0000-000000000008',
    'Clean Code: A Handbook of Agile Software Craftsmanship',
    'Robert C. Martin',
    '9780132350884',
    'Prentice Hall',
    '2008-08-01',
    'Technology',
    'Guide to writing clean, maintainable code.',
    'English',
    464,
    0, -- Available
    3,
    3,
    'C-03-001'
),
(
    '20000000-0000-0000-0000-000000000009',
    'The Lean Startup',
    'Eric Ries',
    '9780307887894',
    'Crown Business',
    '2011-09-13',
    'Business',
    'How constant innovation creates successful businesses.',
    'English',
    336,
    0, -- Available
    2,
    2,
    'C-03-025'
),
(
    '20000000-0000-0000-0000-000000000010',
    'Sapiens: A Brief History of Humankind',
    'Yuval Noah Harari',
    '9780062316097',
    'Harvill Secker',
    '2011-01-01',
    'History',
    'Exploration of the history and impact of Homo sapiens.',
    'English',
    443,
    0, -- Available
    2,
    1,
    'D-04-010'
);

-- Create book category mappings
INSERT INTO BookCategoryMappings (BookId, CategoryId) VALUES
('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001'), -- The Great Gatsby -> Fiction
('20000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001'), -- To Kill a Mockingbird -> Fiction
('20000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003'), -- Dune -> Science Fiction
('20000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000004'), -- LOTR -> Fantasy
('20000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000001'), -- Catcher in the Rye -> Fiction
('20000000-0000-0000-0000-000000000006', '10000000-0000-0000-0000-000000000006'), -- Pride and Prejudice -> Romance
('20000000-0000-0000-0000-000000000006', '10000000-0000-0000-0000-000000000001'), -- Pride and Prejudice -> Fiction
('20000000-0000-0000-0000-000000000007', '10000000-0000-0000-0000-000000000003'), -- Hitchhiker's Guide -> Science Fiction
('20000000-0000-0000-0000-000000000008', '10000000-0000-0000-0000-000000000009'), -- Clean Code -> Technology
('20000000-0000-0000-0000-000000000009', '10000000-0000-0000-0000-000000000010'), -- Lean Startup -> Business
('20000000-0000-0000-0000-000000000010', '10000000-0000-0000-0000-000000000008'); -- Sapiens -> History

-- Insert some borrowing records (using member IDs from auth service)
INSERT INTO BorrowingRecords (Id, BookId, MemberId, BorrowedAt, DueDate, IsReturned) VALUES
(
    '30000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000004', -- LOTR
    '00000000-0000-0000-0000-000000000004', -- member1
    NOW() - INTERVAL '5 days',
    NOW() + INTERVAL '9 days',
    false
),
(
    '30000000-0000-0000-0000-000000000002',
    '20000000-0000-0000-0000-000000000002', -- To Kill a Mockingbird
    '00000000-0000-0000-0000-000000000005', -- member2
    NOW() - INTERVAL '10 days',
    NOW() - INTERVAL '3 days',
    true
),
(
    '30000000-0000-0000-0000-000000000003',
    '20000000-0000-0000-0000-000000000010', -- Sapiens
    '00000000-0000-0000-0000-000000000006', -- member3
    NOW() - INTERVAL '7 days',
    NOW() + INTERVAL '7 days',
    false
);

-- Update the returned record
UPDATE BorrowingRecords 
SET ReturnedAt = NOW() - INTERVAL '2 days'
WHERE Id = '30000000-0000-0000-0000-000000000002';

-- Insert some book reservations
INSERT INTO BookReservations (Id, BookId, MemberId, ReservedAt, ExpiresAt, IsActive) VALUES
(
    '40000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000004', -- LOTR (currently borrowed)
    '00000000-0000-0000-0000-000000000005', -- member2
    NOW() - INTERVAL '1 day',
    NOW() + INTERVAL '6 days',
    true
);

-- Insert some book reviews
INSERT INTO BookReviews (Id, BookId, MemberId, Rating, ReviewText) VALUES
(
    '50000000-0000-0000-0000-000000000001',
    '20000000-0000-0000-0000-000000000001', -- The Great Gatsby
    '00000000-0000-0000-0000-000000000004', -- member1
    5,
    'A timeless classic that captures the essence of the American Dream. Beautifully written and deeply moving.'
),
(
    '50000000-0000-0000-0000-000000000002',
    '20000000-0000-0000-0000-000000000003', -- Dune
    '00000000-0000-0000-0000-000000000005', -- member2
    4,
    'Complex and immersive world-building. Takes time to get into but very rewarding.'
),
(
    '50000000-0000-0000-0000-000000000003',
    '20000000-0000-0000-0000-000000000008', -- Clean Code
    '00000000-0000-0000-0000-000000000006', -- member3
    5,
    'Essential reading for any software developer. Changed how I think about writing code.'
);