#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/WeComLoad.Open.Blazor/WeComLoad.Open.Blazor.csproj", "src/WeComLoad.Open.Blazor/"]
COPY ["src/WeComLoad.Shared/WeComLoad.Shared.csproj", "src/WeComLoad.Shared/"]
RUN dotnet restore "src/WeComLoad.Open.Blazor/WeComLoad.Open.Blazor.csproj"
COPY . .
WORKDIR "/src/src/WeComLoad.Open.Blazor"
RUN dotnet build "WeComLoad.Open.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WeComLoad.Open.Blazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeComLoad.Open.Blazor.dll"]