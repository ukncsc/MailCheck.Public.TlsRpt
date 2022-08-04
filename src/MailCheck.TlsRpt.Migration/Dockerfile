FROM mcr.microsoft.com/dotnet/core/runtime:3.1-focal
ARG publish_dir

WORKDIR /app
COPY $publish_dir .

ENTRYPOINT ["dotnet", "MailCheck.TlsRpt.Migration.dll"]
