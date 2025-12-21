CREATE TABLE Users (
	id SERIAL PRIMARY KEY,
	username VARCHAR(255) UNIQUE,
	pw_hash VARCHAR(255)
);

CREATE TABLE MATCH (
	id SERIAL PRIMARY KEY,
	match_name VARCHAR(255),
	id_host INT,
	id_participant INT,
	done BOOLEAN,
	turn_host BOOLEAN
);

CREATE TABLE Battleground (
	id SERIAL PRIMARY KEY,
	id_match INT,
	field_host BOOLEAN,
	y_cord INT,
	field_a INT,
	field_b INT,
	field_c INT,
	field_d INT,
	field_e INT,
	field_f INT,
	field_g INT,
	field_h INT,
	field_i INT,
	field_j INT
);
	
	
	
	
	