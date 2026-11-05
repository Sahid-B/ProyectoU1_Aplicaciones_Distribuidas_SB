-- Script optimizado para generar EXACTAMENTE 500,000 registros
-- Manteniendo la alta variedad de nombres, categorías, productos y apellidos.
-- Distribución: 
-- Categorías: 100
-- Proveedores: 1,000
-- Empleados: 1,000
-- Clientes: 80,000
-- Productos: 30,000
-- Ventas: 150,000
-- DetallesVenta: 237,800
-- Sucursales: 100
-- TOTAL: 500,000 registros

-- 1. Limpiar datos existentes
TRUNCATE TABLE "Sucursales", "Empleados", "DetallesVenta", "Ventas", "Productos", "Clientes", "Proveedores", "Categorias" RESTART IDENTITY CASCADE;

-- 2. Poblar Categorías (100 registros con nombres base aleatorios)
INSERT INTO "Categorias" ("Nombre", "Descripcion", "Activo", "FechaCreacion")
SELECT 
    (ARRAY['Abarrotes', 'Lácteos', 'Carnes', 'Bebidas', 'Limpieza', 'Panadería', 'Frutas', 'Verduras', 'Golosinas', 'Cuidado Personal', 'Mascotas', 'Congelados', 'Embutidos', 'Condimentos', 'Licores', 'Ferretería', 'Papelería', 'Juguetería', 'Electrónica', 'Ropa Básica', 'Calzado', 'Hogar', 'Jardinería', 'Farmacia', 'Cosméticos', 'Bebés', 'Suplementos', 'Dietéticos', 'Sin Gluten', 'Vinos Premium'])[floor(random()*30)+1] || ' Tipo ' || lpad(i::text, 3, '0'),
    'Descripción de la categoría ' || i, 
    true, NOW()
FROM generate_series(1, 100) s(i);

-- 3. Poblar Proveedores (1,000 registros con nombres combinados)
INSERT INTO "Proveedores" ("Nombre", "Contacto", "Telefono", "Correo", "Activo", "FechaCreacion")
SELECT 
    (ARRAY['Distribuidora', 'Importadora', 'Comercializadora', 'Empacadora', 'Logística', 'Mayorista', 'Almacenes', 'Grupo'])[floor(random()*8)+1] || ' ' || 
    (ARRAY['Nacional', 'Regional', 'Del Norte', 'Del Sur', 'Global', 'Express', 'Andina', 'Latina', 'Mundial', 'Unida'])[floor(random()*10)+1] || ' ' || i::text AS "Nombre",
    (ARRAY['Juan', 'Carlos', 'Ana', 'Maria', 'Pedro', 'Luis', 'Sofia', 'Jorge', 'Elena', 'Diego'])[floor(random()*10)+1] || ' ' || 
    (ARRAY['Perez', 'Gomez', 'Lopez', 'Diaz', 'Ruiz', 'Silva', 'Rojas', 'Mendoza', 'Soto', 'Castro'])[floor(random()*10)+1] AS "Contacto",
    '09' || floor(random() * 90000000 + 10000000)::text AS "Telefono",
    'ventas' || i || '@proveedor.com' AS "Correo",
    true, NOW()
FROM generate_series(1, 1000) s(i);

-- 4. Poblar Empleados (1,000 registros)
INSERT INTO "Empleados" ("Nombre", "Apellido", "Cedula", "Cargo", "Telefono", "Salario", "Activo", "FechaCreacion")
SELECT 
    (ARRAY['Carlos', 'Ana', 'Luis', 'Maria', 'Jorge', 'Lucia', 'Jose', 'Carmen', 'Miguel', 'Rosa', 'Pedro', 'Elena', 'Fernando', 'Laura', 'Diego', 'Alejandro', 'Sofia', 'Manuel', 'Isabella', 'David', 'Valeria', 'Daniel', 'Camila', 'Roberto', 'Marta', 'Alberto', 'Paula', 'Victor', 'Andrea', 'Raul'])[floor(random()*30)+1] AS "Nombre",
    (ARRAY['Garcia', 'Martinez', 'Lopez', 'Gonzalez', 'Perez', 'Rodriguez', 'Sanchez', 'Ramirez', 'Cruz', 'Flores', 'Gomez', 'Diaz', 'Reyes', 'Morales', 'Ortiz', 'Castillo', 'Chavez', 'Romero', 'Herrera', 'Medina'])[floor(random()*20)+1] AS "Apellido",
    floor(random() * 900000000 + 100000000)::text, 
    (ARRAY['Cajero', 'Reponedor', 'Supervisor', 'Gerente'])[floor(random()*4)+1], 
    '09' || floor(random() * 90000000 + 10000000)::text, 
    round((random() * 1000 + 400)::numeric, 2), 
    true, NOW()
FROM generate_series(1, 1000) s(i);

-- 5. Poblar Clientes (80,000 registros con 50 nombres y 50 apellidos)
INSERT INTO "Clientes" ("Nombre", "Apellido", "Cedula", "Telefono", "Activo", "FechaCreacion")
SELECT 
    (ARRAY['Carlos', 'Ana', 'Luis', 'Maria', 'Jorge', 'Lucia', 'Jose', 'Carmen', 'Miguel', 'Rosa', 
           'Pedro', 'Elena', 'Fernando', 'Laura', 'Diego', 'Alejandro', 'Sofia', 'Manuel', 'Isabella', 'David', 
           'Valeria', 'Daniel', 'Camila', 'Roberto', 'Marta', 'Alberto', 'Paula', 'Victor', 'Andrea', 'Raul',
           'Javier', 'Isabel', 'Gabriel', 'Patricia', 'Hugo', 'Natalia', 'Ricardo', 'Silvia', 'Guillermo', 'Teresa',
           'Hector', 'Monica', 'Francisco', 'Beatriz', 'Julio', 'Clara', 'Eduardo', 'Diana', 'Martin', 'Adriana'])[floor(random()*50)+1] AS "Nombre",
    (ARRAY['Garcia', 'Martinez', 'Lopez', 'Gonzalez', 'Perez', 'Rodriguez', 'Sanchez', 'Ramirez', 'Cruz', 'Flores', 
           'Gomez', 'Diaz', 'Reyes', 'Morales', 'Ortiz', 'Castillo', 'Chavez', 'Romero', 'Herrera', 'Medina', 
           'Vargas', 'Castro', 'Guzman', 'Fernandez', 'Ruiz', 'Alvarez', 'Torres', 'Suarez', 'Mendoza', 'Soto',
           'Silva', 'Rojas', 'Delgado', 'Aguilar', 'Vega', 'Rios', 'Navarro', 'Salazar', 'Acosta', 'Cabrera',
           'Miranda', 'Campos', 'Molina', 'Peña', 'Rivas', 'Paredes', 'Pacheco', 'Robles', 'Valenzuela', 'Rosales'])[floor(random()*50)+1] AS "Apellido",
    floor(random() * 900000000 + 100000000)::text AS "Cedula",
    '09' || floor(random() * 90000000 + 10000000)::text AS "Telefono",
    true, NOW()
FROM generate_series(1, 80000) s(i);

-- 6. Poblar Productos (30,000 registros combinados)
INSERT INTO "Productos" ("Nombre", "Descripcion", "Precio", "Stock", "CategoriaId", "ProveedorId", "Activo", "FechaCreacion")
SELECT 
    (ARRAY['Arroz', 'Frijoles', 'Leche', 'Yogurt', 'Queso', 'Pollo', 'Carne', 'Salchichas', 'Jugo', 'Refresco', 
           'Jabón', 'Shampoo', 'Detergente', 'Pan', 'Galletas', 'Manzanas', 'Plátanos', 'Tomates', 'Cebollas', 'Comida Perro', 
           'Helado', 'Mayonesa', 'Ketchup', 'Vino', 'Cerveza', 'Agua', 'Café', 'Té', 'Azúcar', 'Sal',
           'Aceite', 'Vinagre', 'Pasta', 'Lentejas', 'Atún', 'Sardinas', 'Cereales', 'Mermelada', 'Mantequilla', 'Margarina',
           'Mostaza', 'Pimienta', 'Papas Fritas', 'Nachos', 'Chocolate', 'Caramelos', 'Cepillo Dientes', 'Pasta Dental', 'Papel Higiénico', 'Servilletas'])[floor(random()*50)+1] 
    || ' ' || (ARRAY['Premium', 'Económico', 'Familiar', 'Clásico', 'Extra', 'Ligero', 'Orgánico', 'Natural', 'Plus', 'Fresco', 'Artesanal', 'Gourmet'])[floor(random()*12)+1] 
    || ' x' || (ARRAY['100g', '250g', '500g', '1Kg', '2Kg', '5Kg', '1L', '2L', '3L', 'Pack 3', 'Pack 6', 'Pack 12'])[floor(random()*12)+1] 
    || ' - Lote ' || lpad(i::text, 5, '0') AS "Nombre",
    'Producto de excelente calidad para el consumo masivo' AS "Descripcion",
    round((random() * 95 + 0.5)::numeric, 2) AS "Precio",
    floor(random() * 1000 + 10) AS "Stock",
    floor(random() * 100 + 1)::int AS "CategoriaId", -- FK a los 100 que existen
    floor(random() * 1000 + 1)::int AS "ProveedorId", -- FK a los 1000 que existen
    true, NOW()
FROM generate_series(1, 30000) s(i);

-- 7. Poblar Ventas (150,000 registros) - Subtotal y Total inicial en 0
INSERT INTO "Ventas" ("NumeroVenta", "ClienteId", "FechaVenta", "Subtotal", "Iva", "Total", "Estado", "Activo", "FechaCreacion")
SELECT 
    'V-' || lpad(i::text, 8, '0'), 
    floor(random() * 80000 + 1)::int, -- FK a los 80000 Clientes
    NOW() - (random() * 730 || ' days')::interval, 
    0, 0, 0, 
    'Completada', true, NOW()
FROM generate_series(1, 150000) s(i);

-- 7.5. Poblar Sucursales (100 registros)
INSERT INTO "Sucursales" ("Nombre", "Direccion", "Telefono", "Activo", "FechaCreacion")
SELECT 
    'Sucursal ' || (ARRAY['Norte', 'Sur', 'Centro', 'Este', 'Oeste', 'Principal', 'Express', 'Plaza', 'Mall', 'Avenida'])[floor(random()*10)+1] || ' ' || i::text,
    'Dirección de prueba ' || i,
    '09' || floor(random() * 90000000 + 10000000)::text,
    true, NOW()
FROM generate_series(1, 100) s(i);

-- 8. Poblar DetallesVenta (237,800 registros exactos)
INSERT INTO "DetallesVenta" ("VentaId", "ProductoId", "Cantidad", "PrecioUnitario", "FechaCreacion")
SELECT 
    temp.v_id,
    temp.p_id,
    floor(random() * 10 + 1),
    p."Precio", -- Usa el precio real del producto
    NOW()
FROM (
    -- Generar las llaves foraneas aleatoriamente
    SELECT 
        floor(random() * 150000 + 1)::int AS v_id,
        floor(random() * 30000 + 1)::int AS p_id
    FROM generate_series(1, 237800)
) temp
JOIN "Productos" p ON p."Id" = temp.p_id;

-- 9. Actualizar los totales reales de las Ventas en base a los Detalles
UPDATE "Ventas" v
SET "Subtotal" = dt."SumSubtotal",
    "Iva"      = dt."SumSubtotal" * 0.12,
    "Total"    = dt."SumSubtotal" * 1.12
FROM (
    SELECT "VentaId", SUM("Cantidad" * "PrecioUnitario") AS "SumSubtotal"
    FROM "DetallesVenta"
    GROUP BY "VentaId"
) dt
WHERE v."Id" = dt."VentaId";

-- Fin del script

