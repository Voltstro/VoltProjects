# Custom Dockerfile for setting up default azurite container
# From: https://stackoverflow.com/a/72327313

FROM mcr.microsoft.com/azure-storage/azurite:3.34.0

# Install azure-storage-blob python package
RUN apk update && \
    apk --no-cache add py3-pip && \
    apk add --virtual=build gcc libffi-dev musl-dev python3-dev && \
    pip3 install azure-storage-blob==12.25.0 --break-system-packages

# Copy init_azurite.py script
COPY ./init_azurite.py init_azurite.py

# Copy local blobs to azurite
COPY ./init_containers init_containers

# Run the blob emulator and initialize the blob containers
CMD python3 init_azurite.py --directory=init_containers & \
    azurite-blob --blobHost 0.0.0.0 --blobPort 10000 --skipApiVersionCheck
