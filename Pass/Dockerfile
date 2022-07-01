#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal-arm64v8 AS base
RUN apt-get update && apt-get install -y libgdiplus
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal-arm64v8 AS build
WORKDIR /src
COPY ["Pass/Pass.csproj", "Pass/"]
RUN dotnet restore "Pass/Pass.csproj"
COPY . .
WORKDIR "/src/Pass"
RUN dotnet build "Pass.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pass.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pass.dll"]