/*==============================================================*/
/* Table: place                                                 */
/*==============================================================*/
create table place (
   id                   INTEGER PRIMARY KEY,
   name                 TEXT                 null,
   name_lower           TEXT                 null,
   geoposition          TEXT                 null,
   adress               TEXT                 null   
);

/*==============================================================*/
/* Table: tournament                                            */
/*==============================================================*/
create table tournament (
   id                   INTEGER PRIMARY KEY,
   name                 TEXT                 null,
   name_lower                 TEXT                 null
);

/*==============================================================*/
/* Table: season                                                */
/*==============================================================*/
create table season (
   id                  INTEGER PRIMARY KEY,
   name                 TEXT                 null
);

/*==============================================================*/
/* Table: game                                                  */
/*==============================================================*/
create table game (
   id                    INTEGER PRIMARY KEY,
   place_id             INT4                 null,
   date                 TEXT                 null,
   op_team_id           INT4                 null,
   score                INT4                 null,
   op_score             INT4                 null,
   tournament_id        INT4                 null,
   season_id            INT4                 null,
   viewers_count        INT4                 null,
   best_player_id       INT4                 null, 
   penaltygame          text                 null,	
   description                 TEXT                 null, 
   constraint FK_GAME_REFERENCE_playe foreign key (best_player_id)
      references player (id)
      on delete restrict on update restrict, 
   constraint FK_GAME_REFERENCE_PLACE foreign key (place_id)
      references place (id)
      on delete restrict on update restrict,
   constraint FK_GAME_REFERENCE_TOURNAME foreign key (tournament_id)
      references tournament (id)
      on delete restrict on update restrict

);

/*==============================================================*/
/* Table: player                                                */
/*==============================================================*/
create table player (
   id                   INTEGER PRIMARY KEY,
   name                 TEXT                 null,
   lastname             TEXT                 null,
   number               INT4                 null,
   positionid           INT4                 null
);

/*==============================================================*/
/* Table: game_action                                           */
/*==============================================================*/
create table game_action (
   id                    INTEGER PRIMARY KEY,
   game_id              INT4                 null,
   player_id            INT4                 null,
   action               INT4                 null,   
   param               INT4                 null,   
   constraint FK_GAME_ACT_REFERENCE_PLAYER foreign key (player_id)
      references player (id)
      on delete restrict on update restrict,
   constraint FK_GAME_ACT_REFERENCE_GAME foreign key (game_id)
      references game (id)
      on delete restrict on update restrict
);

/*==============================================================*/
/* Table: goal                                           */
/*==============================================================*/
create table goal (
   id                    INTEGER PRIMARY KEY,
   game_id              INT4                 null,
   pp            BOOL                 null,
   sh               BOOL                 null, 
   penalty            text                 null,	  
   constraint FK_GAME_ACT_REFERENCE_GAME foreign key (game_id)
      references game (id)
      on delete restrict on update restrict
);

/*==============================================================*/
/* Table: goal_player                                           */
/*==============================================================*/
create table goal_player (
   id                    INTEGER PRIMARY KEY,
   goal_id              INT4                 null,
	player_id            INT4                 null,
	asist            text                 null,	
	constraint FK_GAME_ACT_REFERENCE_PLAYER foreign key (player_id)
      references player (id)
      on delete restrict on update restrict,	   
   constraint FK_GAME_ACT_REFERENCE_goME foreign key (goal_id)
      references goal (id)
      on delete restrict on update restrict
);

/*==============================================================*/
/* Table: team                                               */
/*==============================================================*/
create table team (
   id                    INTEGER PRIMARY KEY,
   name                 TEXT                 null,
   name_lower                 TEXT                 null,
   logo                 TEXT                 null,
   town                 TEXT                 null
);

INSERT INTO team(name, name_lower) VALUES ('Авиаторы', 'авиаторы');

/*==============================================================*/
/* Table: game_stat                                             */
/*==============================================================*/
create table game_stat (
   id                  INTEGER PRIMARY KEY,
   game_id              INT4                 null,
   team_id              INT4                 null,
   shots                INT4                 null,
   shots_in             INT4                 null,
   faceoff              INT4                 null,
   hits                 INT4                 null,
   block_shots         INT4                 null,
   penalty              INT4                 null,
   constraint FK_GAME_STA_REFERENCE_GAME foreign key (game_id)
      references game (id)
      on delete cascade on update cascade,
   constraint FK_GAME_STA_REFERENCE_TEAMS foreign key (team_id)
      references team (id)
      on delete restrict on update restrict
);

/*==============================================================*/
/* Table: goalies                                               */
/*==============================================================*/
create table goalies (
   id                  INTEGER PRIMARY KEY,
   player_id            INT4                 null,
   shots                INT4                 null,
   saves                INT4                 null,
   percent              INT4                 null,
   GAA                  INT4                 null,
   wins                 INT4                 null,
   shotout              INT4                 null,
   constraint FK_GOALIES_REFERENCE_PLAYER foreign key (player_id)
      references player (id)
      on delete restrict on update restrict
);

/*==============================================================*/
/* Table: player_info                                           */
/*==============================================================*/
create table player_info (
   id                    INTEGER PRIMARY KEY,
   player_id            INT4                 null,
   vk                   TEXT                 null,
   insta                TEXT                 null,
   photo                TEXT                 null,
   constraint FK_PLAYER_I_REFERENCE_PLAYER foreign key (player_id)
      references player (id)
      on delete restrict on update restrict
);

/*==============================================================*/
/* Table: players_stat                                          */
/*==============================================================*/
create table players_stat (
   id                   INT4                 not null,
   player_id            INT4                 null,
   rating               INT4                 null,
   goals                INT4                 null,
   assist               INT4                 null,
   shots                INT4                 null,
   shots_in             INT4                 null,
   win_faceoff          INT4                 null,
   lose_faceoff         INT4                 null,
   hits                 INT4                 null,
   block_shoots         INT4                 null,
   penalty              INT4                 null,
   constraint PK_PLAYERS_STAT primary key (id),
   constraint FK_PLAYERS__REFERENCE_PLAYER foreign key (player_id)
      references player (id)
      on delete restrict on update restrict
);


/*==============================================================*/
/* Table: file_action                                           */
/*==============================================================*/
create table file_action (
   id                    INTEGER PRIMARY KEY,
   game_id              INT4                 null,
   date            TEXT                 null,
   filename               TEXT                 null,
   action               int4                 null
);

/*==============================================================*/
/* Table: chat                                           */
/*==============================================================*/
create table chat (
   id                    INTEGER PRIMARY KEY,
   username            TEXT                 null,
   firstname        TEXT                 null,
   lastname            TEXT                 null,
   isAdmin               TEXT                 null,
   isTextOInly               TEXT                 null,
   tournament_id               int4                 null
);

/*==============================================================*/
/* Table: chat_action                                           */
/*==============================================================*/
create table chat_action (
   id                    INTEGER PRIMARY KEY,
   chat_id              INT4                 null,
   date            TEXT                 null,
   text               TEXT                 null,
   action               int4                 null,
   constraint FK_PLAYfgdERS__REFERENdfgCE_PLAYER foreign key (chat_id)
      references chat (id)
      on delete cascade on update cascade
);