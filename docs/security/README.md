# Security and public-repository hygiene

ExpenseWise is publicly hosted. Documentation and examples must be safe to publish and must not include secrets or private operational information.

## Safe documentation practices

- Never commit passwords, API keys, tokens, private keys, real connection strings, personal data, or production endpoints.
- Use clearly nonfunctional placeholders in examples and explain how values are supplied locally.
- Keep local secret files out of version control; review staged changes before pushing.
- Do not paste real credentials or customer data into logs, test fixtures, issues, or examples.
- Treat generated artifacts and configuration snapshots as public unless explicitly excluded from the repository.

## Application security changes

- Validate untrusted input at the application boundary and avoid exposing internal exception details to clients.
- Review authentication, authorization, session-cookie, and transport-security behavior in the actual startup and controller code before describing it as enforced.
- Keep database credentials out of source and inject them using an appropriate local or deployment secret mechanism.
- For a production deployment, separately establish and review TLS, authentication/authorization policy, secret rotation, database least privilege, logging/retention, backups, and incident response. This repository's local-development instructions do not establish those controls.

This page documents safe contributor practices, not a certification or claim that the application is production-hardened. Report suspected vulnerabilities through the repository's private reporting channel if one is available; do not publish exploit details or sensitive data in a public issue.
