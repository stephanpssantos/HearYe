# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env
WORKDIR /Server
COPY Server/HearYe.Server.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish Server/HearYe.Server.csproj -c Release -o /Publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /Publish
COPY --from=build-env /Publish .
EXPOSE 7193
ENV ASPNETCORE_URLS=http://+:7193
ENTRYPOINT ["dotnet", "HearYe.Server.dll"]