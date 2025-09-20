USE [DEV]
GO

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
	Step varchar(max) not null,
	SubText varchar(max) null
);

create table Ingredient(
	ID int not null identity primary key,
	IngredientName varchar(50) not null,
);
create table RecipeIngredients(
	IngID int not null,
	RecipeID int not null,
	IngredientNameModifier varchar(50),
	Quantity float not null,
	QuantityDesc varchar(40) null
	Primary key(IngID, RecipeID)
);
GO

