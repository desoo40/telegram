DROP TABLE IF EXISTS team;
DROP TABLE IF EXISTS position_dic;
DROP TABLE IF EXISTS player;
DROP TABLE IF EXISTS place;
DROP TABLE IF EXISTS game;
DROP TABLE IF EXISTS game_action;
DROP TABLE IF EXISTS tournament;
DROP TABLE IF EXISTS season;


CREATE TABLE team (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
	name_lower TEXT NOT NULL ,
    logo TEXT NULL,
	town text null    
);

CREATE TABLE position_dic (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL  
);

INSERT INTO position_dic (id, name) VALUES (1, 'Нападающий'),(2, 'Защитник'),(3, 'Вратарь'),(4, 'Тренер');

 
CREATE TABLE player(
    id INTEGER PRIMARY KEY,
    number INTEGER NULL,
    name TEXT NOT NULL,
    lastname TEXT NOT NULL,
	lastname_lower TEXT NOT NULL,
    photo TEXT NULL,
	team_id integer null,
	position_id integer null,
	vk_href TEXT NULL,
	insta_href TEXT NULL,
	FOREIGN KEY(team_id) REFERENCES team(id),
	FOREIGN KEY(position_id) REFERENCES position_dic(id)
);

CREATE TABLE place (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    adress TEXT NULL ,
	location  TEXT NULL 
);


CREATE TABLE game(
    id INTEGER PRIMARY KEY,
    date TEXT NOT NULL,
    opteam_id INTEGER NOT NULL,
    opteamscore INTEGER NULL,
    place_id INTEGER NULL,	
    tournament_id INTEGER NULL,	
	FOREIGN KEY(opteam_id) REFERENCES team(id),
	FOREIGN KEY(place_id) REFERENCES place(id),
	FOREIGN KEY(tournament_id) REFERENCES tournament(id)
	);

CREATE TABLE game_action(
    id INTEGER PRIMARY KEY,
    game_id INTEGER NOT NULL,
    player_id INTEGER NOT NULL,
    action INTEGER NOT NULL,
	FOREIGN KEY(game_id) REFERENCES game(id),
	FOREIGN KEY(player_id) REFERENCES player(id) ON DELETE CASCADE
	);

CREATE TABLE tournament(
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
	name_lower TEXT NOT NULL,
    season_id INTEGER NOT NULL,    
	FOREIGN KEY(season_id) REFERENCES season(id)
	);

CREATE TABLE season(
    id INTEGER PRIMARY KEY,
    name INTEGER NOT NULL    
	);

