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
Purpose: Base router classes providing generic CRUD operations.
         Contains BaseRouter for generic CRUD endpoints and UserRouter
         with user-specific authentication and management features.
"""

from fastapi import APIRouter, HTTPException, Depends
from fastapi.routing import APIRoute
from fastapi.security import OAuth2PasswordRequestForm
from sqlmodel import Session
from typing import List
from database.connection import get_db
from auth.encrypt import get_password_hash
import database.base_schemas as base_schemas
from database.base_orm import user_orm, base_crud
from auth.encrypt import verify_password, create_access_token, get_admin_rights


class BaseRouter:
    """
    Base router class providing generic CRUD operations for database models.

    This class creates standard REST API endpoints (GET, POST, PUT, PATCH, DELETE)
    for any given database model and ORM. It's designed to be inherited by
    model-specific router classes.

    Attributes:
        orm: The ORM instance for database operations
        model: The SQLModel class for response serialization
        input_model: The input model for POST/PUT operations
        update_model: The input model for PATCH operations
        router: FastAPI router instance
        singular_item_slug: URL path parameter for single item operations
    """

    def __init__(self, orm: base_crud, model, input_model, update_model):
        """
        Initialize BaseRouter with model-specific configurations.

        Args:
            orm (base_crud): ORM instance for database operations
            model: SQLModel class for response serialization
            input_model: Input model for POST/PUT operations
            update_model: Input model for PATCH operations
        """
        self.orm = orm
        self.model = model
        self.input_model = input_model
        self.update_model = update_model
        self.router = APIRouter()

    def init_routes(self, get_privilege):
        """
        Initialize all standard CRUD routes with authentication.

        Creates the following endpoints:
        - GET / (list all items with pagination)
        - GET /{item_id} (get single item)
        - POST / (create new item)
        - PUT /{item_id} (full update)
        - PATCH /{item_id} (partial update)
        - DELETE /{item_id} (delete item)

        Args:
            get_privilege: Authentication dependency function
        """
        singular_item = self.model.__tablename__[:-1]
        self.singular_item_slug = "/{" + singular_item + "_id}"

        self.router.get(
            "/", response_model=List[self.model], dependencies=[Depends(get_privilege)]
        )(self._get_all)

        self.router.get(
            self.singular_item_slug,
            response_model=self.model,
            dependencies=[Depends(get_privilege)],
        )(self._get)

        self.router.delete(
            self.singular_item_slug,
            response_model=self.model,
            dependencies=[Depends(get_privilege)],
        )(self._delete)

        # generate update routes for put/patch as input type cannot be determined at runtime
        update = self._make_update_func(input_model=self.input_model, orm=self.orm)
        self.router.put(
            self.singular_item_slug,
            response_model=self.model,
            dependencies=[Depends(get_privilege)],
        )(update)

        patch = self._make_update_func(input_model=self.update_model, orm=self.orm)
        self.router.patch(
            self.singular_item_slug,
            response_model=self.model,
            dependencies=[Depends(get_privilege)],
        )(patch)

        # generate create route as input type cannot be determined at runtime
        create = self._make_create_func(input_model=self.input_model, orm=self.orm)
        self.router.post(
            "/", response_model=self.model, dependencies=[Depends(get_privilege)]
        )(create)

    def _get_all(self, skip: int = 0, limit: int = 100, db: Session = Depends(get_db)):
        """
        Retrieve all items with pagination.

        Args:
            skip (int, optional): Number of records to skip. Defaults to 0.
            limit (int, optional): Maximum records to return. Defaults to 100.
            db (Session): Database session dependency.

        Returns:
            List of model instances.
        """
        items = self.orm.get_all(db=db, skip=skip, limit=limit)
        return items

    def _get(self, id: int, db: Session = Depends(get_db)):
        """
        Retrieve a single item by ID.

        Args:
            id (int): Primary key identifier
            db (Session): Database session dependency

        Returns:
            Model instance

        Raises:
            HTTPException: 404 if item not found
        """
        db_item = self.orm.get(db=db, id=id)
        if db_item is None:
            raise HTTPException(status_code=404, detail="Item not found")
        return db_item

    def _make_create_func(self, input_model, orm):
        """
        Generate a create function with proper type annotations.

        This method is needed because FastAPI requires proper type annotations
        to generate correct API documentation and request validation.

        Args:
            input_model: Input model class for request validation
            orm: ORM instance for database operations

        Returns:
            Function: Create endpoint function with proper annotations
        """
        def create(item, db: Session = Depends(get_db)):
            """
            Create a new item.

            Args:
                item: Input model instance with item data
                db (Session): Database session dependency

            Returns:
                Created model instance
            """
            return orm.create(db=db, obj=item)

        # Set the correct annotation for FastAPI to use as input model
        create.__annotations__ = {"item": input_model, "db": Session}
        return create

    def _make_update_func(self, input_model, orm):
        """
        Generate an update function with proper type annotations.

        This method is needed because FastAPI requires proper type annotations
        to generate correct API documentation and request validation.

        Args:
            input_model: Input model class for request validation
            orm: ORM instance for database operations

        Returns:
            Function: Update endpoint function with proper annotations
        """
        def update(id: int, item, db: Session = Depends(get_db)):
            """
            Update an existing item.

            Args:
                id (int): Primary key identifier
                item: Input model instance with updated data
                db (Session): Database session dependency

            Returns:
                Updated model instance
            """
            return orm.update(db=db, id=id, obj=item)

        # Set the correct annotation for FastAPI to use as input model
        update.__annotations__ = {"id": int, "item": input_model, "db": Session}
        return update

    def _delete(self, id: int, db: Session = Depends(get_db)):
        """
        Delete an item by ID.

        Args:
            id (int): Primary key identifier
            db (Session): Database session dependency

        Returns:
            Deleted model instance

        Raises:
            HTTPException: 404 if item not found
        """
        db_item = self.orm.delete(db=db, id=id)
        if db_item is None:
            raise HTTPException(status_code=404, detail="Item not found")
        return db_item

    def add_new_route(
        self, path: str, method: str, endpoint, response_model=None, get_privilege=None
    ):
        """
        Add or replace a custom route on the router.

        This method allows extending the base router with custom endpoints
        or overriding default behavior for specific routes.

        Args:
            path (str): URL path for the route
            method (str): HTTP method (GET, POST, PUT, PATCH, DELETE)
            endpoint: Function to handle the route
            response_model (optional): Response model for serialization
            get_privilege (optional): Authentication dependency function
        """

        # remove existing path and method
        for i, route in enumerate(self.router.routes):
            if path == route.path and method in route.methods:
                del self.router.routes[i]

        dependencies = None
        if get_privilege:
            dependencies = [Depends(get_privilege)]

        new_route = APIRoute(
            path=path,
            endpoint=endpoint,
            methods=[method],
            response_model=response_model,
            dependencies=dependencies,
            name=endpoint.__name__,
        )
        self.router.routes.append(new_route)


class UserRouter(BaseRouter):
    """
    User-specific router extending BaseRouter with authentication features.

    Provides all standard CRUD operations for users plus additional endpoints
    for user authentication (login) and custom user management logic.
    All user operations require admin privileges except for login.
    """

    def __init__(self):
        """
        Initialize UserRouter with user-specific configurations.

        Sets up the router with user models and admin authentication requirements.
        Also adds custom routes for user creation, updates, and authentication.
        """
        super().__init__(
            orm=user_orm(),
            model=base_schemas.User,
            update_model=base_schemas.UserInputUpdate,
            input_model=base_schemas.UserInput,
        )

        self.init_routes(get_privilege=get_admin_rights)

        # example to add extra routes and set privileges
        self.add_new_route(
            path="/",
            method="POST",
            endpoint=self._create_user,
            response_model=self.model,
            get_privilege=get_admin_rights,
        )

        self.add_new_route(
            path=self.singular_item_slug,
            method="PUT",
            endpoint=self._update_put_user,
            response_model=self.model,
            get_privilege=get_admin_rights,
        )

        self.add_new_route(
            path=self.singular_item_slug,
            method="PATCH",
            endpoint=self._update_patch_user,
            response_model=self.model,
            get_privilege=get_admin_rights,
        )

        self.add_new_route(path="/token", method="POST", endpoint=self._login)

    def _update_put_user(
        self,
        user_id: int,
        user: base_schemas.UserInput,
        db: Session = Depends(get_db),
    ):
        """
        Update user with password hashing (full update).

        All fields are required. Password is hashed before storage.

        Args:
            user_id (int): ID of user to update
            user (UserInput): Complete user data with all required fields
            db (Session): Database session dependency

        Returns:
            Updated User instance

        Raises:
            HTTPException: 404 if user doesn't exist
        """

        current_user = self.orm.get(id=user_id, db=db)
        if not current_user:
            raise HTTPException(status_code=404, detail="User doesn't exist")

        # All fields are required, so directly create the update object
        updated_user = base_schemas.UserInputHashed(
            username=user.username,
            email=user.email,
            privilege=user.privilege,
            hashed_password=get_password_hash(user.password),
        )

        new_user = self.orm.update(id=user_id, db=db, obj=updated_user)
        return new_user

    def _update_patch_user(
        self,
        user_id: int,
        user: base_schemas.UserInputUpdate,
        db: Session = Depends(get_db),
    ):
        """
        Update user with password hashing and field validation.

        Handles partial updates while properly hashing passwords and
        preserving existing values for unchanged fields.

        Args:
            user_id (int): ID of user to update
            user (UserInputUpdate): Updated user data (partial)
            db (Session): Database session dependency

        Returns:
            Updated User instance

        Raises:
            HTTPException: 404 if user doesn't exist
        """

        current_user = self.orm.get(id=user_id, db=db)
        if not current_user:
            raise HTTPException(status_code=404, detail="User doesn't exist")

        # Get update data, excluding unset fields
        update_data = user.model_dump(exclude_unset=True)

        # Only hash password if it's provided
        if "password" in update_data:
            update_data["hashed_password"] = get_password_hash(
                update_data.pop("password")
            )

        # Create update object with only provided fields
        updated_user = base_schemas.UserInputHashed(
            username=update_data.get("username", current_user.username),
            email=update_data.get("email", current_user.email),
            privilege=update_data.get("privilege", current_user.privilege),
            hashed_password=update_data.get(
                "hashed_password", current_user.hashed_password
            ),
        )

        new_user = self.orm.update(id=user_id, db=db, obj=updated_user)
        return new_user

    def _create_user(self, user: base_schemas.UserInput, db: Session = Depends(get_db)):
        """
        Create a new user with password hashing and username validation.

        Args:
            user (UserInput): User data with plain text password
            db (Session): Database session dependency

        Returns:
            Created User instance

        Raises:
            HTTPException: 404 (400) if username already exists
        """

        username_user = self.orm.get_username(db=db, username=user.username)
        if username_user:
            raise HTTPException(status_code=404, detail="User exists")
        else:
            updated_user = base_schemas.UserInputHashed(
                username=user.username,
                email=user.email,
                privilege=user.privilege,
                hashed_password=get_password_hash(user.password),  # Example update
            )
            new_user = self.orm.create(db=db, obj=updated_user)
            return new_user

    def _login(
        self,
        form_data: OAuth2PasswordRequestForm = Depends(),
        db: Session = Depends(get_db),
    ):
        """
        Authenticate user and generate access token.

        Validates username and password, then creates a JWT access token
        containing user information and privileges.

        Args:
            form_data (OAuth2PasswordRequestForm): Username and password
            db (Session): Database session dependency

        Returns:
            dict: Access token and token type

        Raises:
            HTTPException: 400 if credentials are invalid
        """
        user = self.orm.get_username(username=form_data.username, db=db)
        if not user or not verify_password(form_data.password, user.hashed_password):
            raise HTTPException(
                status_code=400, detail="Incorrect username or password"
            )
        encode_data = {"sub": user.username, "priv": user.privilege}
        access_token = create_access_token(data=encode_data)
        return {"access_token": access_token, "token_type": "bearer"}

user_routes = UserRouter()