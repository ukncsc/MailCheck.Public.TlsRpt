FROM mcr.microsoft.com/dotnet/core/runtime:2.1
ARG publish_dir

WORKDIR /app
COPY $publish_dir .

ENTRYPOINT ["dotnet", "MailCheck.TlsRpt.Migration.dll"]
