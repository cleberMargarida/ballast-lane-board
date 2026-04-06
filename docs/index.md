---
title: Ballast Lane Board
---

# 🚀 Ballast Lane Board

**A full-stack task management platform built with Clean Architecture**

## Screenshots

| Sign In | Board | Create Task | Status Change |
|---------|-------|-------------|---------------|
| ![Sign In](images/sign-in.png) | ![Board](images/board.png) | ![Create Task](images/create-task.png) | ![Move Status](images/move-status.png) |

> **Live Demo:** [https://ballast-lane-board.azurewebsites.net](https://ballast-lane-board.azurewebsites.net)

---

## ✨ Features

| | Feature | Description |
|---|---|---|
| 🏗️ | **Clean Architecture** | .NET 10, 4-layer separation with strict dependency flow |
| 🔐 | **Keycloak OIDC** | Enterprise-grade authentication with OpenID Connect |
| 📋 | **Kanban Board** | Angular 19 SPA with drag-and-drop task management |
| 🐳 | **Docker Ready** | One-command local deployment with Docker Compose |
| ☁️ | **Azure Hosted** | Multi-container App Service with PostgreSQL Flexible Server |
| 🐘 | **PostgreSQL** | Reliable, production-grade data storage |
| 🧪 | **Comprehensive Tests** | Unit + integration tests with Testcontainers |
| 🔄 | **CI/CD** | GitHub Actions pipeline: build, test, publish, deploy |

---

## 🎯 Get Started

> [!TIP]
> New here? Start with the **Getting Started** guide to have the app running in minutes.

- [📖 Getting Started](getting-started/index.md) — Overview, installation, and your first request
- [🏛️ Architecture](architecture/index.md) — Deep dive into layers, patterns, and decisions
- [🐳 Docker Deployment](deployment/docker.md) — Run locally with Docker Compose
- [☁️ Azure Deployment](deployment/azure.md) — Multi-container App Service architecture and CI/CD
- 🔌 **API Reference** — Use the "API Reference" tab in the top navigation (auto-generated from XML documentation)

---

## What is Ballast Lane Board?

Ballast Lane Board is a task management web application that demonstrates modern .NET development practices. It features a **Clean Architecture** backend with **.NET 10**, an **Angular 19** frontend with **Tailwind CSS**, **Keycloak** for identity management, and **PostgreSQL** for persistence — orchestrated locally with **Docker Compose** and deployed to **Azure App Service** as a multi-container application via a **GitHub Actions CI/CD** pipeline.

Users can create, update, and track tasks through a Kanban-style board. Admins can view and manage all tasks, while regular users see only their own. Authentication flows through Keycloak's OIDC protocol, and the API enforces ownership and role-based access at every layer.
