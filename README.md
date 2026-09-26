# BACAP Addon Helper

[![GitHub Release](https://img.shields.io/github/v/release/ItzSkyReed/BACAP-Addon-Helper)](https://github.com/ItzSkyReed/BACAP-Addon-Helper/releases)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License: CC BY-NC-SA 4.0](https://img.shields.io/badge/License-CC_BY--NC--SA_4.0-lightgrey.svg)](LICENSE.md)

An interactive TUI (Terminal User Interface) automation and validation toolkit for Minecraft datapack developers building addons and compatibility packs for **[BlazeandCave's Advancements Pack (BACAP)](https://modrinth.com/datapack/blazeandcaves-advancements-pack)**.

---

## Features

- **Datapack Validation Engine**: Statically validates advancement trees, namespace consistency, milestone paths, and formatting rules (Title Case, plain text checks, whitespace sanitization).
- **Functions & Reward Automation**: Automatically sets up advancement execution commands, macro calls, XP rewards, custom items (with Data Components/NBT support), and trophy rewards.
- **Milestones**: Generates advancement milestone tracking and tab completion functions.
- **Checlists**: Generates checklists like [/trigger bacaped_mob_universe](https://github.com/Komaru-cats/BACAP-Enhanced-Discoveries/blob/main/BACAP_Enhanced_Discoveries/data/bacaped/function/triggers_callback/mob_universe_trigger.mcfunction)
- **Language Pack Synchronization**:
    - Discovers all translatable text components across advancements and `.mcfunction` files.
    - Automatically generates and refreshes `base_translation.json`.
    - Patches target language files (e.g. `es_ar.json`), appending missing keys while protecting against syntax corruption and obsolete entries.
- **Interactive Component Wizards**: Terminal UI wizards for configuring items with components: custom names, lore, trim, enchantments, banner patterns, and potion effects.

---

## Installation & Quick Start

### 1. Download Pre-built Binaries (Recommended)
Download the latest standalone executable for your operating system from the **[Releases](https://github.com/ItzSkyReed/BACAP-Addon-Helper/releases)** page.

> **Note:** Standalone binaries are self-contained and do **not** require .NET runtime to be installed.

- **Windows:** `BACAP_Addon_Helper-win-x64.exe`
- **Linux:** `BACAP_Addon_Helper-linux-x64` (`chmod +x` before launching)
- **macOS:** `BACAP_Addon_Helper-osx-x64` / `osx-arm64`

### 2. First Run & Configuration
1. Place the executable into your working directory or datapack project root.
2. Run the application:
   ```bash
   ./BACAP_Addon_Helper-win-x64.exe
   ```
3. On the first launch, the tool will automatically generate a template `config.yaml` in the directory.
4. Configure config for you datapacks.

## 📖 Detailed Configuration Guide:
Check out our full configuration schema and examples on the Project Wiki.

# Building From Source
Prerequisites: .NET 10 SDK

```bash
# Clone the repository
git clone https://github.com/ItzSkyReed/BACAP-Addon-Helper.git
cd BACAP-Addon-Helper

# Run UI project directly
dotnet run --project src/UI/UI.csproj

# Run tests
dotnet test
```
## License
This project is licensed under the terms of the [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International Public License](LICENSE.md).