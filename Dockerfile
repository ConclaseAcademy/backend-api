FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

# Copy everything and build
COPY . ./
RUN dotnet restore \
 && dotnet build -c Release \
 && dotnet publish -c Release -o out

# Install EF tools and add to PATH
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80

# Apply migrations with Release configuration
ENTRYPOINT ["sh", "-c", "dotnet ef database update --project MentalHealthCompanion.API --configuration Release && dotnet out/MentalHealthCompanion.API.dll"]