# Kite & Key .NET foundation

Portable components extracted and adapted from Delphinium Common. Requires .NET 10.
All packages are MIT licensed; the repository's `LICENSE` and this README are included
in each package. Package builds normalize source paths, and symbol packages include
portable PDBs and GitHub Source Link without local machine paths.

| Package | Dependency | Contents |
| --- | --- | --- |
| `KiteKey.Core` | .NET 10 only | Keyed caches, collections, async/concurrency, text/date/enum/reflection helpers, file rotation and file abstractions |
| `KiteKey.Hosting` | Microsoft.Extensions configuration/DI/hosting/options 10.0.0 | Required configuration, development defaults, validated options, scoped values, service discovery |
| `KiteKey.Mail` | `KiteKey.Core`, Microsoft.Extensions.Logging.Abstractions 10.0.0 | SMTP sender, recipient allowlist, plain text mail builder |

The expanded packages are version **0.2.0**; `0.1.0` is already published.
`KiteKey.Core` caches are not thread-safe:
coordinate access externally when sharing instances across threads. .NET 10 supplies
most async LINQ operations: Core deliberately omits duplicate extension signatures
to avoid ambiguous method calls. File rotation manages paths supplied by the caller;
the physical directory abstraction rejects paths escaping its root. `Obfuscate` is
cosmetic, not a way to anonymize sensitive data. SMTP settings
must be supplied by the consuming app; this repository contains no credentials.
Omit the allowlist only when arbitrary recipients are intended (such as production).
Mail messages are owned and disposed by the caller. `MailSender` owns its SMTP
clients and releases them after each send.

```csharp
using KiteKey.Hosting;
using KiteKey.Mail;
using Microsoft.Extensions.DependencyInjection;

services.AddScopeValue<RequestContext>();
services.AddOptionsValidated<MyOptions>("MyOptions");
using IServiceScope scope = provider.CreateScope();
scope.SetScopeValue(new RequestContext());

using MailSender sender = new(
    new MailSettings { SmtpServer = "smtp.example.com", FromAddress = "sender@example.com" },
    allowedDestinations: ["recipient@example.com"]);
using var message = sender.CreateMailMessage("recipient@example.com", "Hello", "Plain text");
// await sender.SendAsync(message);
```

Build locally from the repository root with `dotnet test KiteKey.sln` and
`dotnet pack KiteKey.sln -c Release -o artifacts`.

CI packs on pull requests and main pushes. The active NuGet.org trusted-publishing
policy is owned by the **KiteKey** NuGet organization and trusts GitHub owner
`Kite-Key`, repository `foundation`, workflow `release.yml`, and environment
`nuget`. Its scope is the **exact IDs** `KiteKey.Core`, `KiteKey.Hosting`, and
`KiteKey.Mail`, not a `KiteKey.*` wildcard. The GitHub repository variable
`NUGET_USER` is `taylorchasewhite`, the NuGet.org **policy creator** required
by `NuGet/login@v1`—not the organization name `KiteKey` or an email address.
Protect the `nuget` environment and release tags. Only after verifying tests,
contents, ownership, and a **new, unpublished version**, tag that version on
`main` as `v<major>.<minor>.<patch>`. Version `v0.2.0` is already tagged; do
not reuse it. The workflow uses OIDC and a short-lived token, not a saved API key.

See [architecture and source mapping](docs/architecture.md).
