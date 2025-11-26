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
IMPLITED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Author: Oliver Tattersfield
Date: November 25, 2024
Purpose: Base database schemas for user management.
         Defines SQLModel schemas for user entities, input validation,
         and user privilege levels using enums.
"""

from sqlmodel import SQLModel, Field
from typing import Optional
from datetime import datetime
from enum import IntEnum
from sqlalchemy import func

# User
#################################################


class UserPrivilege(IntEnum):
    """
    Enumeration defining user privilege levels.
    
    Attributes:
        ADMIN (int): Full administrative access (level 0)
        READ_WRITE (int): Read and write access (level 1)
        READ_ONLY (int): Read-only access (level 2)
    """
    ADMIN = 0
    READ_WRITE = 1
    READ_ONLY = 2


class User(SQLModel, table=True):
    """
    User database model for storing user information and authentication data.
    
    Attributes:
        id (Optional[int]): Primary key identifier
        username (str): Unique username for login
        email (str): Unique email address
        privilege (int): User privilege level (0-2, see UserPrivilege enum)
        hashed_password (str): Bcrypt hashed password
        created_at (datetime): Account creation timestamp
        updated_at (Optional[datetime]): Last update timestamp
    """
    __tablename__ = "users"
    id: Optional[int] = Field(default=None, primary_key=True, index=True)
    username: str = Field(index=True, unique=True)
    email: str = Field(index=True, unique=True)
    privilege: int = Field(ge=0, le=2)  # TODO add number guard 0<=x<=2
    hashed_password: str
    created_at: datetime = Field(default_factory=datetime.now)
    updated_at: Optional[datetime] = Field(default=None, sa_column_kwargs={"onupdate": func.now()})

class UserInputUpdate(SQLModel):
    """
    Schema for partial user updates via PATCH requests.
    
    All fields are optional to allow partial updates.
    
    Attributes:
        username (Optional[str]): New username
        email (Optional[str]): New email address
        privilege (Optional[int]): New privilege level
        password (Optional[str]): New plain text password (will be hashed)
    """
    username: Optional[str] = None
    email: Optional[str] = None
    privilege: Optional[int] = None
    password: Optional[str] = None


class UserInput(SQLModel):
    """
    Schema for creating new users with plain text password.
    
    Attributes:
        username (str): Unique username
        email (str): Unique email address
        privilege (int): User privilege level (0-2)
        password (str): Plain text password (will be hashed)
    """
    username: str
    email: str
    privilege: int
    password: str


class UserInputHashed(SQLModel):
    """
    Schema for user creation/update with pre-hashed password.
    
    Used internally after password hashing to avoid exposing plain text passwords.
    
    Attributes:
        username (str): Unique username
        email (str): Unique email address
        privilege (int): User privilege level (0-2)
        hashed_password (str): BCrypt hashed password
    """
    username: str
    email: str
    privilege: int
    hashed_password: str


#################################################
