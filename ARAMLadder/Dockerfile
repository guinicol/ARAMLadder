FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 59864
EXPOSE 44371

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ARAMLadder/ARAMLadder.csproj ARAMLadder/
RUN dotnet restore ARAMLadder/ARAMLadder.csproj
COPY . .
WORKDIR /src/ARAMLadder
RUN dotnet build ARAMLadder.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ARAMLadder.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ARAMLadder.dll"]