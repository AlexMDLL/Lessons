-- ============================================================================
--  Урок 8. Центр управления космической станцией.
--  Скрипт создаёт таблицы и наполняет их данными.
--
--  Запускать повторно безопасно:
--    * таблицы создаются только если их ещё нет (CREATE TABLE IF NOT EXISTS);
--    * данные вставляются только в пустые таблицы (WHERE NOT EXISTS).
--
--  Приложению нужны права: SELECT на обе таблицы и UPDATE на crew.
-- ============================================================================

CREATE TABLE IF NOT EXISTS crew (
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name varchar(100) NOT NULL,
    role varchar(50) NOT NULL,
    health integer NOT NULL CHECK (health BETWEEN 0 AND 100),
    status varchar(30) NOT NULL
);

CREATE TABLE IF NOT EXISTS station_systems (
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name varchar(100) NOT NULL,
    status varchar(30) NOT NULL
);

-- ---------------------------------------------------------------- экипаж
INSERT INTO crew (name, role, health, status)
SELECT v.name, v.role, v.health, v.status
FROM (VALUES
    ('Александр Малыгин',  'Командир',       100, 'На станции'),
    ('Талина Захарова',    'Бортинженер',    100, 'На станции'),
    ('Владислав Чисталев', 'Пилот',           92, 'На станции'),
    ('Никита Андрушкевич', 'Инженер',         84, 'На станции'),
    ('Данил Пономарев',    'Медик',           97, 'На станции'),
    ('Иосиф Колегов',      'Инженер',         71, 'На станции'),
    ('Алексей Григорун',   'Оператор связи',  63, 'Медицинский отсек')
) AS v(name, role, health, status)
WHERE NOT EXISTS (SELECT 1 FROM crew);

-- ------------------------------------------------------- системы станции
INSERT INTO station_systems (name, status)
SELECT v.name, v.status
FROM (VALUES
    ('Двигатели',        'OK'),
    ('Жизнеобеспечение', 'OK'),
    ('Навигация',        'WARNING'),
    ('Связь',            'OK'),
    ('Энергосистема',    'OK'),
    ('Терморегуляция',   'WARNING'),
    ('Стыковочный узел', 'OK')
) AS v(name, status)
WHERE NOT EXISTS (SELECT 1 FROM station_systems);

-- ------------------------------------------------------------- проверка
-- SELECT id, name, role, health, status FROM crew ORDER BY id;
-- SELECT id, name, status FROM station_systems ORDER BY id;
