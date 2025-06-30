-- Enable the uuid-ossp extension for UUID generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE "users" (
	"id" UUID DEFAULT uuid_generate_v4() NOT NULL UNIQUE,
    "login" VARCHAR(250) UNIQUE,
    "password_hash" VARCHAR(250) NOT NULL,
    PRIMARY KEY ("id")
);

CREATE TABLE "notifications" (
	"id" UUID DEFAULT uuid_generate_v4() NOT NULL UNIQUE,
    "recipient_user_id" UUID NOT NULL REFERENCES "users"("id"), -- Кому
    "sender_user_id" UUID NOT NULL REFERENCES "users"("id"), -- От кого\
    "message" TEXT NOT NULL,
    "created_at" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    PRIMARY KEY ("id")
);

CREATE TABLE "statuses" (
	"id" SMALLSERIAL,
    "name" VARCHAR(250),
    "eng_name" VARCHAR(250) NOT NULL,
    PRIMARY KEY ("id")
);

CREATE TABLE "notification_status_log" (
    "id" UUID DEFAULT uuid_generate_v4() NOT NULL UNIQUE,
    "status_id" SMALLINT NOT NULL REFERENCES "statuses"("id"),
    "notification_id" UUID NOT NULL REFERENCES "notifications"("id"),
    "created_at" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    PRIMARY KEY ("id")
);

CREATE INDEX created_at_idx ON "notifications"("created_at");

INSERT INTO "statuses" VALUES ('Отправлено', 'Sent');
INSERT INTO "statuses" VALUES ('Ошибка', 'Error');
ALTER SEQUENCE statuses_id_seq RESTART WITH 3;
