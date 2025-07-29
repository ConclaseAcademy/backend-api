FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

# Copy everything into container
COPY . ./

# Install Node.js
RUN apt-get update && \
    apt-get install -y curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    node -v && npm -v

# Install Node.js dependencies (Node app is in root)
RUN npm install

# Build and publish .NET app
RUN dotnet restore \
 && dotnet build -c Release \
 && dotnet publish -c Release -o out

# Install EF Core CLI tools globally
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80

# Start both Node.js and .NET apps
CMD sh -c "dotnet ef database update --project MentalHealthCompanion.API --configuration Release && \
           node index.js & \
           dotnet out/MentalHealthCompanion.API.dll"
