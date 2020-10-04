# HotRod

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/michaelpduda/HotRod/blob/main/LICENSE.md)
[![Nuget](https://img.shields.io/nuget/v/HotRod)](https://www.nuget.org/packages/HotRod/)

HotRod is an implementation of the repository pattern, where the repository instances behave as ReadOnlyDictionaries (RODs). When a UnitOfWork is opened, they behave like regular Dictionaries so changes can be made and committed.

HotRod currently includes three different implementations:

1. A MemoryRepository, where the data is stored only in memory and not persisted.
2. A JsonFileRepository, where the data is persisted to a JSON text file, useful for debugging.
3. A LiteDbRepository, where the data is persisted to a LiteDb database.

## Download

HotRod implementations are available as Nuget packages:

* [![Nuget](https://img.shields.io/nuget/v/HotRod)](https://www.nuget.org/packages/HotRod/) - HotRod (contains: MemoryRepository, JsonFileRepository)
* [![Nuget](https://img.shields.io/nuget/v/HotRod.LiteDb)](https://www.nuget.org/packages/HotRod.LiteDb/) - HotRod.LiteDb (contains: LiteDbRepository)
