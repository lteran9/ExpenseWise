ARG MYSQL_BASE_IMAGE=docker.io/library/mysql@sha256:9c3380eac945af0736031b200027f581925927c81e010056214a4bd6b6693714
FROM ${MYSQL_BASE_IMAGE}

COPY infra/terraform/mysql-client/create-app-user.sql /scripts/create-app-user.sql
