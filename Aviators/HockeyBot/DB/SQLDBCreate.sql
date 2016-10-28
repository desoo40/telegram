DROP TABLE IF EXISTS player;
DROP TABLE IF EXISTS event;

CREATE TABLE player(
    id INTEGER PRIMARY KEY,
    number INTEGER NULL,
    name TEXT NOT NULL,
    lastname TEXT NOT NULL,
	lastname_lower TEXT NOT NULL,
    photo TEXT NULL,	
	position TEXT NULL
);

CREATE TABLE event(
    id INTEGER PRIMARY KEY,
	type TEXT NOT NULL,
    date TEXT NOT NULL,
    time TEXT NOT NULL,
    place TEXT NULL,
    address TEXT NULL,
    details TEXT NULL,
    members TEXT NULL
);
