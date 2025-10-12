#!/bin/bash
set -e

WWWROOT_PATH="$1"

if [ -z "$WWWROOT_PATH" ]; then
  echo "Error: WWWROOT_PATH not provided"
  echo "Usage: $0 <wwwroot_path>"
  exit 1
fi

if [ ! -d "$WWWROOT_PATH" ]; then
  echo "Error: Directory not found: ${WWWROOT_PATH}"
  exit 1
fi

ASSETS_FILE="${WWWROOT_PATH}/service-worker-assets.js"

if [ ! -f "$ASSETS_FILE" ]; then
  echo "Warning: service-worker-assets.js not found at ${ASSETS_FILE}"
  echo "Skipping integrity hash updates..."
  exit 0
fi

echo "Updating integrity hashes in service-worker-assets.js..."

# Extract the assets array from service-worker-assets.js
# We'll rebuild it with updated hashes

# Files that were modified after build
MODIFIED_FILES=(
  "index.html"
  "service-worker.js"
  "service-worker.published.js"
)

for file in "${MODIFIED_FILES[@]}"; do
  FILE_PATH="${WWWROOT_PATH}/${file}"

  if [ -f "$FILE_PATH" ]; then
    echo "Processing ${file}..."

    # Calculate new SHA-256 hash
    NEW_HASH=$(openssl dgst -sha256 -binary "$FILE_PATH" | openssl base64)
    echo "  New hash: sha256-${NEW_HASH}"

    # Update hash in service-worker-assets.js
    # Pattern: "hash":"sha256-...", followed by "url":"filename"
    sed -i "s|\"hash\":\"sha256-[^\"]*\",\"url\":\"${file}\"|\"hash\":\"sha256-${NEW_HASH}\",\"url\":\"${file}\"|g" "$ASSETS_FILE"

    echo "  Updated!"
  else
    echo "  ${file} not found, skipping..."
  fi
done

# Remove appsettings.json from assets if it exists
echo "Removing appsettings.json from assets..."
sed -i '/appsettings\.json/d' "$ASSETS_FILE"

echo "service-worker-assets.js updated successfully!"
