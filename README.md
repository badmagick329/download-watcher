# Download Watcher

A cross-platform file watcher that automatically sorts files into target folders based on configurable rules. Run it in the background to monitor a folder (like your Downloads folder) and automatically organize newly arriving files, or run it once to sort existing files.

## Features

- **Background monitoring**: Watch a folder and automatically move new files as they arrive
- **One-off sorting**: Sort all existing files in a folder with a single command
- **Flexible rules**: Define rules using regex patterns to match filenames
- **Cross-platform**: Works on Windows, macOS, and Linux
- **Download detection**: Waits for files to finish downloading before moving them
- **Configurable delay**: Add a delay before moving files to handle downloads on a slow connection
- **Duplicate handling**: Automatically renames files if a file with the same name exists at the destination

## Installation

### Download Binary

Download the latest release for your platform from the [Releases](https://github.com/badmagick329/download-watcher/releases) page.

### Build from Source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
# Clone the repository
git clone https://github.com/badmagick329/download-watcher
cd download-watcher

# Build for any platform
# Windows
dotnet publish ./src/DownloadWatcher.Console/DownloadWatcher.Console.csproj -c Release -r win-x64 --output ./publish/windows --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true

# macOS
dotnet publish ./src/DownloadWatcher.Console/DownloadWatcher.Console.csproj -c Release -r osx-x64 --output ./publish/osx --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true

# Linux
dotnet publish ./src/DownloadWatcher.Console/DownloadWatcher.Console.csproj -c Release -r linux-x64 --output ./publish/linux --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true
```

Or use the included Makefile:

```bash
make win    # Build for Windows
make osx    # Build for macOS
make linux  # Build for Linux
make all    # Build for all platforms
```

## Usage

### Basic Usage

Monitor a directory for new files:

```bash
./watcher -d ~/Downloads
```

### Move Existing Files

Sort all existing files in a directory immediately:

```bash
./watcher -d ~/Downloads --move-now
```

### With Custom Rules File

```bash
./watcher -d ~/Downloads -r ~/my-rules.txt
```

### With Move Delay

Add a delay (in seconds) before moving files. Useful for large downloads:

```bash
./watcher -d ~/Downloads --move-delay 600
```

### Debug Mode

Enable debug logging to a file:

```bash
./watcher -d ~/Downloads --debug
```

### Command Line Options

| Option         | Short | Description                                                    |
| -------------- | ----- | -------------------------------------------------------------- |
| `--directory`  | `-d`  | Directory to monitor for downloads (required)                  |
| `--rules-file` | `-r`  | Path to rules file (default: `rules.txt` in current directory) |
| `--move-now`   | `-m`  | Move existing files immediately and exit                       |
| `--move-delay` | `-md` | Delay in seconds before moving files (default: 0)              |
| `--debug`      |       | Enable debug logging to file                                   |

## Rules File

The rules file defines how files are sorted. Each rule consists of a regex pattern and a target path.

### Format

```
@"<regex-pattern>" <target-path>
```

- The pattern uses C# regex syntax wrapped in `@"..."` (verbatim string literal)
- The target path can use `~` to refer to your home directory
- Lines that don't match the format are ignored (use this for comments)
- When multiple rules match a file, the **last matching rule wins**

### Example Rules

```
# Sort video files
@"^.*\.(mp4|avi|webm|mkv)$" ~/Videos

# Sort images
@"^.*\.(jpg|jpeg|png|gif)$" ~/Pictures

# Sort documents
@"^.*\.(pdf|docx|xlsx|txt)$" ~/Documents

# Sort archives and installers
@"^.*\.(zip|tar|gz|exe|msi|dmg)$" ~/Downloads/Archives

# More specific rules override general ones (last match wins)
# Screenshots go to a separate folder
@"^Screenshot.*\.png$" ~/Pictures/Screenshots
```

### Rule Priority

Rules are evaluated in order, and the **last matching rule** is applied. This allows you to define general rules first and add specific overrides later:

```
# General rule: all images go to Pictures
@"^.*\.jpg$" ~/Pictures

# Specific rule: WhatsApp images go to a subfolder
@"^WhatsApp Image.*\.jpg$" ~/Pictures/WhatsApp
```

In this example, `WhatsApp Image 2024-01-01.jpg` would go to `~/Pictures/WhatsApp`, while `photo.jpg` would go to `~/Pictures`.

## How It Works

### Background Mode (default)

1. The watcher monitors the specified directory for new or renamed files
2. When a file is detected, it waits for the configured delay (`--move-delay`)
3. Before moving, it checks if the file is still being written to (downloading) by comparing file sizes over a short interval
4. Once the file is stable, it matches the filename against the rules and moves it to the target folder
5. If a file with the same name exists at the destination, a number suffix is added (e.g., `file (1).pdf`)

### One-off Mode (`--move-now`)

Processes all existing files in the directory once and exits.

## Use Cases

- **Organize Downloads**: Automatically sort downloads by file type as they complete
- **Clean up folders**: Run once to categorize files in a messy folder by extension or naming pattern
- **Media organization**: Sort photos, videos, and music into appropriate folders
- **Document management**: Separate documents by type (PDFs, spreadsheets, text files)

## Tips

- Start with broad rules for file extensions, then add specific rules for special cases
- Use the `--debug` flag to troubleshoot rule matching issues
- For large files or slow connections, increase `--move-delay` to ensure files finish downloading
- Test rules with `--move-now` on a small folder before setting up background monitoring
