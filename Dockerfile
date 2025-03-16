# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base

# Install required Linux dependencies for SkiaSharp
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    libpng16-16 \
    libjpeg62-turbo \
    libglib2.0-0 \
    libx11-6 \
    libxext6 \
    libxrender1 \
    && rm -rf /var/lib/apt/lists/*

# Set the working directory
WORKDIR /app

# Use the .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the entire solution into the build context
COPY . .

# Restore dependencies for the main project
WORKDIR /src/NihongoBot
RUN dotnet restore "NihongoBot.csproj"

# Publish only the NihongoBot project
RUN dotnet publish "NihongoBot.csproj" -c Release -o /app/publish --self-contained false

# Use the base image and copy the published files
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables to locate native libraries
ENV LD_LIBRARY_PATH=/app:$LD_LIBRARY_PATH

# Run the application
ENTRYPOINT ["dotnet", "NihongoBot.dll"]
