# Kite & Key .NET foundation

Portable components extracted and adapted from Delphinium Common. Requires .NET 10.
All packages are MIT licensed; the repository's `LICENSE` and this README are included
in each package. The source packages include portable PDBs and GitHub Source Link.

| Package | Dependency | Contents |
| --- | --- | --- |
| `KiteKey.Core` | .NET 10 only | Keyed caches, keyed multivalue collection, async lock, span line reader |
| `KiteKey.Hosting` | Microsoft.Extensions configuration/DI/hosting/options 10.0.0 | Required configuration, development defaults, validated options, scoped values |
| `KiteKey.Mail` | `KiteKey.Core`, Microsoft.Extensions.Logging.Abstractions 10.0.0 | SMTP sender, recipient allowlist, plain text mail builder |

The packages start at version **0.1.0**. `KiteKey.Core` caches are not thread-safe:
coordinate access externally when sharing instances across threads. SMTP settings
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

CI packs on pull requests and main pushes. The release workflow is a **skeleton**:
before publishing, configure a nuget.org trusted publisher for the **Kite-Key**
package owner, GitHub owner `Kite-Key`, repository `foundation`, workflow
`release.yml`, GitHub environment `nuget`, and policy package scope
`KiteKey.*` (authorize new packages and new versions as needed). Set the `NUGET_USER` GitHub
environment variable to the NuGet **organization profile name** (not email),
and protect the `nuget` environment and release tags. Only after confirming
package IDs are owned by that organization, push a `v0.1.0`-style tag on
`main`. The workflow uses OIDC and a short-lived token, not a saved API key.

See [architecture and source mapping](docs/architecture.md).
