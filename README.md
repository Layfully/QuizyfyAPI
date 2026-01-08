# 🚀 Quizyfy API

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![Aspire](https://img.shields.io/badge/Orchestration-Aspire-blue)](https://learn.microsoft.com/en-us/dotnet/aspire/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Quizyfy is a high-performance, cloud-native Quiz management API built using the absolute latest standards of the .NET ecosystem. This project serves as a reference implementation for **.NET 10**, focusing on zero-reflection performance, extreme resilience, and modern developer experience.

---

## 🌟 Key Features

### 🏗️ Modern Architecture
- **Minimal APIs**: Pure implementation using the **Endpoint Routing** pattern (Vertical Slices), eliminating the overhead of the MVC Controller pipeline.
- **Vertical Feature Folders**: Logic organized by endpoint rather than layer, enhancing maintainability.
- **TypedResults**: Full usage of `Results<T1, T2>` for compile-time safety and self-documenting APIs.

### ⚡ "Beast Mode" Performance
- **Zero Reflection Mapping**: Replaced AutoMapper with manual **C# 14 Extension Members**. All object mapping happens at compile-time with zero allocations.
- **JSON Source Generation**: Native serialization logic is generated during the build, removing runtime reflection overhead.
- **EF Core Compiled Models**: DB models are pre-optimized to ensure near-instant application startup.
- **SearchValues**: Uses CPU-level vectorization (SIMD) for high-speed file extension validation.
- **Response Compression**: Brotli compression enabled by default for all JSON payloads.

### 🧠 Advanced Caching
- **HybridCache (L1/L2)**: Stampede-proof caching using local memory and **Redis**. Supports hierarchical tagging for instant cache eviction across related entities.
- **OutputCache**: Tag-based HTTP response caching that skips the entire request pipeline for repeat requests.

### 🛡️ Resilience & Security
- **.NET Aspire Orchestration**: Fully managed development environment (SQL Server, Redis) with a built-in observability dashboard.
- **Polly V8**: Integrated **Standard Resilience Handlers** for all external HTTP dependencies (Recaptcha, PwnedPasswords).
- **Native Validation**: Leverages the new .NET 10 `AddValidation()` for clean, performant data annotation checks.
- **Security Hardening**: Middleware-injected security headers (CSP, HSTS, X-Content-Type-Options).

---

## 🛠️ Tech Stack

| Area | Technology |
| :--- | :--- |
| **Runtime** | .NET 10 |
| **Language** | C# 14 |
| **Database** | SQL Server (EF Core) |
| **Cache** | Redis (Distributed) + HybridCache |
| **Documentation** | Scalar UI + Native OpenAPI 3.1 |
| **Monitoring** | OpenTelemetry (OTel) + Aspire Dashboard |
| **Solutions** | New XML-based `.slnx` format |

---

## 🚀 Getting Started

### Prerequisites
1. **.NET 10 SDK** (Preview)
2. **Docker Desktop** (or Podman) for Aspire orchestration.
3. **dotnet-ef tool**: `dotnet tool install --global dotnet-ef`

### Running the System
You do not need to run manual Docker commands. **.NET Aspire** handles the entire stack:

```bash
# Clone the repository
git clone https://github.com/yourusername/QuizyfyAPI.git
cd QuizyfyAPI

# Start the Orchestrator
dotnet run --project Quizyfy.AppHost
