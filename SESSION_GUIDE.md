# Session Management Guide - Token Optimization

## 🎯 Start Every Session With Minimal Context

### **Option 1: Ultra-Minimal (150 tokens)**
```
LibraryApp: .NET 8 microservices + React 19.1.0 frontend
Services: Gateway(5000), Auth(5001), Books(5002), Members(5003)
Recent: Dark mode toggle added
Load context on demand when needed.
```

### **Option 2: Quick Context (300 tokens)**
Use: `CONTEXT_MINIMAL.md`

### **Option 3: Task-Specific (400 tokens)**  
- Frontend tasks: `CONTEXT_FRONTEND.md`
- Backend tasks: `CONTEXT_BACKEND.md`

## 🔄 Load Additional Context Only When Needed

**Instead of loading full CLAUDE.md (2500+ tokens):**

### For Development Tasks:
"Load development context" → `docs/context/development-context.md` 

### For CI/CD Issues:
"Load CI/CD context" → `docs/context/cicd-context.md`

### For Architecture Questions:
"Load architecture context" → `docs/context/architecture-context.md`

## 📊 Token Savings Comparison

| Context Type | Tokens | Use Case | Savings |
|-------------|--------|----------|---------|
| Ultra-minimal | 150 | Quick questions | 94% |
| CONTEXT_MINIMAL.md | 300 | Session start | 88% |
| Task-specific | 400 | Focused work | 84% |
| Full CLAUDE.md | 2500+ | Complex tasks | 0% |

## 🎯 Session Strategy Examples

### **New Session - Simple Question:**
```
User context: "LibraryApp .NET 8 + React. Dark mode added recently."
```

### **New Session - Development Work:**
```
User context: CONTEXT_FRONTEND.md (if frontend) or CONTEXT_BACKEND.md (if backend)
Then: "Load development context when needed"
```

### **Continuing Complex Task:**
```
User context: CONTEXT_MINIMAL.md + specific context docs for the task
```

## ⚡ Best Practices

1. **Start Small**: Always begin with minimal context
2. **Load on Demand**: Add context only when the task requires it
3. **Task-Specific**: Use frontend/backend contexts for focused work  
4. **Progressive Loading**: Build context as the conversation develops
5. **Session Breaks**: Reset to minimal context for new sessions

This approach can reduce initial context tokens by 80-95%!