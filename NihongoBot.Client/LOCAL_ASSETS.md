# Downloading Local Assets

The client project uses libman to manage client-side libraries locally instead of CDN references.

## Setup Local Assets

To download the required Bootstrap and Font Awesome files locally:

1. Navigate to the client directory:
   ```bash
   cd NihongoBot.Client
   ```

2. Install libman CLI (if not already installed):
   ```bash
   dotnet tool install -g Microsoft.Web.LibraryManager.Cli
   ```

3. Restore libraries:
   ```bash
   libman restore
   ```

This will download:
- Bootstrap 5.3.0 (CSS and JS)
- Font Awesome 6.0.0 (CSS)

## Alternative Manual Download

If libman fails due to network issues, you can manually download the files:

1. Bootstrap CSS: https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css
   - Save to: `wwwroot/lib/bootstrap/css/bootstrap.min.css`

2. Bootstrap JS: https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js
   - Save to: `wwwroot/lib/bootstrap/js/bootstrap.bundle.min.js`

3. Font Awesome CSS: https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css
   - Save to: `wwwroot/lib/font-awesome/css/all.min.css`