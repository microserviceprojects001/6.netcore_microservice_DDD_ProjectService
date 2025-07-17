# 阶段1：构建
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# 复制项目文件并恢复依赖
COPY *.csproj ./
RUN dotnet restore

# 复制所有文件并构建
COPY . ./
RUN dotnet publish -c Release -o out

# 阶段2：运行时
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out ./
COPY ./cert.pfx /app/

ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/cert.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=YourPassword123

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "User.API.dll"]