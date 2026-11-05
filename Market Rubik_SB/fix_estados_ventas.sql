-- Script para actualizar algunas ventas a estados "Pendiente" y "Cancelada"

-- Pone 10 ventas en estado "Pendiente"
UPDATE "Ventas" 
SET "Estado" = 'Pendiente' 
WHERE "Id" IN (
    SELECT "Id" FROM "Ventas" WHERE "Estado" = 'Completada' LIMIT 10
);

-- Pone 10 ventas en estado "Cancelada"
UPDATE "Ventas" 
SET "Estado" = 'Cancelada' 
WHERE "Id" IN (
    SELECT "Id" FROM "Ventas" WHERE "Estado" = 'Completada' LIMIT 10
);

-- (Opcional) Si en lugar de 10 prefieres que miles de registros se distribuyan
-- aleatoriamente entre los 3 estados para que el reporte se vea más realista, 
-- puedes ejecutar esto en su lugar:
/*
UPDATE "Ventas" 
SET "Estado" = (ARRAY['Completada', 'Pendiente', 'Cancelada'])[floor(random()*3)+1] 
WHERE "Activo" = true;
*/
