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
Purpose: Base ORM classes providing CRUD operations for database models.
         Contains the base_crud class with generic database operations
         and user_orm class with user-specific query methods.
"""

from fastapi import APIRouter, HTTPException, Depends
from database.connection import get_db
from sqlmodel import Session, select
from .base_schemas import User
from .schemas import Item


class base_crud:
    """
    Base CRUD (Create, Read, Update, Delete) operations for database models.
    
    This class provides generic database operations that can be inherited
    by model-specific ORM classes.
    
    Attributes:
        table: The SQLModel table class to perform operations on.
    """
    
    def __init__(self, table):
        """
        Initialize the base CRUD class with a database table.
        
        Args:
            table: SQLModel table class to perform operations on.
        """
        self.table = table

    def get(self, id: int, db: Session):
        """
        Retrieve a single record by ID.
        
        Args:
            id (int): Primary key identifier
            db (Session): Database session
            
        Returns:
            Model instance or None if not found.
        """
        print(id)
        return db.get(self.table, id)

    def get_all(self, skip: int = 0, limit: int = 100, db: Session = None):
        """
        Retrieve multiple records with pagination.
        
        Args:
            skip (int, optional): Number of records to skip. Defaults to 0.
            limit (int, optional): Maximum number of records to return. Defaults to 100.
            db (Session, optional): Database session.
            
        Returns:
            List of model instances.
        """
        statement = select(self.table).offset(skip).limit(limit)
        results = db.exec(statement)
        return results.all()

    def update(self, id: int, obj, db: Session):
        """
        Update an existing record.
        
        Args:
            id (int): Primary key identifier of record to update
            obj: Model instance with updated data
            db (Session): Database session
            
        Returns:
            Updated model instance or None if record not found.
        """
        db_obj = db.get(self.table, id)
        if not db_obj:
            return None

        obj_data = obj.dict(exclude_unset=True)
        for field, value in obj_data.items():
            setattr(db_obj, field, value)

        db.add(db_obj)
        db.commit()
        db.refresh(db_obj)
        return db_obj

    def create(self, obj, db: Session):
        """
        Create a new record.
        
        Args:
            obj: Model instance with data to create
            db (Session): Database session
            
        Returns:
            Created model instance.
        """
        db_obj = self.table(**obj.dict())
        db.add(db_obj)
        db.commit()
        db.refresh(db_obj)
        return db_obj

    def delete(self, id: int, db: Session):
        """
        Delete a record by ID.
        
        Args:
            id (int): Primary key identifier of record to delete
            db (Session): Database session
            
        Returns:
            Deleted model instance or None if record not found.
        """
        db_obj = db.get(self.table, id)
        if db_obj:
            db.delete(db_obj)
            db.commit()
        return db_obj


class user_orm(base_crud):
    """
    User-specific ORM class extending base CRUD operations.
    
    Provides additional methods for user authentication and lookup
    beyond the basic CRUD operations.
    """
    
    def __init__(self):
        """
        Initialize user ORM with User table.
        """
        super().__init__(User)

    def get_email(self, email: str, db: Session):
        """
        Retrieve user by email address.
        
        Args:
            email (str): Email address to search for
            db (Session): Database session
            
        Returns:
            User instance or None if not found.
        """
        statement = select(self.table).where(self.table.email == email)
        result = db.exec(statement).first()
        return result

    def get_username(self, username: str, db: Session):
        """
        Retrieve user by username.
        
        Args:
            username (str): Username to search for
            db (Session): Database session
            
        Returns:
            User instance or None if not found.
        """
        statement = select(self.table).where(self.table.username == username)
        result = db.exec(statement).first()
        return result
