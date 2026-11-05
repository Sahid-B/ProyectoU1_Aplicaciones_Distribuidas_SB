-- ==============================================================================
-- SCRIPT OFICIAL PGSQL PARA POBLAR EXACTAMENTE 1.000.000 DE REGISTROS
-- PROYECTO: SUPERMARKET RUBIK - CLAVES FORÁNEAS DINÁMICAS (CORREGIDO JOIN CLÁUSULA)
-- ==============================================================================

-- 1. LIMPIEZA INICIAL DE DATOS DE NEGOCIO
TRUNCATE TABLE 
    "MovimientosInventario", 
    "TransaccionesPago", 
    "DetallesVenta", 
    "Ventas", 
    "DetallesCompra", 
    "Compras", 
    "Productos", 
    "Clientes", 
    "Empleados", 
    "Sucursales", 
    "Proveedores", 
    "Categorias" 
RESTART IDENTITY CASCADE;

-- 2. POBLAR CATEGORÍAS (200 REGISTROS CON ULTRA VARIEDAD)
INSERT INTO "Categorias" ("Nombre", "Descripcion", "Activo", "FechaCreacion")
SELECT 
    (ARRAY[
        'Abarrotes Granel', 'Lácteos Frescos', 'Carnes Rojas Prime', 'Bebidas Energéticas', 'Limpieza del Hogar', 
        'Panadería Artesanal', 'Frutas Seleccionadas', 'Verduras Orgánicas', 'Golosinas Dulces', 'Cuidado Personal Facial', 
        'Mascotas Perros', 'Congelados Marinos', 'Embutidos Gourmet', 'Condimentos Especias', 'Licores Nacionales', 
        'Ferretería Liviana', 'Papelería Oficina', 'Hogar y Decora', 'Farmacia OTC', 'Cosméticos Capilares',
        'Snacks Salados', 'Jugos Naturales', 'Pastas e Italianos', 'Salsas y Aderezos', 'Café e Infusiones',
        'Higiene Bucal', 'Cuidado del Bebé', 'Mascotas Gatos', 'Aseo de Ropa', 'Desperdicios Reciclaje'
    ])[floor(random()*30)+1] || ' ' ||
    (ARRAY['Especial', 'Premium', 'Económico', 'Familiar', 'Importado', 'Nacional', 'Bio-Eco', 'Sin Azúcar', 'Dietético', 'Sin Gluten'])[floor(random()*10)+1] || ' Cat-' || lpad(i::text, 4, '0'),
    'Línea de productos especializada de alta demanda #' || i, 
    true, NOW() - (floor(random() * 365)::int * INTERVAL '1 day')
FROM generate_series(1, 200) s(i);

-- 3. POBLAR PROVEEDORES (2.000 REGISTROS)
INSERT INTO "Proveedores" ("Nombre", "Contacto", "Telefono", "Correo", "Activo", "FechaCreacion")
SELECT 
    (ARRAY['Distribuidora', 'Importadora', 'Comercializadora', 'Empacadora', 'Logística', 'Mayorista', 'Almacenes', 'Grupo Empresarial', 'Corporación', 'Consorcio', 'Industrias', 'Inversiones', 'Mercantil', 'Procesadora', 'Agropecuaria'])[floor(random()*15)+1] || ' ' || 
    (ARRAY['Nacional', 'Regional', 'Andina', 'Global', 'Express', 'Latina', 'Mundial', 'Unida', 'Pacífico', 'Ecuatoriana', 'del Norte', 'del Sur', 'Intercontinental', 'Occidental', 'Central'])[floor(random()*15)+1] || ' ' || 
    (ARRAY['S.A.', 'Cía. Ltda.', 'Corporación', 'Industrias', 'Logistics', 'Group', 'Solutions', 'Ecuador', 'Trade', 'Enterprises'])[floor(random()*10)+1] || ' #' || i::text,
    (ARRAY['Carlos', 'Ana', 'Luis', 'María', 'Jorge', 'Lucía', 'José', 'Carmen', 'Miguel', 'Rosa', 'Pedro', 'Elena', 'Fernando', 'Laura', 'Diego', 'Alejandro', 'Sofía', 'Manuel', 'Isabella', 'David', 'Francisco', 'Beatriz', 'Sebastián', 'Mariana', 'Mateo', 'Camila', 'Daniel', 'Victoria', 'Gabriel', 'Valentina'])[floor(random()*30)+1] || ' ' || 
    (ARRAY['García', 'Martínez', 'López', 'González', 'Pérez', 'Rodríguez', 'Sánchez', 'Ramírez', 'Cruz', 'Flores', 'Gómez', 'Díaz', 'Reyes', 'Morales', 'Ortiz', 'Castillo', 'Chávez', 'Romero', 'Herrera', 'Medina', 'Vega', 'Guzmán', 'Silva', 'Rojas', 'Mendoza', 'Soto', 'Castro', 'Vargas', 'Suárez', 'Torres'])[floor(random()*30)+1],
    '09' || floor(random() * 90000000 + 10000000)::text,
    'contacto_' || i || '@prov_' || (ARRAY['rubik', 'distribuidor', 'comercial', 'logistica', 'ventas', 'import', 'supply', 'market'])[floor(random()*8)+1] || '.com',
    true, NOW() - (floor(random() * 365)::int * INTERVAL '1 day')
FROM generate_series(1, 2000) s(i);

-- 4. POBLAR SUCURSALES (100 REGISTROS)
INSERT INTO "Sucursales" ("Nombre", "Direccion", "Telefono", "Activo", "FechaCreacion")
SELECT 
    'Supermarket Rubik ' || (ARRAY['Quito Norte', 'Quito Sur', 'Cumbayá', 'Tumbaco', 'Guayaquil Samborondón', 'Guayaquil Ceibos', 'Cuenca Centro', 'Ambato Ficoa', 'Manta Barbasquillo', 'Portoviejo', 'Ibarra', 'Loja', 'Machala', 'Santo Domingo', 'Esmeraldas', 'Riobamba', 'Latacunga', 'Tulcán', 'Babahoyo', 'Quevedo'])[floor(random()*20)+1] || ' #' || i::text,
    'Av. ' || (ARRAY['Amazonas', '10 de Agosto', 'Shyris', 'República', 'Occidental', 'Interoceánica', '6 de Diciembre', 'Eloy Alfaro', 'Orellana', 'Cristóbal Colón', 'Naciones Unidas', '6 de Diciembre', 'Granados', 'La Prensa'])[floor(random()*14)+1] || ' N° ' || (floor(random()*900 + 100)::text) || ' y Calle ' || i,
    '09' || floor(random() * 90000000 + 10000000)::text,
    true, NOW() - (floor(random() * 365)::int * INTERVAL '1 day')
FROM generate_series(1, 100) s(i);

-- 5. POBLAR EMPLEADOS (2.000 REGISTROS)
INSERT INTO "Empleados" ("Nombre", "Apellido", "Cedula", "Cargo", "Telefono", "Salario", "Activo", "FechaCreacion")
SELECT 
    (ARRAY['Carlos', 'Ana', 'Luis', 'María', 'Jorge', 'Lucía', 'José', 'Carmen', 'Miguel', 'Rosa', 'Pedro', 'Elena', 'Fernando', 'Laura', 'Diego', 'Alejandro', 'Sofía', 'Manuel', 'Isabella', 'David', 'Francisco', 'Beatriz', 'Sebastián', 'Mariana', 'Mateo', 'Camila', 'Daniel', 'Victoria', 'Gabriel', 'Valentina', 'Adrián', 'Natalia', 'Samuel', 'Andrea', 'Joaquín', 'Paula', 'Emmanuel', 'Valeria', 'Agustín', 'Claudia'])[floor(random()*40)+1],
    (ARRAY['García', 'Martínez', 'López', 'González', 'Pérez', 'Rodríguez', 'Sánchez', 'Ramírez', 'Cruz', 'Flores', 'Gómez', 'Díaz', 'Reyes', 'Morales', 'Ortiz', 'Castillo', 'Chávez', 'Romero', 'Herrera', 'Medina', 'Vega', 'Guzmán', 'Silva', 'Rojas', 'Mendoza', 'Soto', 'Castro', 'Vargas', 'Suárez', 'Torres', 'Domínguez', 'Vásquez', 'Velasco', 'Paredes', 'Salazar', 'Alarcón', 'Andrade', 'Benítez', 'Bravo', 'Cabrera'])[floor(random()*40)+1],
    lpad((1700000000 + i)::text, 10, '0'),
    (ARRAY['Cajero Principal', 'Bodeguero Senior', 'Vendedor de Mostrador', 'Supervisor de Turno', 'Gerente de Sucursal', 'Asistente de Inventario', 'Coordinador de Logística', 'Encargado de Calidad'])[floor(random()*8)+1],
    '09' || floor(random() * 90000000 + 10000000)::text,
    round((random() * 1500 + 460)::numeric, 2),
    true, NOW() - (floor(random() * 365)::int * INTERVAL '1 day')
FROM generate_series(1, 2000) s(i);

-- 6. POBLAR CLIENTES (150.000 REGISTROS)
INSERT INTO "Clientes" ("Nombre", "Apellido", "Cedula", "Telefono", "PuntosAcumulados", "Activo", "FechaCreacion")
SELECT 
    (ARRAY['Carlos', 'Ana', 'Luis', 'María', 'Jorge', 'Lucía', 'José', 'Carmen', 'Miguel', 'Rosa', 'Pedro', 'Elena', 'Fernando', 'Laura', 'Diego', 'Alejandro', 'Sofía', 'Manuel', 'Isabella', 'David', 'Francisco', 'Beatriz', 'Sebastián', 'Mariana', 'Mateo', 'Camila', 'Daniel', 'Victoria', 'Gabriel', 'Valentina', 'Adrián', 'Natalia', 'Samuel', 'Andrea', 'Joaquín', 'Paula', 'Emmanuel', 'Valeria', 'Agustín', 'Claudia', 'Tomás', 'Daniela', 'Lucas', 'Gabriela', 'Nicolás', 'Emilia', 'Leonardo', 'Renata', 'Santiago', 'Sara'])[floor(random()*50)+1],
    (ARRAY['García', 'Martínez', 'López', 'González', 'Pérez', 'Rodríguez', 'Sánchez', 'Ramírez', 'Cruz', 'Flores', 'Gómez', 'Díaz', 'Reyes', 'Morales', 'Ortiz', 'Castillo', 'Chávez', 'Romero', 'Herrera', 'Medina', 'Vega', 'Guzmán', 'Silva', 'Rojas', 'Mendoza', 'Soto', 'Castro', 'Vargas', 'Suárez', 'Torres', 'Domínguez', 'Vásquez', 'Velasco', 'Paredes', 'Salazar', 'Alarcón', 'Andrade', 'Benítez', 'Bravo', 'Cabrera', 'Campos', 'Cárdenas', 'Carrión', 'Cedeño', 'Delgado', 'Espinoza', 'Figueroa', 'Franco', 'Fuentes', 'Guerrero'])[floor(random()*50)+1],
    lpad((1000000000 + i)::text, 10, '0'),
    '09' || floor(random() * 90000000 + 10000000)::text,
    0,
    true, NOW() - (floor(random() * 365)::int * INTERVAL '1 day')
FROM generate_series(1, 150000) s(i);

-- 7. POBLAR PRODUCTOS (50.000 REGISTROS CON LLAVES FORÁNEAS DINÁMICAS)
WITH cats AS (SELECT array_agg("Id") AS ids, array_agg("Nombre") AS nombres, array_length(array_agg("Id"), 1) as len FROM "Categorias"),
     provs AS (SELECT array_agg("Id") AS ids, array_length(array_agg("Id"), 1) as len FROM "Proveedores")
INSERT INTO "Productos" ("Nombre", "Descripcion", "Precio", "Stock", "CategoriaId", "ProveedorId", "Activo", "FechaCreacion")
SELECT 
    CASE 
        WHEN cats.nombres[c_idx] LIKE '%Abarrotes%' THEN (ARRAY['Arroz Extra', 'Frijol Negro', 'Lenteja Tierna', 'Garbanzo Seleccionado', 'Quinoa Real', 'Harina de Trigo', 'Aceite de Girasol', 'Aceite de Oliva', 'Azúcar Blanca', 'Panela Molida'])[floor(random()*10)+1]
        WHEN cats.nombres[c_idx] LIKE '%Lácteos%' THEN (ARRAY['Leche Entera', 'Leche Semidescremada', 'Leche Deslactosada', 'Yogurt de Fresa', 'Yogurt Natural', 'Queso Fresco', 'Queso Mozzarella', 'Queso Cheddar', 'Mantequilla Criolla'])[floor(random()*9)+1]
        WHEN cats.nombres[c_idx] LIKE '%Carnes%' THEN (ARRAY['Pollo Entero', 'Pechuga de Pollo', 'Carne Molida Especial', 'Lomo de Res', 'Costilla de Cerdo', 'Chuleta de Cerdo'])[floor(random()*6)+1]
        WHEN cats.nombres[c_idx] LIKE '%Bebidas%' OR cats.nombres[c_idx] LIKE '%Jugos%' THEN (ARRAY['Agua Mineral Sin Gas', 'Agua Con Gas', 'Jugo de Naranja', 'Néctar de Guayaba', 'Gaseosa Cola', 'Gaseosa Naranja'])[floor(random()*6)+1]
        WHEN cats.nombres[c_idx] LIKE '%Limpieza%' OR cats.nombres[c_idx] LIKE '%Aseo%' THEN (ARRAY['Jabón de Lavar Ropa', 'Detergente en Polvo', 'Detergente Líquido', 'Suavizante de Telas', 'Cloro Limpiador', 'Desinfectante Multiuso', 'Lavavajillas Líquido'])[floor(random()*7)+1]
        WHEN cats.nombres[c_idx] LIKE '%Panadería%' OR cats.nombres[c_idx] LIKE '%Golosinas%' OR cats.nombres[c_idx] LIKE '%Pastas%' THEN (ARRAY['Pan de Molde', 'Pan Integral', 'Pan de Dulce', 'Galletas de Sal', 'Galletas Dulces', 'Chocolate en Polvo', 'Pasta Espagueti', 'Pasta Tallarín'])[floor(random()*8)+1]
        WHEN cats.nombres[c_idx] LIKE '%Frutas%' OR cats.nombres[c_idx] LIKE '%Verduras%' THEN (ARRAY['Manzanas Rojas', 'Plátano Seda', 'Tomate Riñón', 'Cebolla Paiteña', 'Papas Cholas', 'Zanahoria Lavada'])[floor(random()*6)+1]
        WHEN cats.nombres[c_idx] LIKE '%Cuidado%' OR cats.nombres[c_idx] LIKE '%Higiene%' OR cats.nombres[c_idx] LIKE '%Cosméticos%' THEN (ARRAY['Jabón de Baño', 'Jabón Líquido', 'Shampoo Antiquiebra', 'Crema Dental', 'Enjuague Bucal', 'Desodorante en Roll-On', 'Pañitos Húmedos'])[floor(random()*7)+1]
        WHEN cats.nombres[c_idx] LIKE '%Mascotas%' THEN (ARRAY['Alimento para Perro Adulto', 'Alimento Cachorro', 'Alimento para Gato', 'Arena Sanitaria Gatos'])[floor(random()*4)+1]
        WHEN cats.nombres[c_idx] LIKE '%Congelados%' OR cats.nombres[c_idx] LIKE '%Embutidos%' THEN (ARRAY['Salchicha de Pavo', 'Salchicha Vienesa', 'Jamón de Espalda', 'Mortadela Especial', 'Chorizo Parrillero', 'Salami Italiano', 'Atún en Aceite', 'Sardinas en Tomate'])[floor(random()*8)+1]
        WHEN cats.nombres[c_idx] LIKE '%Licores%' THEN (ARRAY['Cerveza Rubia', 'Cerveza Negra', 'Vino Tinto', 'Vino Blanco', 'Ron Añejo', 'Whisky Escocés', 'Vodka Premium'])[floor(random()*7)+1]
        WHEN cats.nombres[c_idx] LIKE '%Condimentos%' OR cats.nombres[c_idx] LIKE '%Salsas%' THEN (ARRAY['Salsa de Tomate', 'Mayonesa Real', 'Mostaza Especial', 'Salsa de Soya', 'Sal Paramera', 'Orégano Molido', 'Comino Molido', 'Pimienta Negra'])[floor(random()*8)+1]
        WHEN cats.nombres[c_idx] LIKE '%Snacks%' THEN (ARRAY['Papas Fritas', 'Nachos con Queso', 'Maní Salado', 'Chifles', 'Tostones'])[floor(random()*5)+1]
        WHEN cats.nombres[c_idx] LIKE '%Café%' THEN (ARRAY['Café Tostado y Molido', 'Café Soluble', 'Té Negro', 'Té Verde', 'Manzanilla en Filtro'])[floor(random()*5)+1]
        WHEN cats.nombres[c_idx] LIKE '%Papelería%' OR cats.nombres[c_idx] LIKE '%Ferretería%' OR cats.nombres[c_idx] LIKE '%Hogar%' OR cats.nombres[c_idx] LIKE '%Farmacia%' OR cats.nombres[c_idx] LIKE '%Desperdicios%' THEN (ARRAY['Focos LED', 'Pilas AA', 'Cinta Adhesiva', 'Cuaderno Universitario', 'Esferográfico Azul', 'Mascarillas Quirúrgicas', 'Alcohol Antiséptico', 'Fósforos de Madera', 'Fundas de Basura'])[floor(random()*9)+1]
        ELSE (ARRAY['Cesta de Compras', 'Bolsa Ecológica', 'Promoción Sorpresa'])[floor(random()*3)+1]
    END
    || ' ' || (ARRAY['Supermarket', 'Rubik Select', 'Económico', 'Familiar', 'Clásico', 'Extra Crunch', 'Orgánico 100%', 'Natural Light', 'Plus Pack', 'Edición Especial', 'Formato Ahorro', 'Calidad Superior'])[floor(random()*12)+1] 
    || ' - ' || (ARRAY['250g', '500g', '1Kg', '2Kg', '5Kg', '250ml', '500ml', '1L', '2L', 'Pack x6', 'Pack x12'])[floor(random()*11)+1] 
    || ' [Lote ' || lpad(gen.i::text, 6, '0') || ']',
    'Producto de alta demanda seleccionado para el supermercado #' || gen.i,
    round((random() * 150 + 0.50)::numeric, 2),
    floor(random() * 800 + 10)::int,
    cats.ids[c_idx],
    provs.ids[p_idx],
    true, NOW() - (floor(random() * 365)::int * INTERVAL '1 day')
FROM 
    cats, provs,
    LATERAL (
        SELECT i, 
               floor(random() * cats.len + 1)::int AS c_idx,
               floor(random() * provs.len + 1)::int AS p_idx
        FROM generate_series(1, 50000) s(i)
    ) gen;

-- 8. POBLAR COMPRAS (50.000 REGISTROS CON LLAVES FORÁNEAS DINÁMICAS)
WITH provs AS (SELECT array_agg("Id") AS ids FROM "Proveedores"),
     sucs AS (SELECT array_agg("Id") AS ids FROM "Sucursales")
INSERT INTO "Compras" ("NumeroCompra", "ProveedorId", "SucursalId", "FechaCompra", "Subtotal", "Iva", "Total", "Estado", "Activo", "FechaCreacion")
SELECT 
    'CMP-' || lpad(i::text, 7, '0'),
    provs.ids[floor(random() * array_length(provs.ids, 1) + 1)],
    sucs.ids[floor(random() * array_length(sucs.ids, 1) + 1)],
    NOW() - (floor(random() * 365)::int * INTERVAL '1 day'),
    0, 0, 0,
    (ARRAY['Completada', 'Recibida', 'Procesando', 'Aprobada'])[floor(random()*4)+1], 
    true, NOW() - (floor(random() * 365)::int * INTERVAL '1 day')
FROM generate_series(1, 50000) s(i), provs, sucs;

-- 9. POBLAR DETALLESCOMPRA (100.000 REGISTROS CON LLAVES FORÁNEAS DINÁMICAS)
WITH cmps AS (SELECT array_agg("Id") AS ids FROM "Compras"),
     prods AS (SELECT array_agg("Id") AS ids FROM "Productos")
INSERT INTO "DetallesCompra" ("CompraId", "ProductoId", "Cantidad", "PrecioCosto", "FechaCreacion")
SELECT 
    cmps.ids[floor(random() * array_length(cmps.ids, 1) + 1)],
    prods.ids[floor(random() * array_length(prods.ids, 1) + 1)],
    floor(random() * 80 + 5)::int,
    round((random() * 80 + 1.20)::numeric, 2),
    NOW() - (floor(random() * 365)::int * INTERVAL '1 day')
FROM generate_series(1, 100000) s(i), cmps, prods;

-- Actualizar Totales de Compras
UPDATE "Compras" c
SET "Subtotal" = dt."SumSubtotal",
    "Iva"      = round((dt."SumSubtotal" * 0.15)::numeric, 2),
    "Total"    = round((dt."SumSubtotal" * 1.15)::numeric, 2)
FROM (
    SELECT "CompraId", SUM("Cantidad" * "PrecioCosto") AS "SumSubtotal"
    FROM "DetallesCompra"
    GROUP BY "CompraId"
) dt
WHERE c."Id" = dt."CompraId";

-- 10. POBLAR VENTAS (200.000 REGISTROS CON LLAVES FORÁNEAS DINÁMICAS)
WITH clis AS (SELECT array_agg("Id") AS ids FROM "Clientes")
INSERT INTO "Ventas" ("NumeroVenta", "ClienteId", "FechaVenta", "Subtotal", "Iva", "Total", "Estado", "PuntosUtilizados", "DescuentoPuntos", "DescuentoPromociones", "Activo", "FechaCreacion")
SELECT 
    'VNT-' || lpad(i::text, 8, '0'),
    clis.ids[floor(random() * array_length(clis.ids, 1) + 1)],
    NOW() - (floor(random() * 365)::int * INTERVAL '1 day'),
    0, 0, 0,
    (ARRAY['Completada', 'Pendiente', 'Cancelada', 'Entregada'])[floor(random()*4)+1],
    0, 0, 0,
    true, NOW() - (floor(random() * 365)::int * INTERVAL '1 day')
FROM generate_series(1, 200000) s(i), clis;

-- 11. POBLAR DETALLESVENTA (300.000 REGISTROS CON LLAVES FORÁNEAS DINÁMICAS)
WITH vnts AS (SELECT array_agg("Id") AS ids FROM "Ventas"),
     prods AS (SELECT array_agg("Id") AS ids FROM "Productos")
INSERT INTO "DetallesVenta" ("VentaId", "ProductoId", "Cantidad", "PrecioUnitario", "FechaCreacion")
SELECT 
    t.v_id,
    t.p_id,
    floor(random() * 15 + 1)::int,
    p."Precio",
    NOW() - (floor(random() * 365)::int * INTERVAL '1 day')
FROM (
    SELECT 
        vnts.ids[floor(random() * array_length(vnts.ids, 1) + 1)] AS v_id,
        prods.ids[floor(random() * array_length(prods.ids, 1) + 1)] AS p_id
    FROM generate_series(1, 300000) s(i), vnts, prods
) t
JOIN "Productos" p ON p."Id" = t.p_id;

-- Actualizar Totales de Ventas
UPDATE "Ventas" v
SET "Subtotal" = dt."SumSubtotal",
    "Iva"      = round((dt."SumSubtotal" * 0.15)::numeric, 2),
    "Total"    = round((dt."SumSubtotal" * 1.15)::numeric, 2)
FROM (
    SELECT "VentaId", SUM("Cantidad" * "PrecioUnitario") AS "SumSubtotal"
    FROM "DetallesVenta"
    GROUP BY "VentaId"
) dt
WHERE v."Id" = dt."VentaId";

-- 12. POBLAR TRANSACCIONESPAGO (120.000 REGISTROS CON LLAVES FORÁNEAS DINÁMICAS)
WITH vnts AS (SELECT array_agg("Id") AS ids FROM "Ventas")
INSERT INTO "TransaccionesPago" ("VentaId", "ClientTransactionId", "PayphonePaymentUrl", "PayphoneTransactionId", "AmountInCents", "Status", "CreatedAt", "ConfirmedAt", "Provider", "PayPalOrderId", "PayPalCaptureId", "PayPalApprovalUrl", "GatewayResponse")
SELECT 
    vnts.ids[s.i],
    'TX-RUBIK-' || lpad(s.i::text, 8, '0'),
    'https://pay.payphonetodo.com/pay/' || s.i,
    'PP-ID-' || s.i,
    floor(random() * 25000 + 500)::int,
    (ARRAY['APPROVED', 'COMPLETED', 'PENDING'])[floor(random()*3)+1],
    NOW() - (floor(random() * 300)::int * INTERVAL '1 day'),
    NOW(),
    (ARRAY['PayPhone', 'PayPal', 'Tarjeta Crédito', 'Tarjeta Débito'])[floor(random()*4)+1],
    'PAYPAL-ORD-' || s.i,
    'CAP-PAYPAL-' || s.i,
    'https://www.sandbox.paypal.com/checkoutnow?token=PAYPAL-ORD-' || s.i,
    '{"status":"APPROVED", "code":"200"}'
FROM generate_series(1, 120000) s(i), vnts;

-- 13. POBLAR MOVIMIENTOSINVENTARIO (25.700 REGISTROS CON LLAVES FORÁNEAS DINÁMICAS)
WITH prods AS (SELECT array_agg("Id") AS ids FROM "Productos"),
     sucs AS (SELECT array_agg("Id") AS ids FROM "Sucursales")
INSERT INTO "MovimientosInventario" ("ProductoId", "SucursalId", "TipoMovimiento", "Cantidad", "StockAnterior", "StockPosterior", "Referencia", "FechaMovimiento", "UsuarioId", "Observacion")
SELECT 
    prods.ids[floor(random() * array_length(prods.ids, 1) + 1)],
    sucs.ids[floor(random() * array_length(sucs.ids, 1) + 1)],
    (ARRAY['Ingreso Compra', 'Salida Venta', 'Devolucion Cliente', 'Ajuste Fisico', 'Transferencia Sucursal', 'Merma por Caducidad'])[floor(random()*6)+1],
    floor(random() * 30 + 1)::int,
    floor(random() * 150 + 20)::int,
    floor(random() * 180 + 10)::int,
    'KARDEX-REF-' || lpad(i::text, 6, '0'),
    NOW() - (floor(random() * 180)::int * INTERVAL '1 day'),
    'admin@marketrubik.com',
    'Movimiento kardex de alta precisión e intercalado registrado automáticamente'
FROM generate_series(1, 25700) s(i), prods, sucs;

-- ==============================================================================
-- CONSULTA OBLIGATORIA DE COMPROBACIÓN DEL MILLÓN DE REGISTROS
-- ==============================================================================
SELECT 
    (SELECT COUNT(*) FROM "Categorias") AS "Categorias (200)",
    (SELECT COUNT(*) FROM "Proveedores") AS "Proveedores (2.000)",
    (SELECT COUNT(*) FROM "Sucursales") AS "Sucursales (100)",
    (SELECT COUNT(*) FROM "Empleados") AS "Empleados (2.000)",
    (SELECT COUNT(*) FROM "Clientes") AS "Clientes (150.000)",
    (SELECT COUNT(*) FROM "Productos") AS "Productos (50.000)",
    (SELECT COUNT(*) FROM "Compras") AS "Compras (50.000)",
    (SELECT COUNT(*) FROM "DetallesCompra") AS "DetallesCompra (100.000)",
    (SELECT COUNT(*) FROM "Ventas") AS "Ventas (200.000)",
    (SELECT COUNT(*) FROM "DetallesVenta") AS "DetallesVenta (300.000)",
    (SELECT COUNT(*) FROM "TransaccionesPago") AS "TransaccionesPago (120.000)",
    (SELECT COUNT(*) FROM "MovimientosInventario") AS "MovimientosInventario (25.700)",
    ((SELECT COUNT(*) FROM "Categorias") +
     (SELECT COUNT(*) FROM "Proveedores") +
     (SELECT COUNT(*) FROM "Sucursales") +
     (SELECT COUNT(*) FROM "Empleados") +
     (SELECT COUNT(*) FROM "Clientes") +
     (SELECT COUNT(*) FROM "Productos") +
     (SELECT COUNT(*) FROM "Compras") +
     (SELECT COUNT(*) FROM "DetallesCompra") +
     (SELECT COUNT(*) FROM "Ventas") +
     (SELECT COUNT(*) FROM "DetallesVenta") +
     (SELECT COUNT(*) FROM "TransaccionesPago") +
     (SELECT COUNT(*) FROM "MovimientosInventario")) AS "TOTAL_EXACTO";
