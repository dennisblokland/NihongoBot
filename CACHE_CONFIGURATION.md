# Image Cache Configuration

This implementation uses disk-based caching for generated Japanese character images. The cache provides several benefits:

- **Lower memory usage**: Images are stored on disk instead of RAM
- **Persistence**: Cache survives application restarts
- **Scalability**: Better for handling large numbers of characters

## Configuration Options

The cache behavior is controlled via the `ImageCache` section in `appsettings.json`:

```json
{
  "ImageCache": {
    "CacheDirectory": "cache/images",
    "CacheExpirationHours": 168,
    "EnableCleanup": true,
    "CleanupIntervalHours": 24
  }
}
```

### Configuration Properties

- **CacheDirectory**: Directory where cached images are stored (default: `cache/images`)
- **CacheExpirationHours**: How long images are kept before being considered expired (default: 168 hours = 1 week)
- **EnableCleanup**: Whether to automatically clean up expired images (default: `true`)
- **CleanupIntervalHours**: How often to run cleanup (default: 24 hours)

## Docker Deployment

When running in Docker, consider adding a volume mount for the cache directory to persist images across container restarts:

```yaml
services:
  nihongobot:
    volumes:
      - nihongobot-cache:/app/cache/images
```

Or using bind mounts:

```yaml
services:
  nihongobot:
    volumes:
      - ./cache:/app/cache/images
```

This ensures that generated images are preserved when containers are restarted or updated.

## Cache Implementation Details

- Images are stored as PNG files with SHA256 hash filenames to avoid filesystem conflicts
- Cache cleanup runs as a background service when enabled
- Thread-safe operations using semaphores for file writes
- Cache statistics (hit/miss counts) are available for monitoring