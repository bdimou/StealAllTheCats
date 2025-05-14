# StealAllTheCats

A .NET 8 web application that integrates with TheCatAPI to manage and display cat images with a robust tagging system.

## Architecture

The application follows Clean Architecture principles with the following key components:

- **API Layer**: Minimal API endpoints for efficient request handling
- **Application Layer**: Business logic and service implementations
- **Domain Layer**: Core entities and business rules
- **Infrastructure Layer**: Data access and external service integrations

## Technical Stack

- **Framework**: .NET 8
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core
- **Containerization**: Docker & Docker Compose
- **Caching**: In-memory caching for pagination optimization
- **Additional Tools**:
  - FluentValidation for request validation
  - AutoMapper for object mapping
  - Dependency Injection for service management

## Key Features

### Data Management
- Many-to-Many relationship between Cats and Tags
- Automatic handling of association table (CatTag) by Entity Framework
- Image uniqueness constraints using byte array hashing
- Integration with TheCatAPI for image sourcing
- Pagination with caching for improved performance

### Database Design
- Efficient image handling using TheCatAPI photo URLs instead of storing & serving their binary data to the consumer
- Custom image hashing implementation for uniqueness verification
- Optimized database queries through proper indexing

## Setup and Installation

### Prerequisites
- Docker
- Docker Compose
- .NET 8 SDK

### Environment Configuration

Create a `.env` file in the root directory with the following variables:

```env
# API Configuration
API_PORT=5197
API_CONTAINER_PORT=8081

# Database Configuration
DB_PORT=1433
DB_CONTAINER_PORT=1433
DB_NAME=StealCatsDb
DB_USER=sa
DB_PASSWORD=Your_secure_password_here
CAT_API_KEY=Your_TheCatAPI_key
```

### Docker Compose Configuration

The application uses Docker Compose for orchestration. The `docker-compose.yml` file is configured to use environment variables for sensitive information and port mappings. Here's how to customize the configuration:

1. Create a `.env` file as described above
2. Place it in solution root


### Running the Application

1. Clone the repository
2. Create and configure the `.env` file as described above
3. Navigate to the project directory
4. Run the following command:
```bash
docker-compose up
```

The application will be available at:
- API: http://localhost:${API_PORT}
- SQL Server: localhost:${DB_PORT}

### Database Configuration
- Database Name: Configured via DB_NAME in .env
- SQL Server credentials are configured through environment variables
- SQL Server Migrations are handled in-app
- TheCatAPI access is configured through your API key
- Data persistence is handled through Docker volumes (but destroying the container deletes the data)


## Project Structure

```
StealAllTheCats/
├── API/                 # API Layer - Minimal API endpoints and controllers
├── BLL/                 # Business Logic Layer - Services and business rules
├── DAL/                 # Data Access Layer - Repositories and DbContext
├── Tests/              # Unit and integration tests
├── StealAllTheCats.sln # Solution file
├── docker-compose.yml  # Docker configuration
└── .env               # Environment variables (create this)
```

## Best Practices Implemented

- Clean Architecture separation of concerns
- DTOs for data transfer between layers
- Centralized dependency injection
- Repository pattern for data access abstraction
- Minimal API design
- Efficient image handling strategy
- Caching for pagination optimization
- Environment-based configuration
- Secure credential management

## Security Considerations

- SQL Server authentication with secure password management
- Environment-specific configurations
- No direct storage of image binary data
- Proper validation of all incoming requests
- Environment variables for sensitive information
- Docker volume for data persistence

## Performance Optimizations

- Cached pagination objects
- Efficient image handling through URL references
- Optimized database queries
- Proper indexing strategy
