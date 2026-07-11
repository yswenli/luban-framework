# 指定基础镜像
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# 安装 OpenCV 依赖
RUN apt update && apt install -y apt-get install libgtk2.0-0 libjpeg62-turbo libpng16-16 libtiff6 libavcodec59 libavformat59
 
# 设置工作目录
WORKDIR /app 

# 复制所有文件并生成输出
COPY . ./

RUN dotnet restore 

RUN dotnet publish -c Release -o out
 
# 使用 ASP.NET Core 运行时作为基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0
 
# 设置工作目录
WORKDIR /app
 
# 复制生成的输出到当前工作目录
COPY --from=build-env /app/out .
 
# 暴露端口
EXPOSE 80
 
# 设置启动命令
ENTRYPOINT ["dotnet"]