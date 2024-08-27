ALTER TABLE tables ADD COLUMN IF NOT EXISTS resource_path varchar(1024);
ALTER TABLE tables ADD COLUMN IF NOT EXISTS created_by varchar(50) default 'thanh.tran@yokogawa.com';
ALTER TABLE tables ADD COLUMN IF NOT EXISTS asset_created_by varchar(50) default 'thanh.tran@yokogawa.com';
CREATE INDEX IF NOT EXISTS idx_tables_resourcepath_assetcreatedby ON tables(resource_path, asset_created_by);
