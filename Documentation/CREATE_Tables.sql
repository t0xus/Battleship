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
	
CREATE TABLE Ships_battleground (
	id SERIAL PRIMARY KEY,
	id_match INT,
	id_ship INT,
	ship_host BOOLEAN,
	x_cord INT,
	y_cord INT,
	across BOOLEAN
);	

CREATE TABLE Ship (
	id SERIAL PRIMARY KEY,
	boxes INT,
	quantity INT,
	NAME VARCHAR(255)
);

INSERT INTO ship (boxes, quantity, name) VALUES (5, 1, 'Battleship');
INSERT INTO ship (boxes, quantity, name) VALUES (4, 2, 'Cruiser');
INSERT INTO ship (boxes, quantity, name) VALUES (3, 3, 'Destroyer');
INSERT INTO ship (boxes, quantity, name) VALUES (2, 4, 'Submarine');






	
	