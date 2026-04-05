CREATE SCHEMA IF NOT EXISTS app;

CREATE TABLE IF NOT EXISTS app."TaskItem" (
    "Id"          uuid                     NOT NULL,
    "Title"       character varying(200)   NOT NULL,
    "Description" character varying(2000),
    "Status"      text                     NOT NULL,
    "DueDate"     timestamp with time zone,
    "UserId"      uuid                     NOT NULL,
    "CreatedAt"   timestamp with time zone NOT NULL,
    "UpdatedAt"   timestamp with time zone NOT NULL,
    CONSTRAINT "PK_TaskItem" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_TaskItem_UserId"
    ON app."TaskItem" ("UserId");

CREATE TABLE IF NOT EXISTS app."AppUser" (
    "Id"              uuid                     NOT NULL,
    "ExternalSubject" character varying(255)   NOT NULL,
    "Username"        character varying(150)   NOT NULL,
    "Email"           character varying(255)   NOT NULL,
    "Role"            text                     NOT NULL,
    "CreatedAt"       timestamp with time zone NOT NULL,
    "LastSeenAt"      timestamp with time zone NOT NULL,
    CONSTRAINT "PK_AppUser" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_AppUser_ExternalSubject"
    ON app."AppUser" ("ExternalSubject");

CREATE UNIQUE INDEX IF NOT EXISTS "IX_AppUser_Email"
    ON app."AppUser" ("Email");

-- Seed demo users (no-op if they already exist)
INSERT INTO app."AppUser" ("Id", "ExternalSubject", "Username", "Email", "Role", "CreatedAt", "LastSeenAt")
VALUES
    ('00000000-0000-0000-0000-000000000001', 'admin-subject-001', 'admin', 'admin@ballastlaneboard.com', 'Admin', '2026-01-01T00:00:00+00:00', '2026-01-01T00:00:00+00:00'),
    ('00000000-0000-0000-0000-000000000002', 'user-subject-001', 'testuser', 'testuser@ballastlaneboard.com', 'User', '2026-01-01T00:00:00+00:00', '2026-01-01T00:00:00+00:00')
ON CONFLICT ("Id") DO NOTHING;
