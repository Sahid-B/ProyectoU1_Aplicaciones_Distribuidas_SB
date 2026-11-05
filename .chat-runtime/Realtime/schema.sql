CREATE TABLE IF NOT EXISTS market_chat (
 "Id" bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
 "Sender" text NOT NULL, "Recipient" text NOT NULL, "Body" text NOT NULL,
 "ProductId" integer, "ProductName" text, "Price" numeric(10,2),
 "Created" timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS market_chat_pair ON market_chat ("Sender","Recipient","Id");
ALTER TABLE "Ventas" ADD COLUMN IF NOT EXISTS "CompradorId" text;
CREATE OR REPLACE FUNCTION market_change() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 IF TG_TABLE_NAME = 'Productos' THEN
  PERFORM pg_notify('market_change', json_build_object('kind','stock','id',NEW."Id",'stock',NEW."Stock")::text);
 ELSE
  PERFORM pg_notify('market_change', json_build_object('kind',CASE WHEN TG_OP='INSERT' THEN 'sale' ELSE 'order' END,
   'id',NEW."Id",'owner',NEW."CompradorId",'status',NEW."Estado")::text);
 END IF;
 RETURN NEW;
END $$;
CREATE OR REPLACE TRIGGER market_stock AFTER UPDATE OF "Stock" ON "Productos"
 FOR EACH ROW WHEN (OLD."Stock" IS DISTINCT FROM NEW."Stock") EXECUTE FUNCTION market_change();
CREATE OR REPLACE TRIGGER market_sale AFTER INSERT OR UPDATE OF "Estado","Total","Activo" ON "Ventas"
 FOR EACH ROW EXECUTE FUNCTION market_change();

CREATE TABLE IF NOT EXISTS market_presence (connection text PRIMARY KEY, owner text NOT NULL, touched timestamptz NOT NULL);

ALTER TABLE market_chat
 ADD COLUMN IF NOT EXISTS "ReplyTo" bigint REFERENCES market_chat("Id"),
 ADD COLUMN IF NOT EXISTS "Edited" boolean NOT NULL DEFAULT false,
 ADD COLUMN IF NOT EXISTS "Deleted" boolean NOT NULL DEFAULT false,
 ADD COLUMN IF NOT EXISTS "SenderReaction" text,
 ADD COLUMN IF NOT EXISTS "RecipientReaction" text;
