# Rev_n_Roll - Implementation Guide

## 🚀 Quick Start

This guide provides step-by-step instructions for integrating the newly implemented improvements into the Rev_n_Roll project.

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Installation](#installation)
4. [Frontend Integration](#frontend-integration)
5. [Backend Integration](#backend-integration)
6. [Docker Deployment](#docker-deployment)
7. [Testing](#testing)
8. [Troubleshooting](#troubleshooting)

---

## Overview

This implementation includes:

- ✅ **Material Design 3 Theme System** - Complete design system with automotive theming
- ✅ **Shared Component Library** - Reusable UI components (EventCard, LoadingState, EmptyState)
- ✅ **Global Error Handling** - Centralized error management with user-friendly messages
- ✅ **HTTP Interceptor** - Automatic retry and error handling for API calls
- ✅ **Pagination System** - Backend pagination with metadata
- ✅ **Docker Infrastructure** - Complete containerization for all services

---

## Prerequisites

### Development Environment

- **Node.js**: 20.x or higher
- **npm**: 10.x or higher
- **.NET SDK**: 9.0 or higher
- **Docker**: 24.x or higher
- **Docker Compose**: 2.x or higher
- **PostgreSQL**: 16.x (or use Docker)
- **Redis**: 7.x (or use Docker)

### IDE Recommendations

- **Frontend**: Visual Studio Code with Angular Language Service
- **Backend**: Visual Studio 2022 or JetBrains Rider
- **Docker**: Docker Desktop

---

## Installation

### 1. Clone and Setup

```bash
# Navigate to project directory
cd d:/asdads/Thesis

# Install frontend dependencies
cd rev-n-roll
npm install

# Restore backend dependencies
cd ../ThesisBackend
dotnet restore
```

### 2. Environment Configuration

Create a `.env` file in the project root:

```bash
# Database Configuration
DB_NAME=revnroll
DB_USER=postgres
DB_PASSWORD=your_secure_password
DB_PORT=5432

# Redis Configuration
REDIS_PASSWORD=your_redis_password
REDIS_PORT=6379

# Backend Configuration
BACKEND_PORT=5000
JWT_SECRET=your-super-secret-jwt-key-minimum-32-characters
JWT_ISSUER=RevNRoll
JWT_AUDIENCE=RevNRollUsers
JWT_EXPIRATION=60

# Frontend Configuration
FRONTEND_PORT=4200
API_URL=http://localhost:5000

# CORS Configuration
CORS_ORIGINS=http://localhost:4200,http://localhost:80

# Environment
ENVIRONMENT=Development
```

---

## Frontend Integration

### Step 1: Import Material Theme

Add to [`rev-n-roll/src/styles.css`](rev-n-roll/src/styles.css):

```css
/* Import Material Design 3 Theme */
@import './app/theme/material-theme.scss';

/* Apply theme to body */
body {
  background-color: var(--md-sys-color-background);
  color: var(--md-sys-color-on-background);
  font-family: Roboto, sans-serif;
  margin: 0;
  padding: 0;
}
```

### Step 2: Register Global Error Handler

Update [`rev-n-roll/src/app/app.config.ts`](rev-n-roll/src/app/app.config.ts):

```typescript
import { ApplicationConfig, ErrorHandler } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { GlobalErrorHandler } from './core/error-handler/global-error-handler';
import { ErrorInterceptor } from './core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    // Global error handler
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    
    // HTTP client with error interceptor
    provideHttpClient(
      withInterceptors([ErrorInterceptor])
    ),
    
    // ... other providers
  ]
};
```

### Step 3: Use Shared Components

Example: Update [`rev-n-roll/src/app/components/meets/meets.component.ts`](rev-n-roll/src/app/components/meets/meets.component.ts):

```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventCardComponent } from '../../shared/components/event-card/event-card.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { MeetService } from '../../services/meet.service';

@Component({
  selector: 'app-meets',
  standalone: true,
  imports: [
    CommonModule,
    EventCardComponent,
    LoadingStateComponent,
    EmptyStateComponent
  ],
  templateUrl: './meets.component.html',
  styleUrls: ['./meets.component.css']
})
export class MeetsComponent implements OnInit {
  meets: any[] = [];
  isLoading = true;
  error: string | null = null;

  constructor(private meetService: MeetService) {}

  ngOnInit(): void {
    this.loadMeets();
  }

  loadMeets(): void {
    this.isLoading = true;
    this.meetService.getMeets().subscribe({
      next: (data) => {
        this.meets = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = err.message;
        this.isLoading = false;
      }
    });
  }

  onViewDetails(meet: any): void {
    // Handle view details
  }

  onEdit(meet: any): void {
    // Handle edit
  }

  onDelete(meet: any): void {
    // Handle delete
  }

  onCreateMeet(): void {
    // Handle create
  }
}
```

Update [`rev-n-roll/src/app/components/meets/meets.component.html`](rev-n-roll/src/app/components/meets/meets.component.html):

```html
<div class="meets-container">
  <h1 class="headline-medium">Car Meets</h1>

  <!-- Loading State -->
  <app-loading-state
    *ngIf="isLoading"
    type="event-card"
    [count]="3"
    ariaLabel="Loading meets...">
  </app-loading-state>

  <!-- Empty State -->
  <app-empty-state
    *ngIf="!isLoading && meets.length === 0"
    icon="event_busy"
    title="No meets found"
    message="There are no car meets available at the moment. Create one to get started!"
    actionText="Create Meet"
    actionIcon="add"
    (actionClick)="onCreateMeet()">
  </app-empty-state>

  <!-- Meets List -->
  <div *ngIf="!isLoading && meets.length > 0" class="meets-grid">
    <app-event-card
      *ngFor="let meet of meets"
      [event]="meet"
      [eventType]="'meet'"
      [elevation]="2"
      [canEdit]="meet.isCreator"
      [canDelete]="meet.isCreator"
      (cardClick)="onViewDetails(meet)"
      (editClick)="onEdit(meet)"
      (deleteClick)="onDelete(meet)">
    </app-event-card>
  </div>
</div>
```

### Step 4: Apply Design System Styles

Update component CSS to use design tokens:

```css
.meets-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: var(--md-sys-spacing-lg);
}

.meets-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: var(--md-sys-spacing-lg);
  margin-top: var(--md-sys-spacing-xl);
}

@media (max-width: 768px) {
  .meets-container {
    padding: var(--md-sys-spacing-md);
  }

  .meets-grid {
    grid-template-columns: 1fr;
    gap: var(--md-sys-spacing-md);
  }
}
```

---

## Backend Integration

### Step 1: Add PagedResult to Services

Update [`ThesisBackend/ThesisBackend.Services/MeetService.cs`](ThesisBackend/ThesisBackend.Services/MeetService.cs):

```csharp
using ThesisBackend.Domain.Common;
using Microsoft.EntityFrameworkCore;

public class MeetService : IMeetService
{
    private readonly ApplicationDbContext _context;

    public MeetService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<MeetResponse>> GetMeetsAsync(
        PaginationQuery pagination,
        bool includePrivate = false)
    {
        var query = _context.Meets
            .Include(m => m.Creator)
            .Include(m => m.Crew)
            .Where(m => includePrivate || !m.Private)
            .OrderByDescending(m => m.Date);

        var pagedResult = await PagedResult<Meet>.CreateAsync(
            query,
            pagination.PageNumber,
            pagination.PageSize
        );

        return pagedResult.Map(meet => new MeetResponse
        {
            Id = meet.Id,
            Name = meet.Name,
            Description = meet.Description,
            Location = meet.Location,
            Date = meet.Date,
            Tags = meet.Tags,
            CreatorName = meet.Creator.Nickname,
            Private = meet.Private
        });
    }
}
```

### Step 2: Update Controllers

Update [`ThesisBackend/Controllers/MeetController.cs`](ThesisBackend/Controllers/MeetController.cs):

```csharp
using ThesisBackend.Domain.Common;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MeetController : ControllerBase
{
    private readonly IMeetService _meetService;

    public MeetController(IMeetService meetService)
    {
        _meetService = meetService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MeetResponse>>> GetMeets(
        [FromQuery] PaginationQuery pagination)
    {
        var result = await _meetService.GetMeetsAsync(pagination);
        return Ok(result);
    }
}
```

### Step 3: Add Health Check Endpoint

Create [`ThesisBackend/Controllers/HealthController.cs`](ThesisBackend/Controllers/HealthController.cs):

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        });
    }
}
```

---

## Docker Deployment

### Development Deployment

```bash
# Start all services
docker-compose up

# Start in detached mode
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Remove volumes (clean slate)
docker-compose down -v
```

### Production Deployment

```bash
# Build images
docker-compose build

# Start with production profile
docker-compose --profile production up -d

# Check service health
docker-compose ps

# View specific service logs
docker-compose logs -f backend

# Scale backend service
docker-compose up -d --scale backend=3

# Update specific service
docker-compose up -d --no-deps --build backend
```

### Database Migrations

```bash
# Run migrations in Docker
docker-compose exec backend dotnet ef database update

# Create new migration
docker-compose exec backend dotnet ef migrations add MigrationName

# Rollback migration
docker-compose exec backend dotnet ef database update PreviousMigrationName
```

---

## Testing

### Frontend Tests

```bash
cd rev-n-roll

# Run unit tests
npm test

# Run tests with coverage
npm run test:coverage

# Run e2e tests (if configured)
npm run e2e
```

### Backend Tests

```bash
cd ThesisBackend

# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test project
dotnet test ThesisBackend.Api.Tests
```

### Docker Health Checks

```bash
# Check service health
docker-compose ps

# Test backend health endpoint
curl http://localhost:5000/health

# Test frontend
curl http://localhost:4200

# Test database connection
docker-compose exec database pg_isready -U postgres

# Test Redis connection
docker-compose exec cache redis-cli ping
```

---

## Troubleshooting

### Common Issues

#### 1. Port Already in Use

```bash
# Find process using port
netstat -ano | findstr :5000

# Kill process (Windows)
taskkill /PID <process_id> /F

# Or change port in .env file
BACKEND_PORT=5001
```

#### 2. Database Connection Failed

```bash
# Check database is running
docker-compose ps database

# View database logs
docker-compose logs database

# Restart database
docker-compose restart database

# Reset database
docker-compose down -v
docker-compose up database
```

#### 3. Frontend Build Errors

```bash
# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install

# Clear Angular cache
npm run ng cache clean

# Rebuild
npm run build
```

#### 4. Backend Build Errors

```bash
# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Rebuild
dotnet build
```

#### 5. Docker Build Fails

```bash
# Clear Docker cache
docker system prune -a

# Rebuild without cache
docker-compose build --no-cache

# Check Docker resources
docker system df
```

### Debugging

#### Frontend Debugging

```typescript
// Enable verbose logging in error handler
// In global-error-handler.ts
private logError(errorInfo: ErrorInfo): void {
  console.group(`🚨 ${errorInfo.severity.toUpperCase()}`);
  console.error('Full Error:', errorInfo);
  console.groupEnd();
}
```

#### Backend Debugging

```csharp
// Enable detailed logging in appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

#### Docker Debugging

```bash
# Enter container shell
docker-compose exec backend sh

# View container logs
docker-compose logs -f --tail=100 backend

# Inspect container
docker inspect revnroll-backend

# Check network
docker network inspect revnroll-network
```

---

## Performance Optimization

### Frontend

```bash
# Analyze bundle size
npm run build -- --stats-json
npx webpack-bundle-analyzer dist/rev-n-roll/stats.json

# Enable production mode
ng build --configuration production

# Enable AOT compilation
ng build --aot
```

### Backend

```csharp
// Enable response compression in Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

// Enable response caching
builder.Services.AddResponseCaching();
```

---

## Next Steps

1. **Complete Phase 1**: Implement remaining UX enhancements
2. **Phase 2**: Add NgRx state management and crew features
3. **Phase 3**: Implement backend optimizations (Redis, rate limiting)
4. **Phase 4**: Expand testing and deploy to production

---

## Support

For issues or questions:

1. Check the [Implementation Summary](IMPLEMENTATION_SUMMARY.md)
2. Review the [Project Analysis Plan](plans/project_analysis_and_improvement_plan.md)
3. Check existing issues in the repository
4. Create a new issue with detailed information

---

## License

This project is part of a thesis work. All rights reserved.

---

**Last Updated:** March 16, 2026  
**Version:** 1.0.0  
**Status:** Phase 1 - Foundation Complete
