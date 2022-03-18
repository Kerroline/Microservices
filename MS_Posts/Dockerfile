#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MS_Posts.csproj", "MS_Posts/"]
RUN dotnet restore "MS_Posts/MS_Posts.csproj"
COPY . ./MS_Posts/
WORKDIR "/src/MS_Posts"
RUN dotnet build "MS_Posts.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MS_Posts.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MS_Posts.dll"]