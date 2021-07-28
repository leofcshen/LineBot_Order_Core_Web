#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["LineBot_Order_Core_Web/LineBot_Order_Core_Web.csproj", "LineBot_Order_Core_Web/"]
RUN dotnet restore "LineBot_Order_Core_Web/LineBot_Order_Core_Web.csproj"
COPY . .
WORKDIR "/src/LineBot_Order_Core_Web"
RUN dotnet build "LineBot_Order_Core_Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LineBot_Order_Core_Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LineBot_Order_Core_Web.dll"]