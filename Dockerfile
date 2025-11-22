FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Innoshop.ProductManagement/ Innoshop.ProductManagement/
COPY Innoshop.Contracts/ Innoshop.Contracts/
RUN dotnet restore Innoshop.ProductManagement/src/ProductManagement.API/ProductManagement.API.csproj
RUN dotnet publish Innoshop.ProductManagement/src/ProductManagement.API/ProductManagement.API.csproj -c Release -o /app/publish
RUN ls -l /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [ "dotnet", "ProductManagement.API.dll" ]