# ==============================================================================
# STAGE 1: Build de4dotEx (.NET 10.0 Cross-Platform Release)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-builder
WORKDIR /src

# Copy all files to build
COPY . .

# Publish both de4dot and the de4dot.mcp server targeting net10.0
RUN dotnet publish -c Release -f net10.0 -o /app/publish/de4dot de4dot/de4dot.csproj
RUN dotnet publish -c Release -f net10.0 -o /app/publish/mcp de4dot.mcp/de4dot.mcp.csproj
RUN rm -rf /app/publish/**/*.pdb /app/publish/**/*.xml

# ==============================================================================
# STAGE 2: Final Runtime Image (.NET 10.0 on Ubuntu / Runtime)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# Install native dependencies required for execution (e.g. globalization, bash)
RUN apt-get update && apt-get install -y \
    libicu-dev \
    bash \
    && rm -rf /var/lib/apt/lists/*

# Copy de4dot and MCP published folders
COPY --from=dotnet-builder /app/publish/de4dot ./de4dot
COPY --from=dotnet-builder /app/publish/mcp ./mcp

# Create symlinks for global accessibility
RUN ln -s /app/de4dot/de4dot /usr/local/bin/de4dot && \
    ln -s /app/mcp/de4dot.mcp /usr/local/bin/de4dot-mcp

# Create a unified entrypoint script inside the final image
RUN printf '#!/bin/bash\n\
if [ "$1" = "--mcp" ] || [ "$1" = "-mcp" ]; then\n\
    shift\n\
    exec /app/mcp/de4dot.mcp "$@"\n\
else\n\
    exec /app/de4dot/de4dot "$@"\n\
fi\n' > /app/entrypoint.sh && chmod +x /app/entrypoint.sh

# Expose HTTP port 8080 (for MCP Web API mode)
EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
CMD ["--help"]
