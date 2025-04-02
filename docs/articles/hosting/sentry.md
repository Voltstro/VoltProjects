# Sentry

Volt Projects has an optional integration with [Sentry](https://sentry.io) for error and traces tracking. The integration is only for the server side, there is no tracking on the client side as its not needed.

The integration was built using Open Telemetry, so the apps are not directly tied with Sentry, and other more generic telemetry solutions could be integrated.

## Setup

Sentry is configured using the `Tracking.Sentry` option in app settings. The main option you will need to set is your [DSN](https://docs.sentry.io/concepts/key-terms/dsn-explainer/).

```json
"Tracking": {
    "Sentry": {
        "Dsn": "<DSN Here>",
        "SendDefaultPii": true,
        "MaxRequestBodySize": "Always",
        "MinimumBreadcrumbLevel": "Debug",
        "MinimumEventLevel": "Warning",
        "AttachStackTrace": true,
        "TracesSampleRate": 0.7
    }
}
```

You can read more about [Sentry's options on their docs](https://docs.sentry.io/platforms/dotnet/configuration/options/#core-options).

It is recommended to set the DSN using environment variables.

```ini
Tracking__Sentry__Dsn=""
```

## Why no client?

Volt Projects mostly does everything server side, and there is very little JS (most of which is Bootstrap's). In a more complex app (such as an SPA), it would make more sense, but something like this would just add unnecessary overhead to the client. 
