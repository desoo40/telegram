/*==============================================================*/
/* Table: place                                                 */
/*==============================================================*/
create table place (
   id                   SERIAL               not null,
   name                 TEXT                 null,
   geoposition          TEXT                 null,
   adress               TEXT                 null,
   constraint PK_PLACE primary key (id)
);

/*==============================================================*/
/* Table: tournament                                            */
/*==============================================================*/
create table tournament (
   id                   SERIAL               not null,
   name                 TEXT                 null,
   constraint PK_TOURNAMENT primary key (id)
);

/*==============================================================*/
/* Table: season                                                */
/*==============================================================*/
create table season (
   id                   SERIAL               not null,
   name                 TEXT                 null,
   constraint PK_SEASON primary key (id)
);

/*==============================================================*/
/* Table: game                                                  */
/*==============================================================*/
create table game (
   id                   INT4                 not null,
   place_id             INT4                 null,
   op_team_id           INT4                 null,
   score                INT4                 null,
   op_score             INT4                 null,
   tournament_id        INT4                 null,
   season_id            INT4                 null,
   viewers_count        INT4                 null,
   constraint PK_GAME primary key (id),
   constraint FK_GAME_REFERENCE_PLACE foreign key (place_id)
      references place (id)
      on delete restrict on update restrict,
   constraint FK_GAME_REFERENCE_TOURNAME foreign key (tournament_id)
      references tournament (id)
      on delete restrict on update restrict,
   constraint FK_GAME_REFERENCE_SEASON foreign key (season_id)
      references season (id)
      on delete restrict on update restrict
);

/*==============================================================*/
/* Table: player                                                */
/*==============================================================*/
create table player (
   id                   SERIAL               not null,
   name                 TEXT                 null,
   lastname             TEXT                 null,
   number               INT4                 null,
   positionid           INT4                 null,
   constraint PK_PLAYER primary key (id)
);

/*==============================================================*/
/* Table: game_action                                           */
/*==============================================================*/
create table game_action (
   id                   INT4                 not null,
   game_id              INT4                 null,
   player_id            INT4                 null,
   action               INT4                 null,
   constraint PK_GAME_ACTION primary key (id),
   constraint FK_GAME_ACT_REFERENCE_PLAYER foreign key (player_id)
      references player (id)
      on delete restrict on update restrict,
   constraint FK_GAME_ACT_REFERENCE_GAME foreign key (game_id)
      references game (id)
      on delete restrict on update restrict
);

/*==============================================================*/
/* Table: teams                                                 */
/*==============================================================*/
create table teams (
   id                   SERIAL               not null,
   name                 TEXT                 null,
   logo                 PATH                 null,
   town                 TEXT                 null,
   constraint PK_TEAMS primary key (id)
);

/*==============================================================*/
/* Table: game_stat                                             */
/*==============================================================*/
create table game_stat (
   id                   INT4                 not null,
   game_id              INT4                 null,
   team_id              INT4                 null,
   shots                INT4                 null,
   shots_in             INT4                 null,
   faceoff              INT4                 null,
   hits                 INT4                 null,
   block_shoots         INT4                 null,
   penalty              INT4                 null,
   constraint PK_GAME_STAT primary key (id),
   constraint FK_GAME_STA_REFERENCE_GAME foreign key (game_id)
      references game (id)
      on delete cascade on update cascade,
   constraint FK_GAME_STA_REFERENCE_TEAMS foreign key (team_id)
      references teams (id)
      on delete restrict on update restrict
);

/*==============================================================*/
/* Table: goalies                                               */
/*==============================================================*/
create table goalies (
   id                   INT4                 not null,
   player_id            INT4                 null,
   shots                INT4                 null,
   saves                INT4                 null,
   percent              INT4                 null,
   GAA                  INT4                 null,
   wins                 INT4                 null,
   shotout              INT4                 null,
   constraint PK_GOALIES primary key (id),
   constraint FK_GOALIES_REFERENCE_PLAYER foreign key (player_id)
      references player (id)
      on delete restrict on update restrict
);

/*==============================================================*/
/* Table: player_info                                           */
/*==============================================================*/
create table player_info (
   id                   SERIAL               not null,
   player_id            INT4                 null,
   vk                   TEXT                 null,
   insta                TEXT                 null,
   photo                TEXT                 null,
   constraint PK_PLAYER_INFO primary key (id),
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
