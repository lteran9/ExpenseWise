# ExpenseWise documentation

This directory documents the system design, interfaces, and operational considerations for ExpenseWise. It is also a project knowledgebase for contributors and coding agents.

## Start here

- [Product scope and definition of done](product/README.md): the agreed Phase 1 boundaries and proposed detailed acceptance criteria.
- [Current-state assessment](product/current-state.md): built capabilities, gaps, and verification limits.
- [Delivery roadmap](product/roadmap.md): Docker-first work packages and release gates.
- [API](api/README.md): HTTP surface and request flow.
- [Architecture](architecture/README.md): layers, dependencies, and implementation guidance.
- [Architecture decision records](architecture/decisions/README.md): accepted decisions and their consequences. Read the relevant ADRs before proposing changes that affect architecture.
- [Operations](operations/README.md): local build, test, and Compose guidance.
- [Security](security/README.md): public-repository and secret-handling guidance.

## Guidance for coding agents

Before changing a cross-cutting design, read the architecture overview and relevant ADRs. Treat ADRs as records of current decisions, not as a substitute for checking implementation: when code and documentation differ, verify the code, then update the documentation or propose a new ADR as part of the change.

Keep documentation factual and public-safe. Use placeholders for configuration examples; never add credentials, personal data, production endpoints, or private operational details.

## Documentation conventions

- Use Markdown links relative to this directory.
- Keep ADRs short, dated, and append-only. Supersede an accepted decision with a new ADR rather than silently rewriting its history.
- Clearly distinguish verified implementation facts from future recommendations.
