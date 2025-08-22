#!/bin/bash

# Script to download local assets for NihongoBot.Client
# This script downloads Bootstrap and Font Awesome files locally

echo "Downloading Bootstrap and Font Awesome assets..."

# Create directory structure
mkdir -p wwwroot/lib/bootstrap/css
mkdir -p wwwroot/lib/bootstrap/js
mkdir -p wwwroot/lib/font-awesome/css
mkdir -p wwwroot/lib/font-awesome/webfonts

# Download Bootstrap CSS
echo "Downloading Bootstrap CSS..."
curl -o wwwroot/lib/bootstrap/css/bootstrap.min.css \
  https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css

# Download Bootstrap JS
echo "Downloading Bootstrap JS..."
curl -o wwwroot/lib/bootstrap/js/bootstrap.bundle.min.js \
  https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js

# Download Font Awesome CSS
echo "Downloading Font Awesome CSS..."
curl -o wwwroot/lib/font-awesome/css/all.min.css \
  https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css

echo "Asset download complete!"
echo "Note: Font Awesome webfonts may need to be downloaded separately if icons are used."