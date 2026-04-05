---
title: Ballast Lane Board
---

# 🚀 Ballast Lane Board

**A full-stack task management platform built with Clean Architecture**

---

## ✨ Features

| | Feature | Description |
|---|---|---|
| 🏗️ | **Clean Architecture** | .NET 10, 4-layer separation with strict dependency flow |
| 🔐 | **Keycloak OIDC** | Enterprise-grade authentication with OpenID Connect |
| 📋 | **Kanban Board** | Angular 19 SPA with drag-and-drop task management |
| 🐳 | **Docker Ready** | One-command deployment with Docker Compose |
| 🐘 | **PostgreSQL** | Reliable, production-grade data storage |
| 🧪 | **Comprehensive Tests** | Unit + integration tests with Testcontainers |

---

## 🎯 Get Started

> [!TIP]
> New here? Start with the **Getting Started** guide to have the app running in minutes.

- [📖 Getting Started](getting-started/index.md) — Overview, installation, and your first request
- [🏛️ Architecture](architecture/index.md) — Deep dive into layers, patterns, and decisions
- 🔌 **API Reference** — Use the "API Reference" tab in the top navigation (auto-generated from XML documentation)

---

## What is Ballast Lane Board?

Ballast Lane Board is a task management web application that demonstrates modern .NET development practices. It features a **Clean Architecture** backend with **.NET 10**, an **Angular 19** frontend with **Tailwind CSS**, **Keycloak** for identity management, and **PostgreSQL** for persistence — all orchestrated with **Docker Compose**.

Users can create, update, and track tasks through a Kanban-style board. Admins can view and manage all tasks, while regular users see only their own. Authentication flows through Keycloak's OIDC protocol, and the API enforces ownership and role-based access at every layer.
