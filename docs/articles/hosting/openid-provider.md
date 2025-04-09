# OpenId Provider

All authentication in Volt Projects is handled using OpenId. This allows the authentication code in Volt Projects to very pretty simple as the bulk of it gets handled by [Microsoft.AspNetCore.Authentication.OpenIdConnect](https://learn.microsoft.com/en-au/aspnet/core/security/authentication/configure-oidc-web-authentication?view=aspnetcore-9.0). The biggest down-side to this approach is that its another external service that the system relies on.

## Example Providers

Here are some example providers, when picking a provider, ensure that unwanted people cannot just login using a public account.

- [Microsoft Entra Id](https://learn.microsoft.com/en-au/entra/fundamentals/whatis)
- [Google Identity](https://developers.google.com/identity/openid-connect/openid-connect)
- [GitLab](https://docs.gitlab.com/integration/openid_connect_provider/)

Here are also some local openid providers, these have not been tested.

- [Authentik](https://goauthentik.io/)
- [Ory Hydra](https://www.ory.sh/hydra/)

## Config

You will need the provided client ID, client secret and authority from your openid provider.

That can then be set using environment variables.

```ini
Config__OpenIdConfig__ClientId=""
Config__OpenIdConfig__ClientSecret=""
Config__OpenIdConfig__Authority=""
```
