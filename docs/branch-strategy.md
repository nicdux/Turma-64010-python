# Branch strategy

- `main`: production-ready releases; protected, changes enter via pull request.
- `develop`: integration branch; feature work merges here after required CI passes.
- `feature/<short-description>`: branch from `develop`, pull request back to `develop`.
- `release/<version>`: cut from `develop`, validate and pull request to `main`; merge the release back to `develop`.
- `hotfix/<short-description>`: cut from `main`, pull request to `main`, then merge the fix back to `develop`.

Conventional Commit prefixes are `feat`, `fix`, `refactor`, `ci`, `docs`, `security`, `test`, and `chore`. Pull requests run frontend lint/build and audit, backend tests and NuGet advisory checks, Bicep compilation, and the dashboard E2E test. Repository branch protection is a GitHub setting and must require these checks; workflow files cannot enforce their own merge requirements. Cloud provisioning remains a manual, cost-bearing operation and is not triggered by push.
