FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["CreditApi.csproj", "./"]
RUN dotnet restore "CreditApi.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet publish "CreditApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CreditApi.dll"]
