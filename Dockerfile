# Builds the web client and serves it as static files with nginx. No backend is involved: the
# container is a convenience for organizations that want to host Citiz on their own network.
#
#   docker build -t citiz .
#   docker run --rm -p 8080:80 citiz
#   open http://localhost:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Citiz.Web/Citiz.Web.csproj -c Release -o /app/publish

FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY tools/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=3s CMD wget -qO- http://localhost/ >/dev/null || exit 1
