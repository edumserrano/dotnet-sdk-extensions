# use same linux distro as github runner for ubuntu-latest which is what the workflows use
FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy

# When I try to install other dotnet versions the remote test environments fails
# to connnect to the Docker container. At least with dotnet 3.1 and dotnet 5.0.
# RUN wget https://dot.net/v1/dotnet-install.sh && chmod +x ./dotnet-install.sh
# ENV DOTNET_INSTALL_FOLDER=/usr/share/dotnet
# RUN ./dotnet-install.sh --channel 3.1 --install-dir ${DOTNET_INSTALL_FOLDER}
# RUN ./dotnet-install.sh --channel 5.0 --install-dir ${DOTNET_INSTALL_FOLDER}
# RUN ./dotnet-install.sh --channel 6.0 --install-dir ${DOTNET_INSTALL_FOLDER}

# trust dev certs required for integration tests that use https scheme
RUN dotnet dev-certs https --trust
# RUN dotnet --info
# install debugger to allow debugging tests
RUN wget https://aka.ms/getvsdbgsh && sh getvsdbgsh -v latest -l /vsdbg