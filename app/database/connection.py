#!/usr/bin/env python3
"""
MIT License

Copyright (c) 2024 Oliver Tattersfield

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Author: Oliver Tattersfield
Date: November 25, 2024
Purpose: Database connection configuration and session management.
         Handles PostgreSQL connection using SQLModel and provides dependency injection
         for database sessions.
"""

import os
from sqlmodel import SQLModel, create_engine, Session

# Database configuration
DB_HOST = os.getenv("DB_HOST")
DB_PORT = os.getenv("DB_PORT")
DB_USER = os.getenv("DB_USER")
DB_PASSWORD = os.getenv("DB_PASSWORD")
DB_NAME = os.getenv("DB_NAME")

# Database URL
DATABASE_URL = f"postgresql://{DB_USER}:{DB_PASSWORD}@{DB_HOST}:{DB_PORT}/{DB_NAME}"

# SQLModel engine
engine = create_engine(DATABASE_URL, echo=True)


# Dependency to get DB session
def get_db():
    """
    Dependency function to get database session.
    
    Yields:
        Session: SQLModel database session for dependency injection.
        
    Note:
        This function is used as a FastAPI dependency to provide
        database sessions to route handlers. The session is automatically
        closed after the request completes.
    """
    with Session(engine) as session:
        yield session
