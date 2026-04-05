# .NET - Technical Interview Exercise

## Project Overview

Your task is to develop a simple web application with:

* API and data layer using:

  * .NET C#
  * ASP.NET MVC
  * Web API
* A database or data store
* Clean Architecture principles
* Test-Driven Development (TDD)

### Requirements

* Define an **informal user story** (must be included in presentation)
* Implement **CRUD operations**
* Create and authenticate users
* Persist user data

⚠️ Constraints:

* Do **NOT** use:

  * Entity Framework
  * Dapper
  * Mediator

---

## Backend

### Database

* Create a data storage solution with:

  * At least one table/container for application data
  * One table/container for users
* Requirements:

  * Primary key (unique identifier)
  * At least 2 additional fields

---

### API

* Build an ASP.NET Web API with:

  * CRUD endpoints
  * Proper HTTP verbs and responses

#### Additional API (Auth)

* Endpoints for:

  * User creation
  * User login
  * Authorized endpoints
  * Non-authorized endpoints

---

### Data Layer

* Implement data access layer
* Responsible for CRUD operations
* Should interact directly with the data store

---

### Business Logic Layer

* Encapsulates:

  * Business rules
  * Validation
* Must be independent from:

  * Data layer
  * API layer

---

### Unit Tests

* Cover:

  * Data access layer
  * Business logic layer
  * API endpoints

---

## Frontend

* Use any framework:

  * React
  * Vue
  * Others

### Requirements

* Responsive and user-friendly UI
* Full CRUD integration with backend
* Clean structure:

  * Components
  * State management

---

## Submission Guidelines

* Include a `README`:

  * Setup instructions
  * Documentation
* Provide:

  * Seeded data
  * Demo credentials

---

## Generative AI Tools

### Task

Design a RESTful API for a **task management system**:

#### Features

* CRUD operations for tasks
* Task fields:

  * `title`
  * `description`
  * `status`
  * `due_date`
* Tasks associated with a user

---

### Instructions

1. Write a **prompt** for a GenAI coding tool (e.g. Copilot, Cursor, Claude)
2. Show:

   * Generated code (or sample)
3. Explain:

   * How you validated AI output
   * Improvements/corrections made
   * Handling of:

     * Edge cases
     * Authentication
     * Validation

---

## Presentation and Code Review

### Presentation

* Explain:

  * User story
  * Architecture decisions
  * Technical design
* Demo the application

---

### Code Review

Be prepared to explain:

* Coding decisions
* Architectural choices

---

## Evaluation Criteria

### Clean Architecture

* Separation of concerns
* Independent components

### Testing

* Strong test coverage
* Preferably TDD

### Code Quality

* Readable
* Well-organized
* Best practices

### Functionality

* Meets all requirements
* No bugs/errors
* (Optional) No browser console warnings

### Presentation

* Clear and concise
* Demonstrates understanding

### GenAI Usage

* Effective prompt engineering
* Critical evaluation of AI output