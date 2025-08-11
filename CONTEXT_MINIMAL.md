# LibraryApp - Quick Context (~300 tokens vs 2500+ in CLAUDE.md)

## Project Type
.NET 8 microservices + React 19.1.0 frontend with dark/light mode

## Services & Ports
- Gateway(5000) → Auth(5001), Books(5002), Members(5003)
- Frontend: localhost:3000

## Load Context on Demand
**Say**: "Load development context" → `docs/context/development-context.md`
**Say**: "Load CI/CD context" → `docs/context/cicd-context.md`

## Recent Features
✅ Dark mode toggle (Settings + Login screen, cookie persistence)

## Quick Commands
```bash
.\scripts\start-dev.ps1 -Detached  # Start all
dotnet test                        # Test
curl http://localhost:5000/health/services  # Health
```

**Key**: Only load full context when task requires it.