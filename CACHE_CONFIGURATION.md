# Image Cache Configuration

This implementation uses PostgreSQL-based caching for generated Japanese character images, providing better Docker compatibility compared to the previous file-based approach. The cache provides several benefits:

- **Docker-friendly**: Works seamlessly in containerized environments without file system permissions issues
- **Persistence**: Cache survives application restarts and deployments
- **Scalability**: Better for handling large numbers of characters with database-backed storage
- **Reliability**: Leverages existing PostgreSQL infrastructure for consistency

## Configuration Options

The cache behavior is controlled via the `ImageCache` section in `appsettings.json`:

```json
{
  "ImageCache": {
    "CacheExpirationHours": 168,
    "EnableCleanup": true,
    "CleanupIntervalHours": 24
  }
}
```

### Configuration Properties

- **CacheExpirationHours**: How long images are kept before being considered expired (default: 168 hours = 1 week)
- **EnableCleanup**: Whether to automatically clean up expired images (default: `true`)
- **CleanupIntervalHours**: How often to run cleanup (default: 24 hours)

## Database Schema

The cache stores images in a dedicated `ImageCache` table with the following structure:

- **Id**: Primary key (GUID)
- **Character**: The Japanese character (indexed for fast lookups)
- **CacheKey**: SHA256-based hash for efficient retrieval (unique index)
- **ImageData**: PNG image bytes stored as `bytea` in PostgreSQL
- **AccessCount**: Number of times the image has been accessed
- **LastAccessedAt**: Timestamp of last access
- **CreatedAt/UpdatedAt**: Audit timestamps managed by the domain entity

## Migration from File-based Cache

The application now uses `DatabaseImageCacheService` instead of the file-based `ImageCacheService` for character image caching. The old file-based service is still available for other components that may need it (like StrokeOrderService).

No manual migration is required - the new system will automatically regenerate images as needed and store them in the database.

## Cache Implementation Details

- Images are stored as binary data in PostgreSQL with SHA256 hash-based cache keys
- Cache cleanup runs as a background service when enabled
- Thread-safe operations using Entity Framework Core
- Cache statistics (hit/miss counts) are available for monitoring
- Access tracking to monitor cache usage patterns