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
Purpose: Authentication and encryption utilities.
         Provides JWT token generation/validation, password hashing,
         and FastAPI authentication dependencies with role-based access control.
"""

from datetime import datetime, timedelta, timezone
from typing import Optional
from jose import JWTError, jwt
from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
from passlib.context import CryptContext
from database.base_schemas import UserPrivilege

# Secret key and algorithm for JWT
SECRET_KEY = "your-secret-key"
ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_MINUTES = 30

pwd_context = CryptContext(schemes=["sha256_crypt"], deprecated="auto")
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="users/token")


def verify_password(plain_password, hashed_password):
    """
    Verify a plain text password against a hashed password.
    
    Args:
        plain_password (str): Plain text password to verify
        hashed_password (str): Stored hashed password
        
    Returns:
        bool: True if password matches, False otherwise
    """
    return pwd_context.verify(plain_password, hashed_password)


def get_password_hash(password):
    """
    Generate a hash for a plain text password.
    
    Args:
        password (str): Plain text password to hash
        
    Returns:
        str: Hashed password suitable for database storage
    """
    return pwd_context.hash(password)


def create_access_token(data: dict, expires_delta: Optional[timedelta] = None):
    """
    Create a JWT access token with user data and expiration.
    
    Args:
        data (dict): User data to encode in token (username, privileges, etc.)
        expires_delta (Optional[timedelta]): Custom expiration time
        
    Returns:
        str: Encoded JWT token
    """
    to_encode = data.copy()
    expire = datetime.now(timezone.utc) + (
        expires_delta or timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    )
    to_encode.update({"exp": expire})
    return jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)


def decode_access_token(token: str):
    """
    Decode and validate a JWT access token.
    
    Args:
        token (str): JWT token to decode
        
    Returns:
        dict: Decoded token payload
        
    Raises:
        HTTPException: 401 if token is invalid or expired
    """
    try:
        payload = jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
        return payload
    except JWTError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Could not validate credentials",
            headers={"WWW-Authenticate": "Bearer"},
        )


def get_current_user(token: str = Depends(oauth2_scheme)):
    """
    Extract user privilege level from JWT token.
    
    FastAPI dependency function that validates JWT token and extracts
    the user's privilege level for authorization checks.
    
    Args:
        token (str): JWT token from Authorization header
        
    Returns:
        int: User privilege level from token
        
    Raises:
        HTTPException: 401 if token is invalid or missing username
    """
    payload = decode_access_token(token)
    username: str = payload.get("sub")
    privilege: int = payload.get("priv")
    if username is None:
        raise HTTPException(
            status_code=401, detail="Invalid authentication credentials"
        )
    return privilege


def get_admin_rights(privilege: int = Depends(get_current_user)):
    """
    Require admin privileges for endpoint access.
    
    FastAPI dependency that ensures the current user has admin rights.
    
    Args:
        privilege (int): User privilege level from get_current_user
        
    Raises:
        HTTPException: 401 if user doesn't have admin privileges
    """
    if privilege > UserPrivilege.ADMIN.value:
        raise HTTPException(
            status_code=401, detail="Invalid authentication credentials"
        )
    return


def get_read_write_rights(privilege: int = Depends(get_current_user)):
    """
    Require read-write privileges for endpoint access.
    
    FastAPI dependency that ensures the current user has read-write rights
    or higher (admin).
    
    Args:
        privilege (int): User privilege level from get_current_user
        
    Raises:
        HTTPException: 401 if user doesn't have read-write privileges
    """
    if privilege > UserPrivilege.READ_WRITE.value:
        raise HTTPException(
            status_code=401, detail="Invalid authentication credentials"
        )
    return


def get_read_rights(privilege: int = Depends(get_current_user)):
    """
    Require read privileges for endpoint access.
    
    FastAPI dependency that ensures the current user has at least read-only
    access (read-only, read-write, or admin).
    
    Args:
        privilege (int): User privilege level from get_current_user
        
    Raises:
        HTTPException: 401 if user doesn't have any valid privileges
    """
    if privilege > UserPrivilege.READ_ONLY.value:
        raise HTTPException(
            status_code=401, detail="Invalid authentication credentials"
        )
    return
