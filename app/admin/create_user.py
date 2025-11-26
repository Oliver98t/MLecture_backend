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
Purpose: Admin script for creating new users interactively.
         Provides a command-line interface for administrators to create
         user accounts with proper password hashing and validation.
"""

from auth.encrypt import get_password_hash
from database.base_schemas import UserInputHashed
from database.base_orm import user_orm
from database.connection import get_db
from getpass import getpass


def get_user_input():
    """
    Collect user information from admin input.
    
    Returns:
        tuple: (username, email, privilege, password) or None if cancelled
    """
    print("\n=== Create New User ===")
    username = input("Enter username: ")
    email = input("Enter email: ")
    
    while True:
        try:
            privilege = int(input("Enter privilege (0=Admin, 1=Read-Write, 2=Read-Only): "))
            if privilege in [0, 1, 2]:
                break
            else:
                print("Please enter 0, 1, or 2")
        except ValueError:
            print("Please enter a valid number (0, 1, or 2)")
    
    password = getpass("Enter password: ")
    
    return username, email, privilege, password


def confirm_user_creation(username, email, privilege):
    """
    Display user details and confirm creation.
    
    Args:
        username (str): Username to create
        email (str): Email address
        privilege (int): Privilege level
        
    Returns:
        bool: True if confirmed, False otherwise
    """
    privilege_names = {0: "Admin", 1: "Read-Write", 2: "Read-Only"}
    
    print(f"\nUser Details:")
    print(f"Username: {username}")
    print(f"Email: {email}")
    print(f"Privilege: {privilege_names[privilege]} ({privilege})")
    
    create_check = input("\nCreate user (y/n): ").lower().strip()
    return create_check == 'y'


def create_user(username, email, privilege, password):
    """
    Create a new user in the database.
    
    Args:
        username (str): Username for the new user
        email (str): Email address for the new user
        privilege (int): Privilege level (0-2)
        password (str): Plain text password (will be hashed)
        
    Returns:
        bool: True if user created successfully, False otherwise
    """
    try:
        hashed_password = get_password_hash(password)
        user_input = UserInputHashed(
            username=username,
            email=email,
            privilege=privilege,
            hashed_password=hashed_password,
        )

        # Create a database session and use it with the ORM
        db = next(get_db())
        try:
            new_user_orm = user_orm()
            new_user_orm.create(db=db, obj=user_input)
            print(f"✓ User '{username}' created successfully!")
            return True
        finally:
            db.close()
            
    except Exception as e:
        print(f"✗ Error creating user: {e}")
        return False


def main():
    """
    Main function to run the user creation script.
    
    Provides an interactive loop for creating multiple users
    with proper error handling and confirmation.
    """
    print("FastAPI User Creation Tool")
    print("=========================")
    
    while True:
        try:
            # Get user input
            username, email, privilege, password = get_user_input()
            
            # Confirm creation
            if confirm_user_creation(username, email, privilege):
                create_user(username, email, privilege, password)
            else:
                print("User creation cancelled.")
            
            # Ask to continue
            continue_check = input("\nCreate another user (y/n): ").lower().strip()
            if continue_check != 'y':
                break
                
        except KeyboardInterrupt:
            print("\n\nExiting...")
            break
        except Exception as e:
            print(f"\nUnexpected error: {e}")
            continue_check = input("Continue anyway (y/n): ").lower().strip()
            if continue_check != 'y':
                break
    
    print("Goodbye!")


if __name__ == "__main__":
    main()
