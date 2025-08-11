# Service Topology & Ports

## Service Ports
```
API Gateway (Ocelot):     5000  → http://localhost:5000
Auth Service:             5001  → http://localhost:5001
Book Service:             5002  → http://localhost:5002  
Member Service:           5003  → http://localhost:5003

Databases:
Auth Database:            5432  → localhost:5432
Book Database:            5433  → localhost:5433
Member Database:          5434  → localhost:5434
Redis Cache:              6379  → localhost:6379
```

## Container Names (Docker)
```
libraryapp-api-gateway     # API Gateway
libraryapp-auth-service    # Auth Service
libraryapp-book-service    # Book Service  
libraryapp-member-service  # Member Service

libraryapp-auth-db         # Auth PostgreSQL Database
libraryapp-book-db         # Book PostgreSQL Database
libraryapp-member-db       # Member PostgreSQL Database
libraryapp-redis           # Redis Cache
```

## Service Dependencies
```
API Gateway → All Services (routing)
├── Depends on: auth-service, book-service, member-service, redis
├── Health check: /health/simple
└── Startup order: Last (after all other services)

Book Service → Auth Service (JWT validation)  
├── Depends on: book-db, redis, auth-service
├── Health check: /health
└── Validates JWT tokens issued by AuthService

Member Service → Auth Service + Book Service (borrowing info)
├── Depends on: member-db, redis, auth-service  
├── Health check: /health
└── Calls BookService for borrowing operations

Auth Service → Standalone (JWT issuer)
├── Depends on: auth-db, redis
├── Health check: /health  
└── Issues JWT tokens for other services
```

## Internal Docker Network (libraryapp-network)
```
Service URLs (inside Docker network):
├── ServiceUrls__AuthService=http://auth-service:5001
├── ServiceUrls__BookService=http://book-service:5002  
└── ServiceUrls__MemberService=http://member-service:5003

Database URLs (inside Docker network):
├── auth-db:5432
├── book-db:5432 (internal port, external 5433)
├── member-db:5432 (internal port, external 5434)  
└── redis:6379
```

## Health Check Endpoints
```bash
# From host machine
curl http://localhost:5001/health  # Auth Service
curl http://localhost:5002/health  # Book Service
curl http://localhost:5003/health  # Member Service  
curl http://localhost:5000/health/simple  # Gateway (Docker health check)
curl http://localhost:5000/health         # Gateway (full health check)

# Database health (from host)
docker exec libraryapp-auth-db pg_isready -U auth_user -d AuthDatabase
docker exec libraryapp-book-db pg_isready -U book_user -d BookDatabase
docker exec libraryapp-member-db pg_isready -U member_user -d MemberDatabase
docker exec libraryapp-redis redis-cli ping
```

## API Routes (via Gateway)
```
Frontend Requests → API Gateway:5000 → Target Service

/api/auth/login          → auth-service:5001/api/auth/login
/api/auth/validate       → auth-service:5001/api/auth/validate
/api/auth/refresh        → auth-service:5001/api/auth/refresh

/api/books               → book-service:5002/api/books  
/api/books/{id}          → book-service:5002/api/books/{id}
/api/books/{id}/borrow   → book-service:5002/api/books/{id}/borrow
/api/books/{id}/return   → book-service:5002/api/books/{id}/return

/api/members             → member-service:5003/api/members
/api/members/{id}        → member-service:5003/api/members/{id}
/api/members/{id}/borrowed-books → member-service:5003/api/members/{id}/borrowed-books
```

## Rate Limiting (Ocelot Configuration)
```
Auth Service:     100 requests/minute per IP
Book Service:     200 requests/minute per IP  
Member Service:   150 requests/minute per IP
```

## Database Schemas
```sql
-- Auth Database (auth_user@AuthDatabase:5432)
Tables: users, roles, refresh_tokens

-- Book Database (book_user@BookDatabase:5433)  
Tables: books, borrowing_records, categories

-- Member Database (member_user@MemberDatabase:5434)
Tables: members, member_preferences, borrowing_history
```

## Environment Variables by Service
```bash
# Auth Service
ASPNETCORE_URLS=http://+:5001
ConnectionStrings__DefaultConnection=Host=auth-db;Port=5432;Database=AuthDatabase;Username=auth_user;Password=auth_password123
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong123456789
Redis__ConnectionString=redis:6379,password=redis_password123

# Book Service  
ASPNETCORE_URLS=http://+:5002
ConnectionStrings__DefaultConnection=Host=book-db;Port=5432;Database=BookDatabase;Username=book_user;Password=book_password123
ServiceUrls__AuthService=http://auth-service:5001
Redis__ConnectionString=redis:6379,password=redis_password123

# Member Service
ASPNETCORE_URLS=http://+:5003  
ConnectionStrings__DefaultConnection=Host=member-db;Port=5432;Database=MemberDatabase;Username=member_user;Password=member_password123
ServiceUrls__AuthService=http://auth-service:5001
ServiceUrls__BookService=http://book-service:5002
Redis__ConnectionString=redis:6379,password=redis_password123

# API Gateway
ASPNETCORE_URLS=http://+:5000
ServiceUrls__AuthService=http://auth-service:5001  
ServiceUrls__BookService=http://book-service:5002
ServiceUrls__MemberService=http://member-service:5003
Redis__ConnectionString=redis:6379,password=redis_password123
```

## Startup Sequence
```
1. Infrastructure (databases, Redis)
   ├── auth-db, book-db, member-db (PostgreSQL containers)
   └── redis (Redis container)

2. Core Services  
   ├── auth-service (no service dependencies)
   ├── book-service (depends on auth-service)
   └── member-service (depends on auth-service)

3. API Gateway
   └── api-gateway (depends on all services)
```