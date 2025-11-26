# FastAPI Template

A production-ready FastAPI template with database migrations, SQLAlchemy ORM, and Alembic integration.

## Features

- **FastAPI** - Modern, fast web framework for building APIs
- **SQLAlchemy** - Powerful SQL toolkit and ORM
- **Alembic** - Database migration tool
- **Database Schemas** - Well-structured database models
- **Docker Ready** - Easy containerization
- **Auto-generated Docs** - Interactive API documentation
- **Async Support** - Built-in asynchronous request handling

## Quick Start

### Prerequisites

- Python 3.8+
- PostgreSQL/MySQL/SQLite (depending on your configuration)
- Docker and Docker Compose (optional)

### Installation

#### Option 1: Local Development

1. Clone this repository:
```bash
git clone https://github.com/Oliver98t/fastapi-template.git
cd fastapi
```

2. Create a virtual environment:
```bash
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
```

3. Install dependencies:
```bash
pip install -r requirements.txt
```

4. Set up your database configuration in your environment variables or config file.

5. Run database migrations:
```bash
alembic upgrade head
```

6. Start the development server:
```bash
uvicorn app.main:app --reload
```

#### Option 2: Docker Compose

1. Clone this repository:
```bash
git clone https://github.com/Oliver98t/fastapi-template.git
cd fastapi
```

2. Build and start the services:
```bash
docker compose up --build
```

3. Run database migrations (in a separate terminal):
```bash
docker compose exec app alembic upgrade head
```

The API will be available at `http://localhost:8000`

To stop the services:
```bash
docker compose down
```
## Copy over exmaple.env
cp example.env .env

## Database Migrations

This template uses Alembic for database migrations:

- **Create a new migration**: `alembic revision --autogenerate -m "Description"`
- **Apply migrations**: `alembic upgrade head`
- **Rollback migrations**: `alembic downgrade -1`

For Docker Compose, prefix commands with `docker-compose exec app`:
```bash
docker compose exec app alembic revision --autogenerate -m "Description"
docker compose exec app alembic upgrade head
```

## API Documentation

Once the server is running, visit:
- **Swagger UI**: `http://localhost:8000/docs`
- **ReDoc**: `http://localhost:8000/redoc`

## Project Structure

```
fastapi/
├── app/
│   ├── alembic/           # Database migrations
│   │   └── env.py         # Alembic environment configuration
│   ├── database/          # Database related files
│   │   └── schemas.py     # SQLAlchemy models
│   └── main.py           # FastAPI application entry point
├── alembic.ini           # Alembic configuration
├── requirements.txt      # Python dependencies
└── README.md            # This file
```

## Configuration

Update your database configuration and other settings in your environment variables or configuration files.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests
5. Submit a pull request

## License

This project is licensed under the MIT License.
