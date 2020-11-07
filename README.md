# HotRod

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/michaelpduda/HotRod/blob/main/LICENSE.md)
[![Nuget](https://img.shields.io/nuget/v/HotRod)](https://www.nuget.org/packages/HotRod/)

HotRod is an implementation of the repository pattern, where the repository instances behave as ReadOnlyDictionaries (RODs). When a UnitOfWork is opened, they behave like regular Dictionaries so changes can be made and committed.

HotRod currently includes three different implementations:

1. A MemoryRepository, where the data is stored only in memory and not persisted.
2. A JsonFileRepository, where the data is persisted to a JSON text file, useful for debugging.
3. A LiteDbRepository, where the data is persisted to a LiteDb database.

## Installation

HotRod implementations are available as Nuget packages:

* [![Nuget](https://img.shields.io/nuget/v/HotRod)](https://www.nuget.org/packages/HotRod/) - **HotRod** - Contains: MemoryRepository, JsonFileRepository
* [![Nuget](https://img.shields.io/nuget/v/HotRod.LiteDb)](https://www.nuget.org/packages/HotRod.LiteDb/) - **HotRod.LiteDb** - Contains: LiteDbRepository

## Contributing

Additions and bug fixes are welcome. For larger changes please open an issue to discuss the changes.

### Step 1

Start by forking the repository then cloning that to your local machine.

```
git clone https://github.com/[you]/HotRod
```

### Step 2

Make the changes, and please **test** them. For larger changes, include an example.

### Step 3

Create a pull request.

There may be some back and forth, but we appreciate you working with us to get your contributions merged.
