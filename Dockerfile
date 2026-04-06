FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
# Mail gönderimi için eksik olan kütüphaneyi yüklüyoruz
RUN apt-get update && apt-get install -y libgssapi-krb5-2
USER app
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


COPY ["InventorySales.Api/InventorySales.Api.csproj", "InventorySales.Api/"]
COPY ["InventorySales.Application/InventorySales.Application.csproj", "InventorySales.Application/"]
COPY ["InventorySales.Infrastructure/InventorySales.Infrastructure.csproj", "InventorySales.Infrastructure/"]
COPY ["InventorySales.Domain/InventorySales.Domain.csproj", "InventorySales.Domain/"]


RUN dotnet restore "InventorySales.Api/InventorySales.Api.csproj"

COPY . .
WORKDIR "/src/InventorySales.Api"
RUN dotnet build "InventorySales.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
RUN dotnet publish "InventorySales.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventorySales.Api.dll"]