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
Purpose: Main FastAPI application entry point. Sets up the FastAPI app with routers
         for users and items, includes authentication-protected routes.
"""

from fastapi import FastAPI, Depends
from routers.base_routers import user_routes
from routers.routers import item_routes
from auth.encrypt import get_admin_rights, get_read_write_rights

app = FastAPI(title="FastAPI Template Project", version="1.0.0")

app.include_router(item_routes.router, prefix="/items", tags=["items"])
app.include_router(user_routes.router, prefix="/users", tags=["users"])


@app.get("/")
def read_root():
    """
    Health check endpoint for the API.
    
    Returns:
        dict: A dictionary containing the server status.
    """
    return {"Server Status": "Running"}


@app.get("/protected", dependencies=[Depends(get_read_write_rights)])
def protected_route():
    """
    Protected endpoint that requires read-write authentication.
    
    Returns:
        dict: A dictionary containing authentication confirmation message.
        
    Raises:
        HTTPException: 401 if user doesn't have read-write privileges.
    """
    return {"message": f"Authenticated!"}
