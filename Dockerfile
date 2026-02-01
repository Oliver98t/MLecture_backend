# Development Dockerfile with hot reload support
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install Azure Functions Core Tools and network tools
RUN apt-get update && \
    apt-get install -y wget curl dnsutils ca-certificates && \
    update-ca-certificates && \
    wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y azure-functions-core-tools-4 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /workspace

# Copy project file and restore dependencies
COPY *.csproj ./

# Copy your custom CA certificate into the container
COPY Cert.crt /usr/local/share/ca-certificates/

# Update the CA trust store
RUN update-ca-certificates

RUN curl -v https://api.nuget.org/v3/index.json

# Test connectivity and restore packages
RUN dotnet restore --verbosity normal

EXPOSE 80

# Run func start for isolated worker model (auto-detects .NET 8)
#CMD ["func", "start", "--port", "80"]

# allow terminal to be open so container can be used to build/run project
CMD ["sleep", "infinity"]