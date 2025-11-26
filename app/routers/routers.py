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
Purpose: Custom routers for application-specific endpoints.
         Defines ItemRouter class that extends BaseRouter for item management
         operations with read-write authentication requirements.
"""

import database.schemas as schemas
from database.orm import item_orm
from auth.encrypt import get_read_write_rights
from routers.base_routers import BaseRouter


class ItemRouter(BaseRouter):
    """
    Item-specific router extending BaseRouter functionality.
    
    Provides CRUD operations for items with read-write authentication.
    Inherits all standard CRUD endpoints from BaseRouter and configures
    them with item-specific models and authentication requirements.
    """
    
    def __init__(self):
        """
        Initialize ItemRouter with item-specific configurations.
        
        Sets up the router with:
        - item_orm for database operations
        - Item model for responses
        - ItemUpdate model for partial updates
        - ItemInput model for creation/full updates
        - Read-write privilege requirements for all endpoints
        """
        super().__init__(
            orm=item_orm(),
            model=schemas.Item,
            update_model=schemas.ItemUpdate,
            input_model=schemas.ItemInput,
        )
        self.init_routes(get_privilege=get_read_write_rights)


item_routes = ItemRouter()
