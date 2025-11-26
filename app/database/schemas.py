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
Purpose: Custom database schemas for application-specific models.
         Defines SQLModel schemas for item management including input validation
         and update models.
"""

from sqlmodel import SQLModel, Field
from typing import Optional
from datetime import datetime
from sqlalchemy import func

#################################################
class Item(SQLModel, table=True):
    """
    Item database model for managing inventory or catalog items.
    
    Attributes:
        id (Optional[int]): Primary key identifier
        name (str): Item name with database index for fast searches
        price (float): Item price
        is_active (bool): Whether the item is currently active/available
        created_at (datetime): Item creation timestamp
        updated_at (Optional[datetime]): Last update timestamp
    """
    __tablename__ = "items"
    id: Optional[int] = Field(default=None, primary_key=True, index=True)
    name: str = Field(index=True)
    price: float
    is_active: bool = True
    created_at: datetime = Field(default_factory=datetime.now)
    updated_at: Optional[datetime] = Field(default=None, sa_column_kwargs={"onupdate": func.now()})


class ItemInput(SQLModel):
    """
    Schema for creating new items.
    
    Attributes:
        name (str): Item name (indexed for search performance)
        price (float): Item price
        is_active (bool): Whether the item is active/available
    """
    name: str = Field(index=True)
    price: float
    is_active: bool


class ItemUpdate(SQLModel):
    """
    Schema for partial item updates via PATCH requests.
    
    All fields are optional to allow partial updates.
    
    Attributes:
        name (Optional[str]): New item name
        price (Optional[float]): New item price
        is_active (Optional[bool]): New active status
    """
    name: Optional[str] = None
    price: Optional[float] = None
    is_active: Optional[bool] = None


#################################################
