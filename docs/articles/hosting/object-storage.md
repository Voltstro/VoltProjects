# Object Storage

Volt Projects requires an object storage provider for uploading and serving static objects that a page might have (images, scripts, external objects).

Currently, three providers are supported. Azure Storage Containers, Google Cloud Storage and AWS S3. The AWS S3 provider should work with any other providers that use the S3 API.

## Configuring

### Public Access

Public access will need to be configured, as Volt Projects uses the public URL of the object in the page.

- [Configuring public access for Azure Containers](https://learn.microsoft.com/en-au/azure/storage/blobs/anonymous-read-access-configure)
- [Configuring public access for Google Cloud Storage](https://cloud.google.com/storage/docs/access-control/making-data-public)

### App Configurations

Value           |Azure                                                    |Google                            |AWS S3                                                |
--------------- |-------------------------------------------------------- |--------------------------------- |----------------------------------------------------- |
`Provider`      |Azure                                                    |Google                            |S3                                                    |
`ContainerName` |`<Container Name>`                                       |`<Bucket Name>`                   |`<Bucket Name>`                                       |
`BasePath`      |`https://<Account Name>.blob.core.windows.net/`          |`https://storage.googleapis.com/` |`https://<Bucket Name>.s3.<AWS Region>.amazonaws.com` |
`SubPath`       |Optional Sub Path                                        |Optional Sub Path                 |Optional Sub Path                                     |

### Credentials

The credentials must be set using environment variables. The environment variable name changes depending on what storage provider you use.

Provider |Name                                                        |
-------- |----------------------------------------------------------- |
Azure    |`VP_AZURE_CREDENTIAL`                                       |
Google   |`VP_GCS_CREDENTIAL`                                         |
AWS S3   |`VP_S3_KEY_ID`<br>`VP_S3_ACCESS_KEY`<br>`VP_S3_SERVICE_URL` |

### Custom Domains

You can use custom domains for the URL where blobs will be served. Set `BasePath` to your custom domain that you wish to use.
