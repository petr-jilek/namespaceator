# Namespaceator

[![NuGet](https://img.shields.io/nuget/v/Namespaceator.svg)](https://www.nuget.org/packages/Namespaceator/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Namespaceator.svg)](https://www.nuget.org/packages/Namespaceator/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

> Keep your namespaces where they belong.

**Namespaceator** is a .NET tool that automatically synchronizes C# namespace declarations with your project's folder structure — ensuring consistency between your physical organization and logical namespace hierarchy.

- No more mismatches.
- No more manual namespace fixing.
- fNo more structural drift.

## ✨ Why?

In C# projects, namespaces are expected to reflect the folder structure.

Example:

`/App/Models/User.cs`

Should typically be:

`namespace MyProject.App.Models;`

But over time:

- Files get moved
- Namespaces stay unchanged
- Reviews catch it
- You fix it manually
- Repeat forever

Namespaceator removes that entire problem.

## 📦 Installation

### Global installation

```sh
dotnet tool install -g Namespaceator
```

### Local installation (recommended for teams)

```sh
dotnet new tool-manifest
dotnet tool install Namespaceator
```

## 🚀 Usage

Run inside your project directory:

```sh
dotnet namespaceator <dir-path>
```

Example with absolute path:

```sh
dotnet namespaceator /aaa/bbb/ccc
```

Example with relative path:

```sh
dotnet namespaceator ./src
```

The tool will:

- Analyze folder structure
- Update namespace declarations to match
- Update usings if needed

## 🛠 Example

### Before

File location:

/App/Models/User.cs

Namespace:

`namespace MyProject.Entities;`

### After running Namespaceator

`namespace MyProject.App.Models;`

## 🎯 What It Solves

- Prevents namespace drift after file moves
- Enforces architectural consistency
- Reduces PR noise
- Keeps large solutions predictable
- Improves long-term maintainability

## 🧭 Philosophy

Your folder structure is not decoration.
It represents your architecture.

Namespaces are your logical hierarchy.

They should match.

Namespaceator enforces that alignment automatically.

## 📌 Version

Current version: 0.1.0

## 📄 License

MIT

## 🤝 Contributing

Issues and pull requests are welcome.

Repository:
https://github.com/petr-jilek/namespaceator
