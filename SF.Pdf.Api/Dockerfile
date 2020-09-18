#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["SF.Pdf.Api/SF.Pdf.Api.csproj", "SF.Pdf.Api/"]
COPY ["SF.Pdf.Operations/SF.Pdf.Operations.csproj", "SF.Pdf.Operations/"]
RUN dotnet restore "SF.Pdf.Api/SF.Pdf.Api.csproj"
COPY . .
WORKDIR "/src/SF.Pdf.Api"
RUN dotnet build "SF.Pdf.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SF.Pdf.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SF.Pdf.Api.dll"]