Create table Recipes
(
  ID int not null identity primary key,
  RecipeName varchar(50) not null,
  RecipeDesc varchar(255) null,
);

create table RecipeTags(
	ID int not null identity primary key,
	RecipeID int not null foreign key references Recipes(id),
	Tag varchar(40) not null
);

create table RecipeSteps(
	ID int not null identity primary key,
	RecipeID int not null foreign key references Recipes(id),
	Step varchar(255) not null
);

create table RecipeIngredients(
	ID int not null identity primary key,
	RecipeID int not null foreign key references Recipes(id),
	IngredientDesc varchar(50) not null,
	Quantity float not null,
	QuantityDesc varchar(40) null
);
