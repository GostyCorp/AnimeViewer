create table Serie
(
	id           integer not null
		constraint Serie_pk
			primary key autoincrement,
	title        text    not null,
	titleEnglish text,
	titleRomanji text,
	titleFrench  text,
	titleOther   text,
	urlVF        text,
	urlVostFr    text,
	urlImage     text,
	genre        text
);

create table Episode
(
	id       integer not null
		constraint Episode_pk
			primary key autoincrement,
	serie    integer not null
		constraint Episode_Serie_id_fk
			references Serie
			on update cascade on delete cascade,
	number   integer not null,
	type     text    not null,
	url      text,
	urlImage text,
	constraint Episode_pk2
		unique (number, serie, type) on conflict rollback
);

create table History
(
	id      integer not null
		constraint History_pk
			primary key autoincrement,
	episode integer not null
		constraint History_pk2
			unique
				on conflict rollback
		constraint History_Episode_id_fk
			references Episode
			on update cascade on delete cascade,
	time    REAL    not null,
	maxTime REAL not null,
	date    integer not null
);