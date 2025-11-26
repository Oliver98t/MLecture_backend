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
LIABITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Author: Oliver Tattersfield
Date: November 25, 2024
Purpose: Item-specific ORM class for item management.
         Extends base CRUD operations for the Item model.
"""

from fastapi import APIRouter, HTTPException, Depends
from database.connection import get_db
from sqlmodel import Session, select
from .base_schemas import User
from .base_orm import base_crud
from .schemas import Item


class item_orm(base_crud):
    """
    Item-specific ORM class extending base CRUD operations.
    
    Provides item-specific database operations by inheriting
    from base_crud and initializing with the Item table.
    Currently uses only the base CRUD operations but can be
    extended with item-specific methods as needed.
    """
    
    def __init__(self):
        """
        Initialize item ORM with Item table.
        """
        super().__init__(Item)
