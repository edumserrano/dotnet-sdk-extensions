# FROM mcr.microsoft.com/dotnet/core/sdk:5.0.100-preview.3-focal
FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy
# FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine

# RUN apk update
# RUN apk upgrade
# RUN apk add bash



# RUN wget https://dot.net/v1/dotnet-install.sh && chmod +x ./dotnet-install.sh
# ENV DOTNET_INSTALL_DIR=/usr/share/dotnet
# RUN ./dotnet-install.sh --channel 3.1
# RUN ./dotnet-install.sh --channel 5.0
# RUN ./dotnet-install.sh --channel 3.1 --install-dir /usr/share/dotnet
# RUN ./dotnet-install.sh --channel 5.0 --install-dir ${DOTNET_INSTALL_DIR}
# RUN ./dotnet-install.sh --channel 6.0 --install-dir ${DOTNET_INSTALL_DIR}

# # trust dev certs required for integration tests that use https scheme
RUN dotnet dev-certs https --trust
# RUN dotnet --info
# install debugger to allow debugging tests
# RUN wget https://aka.ms/getvsdbgsh && sh getvsdbgsh -v latest -l /vsdbg

# RUN apt-get update && apt-get install -y wget
# RUN wget https://dot.net/v1/dotnet-install.sh && chmod +x ./dotnet-install.sh
# RUN ./dotnet-install.sh --channel 3.1
# RUN ./dotnet-install.sh --channel 5.0
# RUN ./dotnet-install.sh --channel 6.0
# RUN ./dotnet-install.sh --channel 7.0

# ENV USERNAME=root
# ENV DOTNET_ROOT=${USERNAME}/.dotnet
# RUN printenv DOTNET_ROOT
# add dotnet and the dotnet tools folder to the PATH
# ENV PATH=${PATH}:${DOTNET_ROOT}:${DOTNET_ROOT}/tools
# set required value for runtime configuration options for globalization culture. See https://learn.microsoft.com/en-us/dotnet/core/runtime-config/globalization
# ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
# trust dev certs required for integration tests that use https scheme
# RUN dotnet dev-certs https --trust
# install debugger to allow debugging tests
# RUN wget https://aka.ms/getvsdbgsh && sh getvsdbgsh -v latest -l /vsdbg


# FROM ubuntu:22.04

# RUN apt-get update && apt-get install -y wget
# RUN wget https://dot.net/v1/dotnet-install.sh && chmod +x ./dotnet-install.sh
# RUN ./dotnet-install.sh --channel 3.1
# RUN ./dotnet-install.sh --channel 5.0
# RUN ./dotnet-install.sh --channel 6.0
# RUN ./dotnet-install.sh --channel 7.0

# ENV USERNAME=root
# ENV DOTNET_ROOT=${USERNAME}/.dotnet
# RUN printenv DOTNET_ROOT
# # add dotnet and the dotnet tools folder to the PATH
# ENV PATH=${PATH}:${DOTNET_ROOT}:${DOTNET_ROOT}/tools
# # set required value for runtime configuration options for globalization culture. See https://learn.microsoft.com/en-us/dotnet/core/runtime-config/globalization
# ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
# # trust dev certs required for integration tests that use https scheme
# RUN dotnet dev-certs https --trust
# # install debugger to allow debugging tests
# RUN wget https://aka.ms/getvsdbgsh && sh getvsdbgsh -v latest -l /vsdbg

