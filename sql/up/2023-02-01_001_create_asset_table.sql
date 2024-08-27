CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

drop table if exists asset_table_columns;
drop table if exists asset_tables;

create table if not exists tables
(
	id uuid DEFAULT uuid_generate_v4(),
	name varchar(255) not null,
	asset_id uuid null,
	asset_name varchar(255) NULL,
	created_utc timestamp without time zone not null default now(),
    updated_utc timestamp without time zone not null default now(),
	deleted boolean not null default false,
    script varchar null,
    old_name varchar(255) null,
	constraint pk_tables primary key(id)
);

create table if not exists columns
(
	id int GENERATED ALWAYS AS IDENTITY,
	name varchar(255) not null,
	is_primary boolean not null,
	type_code varchar(50) not null,
	default_value text,
	table_id uuid not null,
    allow_null boolean not null default false,
    "type_name" varchar(50) null,
	column_order int default 0,
    deleted boolean not null default false,
	created_utc TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_utc TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
	constraint pk_columns primary key(id),
	constraint fk_columns_table_id foreign key(table_id) references tables(id) ON DELETE CASCADE
);