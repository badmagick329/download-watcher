PROJECT_PATH := ./src/DownloadWatcher.Console/DownloadWatcher.Console.csproj
PROJECT_NAME := DownloadWatcher
CONFIGURATION := Release
OUTPUT_DIR := ./publish

all: win osx linux

win:
	@echo "Building for Windows (win-x64)..."
	dotnet publish $(PROJECT_PATH) -c $(CONFIGURATION) -r win-x64 --output $(OUTPUT_DIR)/windows --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -p:UseAppHost=true

osx:
	@echo "Building for macOS (osx-x64)..."
	dotnet publish $(PROJECT_PATH) -c $(CONFIGURATION) -r osx-x64 --output $(OUTPUT_DIR)/osx --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -p:UseAppHost=true

linux:
	@echo "Building for Linux (linux-x64)..."
	dotnet publish $(PROJECT_PATH) -c $(CONFIGURATION) -r linux-x64 --output $(OUTPUT_DIR)/linux --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -p:UseAppHost=true

clean:
	@echo "Cleaning up..."
	rm -rf $(OUTPUT_DIR)

run:
	@echo "Running with location: $(folder)"
	dotnet run --project ${PROJECT_PATH} "$(folder)"

.PHONY: all win osx linux clean
