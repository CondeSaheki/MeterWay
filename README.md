# MeterWay

**MeterWay** is a Dalamud-based plugin for Final Fantasy XIV that displays and helps you analyze combat data in real-time.

It is designed to work alongside tools like **IINACT** or **ACT**, acting as a live log parser to deliver detailed combat statistics even during encounters.

---

## 🔧 Installation (via Dalamud)

1. Open the **"Experimental"** tab in the Dalamud plugin installer.
2. Paste the following URL into the empty box under **Custom Plugin Repositories**:
   ```plaintext
   https://raw.githubusercontent.com/CondeSaheki/MeterWay/main/repo.json
   ```
3. Click the **`+`** button.
4. Click the **💾 Save** button.
5. Find and install **MeterWay** from the list.

---

## 🛠️ Building from Source

### Prerequisites

- **FINAL FANTASY XIV**, **XIVLauncher**, and **Dalamud** installed.
- Game must have been launched at least once with Dalamud active.
- `.NET 9 SDK` installed and configured.
- `DALAMUD_HOME` environment variable set (only required for custom Dalamud install paths).

> **Note:** This plugin now targets **.NET 9** and **Dalamud API 12**.

### Build Instructions

1. Clone the repository:
   ```bash
   git clone https://github.com/CondeSaheki/MeterWay.git
   cd MeterWay
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   `````

3. Build the plugin:
   ```bash
   dotnet build -c Release
   `````

---

## 📦 Add to Game (Manual Dev Plugin)

To load the plugin manually in Dalamud:

1. Go to the **Dev Plugins** tab.
2. Add the full path to the built `MeterWay.dll`.
3. Use the **Plugin Installer** to install it.
4. Enable/disable it, we don't recommend to turn on the auto-load option for debug builds.

---

## 💬 Support / Feedback

Feel free to open an [issue on GitHub](https://github.com/CondeSaheki/MeterWay/issues) or reach out via Discord if you encounter bugs or have suggestions!

---

Happy parsing!